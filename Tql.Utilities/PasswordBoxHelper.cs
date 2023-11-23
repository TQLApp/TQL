namespace Tql.Utilities;

/// <summary>
/// Attached property to allow binding to the <see cref="PasswordBox"/> password property.
/// </summary>
public static class PasswordBoxHelper
{
    /// <summary>
    /// Attached property for BoundPassword.
    /// </summary>
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(string.Empty, OnBoundPasswordChanged, CoerceBoundPassword)
        );

    private static object CoerceBoundPassword(DependencyObject d, object value)
    {
        var passwordBox = (PasswordBox)d;

        passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
        passwordBox.PasswordChanged += PasswordBox_PasswordChanged;

        return value;
    }

    /// <summary>
    /// Gets the value for the <see cref="BoundPasswordProperty"/> property.
    /// </summary>
    /// <param name="d">Dependency object to get the value from.</param>
    /// <returns>Value of the <see cref="BoundPasswordProperty"/> property.</returns>
    public static string GetBoundPassword(DependencyObject d)
    {
        return (string)d.GetValue(BoundPasswordProperty);
    }

    /// <summary>
    /// Sets the value for the <see cref="BoundPasswordProperty"/> property.
    /// </summary>
    /// <param name="d">Dependency object to get the value from.</param>
    /// <param name="value">Value of the <see cref="BoundPasswordProperty"/> property.</param>
    public static void SetBoundPassword(DependencyObject d, string value)
    {
        if (GetBoundPassword(d) != value)
            d.SetValue(BoundPasswordProperty, value);
    }

    private static void OnBoundPasswordChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var passwordBox = ((PasswordBox)d);

        if (passwordBox.Password != (string)e.NewValue)
            passwordBox.Password = (string)e.NewValue;
    }

    private static void PasswordBox_PasswordChanged(object? sender, RoutedEventArgs e)
    {
        var passwordBox = (PasswordBox)sender!;

        SetBoundPassword(passwordBox, passwordBox.Password);
    }
}
