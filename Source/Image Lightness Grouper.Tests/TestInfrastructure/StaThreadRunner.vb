Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Runtime.ExceptionServices
Imports System.Threading

#End Region

Namespace ImageLightnessGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' Runs test code on a dedicated single-threaded apartment (STA) thread.
    ''' <para></para>
    ''' Windows Forms requires STA for OLE features (e.g. <c>AllowDrop</c>, clipboard), but xUnit runs tests on MTA threads.
    ''' </summary>
    Public NotInheritable Class StaThreadRunner

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="StaThreadRunner"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Runs the specified action on a new STA thread and waits for it to finish.
        ''' Any exception thrown by the action is rethrown on the calling thread, preserving its stack trace.
        ''' </summary>
        ''' <param name="action">The action to run.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <see langword="Nothing"/>.</exception>
        Public Shared Sub Run(action As Action)
            If action Is Nothing Then
                Throw New ArgumentNullException(NameOf(action))
            End If

            Dim capturedException As ExceptionDispatchInfo = Nothing

            Dim thread As New Thread(
                Sub()
                    Try
                        action()
                    Catch ex As Exception
                        capturedException = ExceptionDispatchInfo.Capture(ex)
                    End Try
                End Sub)

            thread.SetApartmentState(ApartmentState.STA)
            thread.IsBackground = True
            thread.Start()
            thread.Join()

            If capturedException IsNot Nothing Then
                capturedException.Throw()
            End If
        End Sub

#End Region

    End Class

End Namespace
