namespace Woof.Windows.Controls;

/// <summary>
/// Windows-style spinner with optional percent display.
/// </summary>
public partial class Spinner : UserControl {

    #region API

    /// <summary>
    /// Shows or hides the spinner, used to bind spinner to boolean view model property.
    /// </summary>
    public bool IsOn {
        get => (bool)GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    /// <summary>
    /// Gets or sets percentage value, set -1 to hide percent display.
    /// </summary>
    public double Percent {
        get => (double)GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    /// <summary>
    /// Initializes spinner.
    /// </summary>
    public Spinner() {
        InitializeComponent();
        StartAnimation();
        SuspendAnimation();
    }

    #endregion

    #region Dependency properties

    private static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
        nameof(IsOn), typeof(bool), typeof(Spinner), new PropertyMetadata(false, OnIsOnChanged)
    );

    private static readonly DependencyProperty PercentProperty = DependencyProperty.Register(
        nameof(Percent), typeof(double), typeof(Spinner), new PropertyMetadata(0d, OnPercentChanged)
    );

    private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var value = (bool)e.NewValue;
        if (value != (bool)e.OldValue && d is Spinner spinner) {
            spinner.PercentBox.Visibility = Visibility.Hidden;
            spinner.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        }
    }

    private static void OnPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var value = (double)e.NewValue;
        if (value == (double)e.OldValue) return;
        var spinner = (Spinner)d;
        if (value < 0) spinner.Visibility = spinner.IsOn ? Visibility.Visible : Visibility.Hidden;
        else {
            spinner.Visibility = Visibility.Visible;
            spinner.PercentAmount.Text = value.ToString();
        }
    }

    #endregion

    #region Animation control

    /// <summary>
    /// Dot animation.
    /// </summary>
    Storyboard A0 => _A0 ??= (Storyboard)FindResource("A0");

    /// <summary>
    /// Dot animation.
    /// </summary>
    Storyboard A1 => _A1 ??= (Storyboard)FindResource("A1");

    /// <summary>
    /// Dot animation.
    /// </summary>
    Storyboard A2 => _A2 ??= (Storyboard)FindResource("A2");

    /// <summary>
    /// Dot animation.
    /// </summary>
    Storyboard A3 => _A3 ??= (Storyboard)FindResource("A3");

    /// <summary>
    /// Dot animation.
    /// </summary>
    Storyboard A4 => _A4 ??= (Storyboard)FindResource("A4");

    /// <summary>
    /// Starts dots animation.
    /// </summary>
    void StartAnimation() {
        A0.Begin(this, true);
        A1.Begin(this, true);
        A2.Begin(this, true);
        A3.Begin(this, true);
        A4.Begin(this, true);
    }

    /// <summary>
    /// Suspends dots animation.
    /// </summary>
    void SuspendAnimation() {
        A0.Pause(this);
        A1.Pause(this);
        A2.Pause(this);
        A3.Pause(this);
        A4.Pause(this);
    }

    /// <summary>
    /// Resumes dots animation.
    /// </summary>
    void ResumeAnimation() {
        A0.Resume(this);
        A1.Resume(this);
        A2.Resume(this);
        A3.Resume(this);
        A4.Resume(this);
    }

    /// <summary>
    /// Suspends animation when the control is not visible.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
        if (IsInitialized) {
            if (e.Property.GlobalIndex == _IsVisiblePropertyGlobalIndex) {
                bool value = (bool)e.NewValue;
                if (value) ResumeAnimation(); else SuspendAnimation();
            }
            if (e.Property.GlobalIndex == _VisibilityPropertyGlobalIndex) {
                Visibility value = (Visibility)e.NewValue;
                if (value == Visibility.Visible) ResumeAnimation(); else SuspendAnimation();
            }
        }
        base.OnPropertyChanged(e);
    }

    #endregion

    #region Private data

    Storyboard? _A0;
    Storyboard? _A1;
    Storyboard? _A2;
    Storyboard? _A3;
    Storyboard? _A4;
    static readonly int _IsVisiblePropertyGlobalIndex = IsVisibleProperty.GlobalIndex;
    static readonly int _VisibilityPropertyGlobalIndex = VisibilityProperty.GlobalIndex;

    #endregion

}
