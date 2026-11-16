Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Tests.Theming

    ''' <summary>
    ''' Tests for <see cref="ThemePalette"/>, including WCAG 2.1 contrast checks
    ''' (4.5:1 for text, 3:1 for non-text UI components such as borders and toggles).
    ''' </summary>
    Public NotInheritable Class ThemePaletteTests

#Region " Constants "

        ''' <summary>
        ''' WCAG 2.1 AA minimum contrast for normal text.
        ''' </summary>
        Private Const MinimumTextContrast As Double = 4.5R

        ''' <summary>
        ''' WCAG 2.1 AA minimum contrast for user interface components (1.4.11).
        ''' </summary>
        Private Const MinimumComponentContrast As Double = 3.0R

#End Region

#Region " Data "

        ''' <summary>
        ''' Both palettes.
        ''' </summary>
        Public Shared ReadOnly Property Palettes As TheoryData(Of AppTheme)
            Get
                Return New TheoryData(Of AppTheme) From {AppTheme.Light, AppTheme.Dark}
            End Get
        End Property

#End Region

#Region " Helpers "

        ''' <summary>
        ''' Calculates the WCAG 2.1 contrast ratio between two opaque colors.
        ''' </summary>
        ''' <param name="first">The first color.</param>
        ''' <param name="second">The second color.</param>
        ''' <returns>The contrast ratio, between 1 and 21.</returns>
        Private Shared Function GetContrastRatio(first As Color, second As Color) As Double
            Dim firstLuminance As Double = ThemePaletteTests.GetRelativeLuminance(first)
            Dim secondLuminance As Double = ThemePaletteTests.GetRelativeLuminance(second)
            Dim lighter As Double = Math.Max(firstLuminance, secondLuminance)
            Dim darker As Double = Math.Min(firstLuminance, secondLuminance)
            Return (lighter + 0.05R) / (darker + 0.05R)
        End Function

        ''' <summary>
        ''' Calculates the WCAG 2.1 relative luminance of an opaque color.
        ''' </summary>
        ''' <param name="color">The color.</param>
        ''' <returns>The relative luminance, between 0 and 1.</returns>
        Private Shared Function GetRelativeLuminance(color As Color) As Double
            Return (0.2126R * ThemePaletteTests.Linearize(color.R)) +
                   (0.7152R * ThemePaletteTests.Linearize(color.G)) +
                   (0.0722R * ThemePaletteTests.Linearize(color.B))
        End Function

        ''' <summary>
        ''' Converts an sRGB channel to linear light.
        ''' </summary>
        ''' <param name="channel">The 8-bit sRGB channel value.</param>
        ''' <returns>The linear value, between 0 and 1.</returns>
        Private Shared Function Linearize(channel As Byte) As Double
            Dim value As Double = channel / 255.0R
            Return If(value <= 0.03928R, value / 12.92R, Math.Pow((value + 0.055R) / 1.055R, 2.4R))
        End Function

        ''' <summary>
        ''' Asserts a minimum contrast, with a descriptive failure message.
        ''' </summary>
        ''' <param name="theme">The theme being checked.</param>
        ''' <param name="description">What is being checked.</param>
        ''' <param name="foreground">The foreground color.</param>
        ''' <param name="background">The background color.</param>
        ''' <param name="minimum">The minimum contrast ratio.</param>
        Private Shared Sub AssertContrast(theme As AppTheme, description As String, foreground As Color, background As Color, minimum As Double)
            Dim ratio As Double = ThemePaletteTests.GetContrastRatio(foreground, background)
            Assert.True(ratio >= minimum, $"{theme}: {description} has a contrast of {ratio:0.00}:1 (minimum {minimum:0.0}:1).")
        End Sub

#End Region

#Region " Tests "

        ''' <summary>
        ''' <see cref="ThemePalette.FromTheme"/> returns the singleton palette of each theme.
        ''' </summary>
        <Fact>
        Public Sub FromTheme_ReturnsSingletons()
            Assert.Same(ThemePalette.Light, ThemePalette.FromTheme(AppTheme.Light))
            Assert.Same(ThemePalette.Dark, ThemePalette.FromTheme(AppTheme.Dark))
            Assert.False(ThemePalette.Light.IsDark)
            Assert.True(ThemePalette.Dark.IsDark)
            Assert.Throws(Of InvalidEnumArgumentException)(Sub() ThemePalette.FromTheme(CType(7, AppTheme)))
        End Sub

        ''' <summary>
        ''' Every severity has a color, and invalid severities fail fast.
        ''' </summary>
        <Fact>
        Public Sub GetLogColor_CoversEverySeverity()
            For Each severity As LogSeverity In [Enum].GetValues(GetType(LogSeverity))
                Assert.NotEqual(Color.Empty, ThemePalette.Dark.GetLogColor(severity))
                Assert.NotEqual(Color.Empty, ThemePalette.Light.GetLogColor(severity))
            Next

            Assert.Throws(Of InvalidEnumArgumentException)(Sub() ThemePalette.Dark.GetLogColor(CType(99, LogSeverity)))
        End Sub

        ''' <summary>
        ''' Primary and secondary text must be readable on every surface it is drawn on.
        ''' </summary>
        ''' <param name="theme">The theme.</param>
        <Theory>
        <MemberData(NameOf(Palettes))>
        Public Sub Text_HasEnoughContrast(theme As AppTheme)
            Dim palette As ThemePalette = ThemePalette.FromTheme(theme)

            AssertContrast(theme, "Foreground on WindowBackground", palette.Foreground, palette.WindowBackground, MinimumTextContrast)
            AssertContrast(theme, "Foreground on SurfaceBackground", palette.Foreground, palette.SurfaceBackground, MinimumTextContrast)
            AssertContrast(theme, "MutedForeground on WindowBackground", palette.MutedForeground, palette.WindowBackground, MinimumTextContrast)
            AssertContrast(theme, "Foreground on ButtonBackground", palette.Foreground, palette.ButtonBackground, MinimumTextContrast)
            AssertContrast(theme, "Foreground on ButtonHover", palette.Foreground, palette.ButtonHover, MinimumTextContrast)
            AssertContrast(theme, "Foreground on ButtonPressed", palette.Foreground, palette.ButtonPressed, MinimumTextContrast)
            AssertContrast(theme, "Foreground on MenuBackground", palette.Foreground, palette.MenuBackground, MinimumTextContrast)
            AssertContrast(theme, "Foreground on MenuHighlight", palette.Foreground, palette.MenuHighlight, MinimumTextContrast)
        End Sub

        ''' <summary>
        ''' Text on accent (primary) and danger buttons must be readable in every interaction state.
        ''' </summary>
        ''' <param name="theme">The theme.</param>
        <Theory>
        <MemberData(NameOf(Palettes))>
        Public Sub ButtonText_HasEnoughContrast(theme As AppTheme)
            Dim palette As ThemePalette = ThemePalette.FromTheme(theme)

            AssertContrast(theme, "AccentForeground on Accent", palette.AccentForeground, palette.Accent, MinimumTextContrast)
            AssertContrast(theme, "AccentForeground on AccentHover", palette.AccentForeground, palette.AccentHover, MinimumTextContrast)
            AssertContrast(theme, "AccentForeground on AccentPressed", palette.AccentForeground, palette.AccentPressed, MinimumTextContrast)
            AssertContrast(theme, "AccentForeground on Danger", palette.AccentForeground, palette.Danger, MinimumTextContrast)
            AssertContrast(theme, "AccentForeground on DangerHover", palette.AccentForeground, palette.DangerHover, MinimumTextContrast)
            AssertContrast(theme, "AccentForeground on DangerPressed", palette.AccentForeground, palette.DangerPressed, MinimumTextContrast)
        End Sub

        ''' <summary>
        ''' Every log line color must be readable on the console background.
        ''' </summary>
        ''' <param name="theme">The theme.</param>
        <Theory>
        <MemberData(NameOf(Palettes))>
        Public Sub LogColors_HaveEnoughContrast(theme As AppTheme)
            Dim palette As ThemePalette = ThemePalette.FromTheme(theme)

            For Each severity As LogSeverity In [Enum].GetValues(GetType(LogSeverity))
                AssertContrast(theme, $"Log color '{severity}' on ConsoleBackground", palette.GetLogColor(severity), palette.ConsoleBackground, MinimumTextContrast)
            Next
        End Sub

        ''' <summary>
        ''' Non-text UI components (focus borders, toggles, progress fills) must be distinguishable from the window.
        ''' </summary>
        ''' <param name="theme">The theme.</param>
        <Theory>
        <MemberData(NameOf(Palettes))>
        Public Sub Components_HaveEnoughContrast(theme As AppTheme)
            Dim palette As ThemePalette = ThemePalette.FromTheme(theme)

            AssertContrast(theme, "Accent on WindowBackground", palette.Accent, palette.WindowBackground, MinimumComponentContrast)
            AssertContrast(theme, "ToggleOffThumb on WindowBackground", palette.ToggleOffThumb, palette.WindowBackground, MinimumComponentContrast)
            AssertContrast(theme, "ToggleOnThumb on Accent", palette.ToggleOnThumb, palette.Accent, MinimumComponentContrast)
            AssertContrast(theme, "ProgressWarning on WindowBackground", palette.ProgressWarning, palette.WindowBackground, MinimumComponentContrast)
            AssertContrast(theme, "ProgressError on WindowBackground", palette.ProgressError, palette.WindowBackground, MinimumComponentContrast)
        End Sub

#End Region

    End Class

End Namespace
