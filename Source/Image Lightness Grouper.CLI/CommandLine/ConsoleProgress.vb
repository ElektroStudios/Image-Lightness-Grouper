Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' A progress sink that writes every report immediately to an <see cref="IConsoleWriter"/>.
    ''' <para></para>
    ''' Unlike <see cref="Progress(Of T)"/>, it does not post the reports to the synchronization context of the thread that
    ''' created it: the command-line mode blocks that thread while waiting for the operation, so posted reports would
    ''' never be written (or would all be written at the end).
    ''' </summary>
    Public NotInheritable Class ConsoleProgress : Implements IProgress(Of ImageGroupingProgress)

#Region " Fields "

        ''' <summary>
        ''' The output.
        ''' </summary>
        Private ReadOnly writer As IConsoleWriter

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ConsoleProgress"/> class.
        ''' </summary>
        ''' <param name="writer">The output (it must be thread-safe).</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="Nothing"/>.</exception>
        Public Sub New(writer As IConsoleWriter)
            If writer Is Nothing Then
                Throw New ArgumentNullException(NameOf(writer))
            End If

            Me.writer = writer
        End Sub

#End Region

#Region " IProgress "

        ''' <inheritdoc/>
        Public Sub Report(value As ImageGroupingProgress) Implements IProgress(Of ImageGroupingProgress).Report
            If value Is Nothing Then
                Throw New ArgumentNullException(NameOf(value))
            End If

            Me.writer.WriteLine(value.Message, value.Severity)
        End Sub

#End Region

    End Class

End Namespace
