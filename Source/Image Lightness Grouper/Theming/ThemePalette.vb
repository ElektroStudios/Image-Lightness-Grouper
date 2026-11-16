Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.Theming

    ''' <summary>
    ''' Immutable set of colors that defines a visual theme.
    ''' </summary>
    Public NotInheritable Class ThemePalette

#Region " Read-only Fields "

        ''' <summary>
        ''' The singleton light palette.
        ''' </summary>
        Public Shared ReadOnly Light As New ThemePalette(AppTheme.Light)

        ''' <summary>
        ''' The singleton dark palette.
        ''' </summary>
        Public Shared ReadOnly Dark As New ThemePalette(AppTheme.Dark)

#End Region

#Region " Properties "

        ''' <summary>Gets the theme this palette represents.</summary>
        Public ReadOnly Property Theme As AppTheme

        ''' <summary>Gets a value indicating whether this is a dark palette.</summary>
        Public ReadOnly Property IsDark As Boolean
            Get
                Return Me.Theme = AppTheme.Dark
            End Get
        End Property

        ''' <summary>Gets the main window background color.</summary>
        Public ReadOnly Property WindowBackground As Color

        ''' <summary>Gets the background color of input surfaces (text boxes, numeric boxes).</summary>
        Public ReadOnly Property SurfaceBackground As Color

        ''' <summary>Gets the primary text color.</summary>
        Public ReadOnly Property Foreground As Color

        ''' <summary>Gets the secondary (muted) text color.</summary>
        Public ReadOnly Property MutedForeground As Color

        ''' <summary>Gets the text color of disabled elements.</summary>
        Public ReadOnly Property DisabledForeground As Color

        ''' <summary>Gets the background color of disabled elements.</summary>
        Public ReadOnly Property DisabledBackground As Color

        ''' <summary>Gets the default border color.</summary>
        Public ReadOnly Property Border As Color

        ''' <summary>Gets the border color of hovered elements.</summary>
        Public ReadOnly Property BorderHover As Color

        ''' <summary>Gets the accent color (focused borders, primary buttons, toggles, progress).</summary>
        Public ReadOnly Property Accent As Color

        ''' <summary>Gets the hovered accent color.</summary>
        Public ReadOnly Property AccentHover As Color

        ''' <summary>Gets the pressed accent color.</summary>
        Public ReadOnly Property AccentPressed As Color

        ''' <summary>Gets the text color drawn on top of accent surfaces.</summary>
        Public ReadOnly Property AccentForeground As Color

        ''' <summary>Gets the background color of destructive (danger) buttons.</summary>
        Public ReadOnly Property Danger As Color

        ''' <summary>Gets the hovered background color of destructive buttons.</summary>
        Public ReadOnly Property DangerHover As Color

        ''' <summary>Gets the pressed background color of destructive buttons.</summary>
        Public ReadOnly Property DangerPressed As Color

        ''' <summary>Gets the standard button background color.</summary>
        Public ReadOnly Property ButtonBackground As Color

        ''' <summary>Gets the hovered standard button background color.</summary>
        Public ReadOnly Property ButtonHover As Color

        ''' <summary>Gets the pressed standard button background color.</summary>
        Public ReadOnly Property ButtonPressed As Color

        ''' <summary>Gets the menu background color.</summary>
        Public ReadOnly Property MenuBackground As Color

        ''' <summary>Gets the highlighted menu item background color.</summary>
        Public ReadOnly Property MenuHighlight As Color

        ''' <summary>Gets the menu border and separator color.</summary>
        Public ReadOnly Property MenuBorder As Color

        ''' <summary>Gets the toggle switch track color when unchecked.</summary>
        Public ReadOnly Property ToggleOffTrack As Color

        ''' <summary>Gets the toggle switch thumb color when unchecked.</summary>
        Public ReadOnly Property ToggleOffThumb As Color

        ''' <summary>Gets the toggle switch thumb color when checked.</summary>
        Public ReadOnly Property ToggleOnThumb As Color

        ''' <summary>Gets the progress bar track color.</summary>
        Public ReadOnly Property ProgressTrack As Color

        ''' <summary>Gets the progress bar fill color when there are warnings.</summary>
        Public ReadOnly Property ProgressWarning As Color

        ''' <summary>Gets the progress bar fill color when there are errors.</summary>
        Public ReadOnly Property ProgressError As Color

        ''' <summary>Gets the virtual console background color.</summary>
        Public ReadOnly Property ConsoleBackground As Color

        ''' <summary>Gets the log color for <see cref="LogSeverity.Information"/>.</summary>
        Public ReadOnly Property LogInformation As Color

        ''' <summary>Gets the log color for <see cref="LogSeverity.Muted"/>.</summary>
        Public ReadOnly Property LogMuted As Color

        ''' <summary>Gets the log color for <see cref="LogSeverity.Moved"/>.</summary>
        Public ReadOnly Property LogMoved As Color

        ''' <summary>Gets the log color for <see cref="LogSeverity.Skipped"/>.</summary>
        Public ReadOnly Property LogSkipped As Color

        ''' <summary>Gets the log color for <see cref="LogSeverity.Success"/>.</summary>
        Public ReadOnly Property LogSuccess As Color

        ''' <summary>Gets the log color for <see cref="LogSeverity.Warning"/>.</summary>
        Public ReadOnly Property LogWarning As Color

        ''' <summary>Gets the log color for <see cref="LogSeverity.Error"/>.</summary>
        Public ReadOnly Property LogError As Color

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemePalette"/> class for the specified theme.
        ''' </summary>
        ''' <param name="theme">The theme.</param>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="theme"/> is not a defined value.</exception>
        Private Sub New(theme As AppTheme)

            Me.Theme = theme

            Select Case theme

                Case AppTheme.Dark
                    Me.WindowBackground = Color.FromArgb(32, 32, 32)
                    Me.SurfaceBackground = Color.FromArgb(43, 43, 43)
                    Me.Foreground = Color.FromArgb(240, 240, 240)
                    Me.MutedForeground = Color.FromArgb(160, 160, 160)
                    Me.DisabledForeground = Color.FromArgb(110, 110, 110)
                    Me.DisabledBackground = Color.FromArgb(38, 38, 38)
                    Me.Border = Color.FromArgb(70, 70, 70)
                    Me.BorderHover = Color.FromArgb(110, 110, 110)
                    Me.Accent = Color.FromArgb(16, 110, 220)
                    Me.AccentHover = Color.FromArgb(24, 116, 214)
                    Me.AccentPressed = Color.FromArgb(0, 90, 170)
                    Me.AccentForeground = Color.White
                    Me.Danger = Color.FromArgb(164, 38, 44)
                    Me.DangerHover = Color.FromArgb(192, 50, 57)
                    Me.DangerPressed = Color.FromArgb(135, 30, 36)
                    Me.ButtonBackground = Color.FromArgb(51, 51, 51)
                    Me.ButtonHover = Color.FromArgb(64, 64, 64)
                    Me.ButtonPressed = Color.FromArgb(42, 42, 42)
                    Me.MenuBackground = Color.FromArgb(43, 43, 43)
                    Me.MenuHighlight = Color.FromArgb(65, 65, 65)
                    Me.MenuBorder = Color.FromArgb(70, 70, 70)
                    Me.ToggleOffTrack = Color.FromArgb(43, 43, 43)
                    Me.ToggleOffThumb = Color.FromArgb(205, 205, 205)
                    Me.ToggleOnThumb = Color.FromArgb(20, 20, 20)
                    Me.ProgressTrack = Color.FromArgb(51, 51, 51)
                    Me.ProgressWarning = Color.FromArgb(252, 196, 25)
                    Me.ProgressError = Color.FromArgb(232, 72, 85)
                    Me.ConsoleBackground = Color.FromArgb(24, 24, 24)
                    Me.LogInformation = Color.FromArgb(212, 212, 212)
                    Me.LogMuted = Color.FromArgb(136, 136, 136)
                    Me.LogMoved = Color.FromArgb(86, 196, 214)
                    Me.LogSkipped = Color.FromArgb(170, 160, 235)
                    Me.LogSuccess = Color.FromArgb(108, 207, 133)
                    Me.LogWarning = Color.FromArgb(230, 192, 110)
                    Me.LogError = Color.FromArgb(244, 92, 92)

                Case AppTheme.Light
                    Me.WindowBackground = Color.FromArgb(243, 243, 243)
                    Me.SurfaceBackground = Color.White
                    Me.Foreground = Color.FromArgb(27, 27, 27)
                    Me.MutedForeground = Color.FromArgb(96, 96, 96)
                    Me.DisabledForeground = Color.FromArgb(160, 160, 160)
                    Me.DisabledBackground = Color.FromArgb(245, 245, 245)
                    Me.Border = Color.FromArgb(190, 190, 190)
                    Me.BorderHover = Color.FromArgb(130, 130, 130)
                    Me.Accent = Color.FromArgb(0, 95, 184)
                    Me.AccentHover = Color.FromArgb(25, 112, 196)
                    Me.AccentPressed = Color.FromArgb(0, 76, 150)
                    Me.AccentForeground = Color.White
                    Me.Danger = Color.FromArgb(196, 43, 28)
                    Me.DangerHover = Color.FromArgb(214, 62, 47)
                    Me.DangerPressed = Color.FromArgb(160, 32, 20)
                    Me.ButtonBackground = Color.FromArgb(253, 253, 253)
                    Me.ButtonHover = Color.FromArgb(234, 234, 234)
                    Me.ButtonPressed = Color.FromArgb(218, 218, 218)
                    Me.MenuBackground = Color.FromArgb(249, 249, 249)
                    Me.MenuHighlight = Color.FromArgb(219, 233, 247)
                    Me.MenuBorder = Color.FromArgb(204, 204, 204)
                    Me.ToggleOffTrack = Color.White
                    Me.ToggleOffThumb = Color.FromArgb(90, 90, 90)
                    Me.ToggleOnThumb = Color.White
                    Me.ProgressTrack = Color.FromArgb(225, 225, 225)
                    Me.ProgressWarning = Color.FromArgb(190, 120, 0)
                    Me.ProgressError = Color.FromArgb(196, 43, 28)
                    Me.ConsoleBackground = Color.White
                    Me.LogInformation = Color.FromArgb(30, 30, 30)
                    Me.LogMuted = Color.FromArgb(112, 112, 112)
                    Me.LogMoved = Color.FromArgb(0, 118, 150)
                    Me.LogSkipped = Color.FromArgb(108, 80, 170)
                    Me.LogSuccess = Color.FromArgb(16, 124, 16)
                    Me.LogWarning = Color.FromArgb(157, 93, 0)
                    Me.LogError = Color.FromArgb(197, 15, 31)

                Case Else
                    Throw New InvalidEnumArgumentException(NameOf(theme), CInt(theme), GetType(AppTheme))

            End Select
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Gets the singleton palette for the specified theme.
        ''' </summary>
        ''' <param name="theme">The theme.</param>
        ''' <returns>The palette.</returns>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="theme"/> is not a defined value.</exception>
        Public Shared Function FromTheme(theme As AppTheme) As ThemePalette
            Select Case theme
                Case AppTheme.Light
                    Return ThemePalette.Light
                Case AppTheme.Dark
                    Return ThemePalette.Dark
                Case Else
                    Throw New InvalidEnumArgumentException(NameOf(theme), CInt(theme), GetType(AppTheme))
            End Select
        End Function

        ''' <summary>
        ''' Gets the log text color associated with the specified severity.
        ''' </summary>
        ''' <param name="severity">The log severity.</param>
        ''' <returns>The text color.</returns>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="severity"/> is not a defined value.</exception>
        Public Function GetLogColor(severity As LogSeverity) As Color
            Select Case severity
                Case LogSeverity.Information
                    Return Me.LogInformation
                Case LogSeverity.Muted
                    Return Me.LogMuted
                Case LogSeverity.Moved
                    Return Me.LogMoved
                Case LogSeverity.Skipped
                    Return Me.LogSkipped
                Case LogSeverity.Success
                    Return Me.LogSuccess
                Case LogSeverity.Warning
                    Return Me.LogWarning
                Case LogSeverity.Error
                    Return Me.LogError
                Case Else
                    Throw New InvalidEnumArgumentException(NameOf(severity), CInt(severity), GetType(LogSeverity))
            End Select
        End Function

#End Region

    End Class

End Namespace
