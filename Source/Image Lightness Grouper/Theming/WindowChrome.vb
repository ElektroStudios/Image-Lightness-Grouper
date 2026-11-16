Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Imports Microsoft.Win32

Imports ImageLightnessGrouper.Infrastructure
Imports ImageLightnessGrouper.Win32

#End Region

Namespace ImageLightnessGrouper.Theming

    ''' <summary>
    ''' Applies dark or light styling to native window chrome (title bars and scroll bars)
    ''' through the Desktop Window Manager and UxTheme APIs.
    ''' </summary>
    Friend NotInheritable Class WindowChrome

#Region " Constants "

        ''' <summary>
        ''' The registry key that stores the user's personalization settings.
        ''' </summary>
        Private Const PersonalizeRegistryKey As String = "Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"

        ''' <summary>
        ''' The registry value that indicates whether apps use the light theme (1) or the dark theme (0).
        ''' </summary>
        Private Const AppsUseLightThemeValueName As String = "AppsUseLightTheme"

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="WindowChrome"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets a value indicating whether the running OS supports immersive dark title bars (Windows 10 1809 or newer).
        ''' </summary>
        Friend Shared ReadOnly Property IsDarkTitleBarSupported As Boolean
            Get
                Return OperatingSystemInfo.BuildNumber >= OperatingSystemInfo.Windows10Build1809
            End Get
        End Property

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Applies (or removes) the immersive dark mode attribute to the window's title bar.
        ''' <para></para>
        ''' On operating systems older than Windows 10 1809 the feature does not exist, so this method is a documented no-op.
        ''' </summary>
        ''' <param name="form">The form whose title bar must be styled.</param>
        ''' <param name="useDarkMode"><see langword="True"/> to render a dark title bar; <see langword="False"/> for the default one.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="form"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidOperationException">Thrown when the form handle has not been created yet.</exception>
        ''' <exception cref="COMException">Thrown when <c>DwmSetWindowAttribute</c> fails.</exception>
        Friend Shared Sub ApplyTitleBarTheme(form As Form, useDarkMode As Boolean)
            If form Is Nothing Then
                Throw New ArgumentNullException(NameOf(form))
            End If

            If Not form.IsHandleCreated Then
                Throw New InvalidOperationException("The form handle must be created before applying the title bar theme. Call this method from OnHandleCreated.")
            End If

            If Not WindowChrome.IsDarkTitleBarSupported Then
                Return
            End If

            ' Deterministic attribute selection based on the real OS build, instead of trial and error.
            Dim attribute As DwmWindowAttribute =
                If(OperatingSystemInfo.BuildNumber >= OperatingSystemInfo.Windows10Build20H1,
                   DwmWindowAttribute.UseImmersiveDarkMode,
                   DwmWindowAttribute.UseImmersiveDarkModeBefore20H1)

            Dim attributeValue As Integer = If(useDarkMode, 1, 0)
            Dim attributeSize As Integer = Marshal.SizeOf(GetType(Integer))

            Dim hResult As Integer = NativeMethods.DwmSetWindowAttribute(form.Handle, attribute, attributeValue, attributeSize)
            If hResult <> 0 Then
                Marshal.ThrowExceptionForHR(hResult)
            End If

            ' When the window is already on screen, the non-client area must be forced to repaint.
            If form.Visible Then
                WindowChrome.RefreshNonClientArea(form)
            End If
        End Sub

        ''' <summary>
        ''' Applies dark (<c>DarkMode_Explorer</c>) or light (<c>Explorer</c>) scroll bars to a native control.
        ''' </summary>
        ''' <param name="control">The control (e.g. a <see cref="RichTextBox"/>).</param>
        ''' <param name="useDarkMode"><see langword="True"/> for dark scroll bars.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="control"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidOperationException">Thrown when the control handle has not been created yet.</exception>
        ''' <exception cref="COMException">Thrown when <c>SetWindowTheme</c> fails.</exception>
        Friend Shared Sub ApplyScrollBarTheme(control As Control, useDarkMode As Boolean)
            If control Is Nothing Then
                Throw New ArgumentNullException(NameOf(control))
            End If

            If Not control.IsHandleCreated Then
                Throw New InvalidOperationException("The control handle must be created before applying the scroll bar theme.")
            End If

            Dim themeName As String = If(useDarkMode, "DarkMode_Explorer", "Explorer")
            Dim hResult As Integer = NativeMethods.SetWindowTheme(control.Handle, themeName, Nothing)
            If hResult <> 0 Then
                Marshal.ThrowExceptionForHR(hResult)
            End If
        End Sub

        ''' <summary>
        ''' Determines if the Windows dark theme is currently enabled for applications.
        ''' </summary>
        ''' <returns>
        ''' <see langword="True"/> if the user's apps theme is dark; otherwise <see langword="False"/>
        ''' (Windows defaults to light when the value has never been written).
        ''' </returns>
        ''' <exception cref="InvalidOperationException">Thrown when the registry cannot be read.</exception>
        Friend Shared Function IsWindowsDarkModeEnabled() As Boolean
            Try
                Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(WindowChrome.PersonalizeRegistryKey, writable:=False)
                    If key Is Nothing Then
                        Return False
                    End If

                    Dim value As Object = key.GetValue(WindowChrome.AppsUseLightThemeValueName)
                    Return value IsNot Nothing AndAlso Convert.ToInt32(value) = 0
                End Using

            Catch ex As Exception
                Throw New InvalidOperationException("Failed to read Windows theme registry key to determine dark mode status.", ex)
            End Try
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Forces an immediate repaint of the non-client area (title bar) of a visible window.
        ''' <para></para>
        ''' Windows 10 does not repaint the caption after changing the DWM attribute until the activation
        ''' state changes, so the activation state is toggled and restored through <c>WM_NCACTIVATE</c>.
        ''' </summary>
        ''' <param name="form">The visible form.</param>
        ''' <exception cref="Win32Exception">Thrown when <c>SetWindowPos</c> fails.</exception>
        Private Shared Sub RefreshNonClientArea(form As Form)
            Dim hwnd As IntPtr = form.Handle
            Dim isActive As Boolean = Form.ActiveForm Is form
            Dim inactive As IntPtr = IntPtr.Zero
            Dim active As New IntPtr(1)

            NativeMethods.SendMessage(hwnd, WindowMessage.NcActivate, If(isActive, inactive, active), IntPtr.Zero)
            NativeMethods.SendMessage(hwnd, WindowMessage.NcActivate, If(isActive, active, inactive), IntPtr.Zero)

            Dim flags As SetWindowPosFlags = SetWindowPosFlags.FrameChanged Or SetWindowPosFlags.NoMove Or
                                             SetWindowPosFlags.NoSize Or SetWindowPosFlags.NoZOrder Or SetWindowPosFlags.NoActivate

            If Not NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, flags) Then
                Throw New Win32Exception(Marshal.GetLastWin32Error())
            End If
        End Sub

#End Region

    End Class

End Namespace
