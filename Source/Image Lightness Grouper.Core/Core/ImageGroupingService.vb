Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

Imports ImageLightnessGrouper.Infrastructure

#End Region

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' UI-agnostic service that scans a directory for images and groups them by their average lightness (CIE L*).
    ''' <para></para>
    ''' Each image is moved into <c>&lt;its directory&gt;\&lt;output folder&gt;\&lt;range folder&gt;</c>, so when the
    ''' subdirectories are included, every directory gets its own output folder and images of different directories
    ''' are never mixed. The output folders themselves are never scanned, so running the tool twice is harmless.
    ''' <para></para>
    ''' All the heavy work runs on a thread-pool thread (<c>Task.Run</c>) and is parallelized with <c>Parallel.ForEach</c>,
    ''' with a degree of parallelism limited by the free memory (see <see cref="ParallelismCalculator"/>).
    ''' Progress is reported through <see cref="IProgress(Of T)"/>, so callers never block.
    ''' </summary>
    Public NotInheritable Class ImageGroupingService

#Region " Private Types "

        ''' <summary>
        ''' Thread-safe mutable counters shared by all the parallel iterations of a single run.
        ''' </summary>
        Private NotInheritable Class RunCounters

            ''' <summary>Number of files processed so far (any outcome).</summary>
            Public Processed As Integer

            ''' <summary>Number of moved files.</summary>
            Public Moved As Integer

            ''' <summary>Number of files not moved because of a name conflict.</summary>
            Public Conflicts As Integer

            ''' <summary>Number of failed files.</summary>
            Public Failed As Integer

        End Class

#End Region

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' The fixed width of the log tags (e.g. <c>"[CONFLICT]"</c>), for column alignment.
        ''' </summary>
        Private Const TagWidth As Integer = 11

        ''' <summary>
        ''' <c>E_INVALIDARG</c>, the HRESULT of the <see cref="ArgumentException"/> thrown by GDI+ for corrupt or unsupported images.
        ''' </summary>
        Private Const InvalidImageHResult As Integer = &H80070057

        ''' <summary>
        ''' The culture used for numeric formatting in log messages (thousands separators, etc).
        ''' </summary>
        Private Shared ReadOnly FormatCulture As CultureInfo = CultureInfo.GetCultureInfo("en-US")

        ''' <summary>
        ''' Gets the available physical memory, in gigabytes, for the given process bitness.
        ''' </summary>
        Private ReadOnly availableMemoryProvider As Func(Of Boolean, Double)

        ''' <summary>
        ''' Whether the current process is a 32-bit process.
        ''' </summary>
        Private ReadOnly isX86Process As Boolean

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ImageGroupingService"/> class.
        ''' </summary>
        Public Sub New()
            Me.New(AddressOf ParallelismCalculator.GetAvailableMemoryGB, IntPtr.Size = 4)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ImageGroupingService"/> class with a custom memory source
        ''' and process bitness (for testing).
        ''' </summary>
        ''' <param name="availableMemoryProvider">Gets the available physical memory, in gigabytes, for the given bitness.</param>
        ''' <param name="isX86Process">Whether to behave as a 32-bit process.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="availableMemoryProvider"/> is <see langword="Nothing"/>.</exception>
        Friend Sub New(availableMemoryProvider As Func(Of Boolean, Double), isX86Process As Boolean)
            If availableMemoryProvider Is Nothing Then
                Throw New ArgumentNullException(NameOf(availableMemoryProvider))
            End If

            Me.availableMemoryProvider = availableMemoryProvider
            Me.isX86Process = isX86Process
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Asynchronously runs the grouping operation on a background thread.
        ''' </summary>
        ''' <param name="options">The options that drive this run.</param>
        ''' <param name="progress">
        ''' The progress sink that receives log messages and counters.
        ''' <para></para>
        ''' It is invoked concurrently from several worker threads at a very high rate (once per file),
        ''' so it must be thread-safe and must not marshal every call to the UI thread.
        ''' </param>
        ''' <param name="cancellationToken">A token to cooperatively cancel the operation.</param>
        ''' <returns>
        ''' A task that completes with an <see cref="ImageGroupingSummary"/>, or faults with
        ''' <see cref="OperationCanceledException"/> when <paramref name="cancellationToken"/> is cancelled.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="progress"/> is <see langword="Nothing"/>.</exception>
        Public Function RunAsync(options As ImageGroupingOptions,
                                 progress As IProgress(Of ImageGroupingProgress),
                                 cancellationToken As CancellationToken) As Task(Of ImageGroupingSummary)

            If options Is Nothing Then
                Throw New ArgumentNullException(NameOf(options))
            End If

            If progress Is Nothing Then
                Throw New ArgumentNullException(NameOf(progress))
            End If

            Return Task.Run(Function() Me.Run(options, progress, cancellationToken), cancellationToken)
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Synchronous core of <see cref="RunAsync"/>. Must never be called from the UI thread.
        ''' </summary>
        ''' <param name="options">The options that drive this run.</param>
        ''' <param name="progress">The progress sink.</param>
        ''' <param name="cancellationToken">The cancellation token.</param>
        ''' <returns>The final run statistics.</returns>
        ''' <exception cref="DirectoryNotFoundException">Thrown when the target directory does not exist.</exception>
        ''' <exception cref="InvalidOperationException">Thrown when the target directory is a folder of grouped images or is inside one.</exception>
        ''' <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        Private Function Run(options As ImageGroupingOptions,
                             progress As IProgress(Of ImageGroupingProgress),
                             cancellationToken As CancellationToken) As ImageGroupingSummary

            Dim stopwatch As Stopwatch = Stopwatch.StartNew()

            If Not Directory.Exists(options.TargetDirectory) Then
                Throw New DirectoryNotFoundException($"The target directory does not exist: {options.TargetDirectory}")
            End If

            ImageGroupingService.EnsureNotInsideGroupedFolder(options)

            progress.Report(New ImageGroupingProgress(0, 0, $"Scanning '{options.TargetDirectory}' (include subdirectories: {options.SearchRecursively})...", LogSeverity.Information))
            progress.Report(New ImageGroupingProgress(0, 0, $"Interval: {options.StepPercentage}% | Alpha threshold: {options.AlphaThreshold} | Output folder: '{ImageGroupingOptions.OutputFolderName}'", LogSeverity.Information))

            Dim imageFiles As List(Of FileInfo) = ImageGroupingService.EnumerateImageFiles(options, progress, cancellationToken)
            Dim totalFiles As Integer = imageFiles.Count

            If totalFiles = 0 Then
                progress.Report(New ImageGroupingProgress(0, 0, "[INFO] No supported image files were found in the target directory.", LogSeverity.Warning))
                stopwatch.Stop()
                Return New ImageGroupingSummary(0, 0, 0, 0, 0, stopwatch.Elapsed)
            End If

            Dim totalFilesFormatted As String = totalFiles.ToString("N0", ImageGroupingService.FormatCulture)
            progress.Report(New ImageGroupingProgress(0, totalFiles, $"Found {totalFilesFormatted} image file(s).", LogSeverity.Information))

            Dim degreeOfParallelism As Integer = Me.ResolveDegreeOfParallelism(options, progress)

            Dim rootDirectoryWithSeparator As String = ImageGroupingService.NormalizeRootDirectory(options.TargetDirectory)
            Dim counters As New RunCounters()
            Dim parallelOptions As New ParallelOptions() With {
                .CancellationToken = cancellationToken,
                .MaxDegreeOfParallelism = degreeOfParallelism
            }

            ' When the token is cancelled, Parallel.ForEach stops scheduling new iterations
            ' and throws OperationCanceledException once the in-flight iterations finish.
            Parallel.ForEach(imageFiles, parallelOptions,
                             Sub(imageFile As FileInfo)
                                 ' Prevent ContextSwitchDeadlock in long-running/concurrent COM-bound contexts.
                                 Thread.CurrentThread.Join(0)

                                 Dim relativePath As String = ImageGroupingService.GetRelativePath(rootDirectoryWithSeparator, imageFile.FullName)
                                 Dim message As String
                                 Dim severity As LogSeverity

                                 Try
                                     Dim averageLightness As Double = ImageLightnessReader.ComputeAverageLightness(imageFile.FullName, options.AlphaThreshold)
                                     Dim lightnessText As String = $"L*={averageLightness.ToString("F1", ImageGroupingService.FormatCulture)}"
                                     Dim rangeFolderName As String = LightnessCalculator.GetFolderName(averageLightness, options.StepPercentage)
                                     Dim relativeDestination As String = Path.Combine(ImageGroupingOptions.OutputFolderName, rangeFolderName)

                                     If ImageGroupingService.MoveToRangeFolder(imageFile, ImageGroupingOptions.OutputFolderName, rangeFolderName) Then
                                         Interlocked.Increment(counters.Moved)
                                         message = $"{FormatTag("MOVED")}'{relativePath}' ({lightnessText}) -> {relativeDestination}"
                                         severity = LogSeverity.Moved
                                     Else
                                         Interlocked.Increment(counters.Conflicts)
                                         message = $"{FormatTag("CONFLICT")}'{relativePath}' ({lightnessText}) was not moved: a file with the same name already exists in '{relativeDestination}'."
                                         severity = LogSeverity.Warning
                                     End If

                                 Catch ex As ArgumentException When ex.HResult = InvalidImageHResult
                                     ' GDI+ reports corrupt and unsupported images as a generic "Parameter is not valid".
                                     Interlocked.Increment(counters.Failed)
                                     message = $"{FormatTag("ERROR")}'{relativePath}' -> corrupt file or unsupported image format (the file extension may not match its content)."
                                     severity = LogSeverity.Error

                                 Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                                     ' A single corrupt/locked image must not abort the whole batch:
                                     ' the failure is explicitly counted and reported, never swallowed.
                                     Interlocked.Increment(counters.Failed)
                                     message = $"{FormatTag("ERROR")}'{relativePath}' -> {ex.GetType().Name}: {ex.Message}"
                                     severity = LogSeverity.Error

                                 Finally
                                     If Me.isX86Process Then
                                         ' A 32-bit address space fragments quickly with big bitmaps; reclaim it after every image.
                                         GC.Collect()
                                         GC.WaitForPendingFinalizers()
                                     End If
                                 End Try

                                 Dim currentIndex As Integer = Interlocked.Increment(counters.Processed)
                                 Dim currentIndexFormatted As String = currentIndex.ToString("N0", ImageGroupingService.FormatCulture).PadLeft(totalFilesFormatted.Length, " "c)
                                 progress.Report(New ImageGroupingProgress(currentIndex, totalFiles, $"[{currentIndexFormatted}/{totalFilesFormatted}] {message}", severity))
                             End Sub)

            stopwatch.Stop()

            Return New ImageGroupingSummary(totalFiles, counters.Moved, counters.Conflicts, counters.Failed, degreeOfParallelism, stopwatch.Elapsed)
        End Function

        ''' <summary>
        ''' Resolves and reports the number of images processed in parallel.
        ''' </summary>
        ''' <param name="options">The options that drive this run.</param>
        ''' <param name="progress">The progress sink.</param>
        ''' <returns>The degree of parallelism.</returns>
        Private Function ResolveDegreeOfParallelism(options As ImageGroupingOptions, progress As IProgress(Of ImageGroupingProgress)) As Integer
            Dim processorCount As Integer = Environment.ProcessorCount
            Dim architecture As String = If(Me.isX86Process, "x86", "x64")

            If options.MaxDegreeOfParallelism <> ParallelismCalculator.Automatic AndAlso Not Me.isX86Process Then
                progress.Report(New ImageGroupingProgress(0, 0, $"Process: {architecture} | Processors: {processorCount} | Parallel images: {options.MaxDegreeOfParallelism} (user defined)", LogSeverity.Information))
                Return options.MaxDegreeOfParallelism
            End If

            Dim availableMemoryGB As Double = Me.availableMemoryProvider(Me.isX86Process)
            Dim degreeOfParallelism As Integer = ParallelismCalculator.CalculateAutomatic(availableMemoryGB, ParallelismCalculator.MemoryRequiredPerThreadGB, processorCount, Me.isX86Process)
            Dim memoryText As String = availableMemoryGB.ToString("F2", ImageGroupingService.FormatCulture)

            progress.Report(New ImageGroupingProgress(0, 0, $"Process: {architecture} | Processors: {processorCount} | Available memory: {memoryText} GB | Parallel images: {degreeOfParallelism} (automatic)", LogSeverity.Information))

            If Me.isX86Process Then
                Dim ignoredRequest As String = If(options.MaxDegreeOfParallelism = ParallelismCalculator.Automatic, String.Empty, $" (the requested {options.MaxDegreeOfParallelism} is ignored)")
                progress.Report(New ImageGroupingProgress(0, 0, $"[WARNING] A 32-bit process decodes a single image at a time{ignoredRequest}, to avoid running out of memory. Expect a longer processing time.", LogSeverity.Warning))
            End If

            Return degreeOfParallelism
        End Function

        ''' <summary>
        ''' Moves an image into <c>&lt;its directory&gt;\&lt;output folder&gt;\&lt;range folder&gt;</c>.
        ''' </summary>
        ''' <param name="imageFile">The image file.</param>
        ''' <param name="outputFolderName">The output folder name.</param>
        ''' <param name="rangeFolderName">The lightness range folder name.</param>
        ''' <returns>
        ''' <see langword="True"/> when the file was moved; <see langword="False"/> when a file with the same name
        ''' already exists in the destination folder (the file is left in place).
        ''' </returns>
        ''' <exception cref="FileNotFoundException">Thrown when the file no longer exists.</exception>
        Private Shared Function MoveToRangeFolder(imageFile As FileInfo, outputFolderName As String, rangeFolderName As String) As Boolean
            imageFile.Refresh()
            If Not imageFile.Exists Then
                Throw New FileNotFoundException($"The file does not exist or was moved: {imageFile.FullName}", imageFile.FullName)
            End If

            Dim destinationDirectory As String = Path.Combine(imageFile.DirectoryName, outputFolderName, rangeFolderName)

            ' CreateDirectory is idempotent and safe when several threads target the same folder.
            Directory.CreateDirectory(destinationDirectory)

            Dim destinationFilePath As String = Path.Combine(destinationDirectory, imageFile.Name)
            If File.Exists(destinationFilePath) Then
                Return False
            End If

            File.Move(imageFile.FullName, destinationFilePath)
            Return True
        End Function

        ''' <summary>
        ''' Locates all the supported image files, strictly filtering by exact extension matches.
        ''' <para></para>
        ''' The directory tree is walked iteratively (never recursively). A single inaccessible subdirectory
        ''' (e.g. "System Volume Information" when a drive root is selected) does not abort the whole operation: it is
        ''' reported as a warning and skipped. Junctions and symbolic links are reported and not followed, which prevents
        ''' infinite loops and processing the same files twice. Output folders (from previous runs) are never scanned.
        ''' </summary>
        ''' <param name="options">The options that drive this run.</param>
        ''' <param name="progress">The progress sink, used to report skipped directories.</param>
        ''' <param name="cancellationToken">The cancellation token.</param>
        ''' <returns>
        ''' A materialized list of files (so that subsequent moves cannot disturb the enumeration),
        ''' sorted in natural order by full path for a deterministic processing order.
        ''' </returns>
        ''' <exception cref="UnauthorizedAccessException">Thrown when the target directory itself is not accessible.</exception>
        Private Shared Function EnumerateImageFiles(options As ImageGroupingOptions,
                                                    progress As IProgress(Of ImageGroupingProgress),
                                                    cancellationToken As CancellationToken) As List(Of FileInfo)

            Dim allowedExtensions As New HashSet(Of String)(options.AllowedExtensions, StringComparer.OrdinalIgnoreCase)
            Dim rootDirectory As New DirectoryInfo(options.TargetDirectory)
            Dim rootDirectoryWithSeparator As String = ImageGroupingService.NormalizeRootDirectory(options.TargetDirectory)
            Dim result As New List(Of FileInfo)()
            Dim pendingDirectories As New Stack(Of DirectoryInfo)()
            pendingDirectories.Push(rootDirectory)

            While pendingDirectories.Count > 0
                ' Prevent ContextSwitchDeadlock in long-running/concurrent COM-bound contexts.
                Thread.CurrentThread.Join(0)
                cancellationToken.ThrowIfCancellationRequested()

                Dim currentDirectory As DirectoryInfo = pendingDirectories.Pop()
                Dim isRoot As Boolean = currentDirectory Is rootDirectory
                Dim files As FileInfo()
                Dim subdirectories As DirectoryInfo()

                Try
                    files = currentDirectory.GetFiles("*", SearchOption.TopDirectoryOnly)
                    subdirectories = If(options.SearchRecursively,
                                        currentDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly),
                                        Array.Empty(Of DirectoryInfo)())

                Catch ex As UnauthorizedAccessException When Not isRoot
                    Dim relativePath As String = ImageGroupingService.GetRelativePath(rootDirectoryWithSeparator, currentDirectory.FullName)
                    progress.Report(New ImageGroupingProgress(0, 0, $"[WARNING] Skipped inaccessible directory '{relativePath}': {ex.Message}", LogSeverity.Warning))
                    Continue While
                End Try

                For Each fileInfo As FileInfo In files
                    ' Strictly filter by extension to avoid Windows 8.3 legacy pattern matching anomalies (e.g., matching .jpg2).
                    If allowedExtensions.Contains(fileInfo.Extension) Then
                        result.Add(fileInfo)
                    End If
                Next

                For Each subdirectory As DirectoryInfo In subdirectories
                    ' The output folders and the range folders hold the images grouped by previous runs: they must never be
                    ' grouped again. The range folders are checked too, because the output folder may have been renamed.
                    If ImageGroupingService.IsGroupedFolderName(subdirectory.Name) Then
                        Dim relativePath As String = ImageGroupingService.GetRelativePath(rootDirectoryWithSeparator, subdirectory.FullName)
                        progress.Report(New ImageGroupingProgress(0, 0, $"[SKIPPED] Already grouped folder not scanned: '{relativePath}'.", LogSeverity.Skipped))
                        Continue For
                    End If

                    If (subdirectory.Attributes And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then
                        Dim relativePath As String = ImageGroupingService.GetRelativePath(rootDirectoryWithSeparator, subdirectory.FullName)
                        progress.Report(New ImageGroupingProgress(0, 0, $"[SKIPPED] Junction or symbolic link not followed: '{relativePath}'.", LogSeverity.Skipped))
                        Continue For
                    End If

                    pendingDirectories.Push(subdirectory)
                Next
            End While

            result.Sort(Function(left As FileInfo, right As FileInfo) NaturalStringComparer.Instance.Compare(left.FullName, right.FullName))
            Return result
        End Function

        ''' <summary>
        ''' Ensures that the target directory is not a folder of grouped images (an output folder or a range folder), nor
        ''' inside one: its images were already grouped, and grouping them again would nest the folders
        ''' (e.g. <c>"&lt;output folder&gt;\&lt;range folder&gt;\&lt;output folder&gt;"</c>).
        ''' </summary>
        ''' <param name="options">The options that drive this run.</param>
        ''' <exception cref="InvalidOperationException">Thrown when the target directory is a grouped folder or is inside one.</exception>
        Private Shared Sub EnsureNotInsideGroupedFolder(options As ImageGroupingOptions)
            Dim currentDirectory As DirectoryInfo = New DirectoryInfo(options.TargetDirectory)
            While currentDirectory IsNot Nothing
                If ImageGroupingService.IsGroupedFolderName(currentDirectory.Name) Then
                    Throw New InvalidOperationException($"The target directory is (or is inside) a folder of already grouped images ('{currentDirectory.Name}'): {options.TargetDirectory}" &
                                                        $"{Environment.NewLine}Select the directory that contains the '{ImageGroupingOptions.OutputFolderName}' folder instead.")
                End If

                currentDirectory = currentDirectory.Parent
            End While
        End Sub

        ''' <summary>
        ''' Determines whether a folder holds images grouped by a previous run: the output folder
        ''' (<see cref="ImageGroupingOptions.OutputFolderName"/>) or a lightness range folder (<see cref="LightnessCalculator.IsRangeFolderName"/>).
        ''' </summary>
        ''' <param name="folderName">The folder name (not a path).</param>
        ''' <returns><see langword="True"/> for a folder of grouped images; otherwise, <see langword="False"/>.</returns>
        Friend Shared Function IsGroupedFolderName(folderName As String) As Boolean
            Return String.Equals(folderName, ImageGroupingOptions.OutputFolderName, StringComparison.OrdinalIgnoreCase) OrElse
                   LightnessCalculator.IsRangeFolderName(folderName)
        End Function

        ''' <summary>
        ''' Formats a log tag padded to a fixed width (e.g. <c>"[MOVED]    "</c>).
        ''' </summary>
        ''' <param name="tag">The tag text, without brackets.</param>
        ''' <returns>The padded tag.</returns>
        Private Shared Function FormatTag(tag As String) As String
            Return $"[{tag}]".PadRight(TagWidth)
        End Function

        ''' <summary>
        ''' Normalizes the root directory to an absolute path that ends with a directory separator.
        ''' </summary>
        ''' <param name="directoryPath">The directory path.</param>
        ''' <returns>The normalized directory path.</returns>
        Private Shared Function NormalizeRootDirectory(directoryPath As String) As String
            Dim fullPath As String = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            Return $"{fullPath}{Path.DirectorySeparatorChar}"
        End Function

        ''' <summary>
        ''' Gets the path of a file relative to the root directory, for compact log output.
        ''' </summary>
        ''' <param name="rootDirectoryWithSeparator">The normalized root directory (see <see cref="NormalizeRootDirectory"/>).</param>
        ''' <param name="fullPath">The full file path.</param>
        ''' <returns>The relative path.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when the file is not located under the root directory.</exception>
        Private Shared Function GetRelativePath(rootDirectoryWithSeparator As String, fullPath As String) As String
            If Not fullPath.StartsWith(rootDirectoryWithSeparator, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException($"The file '{fullPath}' is not located under '{rootDirectoryWithSeparator}'.")
            End If

            Return fullPath.Substring(rootDirectoryWithSeparator.Length)
        End Function

#End Region

    End Class

End Namespace
