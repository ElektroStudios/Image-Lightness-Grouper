Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' xUnit collection for the tests that mutate process-wide shared state
    ''' (<c>My.Settings</c>, <c>ThemeManager</c>): they must never run in parallel with each other.
    ''' </summary>
    <CollectionDefinition(SharedStateCollection.Name, DisableParallelization:=True)>
    Public NotInheritable Class SharedStateCollection

        ''' <summary>
        ''' The collection name.
        ''' </summary>
        Public Const Name As String = "Shared application state"

    End Class

End Namespace
