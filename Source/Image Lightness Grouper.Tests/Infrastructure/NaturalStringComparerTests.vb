Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageLightnessGrouper.Infrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.Infrastructure

    ''' <summary>
    ''' Tests for <see cref="NaturalStringComparer"/>.
    ''' </summary>
    Public NotInheritable Class NaturalStringComparerTests

        ''' <summary>
        ''' Numbers are compared by value and text case-insensitively.
        ''' </summary>
        <Fact>
        Public Sub Sort_UsesNaturalOrder()
            Dim names As String() = {"image10.jpg", "Image2.jpg", "image1.jpg", "image02b.jpg", "a.jpg", "image100.jpg", "image2a.jpg"}

            Dim sorted As String() = names.OrderBy(Function(name As String) name, NaturalStringComparer.Instance).ToArray()

            Assert.Equal(New String() {"a.jpg", "image1.jpg", "Image2.jpg", "image2a.jpg", "image02b.jpg", "image10.jpg", "image100.jpg"}, sorted.ToList())
        End Sub

        ''' <summary>
        ''' Numbers longer than any integer type are still compared by value.
        ''' </summary>
        <Fact>
        Public Sub Compare_HugeNumbers()
            Assert.True(NaturalStringComparer.Instance.Compare("x99999999999999999999999", "x100000000000000000000000") < 0)
        End Sub

        ''' <summary>
        ''' The comparison is consistent: equal strings are equal, prefixes go first, and Nothing goes first.
        ''' </summary>
        <Fact>
        Public Sub Compare_EdgeCases()
            Dim comparer As NaturalStringComparer = NaturalStringComparer.Instance

            Assert.Equal(0, comparer.Compare("same.png", "same.png"))
            Assert.Equal(0, comparer.Compare(Nothing, Nothing))
            Assert.True(comparer.Compare(Nothing, "a") < 0)
            Assert.True(comparer.Compare("a", Nothing) > 0)
            Assert.True(comparer.Compare("image", "image1") < 0)
            Assert.NotEqual(0, comparer.Compare("A.png", "a.png"))
            Assert.Equal(-comparer.Compare("file01", "file1"), comparer.Compare("file1", "file01"))
        End Sub

    End Class

End Namespace
