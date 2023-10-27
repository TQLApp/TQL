using System.Collections.ObjectModel;

namespace Tql.App.Themes
{
    public static class ThemesController
    {
        public static ThemeType CurrentTheme { get; set; }

        private static ResourceDictionary ThemeDictionary
        {
            get => Application.Current.Resources.MergedDictionaries[0];
            set => Application.Current.Resources.MergedDictionaries[0] = value;
        }

        private static ResourceDictionary ControlColours
        {
            get => Application.Current.Resources.MergedDictionaries[1];
            set => Application.Current.Resources.MergedDictionaries[1] = value;
        }

        private static void RefreshControls()
        {
            // This seems to be faster than reloading the whole file, and it also seems to work
            Collection<ResourceDictionary> merged = Application
                .Current
                .Resources
                .MergedDictionaries;
            ResourceDictionary dictionary = merged[2];
            merged.RemoveAt(2);
            merged.Insert(2, dictionary);

            // If the above doesn't work then fall back to this
            // Application.Current.Resources.MergedDictionaries[2] = new ResourceDictionary() { Source = new Uri("Themes/Controls.xaml", UriKind.Relative) };

            // Doesn't work
            // FieldInfo field = typeof(PropertyMetadata).GetField("_defaultValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
            // object style = merged[2][typeof(ComboBox)];
            // field.SetValue(DataGridComboBoxColumn.ElementStyleProperty.DefaultMetadata, style);
        }

        public static void SetTheme(ThemeType theme)
        {
            if (CurrentTheme == theme || theme == ThemeType.None)
                return;

            string themeName = theme.GetName();
            if (string.IsNullOrEmpty(themeName))
            {
                return;
            }

            CurrentTheme = theme;
            ThemeDictionary = new ResourceDictionary()
            {
                Source = new Uri(
                    $"pack://application:,,,/Tql.App;component/Themes/ColourDictionaries/{themeName}.xaml"
                )
            };
            ControlColours = new ResourceDictionary()
            {
                Source = new Uri(
                    "pack://application:,,,/Tql.App;component/Themes/ControlColours.xaml"
                )
            };
            RefreshControls();
        }

        public static object GetResource(object key)
        {
            return ThemeDictionary[key];
        }

        public static SolidColorBrush GetBrush(string name)
        {
            return GetResource(name) as SolidColorBrush ?? new SolidColorBrush(Colors.White);
        }
    }
}
