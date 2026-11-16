Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Controls

    ''' <summary>
    ''' Specifies the visual state of a <see cref="ThemedProgressBar"/>, like the Windows taskbar progress states.
    ''' </summary>
    Public Enum ProgressBarState As Integer

        ''' <summary>
        ''' Normal progress (accent color).
        ''' </summary>
        Normal = 0

        ''' <summary>
        ''' Progress with warnings, or paused/cancelled (yellow).
        ''' </summary>
        Warning = 1

        ''' <summary>
        ''' Progress with errors (red).
        ''' </summary>
        [Error] = 2

    End Enum

End Namespace
