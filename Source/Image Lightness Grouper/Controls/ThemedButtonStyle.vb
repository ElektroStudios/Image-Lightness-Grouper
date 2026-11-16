Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Controls

    ''' <summary>
    ''' Specifies the visual role of a <see cref="ThemedButton"/>.
    ''' </summary>
    Public Enum ThemedButtonStyle As Integer

        ''' <summary>
        ''' Neutral, secondary button.
        ''' </summary>
        Standard = 0

        ''' <summary>
        ''' Primary call-to-action button (accent colored).
        ''' </summary>
        Accent = 1

        ''' <summary>
        ''' Destructive or aborting action (red colored).
        ''' </summary>
        Danger = 2

    End Enum

End Namespace
