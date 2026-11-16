Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Windows.Forms

Imports ImageLightnessGrouper.Controls
Imports ImageLightnessGrouper.Tests.TestInfrastructure
Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Tests.Theming

    ''' <summary>
    ''' Tests for <see cref="ThemeManager"/>. They mutate the process-wide theme, so they never run in parallel.
    ''' </summary>
    <Collection(SharedStateCollection.Name)>
    Public NotInheritable Class ThemeManagerTests

        ''' <summary>
        ''' <see cref="ThemeManager.ThemeChanged"/> is raised only when the theme actually changes.
        ''' </summary>
        <Fact>
        Public Sub SetTheme_RaisesThemeChangedOnlyOnActualChanges()
            Dim originalTheme As AppTheme = ThemeManager.CurrentTheme
            Dim raisedCount As Integer = 0
            Dim handler As EventHandler = Sub(sender As Object, e As EventArgs) raisedCount += 1

            AddHandler ThemeManager.ThemeChanged, handler
            Try
                ThemeManager.SetTheme(AppTheme.Light)
                raisedCount = 0

                ThemeManager.SetTheme(AppTheme.Light)
                Assert.Equal(0, raisedCount)

                ThemeManager.SetTheme(AppTheme.Dark)
                Assert.Equal(1, raisedCount)
                Assert.Equal(AppTheme.Dark, ThemeManager.CurrentTheme)
                Assert.Same(ThemePalette.Dark, ThemeManager.CurrentPalette)
            Finally
                RemoveHandler ThemeManager.ThemeChanged, handler
                ThemeManager.SetTheme(originalTheme)
            End Try
        End Sub

        ''' <summary>
        ''' Invalid themes and null roots must fail fast.
        ''' </summary>
        <Fact>
        Public Sub InvalidArguments_Throw()
            Assert.Throws(Of InvalidEnumArgumentException)(Sub() ThemeManager.SetTheme(CType(5, AppTheme)))
            Assert.Throws(Of ArgumentNullException)(Sub() ThemeManager.ApplyTheme(Nothing))
            Assert.Throws(Of ArgumentNullException)(Sub() ThemeManager.ApplyThemeToToolStrip(Nothing, ThemePalette.Dark))

            Using toolStrip As New ToolStrip()
                Assert.Throws(Of ArgumentNullException)(Sub() ThemeManager.ApplyThemeToToolStrip(toolStrip, Nothing))
            End Using
        End Sub

        ''' <summary>
        ''' The whole control tree (including nested containers and themeable controls) receives the palette.
        ''' </summary>
        <Fact>
        Public Sub ApplyTheme_ThemesTheWholeTree()
            Dim originalTheme As AppTheme = ThemeManager.CurrentTheme
            Try
                ThemeManager.SetTheme(AppTheme.Dark)

                Using root As New Panel(),
                      nested As New Panel(),
                      plainLabel As New Label(),
                      mutedLabel As New ThemedLabel() With {.IsMuted = True},
                      progress As New ThemedProgressBar()

                    nested.Controls.Add(plainLabel)
                    nested.Controls.Add(mutedLabel)
                    root.Controls.Add(nested)
                    root.Controls.Add(progress)

                    ThemeManager.ApplyTheme(root)

                    Assert.Equal(ThemePalette.Dark.WindowBackground, root.BackColor)
                    Assert.Equal(ThemePalette.Dark.WindowBackground, nested.BackColor)
                    Assert.Equal(ThemePalette.Dark.Foreground, plainLabel.ForeColor)
                    Assert.Equal(ThemePalette.Dark.MutedForeground, mutedLabel.ForeColor)
                    Assert.Equal(ThemePalette.Dark.WindowBackground, progress.BackColor)
                End Using
            Finally
                ThemeManager.SetTheme(originalTheme)
            End Try
        End Sub

        ''' <summary>
        ''' A toggle hosted in a menu bar must blend with the menu background, not with the window background
        ''' (regression: the generic tree walk used to re-theme the hosted control after its tool strip).
        ''' </summary>
        <Fact>
        Public Sub ApplyTheme_HostedToggleUsesMenuBackground()
            Dim originalTheme As AppTheme = ThemeManager.CurrentTheme
            Try
                ThemeManager.SetTheme(AppTheme.Dark)

                Using form As New Form(),
                      menu As New MenuStrip()

                    Dim toggleItem As New ToolStripToggleSwitch()
                    menu.Items.Add(toggleItem)
                    form.Controls.Add(menu)

                    ThemeManager.ApplyTheme(form)

                    Assert.Equal(ThemePalette.Dark.MenuBackground, toggleItem.ToggleSwitch.BackColor)
                    Assert.IsType(Of ThemedToolStripRenderer)(menu.Renderer)
                End Using
            Finally
                ThemeManager.SetTheme(originalTheme)
            End Try
        End Sub

    End Class

End Namespace
