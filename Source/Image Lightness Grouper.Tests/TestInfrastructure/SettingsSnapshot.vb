Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' Captures every application setting on creation and restores them all when disposed,
    ''' so that tests which touch <c>My.Settings</c> never leak state into other tests.
    ''' </summary>
    Public NotInheritable Class SettingsSnapshot : Implements IDisposable

#Region " Read-only Fields "

        ''' <summary>
        ''' The captured values, by setting name.
        ''' </summary>
        Private ReadOnly capturedValues As New Dictionary(Of String, Object)(StringComparer.Ordinal)

#End Region

#Region " Fields "

        ''' <summary>
        ''' Whether this instance has been disposed.
        ''' </summary>
        Private isDisposed As Boolean

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SettingsSnapshot"/> class, capturing the current settings.
        ''' </summary>
        Public Sub New()
            For Each propertyName As String In SettingsSnapshot.GetSettingNames()
                Me.capturedValues.Add(propertyName, My.Settings(propertyName))
            Next
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Restores every captured setting value.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If Me.isDisposed Then
                Return
            End If

            Me.isDisposed = True
            For Each pair As KeyValuePair(Of String, Object) In Me.capturedValues
                My.Settings(pair.Key) = pair.Value
            Next
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Gets the names of all the application settings.
        ''' </summary>
        ''' <returns>The setting names.</returns>
        Private Shared Function GetSettingNames() As List(Of String)
            Dim names As New List(Of String)()
            For Each settingProperty As Configuration.SettingsProperty In My.Settings.Properties
                names.Add(settingProperty.Name)
            Next

            Return names
        End Function

#End Region

    End Class

End Namespace
