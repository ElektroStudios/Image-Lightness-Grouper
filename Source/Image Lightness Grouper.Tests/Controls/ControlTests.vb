Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text
Imports System.Windows.Forms

Imports ImageLightnessGrouper.Controls
Imports ImageLightnessGrouper.Tests.TestInfrastructure
Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Tests.Controls

    ''' <summary>
    ''' Tests for the custom controls and their helpers.
    ''' </summary>
    Public NotInheritable Class ControlTests

#Region " ToggleSwitch "

        ''' <summary>
        ''' <see cref="ToggleSwitch.CheckedChanged"/> is raised only on actual changes.
        ''' </summary>
        <Fact>
        Public Sub ToggleSwitch_RaisesCheckedChangedOnlyOnActualChanges()
            Using toggle As New ToggleSwitch()
                Dim raisedCount As Integer = 0
                AddHandler toggle.CheckedChanged, Sub(sender As Object, e As EventArgs) raisedCount += 1

                toggle.Checked = False
                Assert.Equal(0, raisedCount)

                toggle.Checked = True
                toggle.Checked = True
                Assert.Equal(1, raisedCount)
                Assert.Equal("On", toggle.AccessibleDescription)

                toggle.Checked = False
                Assert.Equal(2, raisedCount)
                Assert.Equal("Off", toggle.AccessibleDescription)
            End Using
        End Sub

        ''' <summary>
        ''' Without a caption, the toggle has no hidden inner margin: its size is exactly the size of the drawn track,
        ''' which is derived from the font height (so it scales with the font and the DPI).
        ''' </summary>
        <Fact>
        Public Sub ToggleSwitch_WithoutCaption_IsTheTrackSize()
            Using toggle As New ToggleSwitch()
                Dim trackHeight As Integer = toggle.Font.Height + 4
                Assert.Equal(New Size(trackHeight * 2, trackHeight), toggle.Size)
                Assert.Equal(New Rectangle(Point.Empty, toggle.Size), toggle.ComputeLayout().TrackBounds)
            End Using
        End Sub

        ''' <summary>
        ''' With a caption, the toggle grows to fit it, and the caption is optically centered on the track: the center of
        ''' its capital letters is aligned with the center of the track (regression for the caption looking misaligned).
        ''' </summary>
        <Fact>
        Public Sub ToggleSwitch_WithCaption_GrowsToFitAndCentersTheCaptionOnTheTrack()
            Using font As New Font(FontFamily.GenericSansSerif, 9.0F),
                  toggle As New ToggleSwitch() With {.Font = font}

                Dim trackOnlySize As Size = toggle.Size
                toggle.Text = "Include subdirectories"
                Dim layout As ToggleSwitch.ToggleSwitchLayout = toggle.ComputeLayout()

                Assert.Equal(layout.Size, toggle.Size)
                Assert.True(toggle.Width > trackOnlySize.Width, "The control must grow to fit the caption.")
                Assert.True(layout.CaptionBounds.Left > layout.TrackBounds.Right, "The caption must be on the right of the track.")
                Assert.True(New Rectangle(Point.Empty, layout.Size).Contains(layout.TrackBounds), "The track must fit within the control.")
                Assert.True(New Rectangle(Point.Empty, layout.Size).Contains(layout.CaptionBounds), "The caption must fit within the control.")

                Dim trackCenter As Double = layout.TrackBounds.Top + (layout.TrackBounds.Height / 2.0R)
                Dim captionCapCenter As Double = layout.CaptionBounds.Top + ToggleSwitch.GetCapCenterFromLineTop(toggle.Font)
                Assert.InRange(Math.Abs(captionCapCenter - trackCenter), 0.0R, 0.5R)

                toggle.Text = String.Empty
                Assert.Equal(trackOnlySize, toggle.Size)
            End Using
        End Sub

        ''' <summary>
        ''' The cap-height center lies within the line box, and a null font is rejected.
        ''' </summary>
        <Fact>
        Public Sub ToggleSwitch_CapCenter_LiesWithinTheLineBox()
            Using font As New Font(FontFamily.GenericSansSerif, 9.0F)
                Dim capCenter As Double = ToggleSwitch.GetCapCenterFromLineTop(font)
                Assert.InRange(capCenter, 1.0R, CDbl(font.GetHeight()) - 1.0R)
            End Using

            Assert.Throws(Of ArgumentNullException)(Sub() ToggleSwitch.GetCapCenterFromLineTop(Nothing))
        End Sub

        ''' <summary>
        ''' The tool strip host forwards the checked state and its change event.
        ''' </summary>
        <Fact>
        Public Sub ToolStripToggleSwitch_ForwardsCheckedState()
            Using item As New ToolStripToggleSwitch()
                Dim raisedCount As Integer = 0
                AddHandler item.CheckedChanged, Sub(sender As Object, e As EventArgs) raisedCount += 1

                item.Checked = True
                Assert.True(item.ToggleSwitch.Checked)
                Assert.Equal(1, raisedCount)

                item.ToggleSwitch.Checked = False
                Assert.False(item.Checked)
                Assert.Equal(2, raisedCount)
            End Using
        End Sub

#End Region

#Region " ThemedProgressBar "

        ''' <summary>
        ''' The progress bar validates its range and clamps the value when the maximum decreases.
        ''' </summary>
        <Fact>
        Public Sub ThemedProgressBar_ValidatesRange()
            Using bar As New ThemedProgressBar()
                bar.Maximum = 10
                bar.Value = 10
                bar.Maximum = 4
                Assert.Equal(4, bar.Value)

                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() bar.Value = 5)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() bar.Value = -1)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() bar.Maximum = -1)
            End Using
        End Sub

        ''' <summary>
        ''' The progress bar state accepts only defined values.
        ''' </summary>
        <Fact>
        Public Sub ThemedProgressBar_State_RejectsUndefinedValues()
            Using bar As New ThemedProgressBar()
                bar.State = ProgressBarState.Error
                Assert.Equal(ProgressBarState.Error, bar.State)
                Assert.Throws(Of InvalidEnumArgumentException)(Sub() bar.State = CType(9, ProgressBarState))
            End Using
        End Sub

#End Region

#Region " ThemedButton "

        ''' <summary>
        ''' The owner-drawn button only supports its own style values and the flat style.
        ''' </summary>
        <Fact>
        Public Sub ThemedButton_RejectsUnsupportedStyles()
            Using button As New ThemedButton()
                button.ButtonStyle = ThemedButtonStyle.Danger
                Assert.Equal(ThemedButtonStyle.Danger, button.ButtonStyle)
                Assert.Throws(Of InvalidEnumArgumentException)(Sub() button.ButtonStyle = CType(9, ThemedButtonStyle))
                Assert.Throws(Of NotSupportedException)(Sub() button.FlatStyle = FlatStyle.Standard)
            End Using
        End Sub

#End Region

#Region " IThemeable "

        ''' <summary>
        ''' Every themeable control rejects a null palette.
        ''' </summary>
        <Fact>
        Public Sub Themeables_RejectNullPalette()
            Using toggle As New ToggleSwitch(),
                  button As New ThemedButton(),
                  label As New ThemedLabel(),
                  bar As New ThemedProgressBar(),
                  textBox As New ThemedTextBox()

                For Each themeable As IThemeable In New IThemeable() {toggle, button, label, bar, textBox}
                    Assert.Throws(Of ArgumentNullException)(Sub() themeable.ApplyTheme(Nothing))
                Next
            End Using
        End Sub

#End Region

#Region " ThemedTextBox "

        ''' <summary>
        ''' The text box forwards its text and raises <see cref="Control.TextChanged"/>.
        ''' </summary>
        <Fact>
        Public Sub ThemedTextBox_ForwardsTextAndTextChanged()
            Using textBox As New ThemedTextBox()
                Dim raisedCount As Integer = 0
                AddHandler textBox.TextChanged, Sub(sender As Object, e As EventArgs) raisedCount += 1

                textBox.Text = "C:\Images"
                Assert.Equal("C:\Images", textBox.Text)
                Assert.Equal(1, raisedCount)
            End Using
        End Sub

        ''' <summary>
        ''' A single-line text box has exactly the common input height, whatever height is requested.
        ''' </summary>
        <Fact>
        Public Sub ThemedTextBox_SingleLine_HasTheCommonInputHeight()
            Using textBox As New ThemedTextBox()
                textBox.Height = 300
                Assert.Equal(ControlMetrics.GetInputHeight(textBox.Font), textBox.Height)
            End Using
        End Sub

        ''' <summary>
        ''' Regression: a freshly created text box already has the common input height
        ''' (the base constructor sets the size from DefaultSize without going through SetBoundsCore).
        ''' </summary>
        <Fact>
        Public Sub ThemedTextBox_NewInstance_HasTheCommonInputHeight()
            Using textBox As New ThemedTextBox()
                Assert.Equal(ControlMetrics.GetInputHeight(textBox.Font), textBox.Height)
            End Using
        End Sub

        ''' <summary>
        ''' A multi-line text box keeps the requested height.
        ''' </summary>
        <Fact>
        Public Sub ThemedTextBox_Multiline_KeepsTheRequestedHeight()
            Using textBox As New ThemedTextBox() With {.Multiline = True}
                textBox.Height = 120
                Assert.Equal(120, textBox.Height)
            End Using
        End Sub

#End Region

#Region " ThemedNumericUpDown (Windows only) "

        ''' <summary>
        ''' The numeric box enforces integer values within its range.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub ThemedNumericUpDown_EnforcesRange()
            Using numeric As New ThemedNumericUpDown()
                numeric.Maximum = 50
                numeric.Minimum = 10
                numeric.Value = 25
                Assert.Equal(25, numeric.Value)

                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() numeric.Value = 51)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() numeric.Value = 9)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() numeric.Minimum = 60)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() numeric.Maximum = 5)
            End Using
        End Sub

        ''' <summary>
        ''' <see cref="ThemedNumericUpDown.ValueChanged"/> is raised on every actual change.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub ThemedNumericUpDown_RaisesValueChanged()
            Using numeric As New ThemedNumericUpDown()
                Dim raisedCount As Integer = 0
                AddHandler numeric.ValueChanged, Sub(sender As Object, e As EventArgs) raisedCount += 1

                numeric.Value = 7
                numeric.Value = 7
                Assert.Equal(1, raisedCount)
            End Using
        End Sub

#End Region

#Region " Input height consistency (Windows only) "

        ''' <summary>
        ''' The text box and the numeric box have exactly the same height once created,
        ''' so they line up across the rows of the main window (regression for the misaligned toggle switch).
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub InputControls_HaveTheSameHeight()
            StaThreadRunner.Run(
                Sub()
                    Using form As New Form() With {.Font = New Font("Segoe UI", 9.0F)},
                          textBox As New ThemedTextBox(),
                          numeric As New ThemedNumericUpDown()

                        form.Controls.AddRange(New Control() {textBox, numeric})
                        form.Show()

                        ' Every control must inherit the form font; otherwise its height is computed from another font.
                        Assert.Equal(form.Font.Height, textBox.Font.Height)
                        Assert.Equal(form.Font.Height, numeric.Font.Height)

                        Dim expectedHeight As Integer = ControlMetrics.GetInputHeight(form.Font)
                        Assert.Equal(expectedHeight, textBox.Height)
                        Assert.Equal(expectedHeight, numeric.Height)

                        form.Close()
                    End Using
                End Sub)
        End Sub

#End Region

#Region " LogConsoleBox "

        ''' <summary>
        ''' RTF control characters and non-ASCII characters are escaped correctly.
        ''' </summary>
        ''' <param name="text">The plain text.</param>
        ''' <param name="expectedRtf">The expected escaped RTF.</param>
        <Theory>
        <InlineData("plain text", "plain text")>
        <InlineData("C:\Images\1118", "C:\\Images\\1118")>
        <InlineData("{braces}", "\{braces\}")>
        <InlineData("a" & vbTab & "b", "a\tab b")>
        <InlineData("line1" & vbCrLf & "line2", "line1\line line2")>
        <InlineData("# Already 16" & ChrW(&H2215) & "9", "# Already 16\u8725?9")>
        <InlineData("Espa" & ChrW(241) & "a", "Espa\u241?a")>
        Public Sub LogConsoleBox_EscapesRtfText(text As String, expectedRtf As String)
            Dim builder As New StringBuilder()
            LogConsoleBox.AppendEscapedRtfText(builder, text)
            Assert.Equal(expectedRtf, builder.ToString())
        End Sub

        ''' <summary>
        ''' Characters outside the Basic Multilingual Plane are written as two signed 16-bit \u escapes.
        ''' </summary>
        <Fact>
        Public Sub LogConsoleBox_EscapesSurrogatePairsAsSigned16BitValues()
            Dim builder As New StringBuilder()
            LogConsoleBox.AppendEscapedRtfText(builder, Char.ConvertFromUtf32(&H1F600))
            Assert.Equal("\u-10179?\u-8704?", builder.ToString())
        End Sub

        ''' <summary>
        ''' Null arguments must fail fast.
        ''' </summary>
        <Fact>
        Public Sub LogConsoleBox_EscapeRtf_NullArguments_Throw()
            Assert.Throws(Of ArgumentNullException)(Sub() LogConsoleBox.AppendEscapedRtfText(Nothing, "text"))
            Assert.Throws(Of ArgumentNullException)(Sub() LogConsoleBox.AppendEscapedRtfText(New StringBuilder(), Nothing))
        End Sub

#End Region

#Region " Helpers "

        ''' <summary>
        ''' Rounded rectangles are closed and contained within their bounds; invalid input fails fast.
        ''' </summary>
        <Fact>
        Public Sub DrawingHelper_CreateRoundedRectangle()
            Dim bounds As New RectangleF(0.0F, 0.0F, 40.0F, 20.0F)

            Using path As GraphicsPath = DrawingHelper.CreateRoundedRectangle(bounds, 10.0F)
                Dim pathBounds As RectangleF = path.GetBounds()
                Assert.True(pathBounds.Width <= bounds.Width + 0.01F)
                Assert.True(pathBounds.Height <= bounds.Height + 0.01F)
            End Using

            Assert.Throws(Of ArgumentOutOfRangeException)(Sub() DrawingHelper.CreateRoundedRectangle(RectangleF.Empty, 2.0F))
            Assert.Throws(Of ArgumentOutOfRangeException)(Sub() DrawingHelper.CreateRoundedRectangle(bounds, -1.0F))
        End Sub

        ''' <summary>
        ''' The common input height grows with the font and fails fast on null.
        ''' </summary>
        <Fact>
        Public Sub ControlMetrics_InputHeight_FollowsTheFont()
            Using smallFont As New Font("Arial", 8.0F),
                  largeFont As New Font("Arial", 16.0F)

                Assert.Equal(smallFont.Height + ControlMetrics.InputHeightPadding, ControlMetrics.GetInputHeight(smallFont))
                Assert.True(ControlMetrics.GetInputHeight(largeFont) > ControlMetrics.GetInputHeight(smallFont))
            End Using

            Assert.Throws(Of ArgumentNullException)(Sub() ControlMetrics.GetInputHeight(Nothing))
        End Sub

#End Region

    End Class

End Namespace
