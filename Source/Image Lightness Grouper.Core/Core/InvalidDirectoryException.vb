Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO

#End Region

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' The exception thrown when a directory path specified by the user is not valid: it is empty or malformed,
    ''' points to a file, or the directory does not exist. The message is meant to be shown to the user as is.
    ''' </summary>
    Public NotInheritable Class InvalidDirectoryException : Inherits IOException

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="InvalidDirectoryException"/> class.
        ''' </summary>
        ''' <param name="message">The message, shown to the user.</param>
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="InvalidDirectoryException"/> class.
        ''' </summary>
        ''' <param name="message">The message, shown to the user.</param>
        ''' <param name="innerException">The exception that caused this one.</param>
        Public Sub New(message As String, innerException As Exception)
            MyBase.New(message, innerException)
        End Sub

#End Region

    End Class

End Namespace
