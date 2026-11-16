Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Forms

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class AboutBox
        Inherits ThemedForm

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.contentLayout = New System.Windows.Forms.TableLayoutPanel()
            Me.logoPictureBox = New System.Windows.Forms.PictureBox()
            Me.productNameLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.versionLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.copyrightLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.companyNameLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.descriptionTextBox = New ImageLightnessGrouper.Controls.ThemedTextBox()
            Me.okButton = New ImageLightnessGrouper.Controls.ThemedButton()
            Me.contentLayout.SuspendLayout()
            CType(Me.logoPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'contentLayout
            '
            Me.contentLayout.ColumnCount = 2
            Me.contentLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.0!))
            Me.contentLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.0!))
            Me.contentLayout.Controls.Add(Me.logoPictureBox, 0, 0)
            Me.contentLayout.Controls.Add(Me.productNameLabel, 1, 0)
            Me.contentLayout.Controls.Add(Me.versionLabel, 1, 1)
            Me.contentLayout.Controls.Add(Me.copyrightLabel, 1, 2)
            Me.contentLayout.Controls.Add(Me.companyNameLabel, 1, 3)
            Me.contentLayout.Controls.Add(Me.descriptionTextBox, 1, 4)
            Me.contentLayout.Controls.Add(Me.okButton, 1, 5)
            Me.contentLayout.Dock = System.Windows.Forms.DockStyle.Fill
            Me.contentLayout.Location = New System.Drawing.Point(12, 12)
            Me.contentLayout.Name = "contentLayout"
            Me.contentLayout.RowCount = 6
            Me.contentLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
            Me.contentLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
            Me.contentLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
            Me.contentLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
            Me.contentLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            Me.contentLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
            Me.contentLayout.Size = New System.Drawing.Size(496, 286)
            Me.contentLayout.TabIndex = 0
            '
            'logoPictureBox
            '
            Me.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.logoPictureBox.Location = New System.Drawing.Point(3, 3)
            Me.logoPictureBox.Name = "logoPictureBox"
            Me.contentLayout.SetRowSpan(Me.logoPictureBox, 6)
            Me.logoPictureBox.Size = New System.Drawing.Size(157, 280)
            Me.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
            Me.logoPictureBox.TabIndex = 0
            Me.logoPictureBox.TabStop = False
            '
            'productNameLabel
            '
            Me.productNameLabel.Dock = System.Windows.Forms.DockStyle.Fill
            Me.productNameLabel.Font = New System.Drawing.Font("Segoe UI Semibold", 11.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.productNameLabel.Location = New System.Drawing.Point(172, 0)
            Me.productNameLabel.Margin = New System.Windows.Forms.Padding(9, 0, 3, 0)
            Me.productNameLabel.Name = "productNameLabel"
            Me.productNameLabel.Size = New System.Drawing.Size(321, 28)
            Me.productNameLabel.TabIndex = 0
            Me.productNameLabel.Text = "Product Name"
            Me.productNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'versionLabel
            '
            Me.versionLabel.Dock = System.Windows.Forms.DockStyle.Fill
            Me.versionLabel.Location = New System.Drawing.Point(172, 28)
            Me.versionLabel.Margin = New System.Windows.Forms.Padding(9, 0, 3, 0)
            Me.versionLabel.Name = "versionLabel"
            Me.versionLabel.Size = New System.Drawing.Size(321, 28)
            Me.versionLabel.TabIndex = 1
            Me.versionLabel.Text = "Version"
            Me.versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'copyrightLabel
            '
            Me.copyrightLabel.Dock = System.Windows.Forms.DockStyle.Fill
            Me.copyrightLabel.IsMuted = True
            Me.copyrightLabel.Location = New System.Drawing.Point(172, 56)
            Me.copyrightLabel.Margin = New System.Windows.Forms.Padding(9, 0, 3, 0)
            Me.copyrightLabel.Name = "copyrightLabel"
            Me.copyrightLabel.Size = New System.Drawing.Size(321, 28)
            Me.copyrightLabel.TabIndex = 2
            Me.copyrightLabel.Text = "Copyright"
            Me.copyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'companyNameLabel
            '
            Me.companyNameLabel.Dock = System.Windows.Forms.DockStyle.Fill
            Me.companyNameLabel.IsMuted = True
            Me.companyNameLabel.Location = New System.Drawing.Point(172, 84)
            Me.companyNameLabel.Margin = New System.Windows.Forms.Padding(9, 0, 3, 0)
            Me.companyNameLabel.Name = "companyNameLabel"
            Me.companyNameLabel.Size = New System.Drawing.Size(321, 28)
            Me.companyNameLabel.TabIndex = 3
            Me.companyNameLabel.Text = "Company Name"
            Me.companyNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            '
            'descriptionTextBox
            '
            Me.descriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill
            Me.descriptionTextBox.Location = New System.Drawing.Point(172, 115)
            Me.descriptionTextBox.Margin = New System.Windows.Forms.Padding(9, 3, 3, 3)
            Me.descriptionTextBox.Multiline = True
            Me.descriptionTextBox.Name = "descriptionTextBox"
            Me.descriptionTextBox.ReadOnly = True
            Me.descriptionTextBox.Size = New System.Drawing.Size(321, 137)
            Me.descriptionTextBox.TabIndex = 4
            Me.descriptionTextBox.TabStop = False
            '
            'okButton
            '
            Me.okButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.okButton.ButtonStyle = ImageLightnessGrouper.Controls.ThemedButtonStyle.Accent
            Me.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.okButton.Location = New System.Drawing.Point(403, 258)
            Me.okButton.Name = "okButton"
            Me.okButton.Size = New System.Drawing.Size(90, 25)
            Me.okButton.TabIndex = 5
            Me.okButton.Text = "&OK"
            '
            'AboutBox
            '
            Me.AcceptButton = Me.okButton
            Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.CancelButton = Me.okButton
            Me.ClientSize = New System.Drawing.Size(520, 310)
            Me.Controls.Add(Me.contentLayout)
            Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "AboutBox"
            Me.Padding = New System.Windows.Forms.Padding(12)
            Me.ShowIcon = False
            Me.ShowInTaskbar = False
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
            Me.Text = "AboutBox"
            Me.contentLayout.ResumeLayout(False)
            CType(Me.logoPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub

        Friend WithEvents contentLayout As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents logoPictureBox As System.Windows.Forms.PictureBox
        Friend WithEvents productNameLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents versionLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents copyrightLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents companyNameLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents descriptionTextBox As ImageLightnessGrouper.Controls.ThemedTextBox
        Friend WithEvents okButton As ImageLightnessGrouper.Controls.ThemedButton

    End Class

End Namespace
