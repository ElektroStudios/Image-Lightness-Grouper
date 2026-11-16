Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.Concurrent
Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.Text
Imports System.Windows.Forms

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Theming
Imports ImageLightnessGrouper.Win32

#End Region

Namespace ImageLightnessGrouper.Controls

    ''' <summary>
    ''' A themed, read-only "virtual console" built on a border-less <see cref="RichTextBox"/>.
    ''' <para></para>
    ''' Key features:
    ''' <list type="bullet">
    ''' <item><description><see cref="AppendLine"/> is thread-safe: entries are queued and flushed in batches on the UI thread
    ''' (as a single RTF block per batch), so thousands of lines per second never freeze the UI.</description></item>
    ''' <item><description>Auto-scroll to the last line, preserving any text selection the user is making.</description></item>
    ''' <item><description>The caret is visible and text can be freely selected and copied (Ctrl+C, context menu).</description></item>
    ''' <item><description>Entries keep their <see cref="LogSeverity"/>, so the whole log is re-colored when the theme changes.</description></item>
    ''' </list>
    ''' </summary>
    <ToolboxItem(True)>
    Public Class LogConsoleBox : Inherits UserControl : Implements IThemeable

#Region " Private Types "

        ''' <summary>
        ''' A single immutable log line.
        ''' </summary>
        Private NotInheritable Class LogEntry

            ''' <summary>
            ''' Gets the line text.
            ''' </summary>
            Public ReadOnly Property Text As String

            ''' <summary>
            ''' Gets the line severity.
            ''' </summary>
            Public ReadOnly Property Severity As LogSeverity

            ''' <summary>
            ''' Initializes a new instance of the <see cref="LogEntry"/> class.
            ''' </summary>
            ''' <param name="text">The line text.</param>
            ''' <param name="severity">The line severity.</param>
            Public Sub New(text As String, severity As LogSeverity)
                Me.Text = text
                Me.Severity = severity
            End Sub

        End Class

#End Region

#Region " Constants "

        ''' <summary>
        ''' The interval, in milliseconds, at which queued entries are flushed to the screen.
        ''' </summary>
        Private Const FlushIntervalMilliseconds As Integer = 60

#End Region

#Region " Fields "

        ''' <summary>
        ''' The hosted rich text box.
        ''' </summary>
        Private ReadOnly richTextBox As RichTextBox

        ''' <summary>
        ''' The context menu (Copy, Select All, Clear).
        ''' </summary>
        Private ReadOnly consoleContextMenu As ContextMenuStrip

        ''' <summary>
        ''' The "Copy" context menu item.
        ''' </summary>
        Private ReadOnly copyMenuItem As ToolStripMenuItem

        ''' <summary>
        ''' The UI-thread timer that flushes queued entries.
        ''' </summary>
        Private ReadOnly flushTimer As Timer

        ''' <summary>
        ''' Entries appended (from any thread) but not yet rendered.
        ''' </summary>
        Private ReadOnly pendingEntries As New ConcurrentQueue(Of LogEntry)()

        ''' <summary>
        ''' All rendered entries (UI thread only), kept to re-render on theme changes.
        ''' </summary>
        Private ReadOnly renderedEntries As New List(Of LogEntry)()

        ''' <summary>
        ''' The palette currently applied.
        ''' </summary>
        Private palette As ThemePalette = ThemeManager.CurrentPalette

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="LogConsoleBox"/> class.
        ''' </summary>
        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint Or
                        ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or
                        ControlStyles.ResizeRedraw, True)

            ' 1px padding leaves room for the custom-painted border.
            Me.Padding = New Padding(1)

            Me.richTextBox = New RichTextBox() With {
                .BorderStyle = BorderStyle.None,
                .Dock = DockStyle.Fill,
                .ReadOnly = True,
                .DetectUrls = False,
                .HideSelection = False,
                .WordWrap = False,
                .ScrollBars = RichTextBoxScrollBars.ForcedBoth,
                .ShortcutsEnabled = True,
                .TabStop = True,
                .Font = New Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point)
            }

            Me.copyMenuItem = New ToolStripMenuItem("&Copy", Nothing, AddressOf Me.CopyMenuItem_Click) With {
                .ShortcutKeyDisplayString = "Ctrl+C"
            }
            Dim selectAllMenuItem As New ToolStripMenuItem("Select &All", Nothing, AddressOf Me.SelectAllMenuItem_Click) With {
                .ShortcutKeyDisplayString = "Ctrl+A"
            }
            Dim clearMenuItem As New ToolStripMenuItem("C&lear", Nothing, AddressOf Me.ClearMenuItem_Click)

            Me.consoleContextMenu = New ContextMenuStrip()
            Me.consoleContextMenu.Items.AddRange(New ToolStripItem() {Me.copyMenuItem, selectAllMenuItem, New ToolStripSeparator(), clearMenuItem})
            AddHandler Me.consoleContextMenu.Opening, AddressOf Me.ConsoleContextMenu_Opening

            Me.richTextBox.ContextMenuStrip = Me.consoleContextMenu
            AddHandler Me.richTextBox.HandleCreated, AddressOf Me.RichTextBox_HandleCreated
            AddHandler Me.richTextBox.GotFocus, AddressOf Me.RichTextBox_FocusChanged
            AddHandler Me.richTextBox.LostFocus, AddressOf Me.RichTextBox_FocusChanged

            Me.Controls.Add(Me.richTextBox)

            Me.flushTimer = New Timer() With {.Interval = FlushIntervalMilliseconds}
            AddHandler Me.flushTimer.Tick, AddressOf Me.FlushTimer_Tick

            Me.ApplyTheme(Me.palette)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the number of rendered lines.
        ''' </summary>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property LineCount As Integer
            Get
                Return Me.renderedEntries.Count
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property DefaultSize As Size
            Get
                Return New Size(400, 200)
            End Get
        End Property

#End Region

#Region " IThemeable "

        ''' <inheritdoc/>
        Public Sub ApplyTheme(palette As ThemePalette) Implements IThemeable.ApplyTheme
            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            Me.palette = palette
            Me.BackColor = palette.ConsoleBackground
            Me.richTextBox.BackColor = palette.ConsoleBackground
            Me.richTextBox.ForeColor = palette.LogInformation
            ThemeManager.ApplyThemeToToolStrip(Me.consoleContextMenu, palette)

            If Me.richTextBox.IsHandleCreated AndAlso Not Me.DesignMode Then
                WindowChrome.ApplyScrollBarTheme(Me.richTextBox, palette.IsDark)
                Me.RenderAllEntries()
            End If

            Me.Invalidate()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Queues a line to be appended to the console. This method is thread-safe.
        ''' </summary>
        ''' <param name="text">The line text.</param>
        ''' <param name="severity">The line severity (drives its color).</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="severity"/> is not a defined value.</exception>
        Public Sub AppendLine(text As String, severity As LogSeverity)
            If text Is Nothing Then
                Throw New ArgumentNullException(NameOf(text))
            End If

            If Not [Enum].IsDefined(GetType(LogSeverity), severity) Then
                Throw New InvalidEnumArgumentException(NameOf(severity), CInt(severity), GetType(LogSeverity))
            End If

            Me.pendingEntries.Enqueue(New LogEntry(text, severity))
        End Sub

        ''' <summary>
        ''' Immediately renders every queued entry. Must be called from the UI thread.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when called from a non-UI thread.</exception>
        Public Sub Flush()
            Me.ThrowIfNotUiThread()

            If Me.pendingEntries.IsEmpty OrElse Not Me.richTextBox.IsHandleCreated Then
                Return
            End If

            Dim batch As New List(Of LogEntry)()
            Dim entry As LogEntry = Nothing
            While Me.pendingEntries.TryDequeue(entry)
                batch.Add(entry)
            End While

            Me.renderedEntries.AddRange(batch)
            Me.AppendEntriesToRichTextBox(batch)
        End Sub

        ''' <summary>
        ''' Clears the console, including queued entries. Must be called from the UI thread.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when called from a non-UI thread.</exception>
        Public Sub Clear()
            Me.ThrowIfNotUiThread()

            Dim discarded As LogEntry = Nothing
            While Me.pendingEntries.TryDequeue(discarded)
            End While

            Me.renderedEntries.Clear()
            Me.richTextBox.Clear()
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnHandleCreated(e As EventArgs)
            MyBase.OnHandleCreated(e)

            If Not Me.DesignMode Then
                Me.flushTimer.Start()
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
            Me.flushTimer.Stop()
            MyBase.OnHandleDestroyed(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.Clear(Me.BackColor)

            Dim borderColor As Color = If(Me.richTextBox.Focused, Me.palette.Accent, Me.palette.Border)
            Using borderPen As New Pen(borderColor, 1.0F)
                g.DrawRectangle(borderPen, 0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1)
            End Using
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub Dispose(disposing As Boolean)
            Try
                If disposing Then
                    Me.flushTimer.Dispose()
                    Me.consoleContextMenu.Dispose()

                    ' A control never disposes a Font assigned to it: this one was created (and is owned) by this control.
                    Me.richTextBox.Font.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Throws when the current thread is not the thread that owns this control.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when called from a non-UI thread.</exception>
        Private Sub ThrowIfNotUiThread()
            If Me.InvokeRequired Then
                Throw New InvalidOperationException($"This member of {NameOf(LogConsoleBox)} must be called from the UI thread.")
            End If
        End Sub

        ''' <summary>
        ''' Re-renders every stored entry with the current palette (used after a theme change).
        ''' </summary>
        Private Sub RenderAllEntries()
            Me.richTextBox.Clear()
            If Me.renderedEntries.Count > 0 Then
                Me.AppendEntriesToRichTextBox(Me.renderedEntries)
            End If
        End Sub

        ''' <summary>
        ''' Appends a batch of entries as a single RTF block at the end of the document,
        ''' preserving the user's selection and auto-scrolling to the last line.
        ''' </summary>
        ''' <param name="entries">The entries to append.</param>
        Private Sub AppendEntriesToRichTextBox(entries As IList(Of LogEntry))
            Dim rtf As String = Me.BuildRtf(entries, startsNewParagraph:=Me.richTextBox.TextLength > 0)

            Dim userSelectionStart As Integer = Me.richTextBox.SelectionStart
            Dim userSelectionLength As Integer = Me.richTextBox.SelectionLength
            Dim hasUserSelection As Boolean = userSelectionLength > 0

            Me.SetRedraw(False)
            Try
                Me.richTextBox.Select(Me.richTextBox.TextLength, 0)
                Me.richTextBox.SelectedRtf = rtf

                If hasUserSelection Then
                    Me.richTextBox.Select(userSelectionStart, userSelectionLength)
                Else
                    ' Park the caret at the START of the last line (not at the end of the text),
                    ' so the rich text box never auto-scrolls horizontally to follow it.
                    Dim lastLineIndex As Integer = Math.Max(0, Me.richTextBox.GetLineFromCharIndex(Me.richTextBox.TextLength))
                    Me.richTextBox.Select(Me.richTextBox.GetFirstCharIndexFromLine(lastLineIndex), 0)
                End If

                ' Scroll to the very bottom (the last line, fully visible) and back to the first column,
                ' without moving the caret/selection. The scroll bars are always present (ForcedBoth), so the
                ' client height cannot shrink afterwards (e.g. when a horizontal scroll bar would appear because
                ' of a long line) and cover the last line.
                NativeMethods.SendMessage(Me.richTextBox.Handle, WindowMessage.VScroll, New IntPtr(ScrollBarCommand.Bottom), IntPtr.Zero)
                NativeMethods.SendMessage(Me.richTextBox.Handle, WindowMessage.HScroll, New IntPtr(ScrollBarCommand.Left), IntPtr.Zero)
            Finally
                Me.SetRedraw(True)
            End Try

            ' Repaint the text AND the non-client area: after WM_SETREDRAW the scroll bars are not repainted
            ' until something else invalidates the frame (e.g. hovering them with the mouse).
            RedrawSuspension.RedrawEverything(Me.richTextBox)
        End Sub

        ''' <summary>
        ''' Enables or disables redrawing of the rich text box (<c>WM_SETREDRAW</c>).
        ''' </summary>
        ''' <param name="enabled"><see langword="True"/> to enable redrawing.</param>
        Private Sub SetRedraw(enabled As Boolean)
            NativeMethods.SendMessage(Me.richTextBox.Handle, WindowMessage.SetRedraw, New IntPtr(If(enabled, 1, 0)), IntPtr.Zero)
        End Sub

        ''' <summary>
        ''' Builds an RTF document fragment for the specified entries, using one color per <see cref="LogSeverity"/>.
        ''' </summary>
        ''' <param name="entries">The entries.</param>
        ''' <param name="startsNewParagraph">Whether the fragment must begin with a paragraph break.</param>
        ''' <returns>The RTF fragment.</returns>
        Private Function BuildRtf(entries As IList(Of LogEntry), startsNewParagraph As Boolean) As String
            Dim severities As LogSeverity() = DirectCast([Enum].GetValues(GetType(LogSeverity)), LogSeverity())
            Dim fontSizeHalfPoints As Integer = CInt(Math.Round(Me.richTextBox.Font.SizeInPoints * 2.0F))
            Dim builder As New StringBuilder(entries.Count * 96)

            builder.Append("{\rtf1\ansi\deff0{\fonttbl{\f0\fmodern ")
            LogConsoleBox.AppendEscapedRtfText(builder, Me.richTextBox.Font.Name)
            builder.Append(";}}{\colortbl ;")

            For Each severity As LogSeverity In severities
                Dim color As Color = Me.palette.GetLogColor(severity)
                builder.Append(String.Format(CultureInfo.InvariantCulture, "\red{0}\green{1}\blue{2};", color.R, color.G, color.B))
            Next

            builder.Append("}\f0")
            builder.Append(String.Format(CultureInfo.InvariantCulture, "\fs{0} ", fontSizeHalfPoints))

            For index As Integer = 0 To entries.Count - 1
                Dim entry As LogEntry = entries(index)

                If index > 0 OrElse startsNewParagraph Then
                    builder.Append("\par ")
                End If

                ' Color table index 0 is "auto", so severity N maps to index (position + 1).
                Dim colorIndex As Integer = Array.IndexOf(severities, entry.Severity) + 1
                builder.Append(String.Format(CultureInfo.InvariantCulture, "\cf{0} ", colorIndex))
                LogConsoleBox.AppendEscapedRtfText(builder, entry.Text)
            Next

            builder.Append("}")
            Return builder.ToString()
        End Function

        ''' <summary>
        ''' Appends text to an RTF builder, escaping control characters and non-ASCII characters (as <c>\uN?</c>).
        ''' </summary>
        ''' <param name="builder">The target builder.</param>
        ''' <param name="text">The plain text.</param>
        Friend Shared Sub AppendEscapedRtfText(builder As StringBuilder, text As String)
            If builder Is Nothing Then
                Throw New ArgumentNullException(NameOf(builder))
            End If

            If text Is Nothing Then
                Throw New ArgumentNullException(NameOf(text))
            End If

            For Each c As Char In text
                Select Case c
                    Case "\"c
                        builder.Append("\\")
                    Case "{"c
                        builder.Append("\{")
                    Case "}"c
                        builder.Append("\}")
                    Case ControlChars.Tab
                        builder.Append("\tab ")
                    Case ControlChars.Cr
                        ' Ignored: line feeds are rendered as soft line breaks below.
                    Case ControlChars.Lf
                        builder.Append("\line ")
                    Case Else
                        Dim code As Integer = AscW(c)
                        If code < 128 Then
                            builder.Append(c)
                        Else
                            ' RTF \u takes a signed 16-bit value, followed by an ANSI fallback character.
                            Dim signedCode As Short = CShort(If(code > Short.MaxValue, code - 65536, code))
                            builder.Append(String.Format(CultureInfo.InvariantCulture, "\u{0}?", signedCode))
                        End If
                End Select
            Next
        End Sub

#End Region

#Region " Event Handlers "

        ''' <summary>
        ''' Flushes queued entries on every timer tick.
        ''' </summary>
        Private Sub FlushTimer_Tick(sender As Object, e As EventArgs)
            Me.Flush()
        End Sub

        ''' <summary>
        ''' Applies themed scroll bars once the native rich text box exists.
        ''' </summary>
        Private Sub RichTextBox_HandleCreated(sender As Object, e As EventArgs)
            If Not Me.DesignMode Then
                WindowChrome.ApplyScrollBarTheme(Me.richTextBox, Me.palette.IsDark)
            End If
        End Sub

        ''' <summary>
        ''' Repaints the border when the rich text box gains or loses focus.
        ''' </summary>
        Private Sub RichTextBox_FocusChanged(sender As Object, e As EventArgs)
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Enables the "Copy" item only when there is selected text.
        ''' </summary>
        Private Sub ConsoleContextMenu_Opening(sender As Object, e As CancelEventArgs)
            Me.copyMenuItem.Enabled = Me.richTextBox.SelectionLength > 0
        End Sub

        ''' <summary>
        ''' Copies the selected text to the clipboard.
        ''' </summary>
        Private Sub CopyMenuItem_Click(sender As Object, e As EventArgs)
            Me.richTextBox.Copy()
        End Sub

        ''' <summary>
        ''' Selects all the console text.
        ''' </summary>
        Private Sub SelectAllMenuItem_Click(sender As Object, e As EventArgs)
            Me.richTextBox.Focus()
            Me.richTextBox.SelectAll()
        End Sub

        ''' <summary>
        ''' Clears the console.
        ''' </summary>
        Private Sub ClearMenuItem_Click(sender As Object, e As EventArgs)
            Me.Clear()
        End Sub

#End Region

    End Class

End Namespace
