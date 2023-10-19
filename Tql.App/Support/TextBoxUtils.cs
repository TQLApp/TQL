using System.Globalization;
using System.Windows.Controls.Primitives;

namespace Tql.App.Support;

internal static class TextBoxUtils
{
    public static void ConfigureAsNumericOnly(
        this TextBoxBase self,
        NumberStyles numberStyles,
        bool required = true
    )
    {
        self.PreviewTextInput += (s, e) =>
        {
            e.Handled = !IsValidInput(e.Text);
        };

        DataObject.AddPastingHandler(
            self,
            (s, e) =>
            {
                if (e.DataObject.GetDataPresent(typeof(string)))
                {
                    var text = e.DataObject.GetData(typeof(string)) as string;

                    if (!IsValidInput(text))
                        e.CancelCommand();
                }
                else
                {
                    e.CancelCommand();
                }
            }
        );

        bool IsValidInput(string? input) =>
            input != null && int.TryParse(input, numberStyles, CultureInfo.CurrentCulture, out _);
    }
}
