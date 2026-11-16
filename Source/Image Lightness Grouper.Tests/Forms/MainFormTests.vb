Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Windows.Forms

Imports ImageLightnessGrouper.CommandLine
Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Forms
Imports ImageLightnessGrouper.Infrastructure
Imports ImageLightnessGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.Forms

    ''' <summary>
    ''' Tests for <see cref="MainForm"/> and <see cref="AboutBox"/>. They need the real Windows Forms implementation
    ''' (DWM, uxtheme, native NumericUpDown internals), so they only run on .NET Framework on Windows.
    ''' </summary>
    <Collection(SharedStateCollection.Name)>
    Public NotInheritable Class MainFormTests

        ''' <summary>
        ''' Regression: the designer assigns default values to the numeric boxes inside InitializeComponent,
        ''' which raises ValueChanged. That must never overwrite the persisted preferences.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Constructor_DoesNotOverwritePersistedPreferences()
            Using New SettingsSnapshot()
                UserPreferences.StepPercentage = 3
                UserPreferences.AlphaThreshold = 99
                UserPreferences.MaxDegreeOfParallelism = 5

                StaThreadRunner.Run(
                    Sub()
                        Using form As New MainForm()
                        End Using
                    End Sub)

                Assert.Equal(3, UserPreferences.StepPercentage)
                Assert.Equal(99, UserPreferences.AlphaThreshold)
                Assert.Equal(5, UserPreferences.MaxDegreeOfParallelism)
            End Using
        End Sub

        ''' <summary>
        ''' Loading the form restores every persisted preference into its control,
        ''' and closing it keeps (persists) the same values.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Load_RestoresPreferencesIntoControls_AndCloseKeepsThem()
            Using New SettingsSnapshot()
                UserPreferences.StepPercentage = 3
                UserPreferences.AlphaThreshold = 99
                UserPreferences.MaxDegreeOfParallelism = 5
                UserPreferences.SearchRecursively = True

                Dim restoredStep As Integer
                Dim restoredAlpha As Integer
                Dim restoredParallelism As Integer
                Dim restoredRecursive As Boolean

                StaThreadRunner.Run(
                    Sub()
                        Using form As New MainForm()
                            form.Show()

                            restoredStep = form.stepNumericUpDown.Value
                            restoredAlpha = form.alphaThresholdNumericUpDown.Value
                            restoredParallelism = form.parallelismNumericUpDown.Value
                            restoredRecursive = form.recursiveToggle.Checked

                            form.Close()
                        End Using
                    End Sub)

                Assert.Equal(3, restoredStep)
                Assert.Equal(99, restoredAlpha)
                Assert.Equal(5, restoredParallelism)
                Assert.True(restoredRecursive)

                Assert.Equal(3, UserPreferences.StepPercentage)
                Assert.Equal(99, UserPreferences.AlphaThreshold)
                Assert.Equal(5, UserPreferences.MaxDegreeOfParallelism)
                Assert.True(UserPreferences.SearchRecursively)
            End Using
        End Sub

        ''' <summary>
        ''' Changing a numeric box after loading persists the new value immediately.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub ChangingSettings_PersistsTheNewValues()
            Using New SettingsSnapshot()
                StaThreadRunner.Run(
                    Sub()
                        Using form As New MainForm()
                            form.Show()
                            form.stepNumericUpDown.Value = 2
                            form.alphaThresholdNumericUpDown.Value = 128
                            form.parallelismNumericUpDown.Value = 6

                            Assert.Equal(2, UserPreferences.StepPercentage)
                            Assert.Equal(128, UserPreferences.AlphaThreshold)
                            Assert.Equal(6, UserPreferences.MaxDegreeOfParallelism)

                            form.Close()
                        End Using
                    End Sub)
            End Using
        End Sub

        ''' <summary>
        ''' The numeric boxes accept exactly the ranges of the core.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub NumericBoxes_UseTheCoreRanges()
            StaThreadRunner.Run(
                Sub()
                    Using form As New MainForm()
                        form.Show()

                        Assert.Equal(LightnessCalculator.MinimumStepPercentage, form.stepNumericUpDown.Minimum)
                        Assert.Equal(LightnessCalculator.MaximumStepPercentage, form.stepNumericUpDown.Maximum)
                        Assert.Equal(0, form.alphaThresholdNumericUpDown.Minimum)
                        Assert.Equal(255, form.alphaThresholdNumericUpDown.Maximum)
                        Assert.Equal(ParallelismCalculator.Automatic, form.parallelismNumericUpDown.Minimum)
                        Assert.Equal(ParallelismCalculator.MaximumRequestable, form.parallelismNumericUpDown.Maximum)

                        form.Close()
                    End Using
                End Sub)
        End Sub

        ''' <summary>
        ''' Tool tips are split into short lines, so they never span a large part of the screen.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub ToolTips_AreSplitIntoShortLines()
            Const MaximumLineLength As Integer = 60

            StaThreadRunner.Run(
                Sub()
                    Using form As New MainForm()
                        Dim pending As New Stack(Of Windows.Forms.Control)()
                        pending.Push(form)

                        While pending.Count > 0
                            Dim control As Windows.Forms.Control = pending.Pop()
                            Dim toolTipText As String = form.toolTip.GetToolTip(control)

                            For Each line As String In toolTipText.Split({Environment.NewLine}, StringSplitOptions.None)
                                Assert.True(line.Length <= MaximumLineLength, $"The tool tip of '{control.Name}' has a {line.Length}-character line: {line}")
                            Next

                            For Each child As Windows.Forms.Control In control.Controls
                                pending.Push(child)
                            Next
                        End While
                    End Using
                End Sub)
        End Sub

        ''' <summary>
        ''' The target directory is never restored at startup.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Load_DoesNotRestoreTheTargetDirectory()
            Using New SettingsSnapshot()
                Dim directoryText As String = Nothing

                StaThreadRunner.Run(
                    Sub()
                        Using form As New MainForm()
                            form.Show()
                            directoryText = form.directoryTextBox.Text
                            form.Close()
                        End Using
                    End Sub)

                Assert.Equal(String.Empty, directoryText)
            End Using
        End Sub

        ''' <summary>
        ''' A directory specified in the command line (<c>"Image Lightness Grouper.exe" "C:\Images"</c>) is selected on load,
        ''' with the caret at the end of the path (its most relevant part), not at the beginning.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Load_SelectsTheStartupDirectory_WithTheCaretAtTheEnd()
            Using snapshot As New SettingsSnapshot(),
                  temp As New TemporaryDirectory()

                Dim directoryText As String = Nothing
                Dim selectionStart As Integer = -1
                Dim selectionLength As Integer = -1
                StartupArguments.Current = StartupArguments.Parse({temp.FullPath}, Environment.CurrentDirectory)
                Try
                    StaThreadRunner.Run(
                        Sub()
                            Using form As New MainForm()
                                form.Show()
                                Application.DoEvents()
                                directoryText = form.directoryTextBox.Text
                                selectionStart = form.directoryTextBox.SelectionStart
                                selectionLength = form.directoryTextBox.SelectionLength
                                form.Close()
                            End Using
                        End Sub)
                Finally
                    StartupArguments.Current = New StartupArguments(Nothing, Nothing)
                End Try

                Assert.Equal(temp.FullPath, directoryText)
                Assert.Equal(temp.FullPath.Length, selectionStart)
                Assert.Equal(0, selectionLength)
            End Using
        End Sub

        ''' <summary>
        ''' An invalid command-line argument is reported in the console of the main window (never in a message box),
        ''' and the window opens without a directory selected.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Load_InvalidStartupArgument_IsReportedInTheConsole()
            Using snapshot As New SettingsSnapshot(),
                  temp As New TemporaryDirectory()

                Dim directoryText As String = Nothing
                Dim consoleText As String = Nothing
                StartupArguments.Current = StartupArguments.Parse({temp.Combine("missing")}, Environment.CurrentDirectory)
                Try
                    StaThreadRunner.Run(
                        Sub()
                            Using form As New MainForm()
                                form.Show()
                                form.logConsole.Flush()
                                directoryText = form.directoryTextBox.Text
                                consoleText = form.logConsole.Controls.OfType(Of RichTextBox)().Single().Text
                                form.Close()
                            End Using
                        End Sub)
                Finally
                    StartupArguments.Current = New StartupArguments(Nothing, Nothing)
                End Try

                Assert.Equal(String.Empty, directoryText)
                Assert.Contains($"[ERROR] The directory does not exist: {temp.Combine("missing")}", consoleText)
            End Using
        End Sub

        ''' <summary>
        ''' Help &gt; Command-Line Reference shows the header and the same help as the command-line application,
        ''' from the top, with the focus on the Close button.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub CommandLineReference_ShowsTheHelp()
            Dim referenceText As String = Nothing
            Dim selectionStart As Integer = -1
            Dim selectionLength As Integer = -1
            Dim focusedOnClose As Boolean = False

            StaThreadRunner.Run(
                Sub()
                    Using form As New CommandLineReferenceForm()
                        form.Show()
                        Application.DoEvents()
                        referenceText = form.referenceTextBox.Text
                        selectionStart = form.referenceTextBox.SelectionStart
                        selectionLength = form.referenceTextBox.SelectionLength
                        focusedOnClose = form.ActiveControl Is form.closeButton
                        form.Close()
                    End Using
                End Sub)

            Assert.StartsWith($"{My.Application.Info.Title} v{My.Application.Info.Version} (", referenceText, StringComparison.Ordinal)
            Assert.Contains(ProductInfo.RepositoryUrl, referenceText)
            Assert.EndsWith(CommandLineHelp.GetUsageText(), referenceText, StringComparison.Ordinal)
            Assert.Equal(0, selectionStart)
            Assert.Equal(0, selectionLength)
            Assert.True(focusedOnClose)
        End Sub

        ''' <summary>
        ''' The window title follows the required format, and the about box reads the assembly information.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Titles_FollowTheRequiredFormat()
            StaThreadRunner.Run(
                Sub()
                    Using form As New MainForm()
                        Dim expectedTitle As String = $"{My.Application.Info.Title} v{My.Application.Info.Version} ({UtilApplication.ProcessArchitecture.ToString().ToUpper()})"
                        Assert.Equal(expectedTitle, form.Text)
                    End Using

                    Using about As New AboutBox()
                        Assert.Equal($"About {My.Application.Info.Title}", about.Text)
                        Assert.NotNull(about.logoPictureBox.Image)
                    End Using
                End Sub)
        End Sub

    End Class

End Namespace
