Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Forms

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class MainForm
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
            Me.components = New System.ComponentModel.Container()
            Me.appMenuStrip = New System.Windows.Forms.MenuStrip()
            Me.fileMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.openDirectoryMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.fileMenuSeparator = New System.Windows.Forms.ToolStripSeparator()
            Me.exitMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.helpMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.aboutMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.themeToggleItem = New ImageLightnessGrouper.Controls.ToolStripToggleSwitch()
            Me.rootLayout = New System.Windows.Forms.TableLayoutPanel()
            Me.settingsLayout = New System.Windows.Forms.TableLayoutPanel()
            Me.directoryLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.directoryTextBox = New ImageLightnessGrouper.Controls.ThemedTextBox()
            Me.browseButton = New ImageLightnessGrouper.Controls.ThemedButton()
            Me.recursiveToggle = New ImageLightnessGrouper.Controls.ToggleSwitch()
            Me.stepLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.optionsLayout = New System.Windows.Forms.TableLayoutPanel()
            Me.stepNumericUpDown = New ImageLightnessGrouper.Controls.ThemedNumericUpDown()
            Me.alphaThresholdLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.alphaThresholdNumericUpDown = New ImageLightnessGrouper.Controls.ThemedNumericUpDown()
            Me.parallelismLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.parallelismNumericUpDown = New ImageLightnessGrouper.Controls.ThemedNumericUpDown()
            Me.executionLayout = New System.Windows.Forms.TableLayoutPanel()
            Me.startButton = New ImageLightnessGrouper.Controls.ThemedButton()
            Me.abortButton = New ImageLightnessGrouper.Controls.ThemedButton()
            Me.progressBar = New ImageLightnessGrouper.Controls.ThemedProgressBar()
            Me.progressLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.logLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.logConsole = New ImageLightnessGrouper.Controls.LogConsoleBox()
            Me.statusLabel = New ImageLightnessGrouper.Controls.ThemedLabel()
            Me.toolTip = New System.Windows.Forms.ToolTip(Me.components)
            Me.progressTimer = New System.Windows.Forms.Timer(Me.components)
            Me.appMenuStrip.SuspendLayout()
            Me.rootLayout.SuspendLayout()
            Me.settingsLayout.SuspendLayout()
            Me.optionsLayout.SuspendLayout()
            Me.executionLayout.SuspendLayout()
            Me.SuspendLayout()
            '
            'appMenuStrip
            '
            Me.appMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.fileMenuItem, Me.helpMenuItem, Me.themeToggleItem})
            Me.appMenuStrip.Location = New System.Drawing.Point(0, 0)
            Me.appMenuStrip.Name = "appMenuStrip"
            Me.appMenuStrip.Padding = New System.Windows.Forms.Padding(6, 3, 0, 3)
            Me.appMenuStrip.Size = New System.Drawing.Size(804, 28)
            Me.appMenuStrip.TabIndex = 0
            '
            'fileMenuItem
            '
            Me.fileMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.openDirectoryMenuItem, Me.fileMenuSeparator, Me.exitMenuItem})
            Me.fileMenuItem.Name = "fileMenuItem"
            Me.fileMenuItem.Size = New System.Drawing.Size(37, 22)
            Me.fileMenuItem.Text = "&File"
            '
            'openDirectoryMenuItem
            '
            Me.openDirectoryMenuItem.Name = "openDirectoryMenuItem"
            Me.openDirectoryMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.O), System.Windows.Forms.Keys)
            Me.openDirectoryMenuItem.Size = New System.Drawing.Size(206, 22)
            Me.openDirectoryMenuItem.Text = "&Open Directory..."
            '
            'fileMenuSeparator
            '
            Me.fileMenuSeparator.Name = "fileMenuSeparator"
            Me.fileMenuSeparator.Size = New System.Drawing.Size(203, 6)
            '
            'exitMenuItem
            '
            Me.exitMenuItem.Name = "exitMenuItem"
            Me.exitMenuItem.ShortcutKeyDisplayString = "Alt+F4"
            Me.exitMenuItem.Size = New System.Drawing.Size(206, 22)
            Me.exitMenuItem.Text = "E&xit"
            '
            'helpMenuItem
            '
            Me.helpMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.aboutMenuItem})
            Me.helpMenuItem.Name = "helpMenuItem"
            Me.helpMenuItem.Size = New System.Drawing.Size(44, 22)
            Me.helpMenuItem.Text = "&Help"
            '
            'aboutMenuItem
            '
            Me.aboutMenuItem.Name = "aboutMenuItem"
            Me.aboutMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1
            Me.aboutMenuItem.Size = New System.Drawing.Size(135, 22)
            Me.aboutMenuItem.Text = "&About..."
            '
            'themeToggleItem
            '
            Me.themeToggleItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
            Me.themeToggleItem.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.themeToggleItem.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.themeToggleItem.Name = "themeToggleItem"
            Me.themeToggleItem.Text = "Dark mode"
            Me.themeToggleItem.ToolTipText = "Switch between the dark and the light theme."
            '
            'rootLayout
            '
            Me.rootLayout.ColumnCount = 1
            Me.rootLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.rootLayout.Controls.Add(Me.settingsLayout, 0, 0)
            Me.rootLayout.Controls.Add(Me.executionLayout, 0, 1)
            Me.rootLayout.Controls.Add(Me.logLabel, 0, 2)
            Me.rootLayout.Controls.Add(Me.logConsole, 0, 3)
            Me.rootLayout.Controls.Add(Me.statusLabel, 0, 4)
            Me.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill
            Me.rootLayout.Location = New System.Drawing.Point(0, 28)
            Me.rootLayout.Name = "rootLayout"
            Me.rootLayout.Padding = New System.Windows.Forms.Padding(12, 10, 12, 8)
            Me.rootLayout.RowCount = 5
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.rootLayout.Size = New System.Drawing.Size(804, 473)
            Me.rootLayout.TabIndex = 1
            '
            'settingsLayout
            '
            Me.settingsLayout.AutoSize = True
            Me.settingsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.settingsLayout.ColumnCount = 3
            Me.settingsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.settingsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.settingsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.settingsLayout.Controls.Add(Me.directoryLabel, 0, 0)
            Me.settingsLayout.Controls.Add(Me.directoryTextBox, 1, 0)
            Me.settingsLayout.Controls.Add(Me.browseButton, 2, 0)
            Me.settingsLayout.Controls.Add(Me.recursiveToggle, 1, 1)
            Me.settingsLayout.Controls.Add(Me.stepLabel, 0, 2)
            Me.settingsLayout.Controls.Add(Me.optionsLayout, 1, 2)
            Me.settingsLayout.Dock = System.Windows.Forms.DockStyle.Fill
            Me.settingsLayout.Location = New System.Drawing.Point(12, 10)
            Me.settingsLayout.Margin = New System.Windows.Forms.Padding(0, 0, 0, 10)
            Me.settingsLayout.Name = "settingsLayout"
            Me.settingsLayout.RowCount = 3
            Me.settingsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
            Me.settingsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
            Me.settingsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
            Me.settingsLayout.Size = New System.Drawing.Size(780, 102)
            Me.settingsLayout.TabIndex = 0
            '
            'directoryLabel
            '
            Me.directoryLabel.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.directoryLabel.AutoSize = True
            Me.directoryLabel.Location = New System.Drawing.Point(3, 9)
            Me.directoryLabel.Name = "directoryLabel"
            Me.directoryLabel.Size = New System.Drawing.Size(92, 15)
            Me.directoryLabel.TabIndex = 0
            Me.directoryLabel.Text = "Target directory:"
            '
            'directoryTextBox
            '
            Me.directoryTextBox.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.directoryTextBox.BackColor = System.Drawing.Color.White
            Me.directoryTextBox.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.directoryTextBox.Location = New System.Drawing.Point(101, 3)
            Me.directoryTextBox.Name = "directoryTextBox"
            Me.directoryTextBox.ReadOnly = True
            Me.directoryTextBox.Size = New System.Drawing.Size(572, 28)
            Me.directoryTextBox.TabIndex = 1
            Me.toolTip.SetToolTip(Me.directoryTextBox, "Tip: you can also drag and drop a single folder" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "anywhere on this window.")
            '
            'browseButton
            '
            Me.browseButton.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.browseButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.browseButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.browseButton.Location = New System.Drawing.Point(679, 3)
            Me.browseButton.Name = "browseButton"
            Me.browseButton.Size = New System.Drawing.Size(98, 28)
            Me.browseButton.TabIndex = 2
            Me.browseButton.Text = "&Browse..."
            Me.toolTip.SetToolTip(Me.browseButton, "Select the directory that contains" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "the images to group (Ctrl+O).")
            Me.browseButton.UseVisualStyleBackColor = False
            '
            'recursiveToggle
            '
            Me.recursiveToggle.AccessibleName = "Include subdirectories"
            Me.recursiveToggle.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton
            Me.recursiveToggle.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.recursiveToggle.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.recursiveToggle.Cursor = System.Windows.Forms.Cursors.Hand
            Me.recursiveToggle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.recursiveToggle.Location = New System.Drawing.Point(101, 40)
            Me.recursiveToggle.Name = "recursiveToggle"
            Me.recursiveToggle.Size = New System.Drawing.Size(171, 20)
            Me.recursiveToggle.TabIndex = 3
            Me.recursiveToggle.Text = "Include subdirectories"
            Me.toolTip.SetToolTip(Me.recursiveToggle, "When enabled, the images inside all the" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "subdirectories are processed too, and each" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "directory gets its own output folder.")
            '
            'stepLabel
            '
            Me.stepLabel.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.stepLabel.AutoSize = True
            Me.stepLabel.Location = New System.Drawing.Point(3, 77)
            Me.stepLabel.Name = "stepLabel"
            Me.stepLabel.Size = New System.Drawing.Size(73, 15)
            Me.stepLabel.TabIndex = 4
            Me.stepLabel.Text = "Interval (%):"
            '
            'optionsLayout
            '
            Me.optionsLayout.AutoSize = True
            Me.optionsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.optionsLayout.ColumnCount = 6
            Me.settingsLayout.SetColumnSpan(Me.optionsLayout, 2)
            Me.optionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.optionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.optionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.optionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.optionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.optionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.optionsLayout.Controls.Add(Me.stepNumericUpDown, 0, 0)
            Me.optionsLayout.Controls.Add(Me.alphaThresholdLabel, 1, 0)
            Me.optionsLayout.Controls.Add(Me.alphaThresholdNumericUpDown, 2, 0)
            Me.optionsLayout.Controls.Add(Me.parallelismLabel, 3, 0)
            Me.optionsLayout.Controls.Add(Me.parallelismNumericUpDown, 4, 0)
            Me.optionsLayout.Dock = System.Windows.Forms.DockStyle.Fill
            Me.optionsLayout.Location = New System.Drawing.Point(98, 68)
            Me.optionsLayout.Margin = New System.Windows.Forms.Padding(0)
            Me.optionsLayout.Name = "optionsLayout"
            Me.optionsLayout.RowCount = 1
            Me.optionsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.optionsLayout.Size = New System.Drawing.Size(682, 34)
            Me.optionsLayout.TabIndex = 5
            '
            'stepNumericUpDown
            '
            Me.stepNumericUpDown.AccessibleName = "Interval (%)"
            Me.stepNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.stepNumericUpDown.Location = New System.Drawing.Point(3, 3)
            Me.stepNumericUpDown.Maximum = 10
            Me.stepNumericUpDown.Minimum = 1
            Me.stepNumericUpDown.Name = "stepNumericUpDown"
            Me.stepNumericUpDown.Size = New System.Drawing.Size(70, 28)
            Me.stepNumericUpDown.TabIndex = 0
            Me.stepNumericUpDown.Value = 10
            '
            'alphaThresholdLabel
            '
            Me.alphaThresholdLabel.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.alphaThresholdLabel.AutoSize = True
            Me.alphaThresholdLabel.Location = New System.Drawing.Point(96, 9)
            Me.alphaThresholdLabel.Margin = New System.Windows.Forms.Padding(20, 0, 3, 0)
            Me.alphaThresholdLabel.Name = "alphaThresholdLabel"
            Me.alphaThresholdLabel.Size = New System.Drawing.Size(97, 15)
            Me.alphaThresholdLabel.TabIndex = 1
            Me.alphaThresholdLabel.Text = "Alpha threshold:"
            Me.toolTip.SetToolTip(Me.alphaThresholdLabel, "Pixels whose alpha (opacity, 0-255) is below this" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "value are ignored when averaging the lightness," & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "so transparent areas do not count." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Default: 8.")
            '
            'alphaThresholdNumericUpDown
            '
            Me.alphaThresholdNumericUpDown.AccessibleName = "Alpha threshold"
            Me.alphaThresholdNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.alphaThresholdNumericUpDown.Location = New System.Drawing.Point(199, 3)
            Me.alphaThresholdNumericUpDown.Maximum = 255
            Me.alphaThresholdNumericUpDown.Name = "alphaThresholdNumericUpDown"
            Me.alphaThresholdNumericUpDown.Size = New System.Drawing.Size(70, 28)
            Me.alphaThresholdNumericUpDown.TabIndex = 2
            Me.toolTip.SetToolTip(Me.alphaThresholdNumericUpDown, "Pixels whose alpha (opacity, 0-255) is below this" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "value are ignored when averaging the lightness," & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "so transparent areas do not count." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Default: 8.")
            Me.alphaThresholdNumericUpDown.Value = 8
            '
            'parallelismLabel
            '
            Me.parallelismLabel.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.parallelismLabel.AutoSize = True
            Me.parallelismLabel.Location = New System.Drawing.Point(292, 9)
            Me.parallelismLabel.Margin = New System.Windows.Forms.Padding(20, 0, 3, 0)
            Me.parallelismLabel.Name = "parallelismLabel"
            Me.parallelismLabel.Size = New System.Drawing.Size(132, 15)
            Me.parallelismLabel.TabIndex = 3
            Me.parallelismLabel.Text = "Parallel images (0 = auto):"
            Me.toolTip.SetToolTip(Me.parallelismLabel, "Maximum number of images processed in parallel." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "0 = automatic: limited by the free memory" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(about 1 GB per image) and the processor count." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "32-bit processes always use a single thread." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Default: 0 (automatic).")
            '
            'parallelismNumericUpDown
            '
            Me.parallelismNumericUpDown.AccessibleName = "Parallel images"
            Me.parallelismNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.parallelismNumericUpDown.Location = New System.Drawing.Point(430, 3)
            Me.parallelismNumericUpDown.Maximum = 256
            Me.parallelismNumericUpDown.Name = "parallelismNumericUpDown"
            Me.parallelismNumericUpDown.Size = New System.Drawing.Size(70, 28)
            Me.parallelismNumericUpDown.TabIndex = 4
            Me.toolTip.SetToolTip(Me.parallelismNumericUpDown, "Maximum number of images processed in parallel." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "0 = automatic: limited by the free memory" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(about 1 GB per image) and the processor count." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "32-bit processes always use a single thread." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Default: 0 (automatic).")
            '
            'executionLayout
            '
            Me.executionLayout.AutoSize = True
            Me.executionLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.executionLayout.ColumnCount = 4
            Me.executionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.executionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.executionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.executionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.executionLayout.Controls.Add(Me.startButton, 0, 0)
            Me.executionLayout.Controls.Add(Me.abortButton, 1, 0)
            Me.executionLayout.Controls.Add(Me.progressBar, 2, 0)
            Me.executionLayout.Controls.Add(Me.progressLabel, 3, 0)
            Me.executionLayout.Dock = System.Windows.Forms.DockStyle.Fill
            Me.executionLayout.Location = New System.Drawing.Point(12, 122)
            Me.executionLayout.Margin = New System.Windows.Forms.Padding(0, 0, 0, 8)
            Me.executionLayout.Name = "executionLayout"
            Me.executionLayout.RowCount = 1
            Me.executionLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.executionLayout.Size = New System.Drawing.Size(780, 40)
            Me.executionLayout.TabIndex = 1
            '
            'startButton
            '
            Me.startButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.startButton.ButtonStyle = ImageLightnessGrouper.Controls.ThemedButtonStyle.Accent
            Me.startButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.startButton.Location = New System.Drawing.Point(3, 3)
            Me.startButton.Name = "startButton"
            Me.startButton.Size = New System.Drawing.Size(150, 34)
            Me.startButton.TabIndex = 0
            Me.startButton.Text = "&Start Processing"
            Me.toolTip.SetToolTip(Me.startButton, "Scan the target directory and group every image" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "by its average lightness.")
            Me.startButton.UseVisualStyleBackColor = False
            '
            'abortButton
            '
            Me.abortButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.abortButton.ButtonStyle = ImageLightnessGrouper.Controls.ThemedButtonStyle.Danger
            Me.abortButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.abortButton.Location = New System.Drawing.Point(159, 3)
            Me.abortButton.Name = "abortButton"
            Me.abortButton.Size = New System.Drawing.Size(130, 34)
            Me.abortButton.TabIndex = 1
            Me.abortButton.Text = "&Cancel / Abort"
            Me.toolTip.SetToolTip(Me.abortButton, "Stop the operation cleanly." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Files already moved stay where they are.")
            Me.abortButton.UseVisualStyleBackColor = False
            '
            'progressBar
            '
            Me.progressBar.AccessibleRole = System.Windows.Forms.AccessibleRole.ProgressBar
            Me.progressBar.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.progressBar.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.progressBar.Location = New System.Drawing.Point(307, 16)
            Me.progressBar.Margin = New System.Windows.Forms.Padding(15, 3, 9, 3)
            Me.progressBar.Name = "progressBar"
            Me.progressBar.Size = New System.Drawing.Size(370, 8)
            Me.progressBar.TabIndex = 2
            Me.progressBar.TabStop = False
            '
            'progressLabel
            '
            Me.progressLabel.Anchor = System.Windows.Forms.AnchorStyles.Right
            Me.progressLabel.AutoSize = True
            Me.progressLabel.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.progressLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer))
            Me.progressLabel.IsMuted = True
            Me.progressLabel.Location = New System.Drawing.Point(689, 12)
            Me.progressLabel.MinimumSize = New System.Drawing.Size(88, 0)
            Me.progressLabel.Name = "progressLabel"
            Me.progressLabel.Size = New System.Drawing.Size(88, 15)
            Me.progressLabel.TabIndex = 3
            Me.progressLabel.Text = "0 / 0"
            Me.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
            '
            'logLabel
            '
            Me.logLabel.AutoSize = True
            Me.logLabel.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.logLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer))
            Me.logLabel.IsMuted = True
            Me.logLabel.Location = New System.Drawing.Point(15, 170)
            Me.logLabel.Margin = New System.Windows.Forms.Padding(3, 0, 3, 4)
            Me.logLabel.Name = "logLabel"
            Me.logLabel.Size = New System.Drawing.Size(68, 15)
            Me.logLabel.TabIndex = 2
            Me.logLabel.Text = "Output log:"
            '
            'logConsole
            '
            Me.logConsole.BackColor = System.Drawing.Color.White
            Me.logConsole.Dock = System.Windows.Forms.DockStyle.Fill
            Me.logConsole.Location = New System.Drawing.Point(15, 192)
            Me.logConsole.Name = "logConsole"
            Me.logConsole.Padding = New System.Windows.Forms.Padding(1)
            Me.logConsole.Size = New System.Drawing.Size(774, 244)
            Me.logConsole.TabIndex = 3
            '
            'statusLabel
            '
            Me.statusLabel.AutoEllipsis = True
            Me.statusLabel.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill
            Me.statusLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer))
            Me.statusLabel.IsMuted = True
            Me.statusLabel.Location = New System.Drawing.Point(15, 445)
            Me.statusLabel.Margin = New System.Windows.Forms.Padding(3, 6, 3, 0)
            Me.statusLabel.Name = "statusLabel"
            Me.statusLabel.Size = New System.Drawing.Size(774, 20)
            Me.statusLabel.TabIndex = 4
            Me.statusLabel.Text = "Ready."
            '
            'progressTimer
            '
            'MainForm
            '
            Me.AllowDrop = True
            Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(804, 501)
            Me.Controls.Add(Me.rootLayout)
            Me.Controls.Add(Me.appMenuStrip)
            Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.MainMenuStrip = Me.appMenuStrip
            Me.MinimumSize = New System.Drawing.Size(820, 460)
            Me.Name = "MainForm"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "MainForm"
            Me.appMenuStrip.ResumeLayout(False)
            Me.appMenuStrip.PerformLayout()
            Me.rootLayout.ResumeLayout(False)
            Me.rootLayout.PerformLayout()
            Me.settingsLayout.ResumeLayout(False)
            Me.settingsLayout.PerformLayout()
            Me.optionsLayout.ResumeLayout(False)
            Me.optionsLayout.PerformLayout()
            Me.executionLayout.ResumeLayout(False)
            Me.executionLayout.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Friend WithEvents appMenuStrip As System.Windows.Forms.MenuStrip
        Friend WithEvents fileMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents openDirectoryMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents fileMenuSeparator As System.Windows.Forms.ToolStripSeparator
        Friend WithEvents exitMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents helpMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents aboutMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents themeToggleItem As ImageLightnessGrouper.Controls.ToolStripToggleSwitch
        Friend WithEvents rootLayout As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents settingsLayout As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents directoryLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents directoryTextBox As ImageLightnessGrouper.Controls.ThemedTextBox
        Friend WithEvents browseButton As ImageLightnessGrouper.Controls.ThemedButton
        Friend WithEvents recursiveToggle As ImageLightnessGrouper.Controls.ToggleSwitch
        Friend WithEvents stepLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents optionsLayout As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents stepNumericUpDown As ImageLightnessGrouper.Controls.ThemedNumericUpDown
        Friend WithEvents alphaThresholdLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents alphaThresholdNumericUpDown As ImageLightnessGrouper.Controls.ThemedNumericUpDown
        Friend WithEvents parallelismLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents parallelismNumericUpDown As ImageLightnessGrouper.Controls.ThemedNumericUpDown
        Friend WithEvents executionLayout As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents startButton As ImageLightnessGrouper.Controls.ThemedButton
        Friend WithEvents abortButton As ImageLightnessGrouper.Controls.ThemedButton
        Friend WithEvents progressBar As ImageLightnessGrouper.Controls.ThemedProgressBar
        Friend WithEvents progressLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents logLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents logConsole As ImageLightnessGrouper.Controls.LogConsoleBox
        Friend WithEvents statusLabel As ImageLightnessGrouper.Controls.ThemedLabel
        Friend WithEvents toolTip As System.Windows.Forms.ToolTip
        Friend WithEvents progressTimer As System.Windows.Forms.Timer

    End Class

End Namespace
