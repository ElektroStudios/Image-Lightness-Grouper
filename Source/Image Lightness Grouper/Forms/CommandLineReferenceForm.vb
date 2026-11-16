Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Windows.Forms

Imports ImageLightnessGrouper.CommandLine
Imports ImageLightnessGrouper.Controls
Imports ImageLightnessGrouper.Infrastructure

#End Region

Namespace ImageLightnessGrouper.Forms

    ''' <summary>
    ''' Shows the command-line reference (Help &gt; Command-Line Reference): how the program works, the usage of
    ''' the command-line application and its exit codes. It is the same text that the command-line application shows
    ''' with <c>/?</c> (see <see cref="CommandLineHelp"/>).
    ''' </summary>
    Friend NotInheritable Class CommandLineReferenceForm : Inherits ThemedForm

#Region " Fields "

        ''' <summary>
        ''' The read-only box that shows the reference text.
        ''' </summary>
        Friend ReadOnly referenceTextBox As ThemedTextBox

        ''' <summary>
        ''' The button that closes the window.
        ''' </summary>
        Friend ReadOnly closeButton As ThemedButton

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="CommandLineReferenceForm"/> class.
        ''' </summary>
        Friend Sub New()
            Me.SuspendLayout()

            Me.referenceTextBox = New ThemedTextBox() With {
                .Dock = DockStyle.Fill,
                .Multiline = True,
                .ReadOnly = True,
                .Font = New Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point),
                .TabIndex = 0
            }

            Me.closeButton = New ThemedButton() With {
                .ButtonStyle = ThemedButtonStyle.Accent,
                .DialogResult = DialogResult.Cancel,
                .Size = New Size(90, 25),
                .TabIndex = 1,
                .Text = "&Close"
            }

            Dim buttonPanel As New Panel() With {
                .Dock = DockStyle.Bottom,
                .Height = Me.closeButton.Height + 16
            }
            Me.closeButton.Location = New Point(buttonPanel.Width - Me.closeButton.Width, 8)
            Me.closeButton.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            buttonPanel.Controls.Add(Me.closeButton)

            Me.Controls.Add(Me.referenceTextBox)
            Me.Controls.Add(buttonPanel)

            Me.AcceptButton = Me.closeButton
            Me.CancelButton = Me.closeButton
            Me.AutoScaleDimensions = New SizeF(6.0F, 13.0F)
            Me.AutoScaleMode = AutoScaleMode.Font
            Me.ClientSize = New Size(700, 600)
            Me.MinimumSize = New Size(400, 300)
            Me.MinimizeBox = False
            Me.MaximizeBox = True
            Me.Padding = New Padding(12, 12, 12, 4)
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.StartPosition = FormStartPosition.CenterParent
            Me.Text = "Command-Line Reference"

            Dim title As String = $"{My.Application.Info.Title} v{My.Application.Info.Version} ({UtilApplication.ProcessArchitecture.ToString().ToUpper()})"
            Dim lines As New List(Of String)(CommandLineHelp.GetHeaderLines(title)) From {
                String.Empty,
                CommandLineHelp.GetUsageText()
            }
            Me.referenceTextBox.Text = String.Join(Environment.NewLine, lines)

            Me.ResumeLayout(False)
            Me.PerformLayout()
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)

            ' The keyboard focus starts on the button: a text box that receives the focus first selects all its text.
            Me.ActiveControl = Me.closeButton
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnShown(e As EventArgs)
            MyBase.OnShown(e)

            ' Start at the top of the text, with nothing selected.
            Me.referenceTextBox.SelectionStart = 0
            Me.referenceTextBox.SelectionLength = 0
        End Sub

#End Region

    End Class

End Namespace
