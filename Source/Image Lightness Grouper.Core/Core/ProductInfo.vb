Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Information shared by the applications of the product (the main window and the command-line interface).
    ''' </summary>
    Public NotInheritable Class ProductInfo

#Region " Constants "

        ''' <summary>
        ''' The address of the source code repository.
        ''' </summary>
        Public Const RepositoryUrl As String = "https://github.com/ElektroStudios/Image-Lightness-Grouper"

        ''' <summary>
        ''' The file name of the main window application.
        ''' </summary>
        Public Const GuiExecutableName As String = "Image Lightness Grouper.exe"

        ''' <summary>
        ''' The file name of the command-line application.
        ''' </summary>
        Public Const CliExecutableName As String = "Image Lightness Grouper.CLI.exe"

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ProductInfo"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

    End Class

End Namespace
