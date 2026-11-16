Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Windows.Forms

Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Forms

    ''' <summary>
    ''' Base form that keeps itself (client area and DWM title bar) in sync with <see cref="ThemeManager"/>.
    ''' </summary>
    Public Class ThemedForm : Inherits Form

#Region " Read-only Fields "

        ''' <summary>
        ''' The application icon, shared by every form for the whole application lifetime.
        ''' <para></para>
        ''' <c>My.Resources.AppIcon</c> deserializes a brand new <see cref="Drawing.Icon"/> on every access and a form never
        ''' disposes its <see cref="Form.Icon"/>, so reading the resource per form would leak one icon handle per opened form.
        ''' </summary>
        Private Shared ReadOnly SharedApplicationIcon As New Lazy(Of Drawing.Icon)(Function() My.Resources.AppIcon)

#End Region

#Region " Overridden Methods "

        ''' <summary>
        ''' Raises the <see cref="Control.HandleCreated"/> event and applies the themed title bar.
        ''' <para></para>
        ''' The DWM attribute must be set here (not in <see cref="Form.OnLoad"/>), before the window is first shown.
        ''' </summary>
        ''' <param name="e">The event data.</param>
        Protected Overrides Sub OnHandleCreated(e As EventArgs)
            MyBase.OnHandleCreated(e)

            If Not Me.DesignMode Then
                WindowChrome.ApplyTitleBarTheme(Me, ThemeManager.CurrentPalette.IsDark)
            End If
        End Sub

        ''' <summary>
        ''' Raises the <see cref="Form.Load"/> event, applies the active theme and starts listening for theme changes.
        ''' </summary>
        ''' <param name="e">The event data.</param>
        Protected Overrides Sub OnLoad(e As EventArgs)
            If Not Me.DesignMode Then
                Me.Icon = ThemedForm.SharedApplicationIcon.Value
                ThemeManager.ApplyTheme(Me)
                AddHandler ThemeManager.ThemeChanged, AddressOf Me.ThemeManager_ThemeChanged
            End If

            MyBase.OnLoad(e)
        End Sub

        ''' <summary>
        ''' Raises the <see cref="Form.FormClosed"/> event and stops listening for theme changes,
        ''' so the shared event never keeps a closed form alive.
        ''' </summary>
        ''' <param name="e">The event data.</param>
        Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
            If Not Me.DesignMode Then
                RemoveHandler ThemeManager.ThemeChanged, AddressOf Me.ThemeManager_ThemeChanged
            End If

            MyBase.OnFormClosed(e)
        End Sub

#End Region

#Region " Event Handlers "

        ''' <summary>
        ''' Re-applies the theme to the whole form, including its title bar.
        ''' </summary>
        Private Sub ThemeManager_ThemeChanged(sender As Object, e As EventArgs)
            ' Every color change would otherwise repaint its control immediately (and native controls also
            ' repaint when their visual style changes), producing a cascade of white flashes. Painting of the
            ' whole window tree is suspended while the theme is applied, then everything is repainted at once.
            Using New RedrawSuspension(Me)
                ThemeManager.ApplyTheme(Me)
            End Using

            WindowChrome.ApplyTitleBarTheme(Me, ThemeManager.CurrentPalette.IsDark)
        End Sub

#End Region

    End Class

End Namespace
