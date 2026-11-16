Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Theming

    ''' <summary>
    ''' Defines a control that knows how to render itself using a <see cref="ThemePalette"/>.
    ''' <para></para>
    ''' <see cref="ThemeManager.ApplyTheme(Windows.Forms.Control)"/> does not descend into the child controls
    ''' of an <see cref="IThemeable"/> control: the control is fully responsible for its own subtree.
    ''' </summary>
    Public Interface IThemeable

        ''' <summary>
        ''' Applies the specified palette to this control and its private child controls.
        ''' </summary>
        ''' <param name="palette">The palette to apply.</param>
        Sub ApplyTheme(palette As ThemePalette)

    End Interface

End Namespace
