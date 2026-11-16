Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Windows.Forms

Imports ImageLightnessGrouper.Win32

#End Region

Namespace ImageLightnessGrouper.Theming

    ''' <summary>
    ''' Suspends all painting of a window and of its whole child window tree (<c>WM_SETREDRAW</c>) for the
    ''' lifetime of this object. When disposed, painting is resumed and the window, its non-client area
    ''' (borders, scroll bars) and every child are repainted at once (<c>RedrawWindow</c>).
    ''' <para></para>
    ''' This turns a cascade of individual repaints (visible as flicker and white flashes) into a single final repaint.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Using New RedrawSuspension(Me)
    '''     ThemeManager.ApplyTheme(Me)
    ''' End Using
    ''' </code>
    ''' </example>
    Friend NotInheritable Class RedrawSuspension : Implements IDisposable

#Region " Read-only Fields "

        ''' <summary>
        ''' The control whose painting is suspended.
        ''' </summary>
        Private ReadOnly control As Control

#End Region

#Region " Fields "

        ''' <summary>
        ''' Whether this instance has been disposed.
        ''' </summary>
        Private isDisposed As Boolean

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RedrawSuspension"/> class, suspending the painting of the specified control.
        ''' </summary>
        ''' <param name="control">The control (typically a form).</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="control"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidOperationException">Thrown when the control handle has not been created yet.</exception>
        Friend Sub New(control As Control)
            If control Is Nothing Then
                Throw New ArgumentNullException(NameOf(control))
            End If

            If Not control.IsHandleCreated Then
                Throw New InvalidOperationException("Painting can only be suspended once the control handle has been created.")
            End If

            Me.control = control
            RedrawSuspension.SetRedraw(control, enabled:=False)
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Resumes painting and repaints the window, its non-client area and all its children at once.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If Me.isDisposed Then
                Return
            End If

            Me.isDisposed = True
            RedrawSuspension.SetRedraw(Me.control, enabled:=True)
            RedrawSuspension.RedrawEverything(Me.control)
        End Sub

        ''' <summary>
        ''' Immediately repaints a window, its non-client area (borders, scroll bars) and all its children.
        ''' </summary>
        ''' <param name="control">The control.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="control"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidOperationException">Thrown when the control handle has not been created yet, or the repaint fails.</exception>
        Friend Shared Sub RedrawEverything(control As Control)
            If control Is Nothing Then
                Throw New ArgumentNullException(NameOf(control))
            End If

            If Not control.IsHandleCreated Then
                Throw New InvalidOperationException("A control can only be repainted once its handle has been created.")
            End If

            Dim flags As RedrawWindowFlags = RedrawWindowFlags.Invalidate Or RedrawWindowFlags.Erase Or
                                             RedrawWindowFlags.Frame Or RedrawWindowFlags.AllChildren Or
                                             RedrawWindowFlags.UpdateNow

            If Not NativeMethods.RedrawWindow(control.Handle, IntPtr.Zero, IntPtr.Zero, flags) Then
                Throw New InvalidOperationException($"RedrawWindow failed for '{control.Name}'.")
            End If
        End Sub

        ''' <summary>
        ''' Enables or disables the painting of a window and of its whole child window tree (<c>WM_SETREDRAW</c>).
        ''' </summary>
        ''' <param name="control">The control.</param>
        ''' <param name="enabled"><see langword="True"/> to enable painting.</param>
        Friend Shared Sub SetRedraw(control As Control, enabled As Boolean)
            If control Is Nothing Then
                Throw New ArgumentNullException(NameOf(control))
            End If

            NativeMethods.SendMessage(control.Handle, WindowMessage.SetRedraw, New IntPtr(If(enabled, 1, 0)), IntPtr.Zero)
        End Sub

#End Region

    End Class

End Namespace
