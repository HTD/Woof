namespace Woof.Windows.Controls;

/// <summary>
/// Text box with "water-marked" label inside.
/// </summary>
public class TextBoxEx : TextBox {

    /// <summary>
    /// Label property definition.
    /// </summary>
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        "Label",
        typeof(string),
        typeof(TextBoxEx),
        new PropertyMetadata(new PropertyChangedCallback(OnLabelChanged))
    );

    /// <summary>
    /// Actual text property definition.
    /// </summary>
    public new static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        "Text",
        typeof(string),
        typeof(TextBoxEx),
        new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnOverridenTextChanged)
    );

    /// <summary>
    /// Gets or sets the text displayed as a label when the content is empty.
    /// </summary>
    public string Label {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets the text contents of the text box.
    /// </summary>
    public new string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Creates the extended text box element.
    /// </summary>
    public TextBoxEx() => ForegroundDefault = Foreground;

    /// <summary>
    /// Removes the label and graying if the control gets focus.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnGotFocus(RoutedEventArgs e) {
        ResetLabel();
        base.OnGotFocus(e);
    }

    /// <summary>
    /// Displays the grayed labal when the control loses focus.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnLostFocus(RoutedEventArgs e) {
        SetLabel(Label);
        base.OnLostFocus(e);
    }

    /// <summary>
    /// Invoked whenever the base text property changes.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnTextChanged(TextChangedEventArgs e) {
        base.OnTextChanged(e);
        if (!IsBaseTextPropagationDisabled) Text = base.Text;
    }

    /// <summary>
    /// Invoked whenever overriden text property changes.
    /// </summary>
    /// <param name="d">This <see cref="TextBoxEx"/> as <see cref="DependencyObject"/>.</param>
    /// <param name="e">Value being changed.</param>
    private static void OnOverridenTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is TextBoxEx textBoxEx && e.NewValue is string value) textBoxEx.SetBaseText(value);
    }

    /// <summary>
    /// Invoked whenever label property changes.
    /// </summary>
    /// <param name="d">This <see cref="TextBoxEx"/> as <see cref="DependencyObject"/>.</param>
    /// <param name="e">Value being changed.</param>
    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is TextBoxEx textBoxEx && e.NewValue is string value) textBoxEx.SetLabel(value);
    }

    /// <summary>
    /// Sets the base text when the text propagation is not disabled.
    /// </summary>
    /// <param name="value">Overriden text value.</param>
    private void SetBaseText(string value) {
        if (value == null) return;
        IsBaseTextPropagationDisabled = true;
        base.Text = value;
        IsBaseTextPropagationDisabled = false;
    }

    /// <summary>
    /// Sets the visual label.
    /// </summary>
    /// <param name="value">Label text.</param>
    private void SetLabel(string value) {
        if (string.IsNullOrEmpty(Text)) {
            Foreground = SystemColors.InactiveCaptionBrush;
            SetBaseText(value);
        }
    }

    /// <summary>
    /// Resets (removes) the visual label.
    /// </summary>
    private void ResetLabel() {
        Foreground = ForegroundDefault;
        if (base.Text == Label) SetBaseText(string.Empty);
    }

    /// <summary>
    /// Default foreground brush.
    /// </summary>
    private readonly Brush ForegroundDefault;

    /// <summary>
    /// If set true base text is not propagated to overriden text.
    /// </summary>
    private bool IsBaseTextPropagationDisabled;

}
