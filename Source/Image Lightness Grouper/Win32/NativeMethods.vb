Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security

#End Region

Namespace ImageLightnessGrouper.Win32

    ''' <summary>
    ''' Platform Invocation (P/Invoke) declarations used by this application.
    ''' </summary>
    <SuppressUnmanagedCodeSecurity>
    Friend NotInheritable Class NativeMethods

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="NativeMethods"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " dwmapi.dll "

        ''' <summary>
        ''' Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window.
        ''' </summary>
        ''' <param name="hwnd">The handle to the window.</param>
        ''' <param name="attr">The attribute to set.</param>
        ''' <param name="attrValue">The attribute value.</param>
        ''' <param name="attrSize">The size, in bytes, of <paramref name="attrValue"/>.</param>
        ''' <returns><c>S_OK</c> (0) on success; otherwise, an <c>HRESULT</c> error code.</returns>
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmsetwindowattribute"/>
        ''' </remarks>
        <DllImport("dwmapi.dll", PreserveSig:=True)>
        Friend Shared Function DwmSetWindowAttribute(hwnd As IntPtr,
                                                     attr As DwmWindowAttribute,
                                                     ByRef attrValue As Integer,
                                                     attrSize As Integer) As Integer
        End Function

#End Region

#Region " user32.dll "

        ''' <summary>
        ''' Sends the specified message to a window and does not return until the window procedure has processed it.
        ''' </summary>
        ''' <param name="hwnd">The handle to the target window.</param>
        ''' <param name="msg">The message to send.</param>
        ''' <param name="wParam">Additional message-specific information.</param>
        ''' <param name="lParam">Additional message-specific information.</param>
        ''' <returns>The result of the message processing.</returns>
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendmessagew"/>
        ''' </remarks>
        <DllImport("user32.dll", EntryPoint:="SendMessageW", SetLastError:=False)>
        Friend Shared Function SendMessage(hwnd As IntPtr,
                                           msg As WindowMessage,
                                           wParam As IntPtr,
                                           lParam As IntPtr) As IntPtr
        End Function

        ''' <summary>
        ''' Changes the size, position, and Z order of a window.
        ''' </summary>
        ''' <param name="hwnd">The handle to the window.</param>
        ''' <param name="hwndInsertAfter">The handle to the window to precede the positioned window in the Z order.</param>
        ''' <param name="x">The new position of the left side of the window.</param>
        ''' <param name="y">The new position of the top of the window.</param>
        ''' <param name="cx">The new width of the window.</param>
        ''' <param name="cy">The new height of the window.</param>
        ''' <param name="flags">The window sizing and positioning flags.</param>
        ''' <returns><see langword="True"/> on success; otherwise <see langword="False"/>.</returns>
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos"/>
        ''' </remarks>
        <DllImport("user32.dll", SetLastError:=True)>
        Friend Shared Function SetWindowPos(hwnd As IntPtr,
                                            hwndInsertAfter As IntPtr,
                                            x As Integer, y As Integer, cx As Integer, cy As Integer,
                                            flags As SetWindowPosFlags) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        ''' <summary>
        ''' Updates the specified rectangle or region in a window's client area (and optionally its non-client area).
        ''' </summary>
        ''' <param name="hwnd">The handle to the window.</param>
        ''' <param name="updateRect">A pointer to the rectangle to update, or <see cref="IntPtr.Zero"/> for the whole window.</param>
        ''' <param name="updateRegion">A handle to the region to update, or <see cref="IntPtr.Zero"/> for the whole window.</param>
        ''' <param name="flags">The redraw flags.</param>
        ''' <returns><see langword="True"/> on success; otherwise <see langword="False"/>.</returns>
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-redrawwindow"/>
        ''' </remarks>
        <DllImport("user32.dll", SetLastError:=False)>
        Friend Shared Function RedrawWindow(hwnd As IntPtr,
                                            updateRect As IntPtr,
                                            updateRegion As IntPtr,
                                            flags As RedrawWindowFlags) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

#End Region

#Region " uxtheme.dll "

        ''' <summary>
        ''' Causes a window to use a different set of visual style information than its class normally uses.
        ''' <para></para>
        ''' Passing <c>"DarkMode_Explorer"</c> renders dark scroll bars on Windows 10 1809 and later.
        ''' </summary>
        ''' <param name="hwnd">The handle to the window.</param>
        ''' <param name="subAppName">The application name to use in place of the calling application's name.</param>
        ''' <param name="subIdList">A semicolon-separated list of class identifier names.</param>
        ''' <returns><c>S_OK</c> (0) on success; otherwise, an <c>HRESULT</c> error code.</returns>
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/uxtheme/nf-uxtheme-setwindowtheme"/>
        ''' </remarks>
        <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode, PreserveSig:=True)>
        Friend Shared Function SetWindowTheme(hwnd As IntPtr,
                                              subAppName As String,
                                              subIdList As String) As Integer
        End Function

#End Region

#Region " kernel32.dll "

        ''' <summary>
        ''' Retrieves information about the current usage of both physical and virtual memory.
        ''' </summary>
        ''' <param name="buffer">
        ''' A <see cref="MemoryStatusEx"/> structure whose <see cref="MemoryStatusEx.Length"/> member
        ''' must be set to the size of the structure before the call.
        ''' </param>
        ''' <returns><see langword="True"/> on success; otherwise, <see langword="False"/> (see <c>GetLastError</c>).</returns>
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/nf-sysinfoapi-globalmemorystatusex"/>
        ''' </remarks>
        <DllImport("kernel32.dll", SetLastError:=True)>
        Friend Shared Function GlobalMemoryStatusEx(ByRef buffer As MemoryStatusEx) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

#End Region

#Region " ntdll.dll "

        ''' <summary>
        ''' Returns version information about the currently running operating system.
        ''' <para></para>
        ''' Unlike <see cref="Environment.OSVersion"/>, this is not affected by application manifest compatibility shims.
        ''' </summary>
        ''' <param name="versionInfo">The structure that receives the version information.</param>
        ''' <returns><c>STATUS_SUCCESS</c> (0).</returns>
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/nf-wdm-rtlgetversion"/>
        ''' </remarks>
        <DllImport("ntdll.dll", SetLastError:=False)>
        Friend Shared Function RtlGetVersion(ByRef versionInfo As OsVersionInfo) As Integer
        End Function

#End Region

    End Class

End Namespace
