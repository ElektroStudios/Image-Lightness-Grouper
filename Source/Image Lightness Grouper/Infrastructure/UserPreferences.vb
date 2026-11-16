Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Infrastructure

    ''' <summary>
    ''' Strongly-typed, validated facade over <c>My.Settings</c> that persists the user's preferences across restarts.
    ''' <para></para>
    ''' The target directory is deliberately not persisted: every session starts without a directory selected.
    ''' </summary>
    Friend NotInheritable Class UserPreferences

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UserPreferences"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the preferred theme.
        ''' <para></para>
        ''' Until the user explicitly picks a theme, the current Windows apps theme is used.
        ''' </summary>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when the assigned value is not a defined <see cref="AppTheme"/>.</exception>
        Friend Shared Property Theme As AppTheme
            Get
                If Not My.Settings.HasUserThemePreference Then
                    Return If(WindowChrome.IsWindowsDarkModeEnabled(), AppTheme.Dark, AppTheme.Light)
                End If

                Return If(My.Settings.IsDarkTheme, AppTheme.Dark, AppTheme.Light)
            End Get
            Set(value As AppTheme)
                If Not [Enum].IsDefined(GetType(AppTheme), value) Then
                    Throw New InvalidEnumArgumentException(NameOf(value), CInt(value), GetType(AppTheme))
                End If

                My.Settings.IsDarkTheme = (value = AppTheme.Dark)
                My.Settings.HasUserThemePreference = True
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the width of each lightness range, in percent points.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the persisted value is out of range (corrupted <c>user.config</c>).</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is out of range.</exception>
        Friend Shared Property StepPercentage As Integer
            Get
                Dim value As Integer = My.Settings.StepPercentage
                UserPreferences.EnsurePersistedValueIsValid(Sub() LightnessCalculator.ValidateStepPercentage(value), NameOf(StepPercentage), value)
                Return value
            End Get
            Set(value As Integer)
                LightnessCalculator.ValidateStepPercentage(value)
                My.Settings.StepPercentage = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the alpha threshold (0-255): pixels whose alpha is below this value are ignored.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the persisted value is out of range (corrupted <c>user.config</c>).</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is out of range.</exception>
        Friend Shared Property AlphaThreshold As Integer
            Get
                Dim value As Integer = My.Settings.AlphaThreshold
                UserPreferences.EnsurePersistedValueIsValid(Sub() LightnessCalculator.ValidateAlphaThreshold(value), NameOf(AlphaThreshold), value)
                Return value
            End Get
            Set(value As Integer)
                LightnessCalculator.ValidateAlphaThreshold(value)
                My.Settings.AlphaThreshold = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the requested maximum number of images processed in parallel (0 = automatic).
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the persisted value is out of range (corrupted <c>user.config</c>).</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is out of range.</exception>
        Friend Shared Property MaxDegreeOfParallelism As Integer
            Get
                Dim value As Integer = My.Settings.MaxDegreeOfParallelism
                UserPreferences.EnsurePersistedValueIsValid(Sub() ParallelismCalculator.ValidateRequested(value), NameOf(MaxDegreeOfParallelism), value)
                Return value
            End Get
            Set(value As Integer)
                ParallelismCalculator.ValidateRequested(value)
                My.Settings.MaxDegreeOfParallelism = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the images inside the subdirectories are processed too.
        ''' </summary>
        Friend Shared Property SearchRecursively As Boolean
            Get
                Return My.Settings.SearchRecursively
            End Get
            Set(value As Boolean)
                My.Settings.SearchRecursively = value
            End Set
        End Property

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Migrates the settings of a previous application version (if any) to the current version.
        ''' <para></para>
        ''' <c>user.config</c> is stored per assembly version, so without this step every version bump would reset the preferences.
        ''' </summary>
        Friend Shared Sub UpgradeIfRequired()
            If Not My.Settings.UpgradeRequired Then
                Return
            End If

            My.Settings.Upgrade()
            My.Settings.UpgradeRequired = False
            My.Settings.Save()
        End Sub

        ''' <summary>
        ''' Persists the current preferences to disk.
        ''' </summary>
        Friend Shared Sub Save()
            My.Settings.Save()
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Converts the range validation failure of a persisted value into an explicit corruption error.
        ''' </summary>
        ''' <param name="validate">The validation to run.</param>
        ''' <param name="settingName">The setting name, for the error message.</param>
        ''' <param name="value">The persisted value, for the error message.</param>
        ''' <exception cref="InvalidOperationException">Thrown when the persisted value is out of range.</exception>
        Private Shared Sub EnsurePersistedValueIsValid(validate As Action, settingName As String, value As Integer)
            Try
                validate()
            Catch ex As ArgumentOutOfRangeException
                Throw New InvalidOperationException($"The persisted {settingName} value ({value}) is out of range. The user.config file may be corrupted.", ex)
            End Try
        End Sub

#End Region

    End Class

End Namespace
