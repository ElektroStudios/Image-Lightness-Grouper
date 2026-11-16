Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Imports Microsoft.VisualBasic.ApplicationServices

Imports ImageLightnessGrouper.Infrastructure

#End Region

Namespace ImageLightnessGrouper.Forms

    ''' <summary>
    ''' The standard "About" dialog. Every piece of information is read from <c>My.Application.Info</c>
    ''' (that is, from the assembly attributes declared in <c>AssemblyInfo.vb</c>).
    ''' </summary>
    Partial Public NotInheritable Class AboutBox

#Region " Constants "

        ''' <summary>
        ''' The preferred logo size, in pixels.
        ''' </summary>
        Private Const LogoSize As Integer = 128

#End Region

#Region " Fields "

        ''' <summary>
        ''' The logo bitmap (owned by this form, disposed when it closes).
        ''' </summary>
        Private logoBitmap As Bitmap

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AboutBox"/> class.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when a required assembly attribute is missing.</exception>
        Public Sub New()
            Me.InitializeComponent()

            Dim info As AssemblyInfo = My.Application.Info

            If String.IsNullOrWhiteSpace(info.Title) Then
                Throw New InvalidOperationException("The AssemblyTitle attribute is missing or empty (see AssemblyInfo.vb).")
            End If

            If String.IsNullOrWhiteSpace(info.ProductName) Then
                Throw New InvalidOperationException("The AssemblyProduct attribute is missing or empty (see AssemblyInfo.vb).")
            End If

            Me.Text = $"About {info.Title}"
            Me.productNameLabel.Text = info.ProductName
            Me.versionLabel.Text = $"Version {info.Version} ({UtilApplication.ProcessArchitecture.ToString().ToUpper()})"
            Me.copyrightLabel.Text = info.Copyright
            Me.companyNameLabel.Text = info.CompanyName

            Me.descriptionTextBox.Text = String.Join(Environment.NewLine, {
                info.Description,
                String.Empty,
                $"Runtime: {RuntimeInformation.FrameworkDescription}",
                $"Operating system: {RuntimeInformation.OSDescription}",
                $"OS architecture: {RuntimeInformation.OSArchitecture}"
            })

            ' My.Resources.AppIcon returns a new Icon on every access: both icons are owned (and disposed) here.
            Using applicationIcon As Icon = My.Resources.AppIcon,
                  sizedIcon As New Icon(applicationIcon, LogoSize, LogoSize)

                Me.logoBitmap = sizedIcon.ToBitmap()
            End Using

            Me.logoPictureBox.Image = Me.logoBitmap
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
            Me.logoPictureBox.Image = Nothing
            Me.logoBitmap.Dispose()
            MyBase.OnFormClosed(e)
        End Sub

#End Region

    End Class

End Namespace
