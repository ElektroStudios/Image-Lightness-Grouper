Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' Describes the environment the tests run in, so that tests which depend on the native
    ''' Windows Forms implementation of .NET Framework (e.g. internal child controls of <c>NumericUpDown</c>,
    ''' <c>uxtheme.dll</c>, <c>dwmapi.dll</c>) are skipped elsewhere (e.g. under Mono) instead of failing.
    ''' </summary>
    Public NotInheritable Class TestEnvironment

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="TestEnvironment"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets a value indicating whether the tests run on the Mono runtime.
        ''' </summary>
        Public Shared ReadOnly Property IsMono As Boolean
            Get
                Return Type.GetType("Mono.Runtime") IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the tests run on .NET Framework on Windows
        ''' (the real target of the application).
        ''' </summary>
        Public Shared ReadOnly Property IsWindowsDesktopFramework As Boolean
            Get
                Return Environment.OSVersion.Platform = PlatformID.Win32NT AndAlso Not TestEnvironment.IsMono
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the tests run on a Unix platform (e.g. Linux under Mono).
        ''' </summary>
        Public Shared ReadOnly Property IsUnix As Boolean
            Get
                Return Environment.OSVersion.Platform = PlatformID.Unix
            End Get
        End Property

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates a directory link that points to <paramref name="targetPath"/>, without requiring elevation:
        ''' a junction (<c>mklink /J</c>) on Windows, or a symbolic link (<c>ln -s</c>) on Unix.
        ''' </summary>
        ''' <param name="linkPath">The path of the link to create.</param>
        ''' <param name="targetPath">The directory the link points to.</param>
        ''' <exception cref="InvalidOperationException">Thrown when the link cannot be created.</exception>
        Public Shared Sub CreateDirectoryLink(linkPath As String, targetPath As String)
            Dim startInfo As Diagnostics.ProcessStartInfo =
                If(TestEnvironment.IsUnix,
                   New Diagnostics.ProcessStartInfo("ln", $"-s ""{targetPath}"" ""{linkPath}"""),
                   New Diagnostics.ProcessStartInfo("cmd.exe", $"/c mklink /J ""{linkPath}"" ""{targetPath}"""))

            startInfo.UseShellExecute = False
            startInfo.CreateNoWindow = True
            startInfo.RedirectStandardOutput = True
            startInfo.RedirectStandardError = True

            Using process As Diagnostics.Process = Diagnostics.Process.Start(startInfo)
                Dim standardOutput As String = process.StandardOutput.ReadToEnd()
                Dim standardError As String = process.StandardError.ReadToEnd()
                process.WaitForExit()

                If process.ExitCode <> 0 OrElse Not IO.Directory.Exists(linkPath) Then
                    Throw New InvalidOperationException($"Could not create the directory link '{linkPath}' (exit code {process.ExitCode}): {standardOutput} {standardError}")
                End If
            End Using
        End Sub

#End Region

#Region " Constants "

        ''' <summary>
        ''' The skip reason used by the tests that require <see cref="IsWindowsDesktopFramework"/>.
        ''' </summary>
        Public Const WindowsOnlyReason As String = "Requires the native Windows Forms implementation of .NET Framework on Windows."

#End Region

    End Class

End Namespace
