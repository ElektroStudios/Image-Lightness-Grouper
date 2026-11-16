Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' The command-line switches and the rules that tell a switch from a path.
    ''' <para></para>
    ''' A switch starts with <c>/</c>, <c>-</c> or <c>--</c>, is case-insensitive, and takes its value after
    ''' <c>:</c> or <c>=</c> (e.g. <c>/step:5</c>, <c>--step=5</c>).
    ''' </summary>
    Public NotInheritable Class CommandLineSyntax

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' The canonical name of the switch that includes the subdirectories.
        ''' </summary>
        Public Const RecursiveSwitch As String = "recursive"

        ''' <summary>
        ''' The canonical name of the switch that sets the lightness range width.
        ''' </summary>
        Public Const StepSwitch As String = "step"

        ''' <summary>
        ''' The canonical name of the switch that sets the alpha threshold.
        ''' </summary>
        Public Const AlphaSwitch As String = "alpha"

        ''' <summary>
        ''' The canonical name of the switch that sets the maximum degree of parallelism.
        ''' </summary>
        Public Const ParallelSwitch As String = "parallel"

        ''' <summary>
        ''' The switch names (and short aliases) mapped to their canonical name.
        ''' </summary>
        Public Shared ReadOnly SwitchAliases As New ReadOnlyDictionary(Of String, String)(
            New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
                {RecursiveSwitch, RecursiveSwitch}, {"r", RecursiveSwitch},
                {StepSwitch, StepSwitch}, {"s", StepSwitch},
                {AlphaSwitch, AlphaSwitch}, {"a", AlphaSwitch},
                {ParallelSwitch, ParallelSwitch}, {"p", ParallelSwitch}
            })

        ''' <summary>
        ''' The canonical names of the switches that take a value.
        ''' </summary>
        Public Shared ReadOnly ValueSwitches As ReadOnlyCollection(Of String) = Array.AsReadOnly({StepSwitch, AlphaSwitch, ParallelSwitch})

        ''' <summary>
        ''' The names of the help switch.
        ''' </summary>
        Public Shared ReadOnly HelpSwitchNames As ReadOnlyCollection(Of String) = Array.AsReadOnly({"?", "h", "help"})

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="CommandLineSyntax"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Determines whether an argument is a switch: it starts with <c>/</c> or <c>-</c>, and its name (the part
        ''' before <c>:</c> or <c>=</c>) contains no path separator.
        ''' <para></para>
        ''' So <c>/r</c>, <c>/step:5</c> and even a switch glued to a path, such as <c>/rC:\Images</c>, are switches
        ''' (the last one is reported as an unknown switch instead of being taken as a directory), while an absolute
        ''' Unix-style path such as <c>/data/images</c> is not.
        ''' </summary>
        ''' <param name="argument">The argument.</param>
        ''' <returns><see langword="True"/> for a switch; otherwise, <see langword="False"/>.</returns>
        Public Shared Function IsSwitch(argument As String) As Boolean
            If argument Is Nothing OrElse argument.Length < 2 OrElse (argument(0) <> "/"c AndAlso argument(0) <> "-"c) Then
                Return False
            End If

            Return CommandLineSyntax.GetWrittenName(argument).IndexOfAny({"/"c, "\"c}) < 0
        End Function

        ''' <summary>
        ''' Determines whether an argument is a help switch (<c>/?</c>, <c>/h</c>, <c>/help</c>, with any prefix).
        ''' </summary>
        ''' <param name="argument">The argument.</param>
        ''' <returns><see langword="True"/> for a help switch; otherwise, <see langword="False"/>.</returns>
        Public Shared Function IsHelpSwitch(argument As String) As Boolean
            Return CommandLineSyntax.IsSwitch(argument) AndAlso
                   CommandLineSyntax.HelpSwitchNames.Contains(CommandLineSyntax.RemovePrefix(argument), StringComparer.OrdinalIgnoreCase)
        End Function

        ''' <summary>
        ''' Removes the switch prefix (<c>/</c>, <c>-</c> or <c>--</c>) of a switch.
        ''' </summary>
        ''' <param name="argument">The switch.</param>
        ''' <returns>The switch without its prefix.</returns>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="argument"/> does not start with a switch prefix.</exception>
        Public Shared Function RemovePrefix(argument As String) As String
            If String.IsNullOrEmpty(argument) OrElse (argument(0) <> "/"c AndAlso argument(0) <> "-"c) Then
                Throw New ArgumentException($"The argument is not a switch: {argument}", NameOf(argument))
            End If

            If argument.StartsWith("--", StringComparison.Ordinal) Then
                Return argument.Substring(2)
            End If

            Return argument.Substring(1)
        End Function

        ''' <summary>
        ''' Gets the name of a switch as written (without prefix and value).
        ''' </summary>
        ''' <param name="argument">The switch.</param>
        ''' <returns>The written name.</returns>
        Public Shared Function GetWrittenName(argument As String) As String
            Dim body As String = CommandLineSyntax.RemovePrefix(argument)
            Dim separatorIndex As Integer = body.IndexOfAny({":"c, "="c})
            Return If(separatorIndex < 0, body, body.Substring(0, separatorIndex))
        End Function

#End Region

    End Class

End Namespace
