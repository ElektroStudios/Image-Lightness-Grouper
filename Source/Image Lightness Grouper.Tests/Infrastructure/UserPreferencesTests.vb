Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Infrastructure
Imports ImageLightnessGrouper.Tests.TestInfrastructure
Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Tests.Infrastructure

    ''' <summary>
    ''' Tests for <see cref="UserPreferences"/> (and therefore for the application settings it wraps).
    ''' </summary>
    <Collection(SharedStateCollection.Name)>
    Public NotInheritable Class UserPreferencesTests

        ''' <summary>
        ''' The default values must match the defaults of the console version and of the UI.
        ''' </summary>
        <Fact>
        Public Sub Defaults_MatchTheDesignDefaults()
            Dim settingsProperties As Configuration.SettingsPropertyCollection = My.Settings.Properties

            Assert.Equal(CStr(LightnessCalculator.DefaultStepPercentage), CStr(settingsProperties("StepPercentage").DefaultValue))
            Assert.Equal(CStr(LightnessCalculator.DefaultAlphaThreshold), CStr(settingsProperties("AlphaThreshold").DefaultValue))
            Assert.Equal(CStr(ParallelismCalculator.Automatic), CStr(settingsProperties("MaxDegreeOfParallelism").DefaultValue))
            Assert.Equal("False", CStr(settingsProperties("SearchRecursively").DefaultValue))
            Assert.Equal("True", CStr(settingsProperties("UpgradeRequired").DefaultValue))
        End Sub

        ''' <summary>
        ''' The target directory is intentionally never persisted: no setting may store a directory or path.
        ''' </summary>
        <Fact>
        Public Sub Settings_NeverPersistTheTargetDirectory()
            For Each settingProperty As Configuration.SettingsProperty In My.Settings.Properties
                Assert.DoesNotContain("Directory", settingProperty.Name, StringComparison.OrdinalIgnoreCase)
                Assert.DoesNotContain("Path", settingProperty.Name, StringComparison.OrdinalIgnoreCase)
            Next
        End Sub

        ''' <summary>
        ''' Every preference round-trips through the settings.
        ''' </summary>
        <Fact>
        Public Sub Preferences_RoundTrip()
            Using New SettingsSnapshot()
                UserPreferences.StepPercentage = 4
                UserPreferences.AlphaThreshold = 200
                UserPreferences.MaxDegreeOfParallelism = 12
                UserPreferences.SearchRecursively = True
                UserPreferences.Theme = AppTheme.Dark

                Assert.Equal(4, UserPreferences.StepPercentage)
                Assert.Equal(200, UserPreferences.AlphaThreshold)
                Assert.Equal(12, UserPreferences.MaxDegreeOfParallelism)
                Assert.True(UserPreferences.SearchRecursively)
                Assert.Equal(AppTheme.Dark, UserPreferences.Theme)

                UserPreferences.Theme = AppTheme.Light
                Assert.Equal(AppTheme.Light, UserPreferences.Theme)
                Assert.True(My.Settings.HasUserThemePreference)
            End Using
        End Sub

        ''' <summary>
        ''' Invalid assignments must fail fast and leave the stored value untouched.
        ''' </summary>
        <Fact>
        Public Sub InvalidAssignments_Throw()
            Using New SettingsSnapshot()
                UserPreferences.StepPercentage = 5

                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.StepPercentage = 0)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.StepPercentage = 11)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.AlphaThreshold = -1)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.AlphaThreshold = 256)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.MaxDegreeOfParallelism = -1)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.MaxDegreeOfParallelism = ParallelismCalculator.MaximumRequestable + 1)
                Assert.Throws(Of InvalidEnumArgumentException)(Sub() UserPreferences.Theme = CType(42, AppTheme))

                Assert.Equal(5, UserPreferences.StepPercentage)
            End Using
        End Sub

        ''' <summary>
        ''' Corrupted persisted values must fail fast instead of being silently accepted.
        ''' </summary>
        <Fact>
        Public Sub CorruptedPersistedValues_Throw()
            Using New SettingsSnapshot()
                My.Settings.StepPercentage = 50
                Assert.Throws(Of InvalidOperationException)(Function() UserPreferences.StepPercentage)

                My.Settings.AlphaThreshold = 300
                Assert.Throws(Of InvalidOperationException)(Function() UserPreferences.AlphaThreshold)

                My.Settings.MaxDegreeOfParallelism = -3
                Assert.Throws(Of InvalidOperationException)(Function() UserPreferences.MaxDegreeOfParallelism)
            End Using
        End Sub

        ''' <summary>
        ''' Until the user explicitly picks a theme, the stored dark flag is ignored (the Windows theme is followed).
        ''' </summary>
        <Fact>
        Public Sub Theme_WithoutUserPreference_IgnoresTheStoredFlag()
            Using New SettingsSnapshot()
                My.Settings.HasUserThemePreference = False

                My.Settings.IsDarkTheme = True
                Dim themeWhenFlagIsDark As AppTheme = UserPreferences.Theme

                My.Settings.IsDarkTheme = False
                Dim themeWhenFlagIsLight As AppTheme = UserPreferences.Theme

                Assert.Equal(themeWhenFlagIsDark, themeWhenFlagIsLight)
            End Using
        End Sub

    End Class

End Namespace
