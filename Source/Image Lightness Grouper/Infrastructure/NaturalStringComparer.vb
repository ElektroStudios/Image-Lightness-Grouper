Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Globalization

#End Region

Namespace ImageLightnessGrouper.Infrastructure

    ''' <summary>
    ''' Compares strings in natural (human) order: runs of decimal digits are compared by their numeric value,
    ''' so <c>"image2.jpg"</c> sorts before <c>"image10.jpg"</c>. The remaining text is compared case-insensitively.
    ''' <para></para>
    ''' It is fully managed (no <c>StrCmpLogicalW</c>), so it behaves the same on every platform and in the tests.
    ''' </summary>
    Public NotInheritable Class NaturalStringComparer : Implements IComparer(Of String)

#Region " Properties "

        ''' <summary>
        ''' Gets the shared instance.
        ''' </summary>
        Public Shared ReadOnly Property Instance As New NaturalStringComparer()

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents other instances of the <see cref="NaturalStringComparer"/> class from being created (use <see cref="Instance"/>).
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <inheritdoc/>
        Public Function Compare(x As String, y As String) As Integer Implements IComparer(Of String).Compare
            If x Is Nothing OrElse y Is Nothing Then
                Return If(x Is Nothing, If(y Is Nothing, 0, -1), 1)
            End If

            Dim indexX As Integer = 0
            Dim indexY As Integer = 0

            While indexX < x.Length AndAlso indexY < y.Length
                If Char.IsDigit(x(indexX)) AndAlso Char.IsDigit(y(indexY)) Then
                    Dim endX As Integer = NaturalStringComparer.GetDigitRunEnd(x, indexX)
                    Dim endY As Integer = NaturalStringComparer.GetDigitRunEnd(y, indexY)

                    Dim result As Integer = NaturalStringComparer.CompareDigitRuns(x.Substring(indexX, endX - indexX), y.Substring(indexY, endY - indexY))
                    If result <> 0 Then
                        Return result
                    End If

                    indexX = endX
                    indexY = endY
                Else
                    Dim result As Integer = String.Compare(x, indexX, y, indexY, 1, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase)
                    If result <> 0 Then
                        Return result
                    End If

                    indexX += 1
                    indexY += 1
                End If
            End While

            ' One string is a prefix of the other: the shorter one goes first. Equal strings fall back to an ordinal
            ' comparison, so the order is deterministic even for names that differ only in case or leading zeros.
            Dim lengthResult As Integer = (x.Length - indexX).CompareTo(y.Length - indexY)
            Return If(lengthResult <> 0, lengthResult, String.CompareOrdinal(x, y))
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Gets the index just after the run of decimal digits that starts at <paramref name="startIndex"/>.
        ''' </summary>
        ''' <param name="value">The string.</param>
        ''' <param name="startIndex">The index of the first digit.</param>
        ''' <returns>The end index (exclusive).</returns>
        Private Shared Function GetDigitRunEnd(value As String, startIndex As Integer) As Integer
            Dim index As Integer = startIndex
            While index < value.Length AndAlso Char.IsDigit(value(index))
                index += 1
            End While

            Return index
        End Function

        ''' <summary>
        ''' Compares two runs of decimal digits by their numeric value, without any size limit.
        ''' </summary>
        ''' <param name="x">The first run of digits.</param>
        ''' <param name="y">The second run of digits.</param>
        ''' <returns>A negative number, zero or a positive number.</returns>
        Private Shared Function CompareDigitRuns(x As String, y As String) As Integer
            Dim trimmedX As String = x.TrimStart("0"c)
            Dim trimmedY As String = y.TrimStart("0"c)

            ' Without leading zeros, a longer run is a bigger number; equal lengths compare digit by digit.
            If trimmedX.Length <> trimmedY.Length Then
                Return trimmedX.Length.CompareTo(trimmedY.Length)
            End If

            Return String.CompareOrdinal(trimmedX, trimmedY)
        End Function

#End Region

    End Class

End Namespace
