﻿using System.Windows.Controls.Primitives;

namespace Tql.App.Themes.Attached
{
    /// <summary>
    /// An attached property class for showing a hint when a text box is empty. Set the text box's Tag property to the text
    /// </summary>
    public static class TextHinting
    {
        public static readonly DependencyProperty ShowWhenFocusedProperty =
            DependencyProperty.RegisterAttached(
                "ShowWhenFocused",
                typeof(bool),
                typeof(TextHinting),
                new FrameworkPropertyMetadata(false)
            );

        public static void SetShowWhenFocused(Control control, bool value)
        {
            if (control is not (TextBoxBase or PasswordBox))
                throw new ArgumentException("Control was not a textbox", nameof(control));

            control.SetValue(ShowWhenFocusedProperty, value);
        }

        public static bool GetShowWhenFocused(Control control)
        {
            if (control is not (TextBoxBase or PasswordBox))
                throw new ArgumentException("Control was not a textbox", nameof(control));

            return (bool)control.GetValue(ShowWhenFocusedProperty);
        }

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.RegisterAttached("Foreground", typeof(Brush), typeof(TextHinting));

        public static void SetForeground(Control control, Brush value)
        {
            if (control is not (TextBoxBase or PasswordBox))
                throw new ArgumentException("Control was not a textbox", nameof(control));

            control.SetValue(ForegroundProperty, value);
        }

        public static Brush GetForeground(Control control)
        {
            if (control is not (TextBoxBase or PasswordBox))
                throw new ArgumentException("Control was not a textbox", nameof(control));

            return (Brush)control.GetValue(ForegroundProperty);
        }
    }
}
