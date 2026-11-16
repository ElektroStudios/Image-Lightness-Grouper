Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel

#End Region

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' An immutable line of log text and its severity.
    ''' </summary>
    Public Structure LogLine

#Region " Properties "

        ''' <summary>
        ''' Gets the text.
        ''' </summary>
        Public ReadOnly Property Text As String

        ''' <summary>
        ''' Gets the severity.
        ''' </summary>
        Public ReadOnly Property Severity As LogSeverity

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="LogLine"/> structure.
        ''' </summary>
        ''' <param name="text">The text.</param>
        ''' <param name="severity">The severity.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="severity"/> is not a defined value.</exception>
        Public Sub New(text As String, severity As LogSeverity)
            If text Is Nothing Then
                Throw New ArgumentNullException(NameOf(text))
            End If

            If Not [Enum].IsDefined(GetType(LogSeverity), severity) Then
                Throw New InvalidEnumArgumentException(NameOf(severity), CInt(severity), GetType(LogSeverity))
            End If

            Me.Text = text
            Me.Severity = severity
        End Sub

#End Region

    End Structure

End Namespace
