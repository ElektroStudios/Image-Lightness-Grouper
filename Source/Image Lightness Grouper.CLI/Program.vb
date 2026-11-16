Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Reflection
Imports System.Threading

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Infrastructure

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' The entry point of the command-line application: binds <see cref="CommandLineApplication"/> to the real console.
    ''' </summary>
    Friend NotInheritable Class Program

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="Program"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' The entry point.
        ''' </summary>
        ''' <param name="args">The command-line arguments.</param>
        ''' <returns>The process exit code (see <see cref="CommandLineExitCode"/>).</returns>
        Friend Shared Function Main(args As String()) As Integer
            ' Colors only make sense in a real console, not in a file or a pipe.
            Dim writer As New TextConsoleWriter(Console.Out, Console.Error, useColors:=Not Console.IsOutputRedirected)

            Using cancellationSource As New CancellationTokenSource()
                Dim cancelHandler As ConsoleCancelEventHandler =
                    Sub(sender As Object, e As ConsoleCancelEventArgs)
                        ' The first Ctrl+C stops the operation cleanly (and reports its exit code); a second one ends the process.
                        e.Cancel = Not cancellationSource.IsCancellationRequested
                        cancellationSource.Cancel()
                    End Sub

                AddHandler Console.CancelKeyPress, cancelHandler
                Try
                    Return CInt(CommandLineApplication.Run(args, writer, Program.GetTitle(), New ImageGroupingService(),
                                                           Environment.CurrentDirectory, cancellationSource.Token))
                Finally
                    RemoveHandler Console.CancelKeyPress, cancelHandler
                End Try
            End Using
        End Function

        ''' <summary>
        ''' Gets the application title, version and process architecture (e.g. <c>"Image Lightness Grouper CLI v1.1.0.0 (X64)"</c>).
        ''' </summary>
        ''' <returns>The title.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when the assembly title is missing.</exception>
        Friend Shared Function GetTitle() As String
            Dim assembly As Assembly = GetType(Program).Assembly
            Dim titleAttribute As AssemblyTitleAttribute = assembly.GetCustomAttribute(Of AssemblyTitleAttribute)()
            If titleAttribute Is Nothing OrElse String.IsNullOrWhiteSpace(titleAttribute.Title) Then
                Throw New InvalidOperationException("The AssemblyTitle attribute is missing or empty.")
            End If

            Return $"{titleAttribute.Title} v{assembly.GetName().Version} ({UtilApplication.ProcessArchitecture.ToString().ToUpperInvariant()})"
        End Function

#End Region

    End Class

End Namespace
