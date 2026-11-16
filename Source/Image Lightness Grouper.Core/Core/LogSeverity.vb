Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Specifies the semantic severity (and therefore the visual style) of a log entry.
    ''' </summary>
    Public Enum LogSeverity As Integer

        ''' <summary>
        ''' Neutral informational message.
        ''' </summary>
        Information = 0

        ''' <summary>
        ''' Secondary, low-relevance message (banners, hints, configuration dumps).
        ''' </summary>
        Muted = 1

        ''' <summary>
        ''' A file was successfully moved to its target folder.
        ''' </summary>
        Moved = 2

        ''' <summary>
        ''' A file was intentionally skipped (e.g. it already resides in its target folder).
        ''' </summary>
        Skipped = 3

        ''' <summary>
        ''' The whole operation finished successfully.
        ''' </summary>
        Success = 4

        ''' <summary>
        ''' Non-fatal anomaly that deserves the user's attention.
        ''' </summary>
        Warning = 5

        ''' <summary>
        ''' An operation failed.
        ''' </summary>
        [Error] = 6

    End Enum

End Namespace
