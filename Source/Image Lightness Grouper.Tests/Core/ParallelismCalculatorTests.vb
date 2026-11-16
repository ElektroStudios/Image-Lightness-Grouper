Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.Tests.Core

    ''' <summary>
    ''' Tests for <see cref="ParallelismCalculator"/>.
    ''' </summary>
    Public NotInheritable Class ParallelismCalculatorTests

        ''' <summary>
        ''' The automatic value is limited by the memory and by the processors, and it is never lower than 1.
        ''' </summary>
        <Theory>
        <InlineData(16.0R, 1.0R, 8, 8)>
        <InlineData(4.0R, 1.0R, 8, 4)>
        <InlineData(4.9R, 1.0R, 8, 4)>
        <InlineData(8.0R, 2.0R, 16, 4)>
        <InlineData(0.5R, 1.0R, 8, 1)>
        <InlineData(0.0R, 1.0R, 8, 1)>
        <InlineData(64.0R, 1.0R, 1, 1)>
        <InlineData(1.0E+300R, 1.0R, 32, 32)>
        Public Sub CalculateAutomatic_X64(availableMemoryGB As Double, memoryPerThreadGB As Double, processorCount As Integer, expected As Integer)
            Assert.Equal(expected, ParallelismCalculator.CalculateAutomatic(availableMemoryGB, memoryPerThreadGB, processorCount, isX86Process:=False))
        End Sub

        ''' <summary>
        ''' A 32-bit process always uses a single thread.
        ''' </summary>
        <Theory>
        <InlineData(64.0R, 16)>
        <InlineData(0.0R, 1)>
        Public Sub CalculateAutomatic_X86_IsAlwaysOne(availableMemoryGB As Double, processorCount As Integer)
            Assert.Equal(1, ParallelismCalculator.CalculateAutomatic(availableMemoryGB, 1.0R, processorCount, isX86Process:=True))
        End Sub

        ''' <summary>
        ''' Invalid arguments are rejected, even for 32-bit processes.
        ''' </summary>
        <Theory>
        <InlineData(Double.NaN, 1.0R, 4)>
        <InlineData(-1.0R, 1.0R, 4)>
        <InlineData(Double.PositiveInfinity, 1.0R, 4)>
        <InlineData(4.0R, 0.0R, 4)>
        <InlineData(4.0R, Double.NaN, 4)>
        <InlineData(4.0R, 1.0R, 0)>
        Public Sub CalculateAutomatic_InvalidArguments_Throw(availableMemoryGB As Double, memoryPerThreadGB As Double, processorCount As Integer)
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() ParallelismCalculator.CalculateAutomatic(availableMemoryGB, memoryPerThreadGB, processorCount, False))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() ParallelismCalculator.CalculateAutomatic(availableMemoryGB, memoryPerThreadGB, processorCount, True))
        End Sub

        ''' <summary>
        ''' The requested value must be 0 (automatic) or a positive value up to the maximum.
        ''' </summary>
        <Fact>
        Public Sub ValidateRequested_AcceptsOnlyTheAllowedRange()
            ParallelismCalculator.ValidateRequested(ParallelismCalculator.Automatic)
            ParallelismCalculator.ValidateRequested(ParallelismCalculator.MaximumRequestable)

            Assert.Throws(Of ArgumentOutOfRangeException)(Sub() ParallelismCalculator.ValidateRequested(-1))
            Assert.Throws(Of ArgumentOutOfRangeException)(Sub() ParallelismCalculator.ValidateRequested(ParallelismCalculator.MaximumRequestable + 1))
        End Sub

        ''' <summary>
        ''' A 32-bit process uses a fixed 1 GB budget without querying the system.
        ''' </summary>
        <Fact>
        Public Sub GetAvailableMemoryGB_X86_IsOneGigabyte()
            Assert.Equal(1.0R, ParallelismCalculator.GetAvailableMemoryGB(isX86Process:=True))
        End Sub

    End Class

End Namespace
