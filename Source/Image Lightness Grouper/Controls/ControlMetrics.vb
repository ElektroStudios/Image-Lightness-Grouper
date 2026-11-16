Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing

#End Region

Namespace ImageLightnessGrouper.Controls

    ''' <summary>
    ''' Shared sizing rules of the custom controls, so that every input control
    ''' (text box, numeric box) has exactly the same height for a given font.
    ''' </summary>
    ''' <remarks>
    ''' The native controls do not agree on their height: a border-less <c>TextBox</c> is <c>FontHeight</c> tall
    ''' and a border-less <c>NumericUpDown</c> is <c>FontHeight + 3</c>. Without a common rule, controls placed in
    ''' consecutive rows end up with different heights and look vertically misaligned.
    ''' </remarks>
    Friend NotInheritable Class ControlMetrics

#Region " Constants "

        ''' <summary>
        ''' Extra height added to the font height to obtain the input control height (border + padding).
        ''' </summary>
        Friend Const InputHeightPadding As Integer = 12

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ControlMetrics"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Gets the common height of every single-line input control for the specified font.
        ''' </summary>
        ''' <param name="font">The control font.</param>
        ''' <returns>The input control height, in pixels.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="font"/> is <see langword="Nothing"/>.</exception>
        Friend Shared Function GetInputHeight(font As Font) As Integer
            If font Is Nothing Then
                Throw New ArgumentNullException(NameOf(font))
            End If

            Return font.Height + InputHeightPadding
        End Function

#End Region

    End Class

End Namespace
