Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Win32

    ''' <summary>
    ''' Window attributes accepted by the <c>DwmSetWindowAttribute</c> function.
    ''' </summary>
    ''' <remarks>
    ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute"/>
    ''' </remarks>
    Friend Enum DwmWindowAttribute As Integer

        ''' <summary>
        ''' Undocumented immersive dark mode attribute used by Windows 10 builds 17763 (1809) to 18985.
        ''' </summary>
        UseImmersiveDarkModeBefore20H1 = 19

        ''' <summary>
        ''' <c>DWMWA_USE_IMMERSIVE_DARK_MODE</c>. Supported starting with Windows 10 build 18985 (20H1).
        ''' </summary>
        UseImmersiveDarkMode = 20

    End Enum

    ''' <summary>
    ''' Window messages used by this application.
    ''' </summary>
    ''' <remarks>
    ''' <see href="https://learn.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues"/>
    ''' </remarks>
    Friend Enum WindowMessage As Integer

        ''' <summary>
        ''' <c>WM_SETREDRAW</c>: allows or prevents changes in a window from being redrawn.
        ''' </summary>
        SetRedraw = &HB

        ''' <summary>
        ''' <c>WM_NCACTIVATE</c>: sent to a window when its non-client area needs to be repainted as active or inactive.
        ''' </summary>
        NcActivate = &H86

        ''' <summary>
        ''' <c>WM_HSCROLL</c>: horizontal scroll event.
        ''' </summary>
        HScroll = &H114

        ''' <summary>
        ''' <c>WM_VSCROLL</c>: vertical scroll event.
        ''' </summary>
        VScroll = &H115

    End Enum

    ''' <summary>
    ''' Scroll bar commands for the <c>WM_HSCROLL</c> and <c>WM_VSCROLL</c> messages (low-order word of <c>wParam</c>).
    ''' </summary>
    Friend Enum ScrollBarCommand As Integer

        ''' <summary>
        ''' <c>SB_LEFT</c>: scrolls to the upper left (horizontal scroll bar).
        ''' </summary>
        Left = 6

        ''' <summary>
        ''' <c>SB_BOTTOM</c>: scrolls to the lower right.
        ''' </summary>
        Bottom = 7

    End Enum

    ''' <summary>
    ''' Flags for the <c>RedrawWindow</c> function.
    ''' </summary>
    ''' <remarks>
    ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-redrawwindow"/>
    ''' </remarks>
    <Flags>
    Friend Enum RedrawWindowFlags As UInteger

        ''' <summary>
        ''' No flags.
        ''' </summary>
        None = 0UI

        ''' <summary>
        ''' <c>RDW_INVALIDATE</c>: invalidates the window.
        ''' </summary>
        Invalidate = 1UI << 0

        ''' <summary>
        ''' <c>RDW_ERASE</c>: the window receives <c>WM_ERASEBKGND</c> when repainted.
        ''' </summary>
        [Erase] = 1UI << 2

        ''' <summary>
        ''' <c>RDW_ALLCHILDREN</c>: includes all child windows.
        ''' </summary>
        AllChildren = 1UI << 7

        ''' <summary>
        ''' <c>RDW_UPDATENOW</c>: repaints immediately, before the function returns.
        ''' </summary>
        UpdateNow = 1UI << 8

        ''' <summary>
        ''' <c>RDW_FRAME</c>: also repaints the non-client area (borders, scroll bars).
        ''' </summary>
        Frame = 1UI << 10

    End Enum

    ''' <summary>
    ''' Flags for the <c>SetWindowPos</c> function.
    ''' </summary>
    ''' <remarks>
    ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos"/>
    ''' </remarks>
    <Flags>
    Friend Enum SetWindowPosFlags As UInteger

        ''' <summary>
        ''' No flags.
        ''' </summary>
        None = 0UI

        ''' <summary>
        ''' <c>SWP_NOSIZE</c>: retains the current size.
        ''' </summary>
        NoSize = 1UI << 0

        ''' <summary>
        ''' <c>SWP_NOMOVE</c>: retains the current position.
        ''' </summary>
        NoMove = 1UI << 1

        ''' <summary>
        ''' <c>SWP_NOZORDER</c>: retains the current Z order.
        ''' </summary>
        NoZOrder = 1UI << 2

        ''' <summary>
        ''' <c>SWP_NOACTIVATE</c>: does not activate the window.
        ''' </summary>
        NoActivate = 1UI << 4

        ''' <summary>
        ''' <c>SWP_FRAMECHANGED</c>: applies new frame styles and sends <c>WM_NCCALCSIZE</c>.
        ''' </summary>
        FrameChanged = 1UI << 5

    End Enum

End Namespace
