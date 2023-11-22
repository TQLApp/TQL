using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Tql.App.Support;
using Tql.Utilities;

namespace Tql.App.Services;

internal class ShowOnScreenManager
{
    public static ShowOnScreenManager Create(string? settings)
    {
        var configurations = new List<Configuration>();

        if (settings != null)
        {
            foreach (var item in settings.Split(';').Select(p => p.Trim()).Where(p => !p.IsEmpty()))
            {
                var match = Regex.Match(item, @"(.*):(\d+)");
                if (!match.Success)
                    continue;

                configurations.Add(
                    new Configuration(
                        match.Groups[1].Value,
                        int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture)
                    )
                );
            }
        }

        return new ShowOnScreenManager(configurations);
    }

    private readonly List<Configuration> _configurations;

    private ShowOnScreenManager(List<Configuration> configurations)
    {
        _configurations = configurations;
    }

    public int GetScreenIndex()
    {
        var screenLayout = GetScreenLayout();

        foreach (var configuration in _configurations)
        {
            if (configuration.ScreenLayout == screenLayout)
                return configuration.Screen;
        }

        return GetPrimaryScreenIndex();
    }

    public int GetPrimaryScreenIndex()
    {
        return GetAllScreens().ToList().IndexOf(Screen.PrimaryScreen!);
    }

    public Screen GetScreen()
    {
        var screenIndex = GetScreenIndex();

        return GetAllScreens().Skip(screenIndex).FirstOrDefault() ?? Screen.PrimaryScreen!;
    }

    public void SetScreen(Screen? screen)
    {
        if (screen != null && Equals(screen, Screen.PrimaryScreen))
            screen = null;

        var screenIndex = default(int?);
        if (screen != null)
            screenIndex = GetAllScreens().ToList().IndexOf(screen);

        SetScreenIndex(screenIndex);
    }

    public void SetScreenIndex(int? screenIndex)
    {
        var screenLayout = GetScreenLayout();

        for (var i = 0; i < _configurations.Count; i++)
        {
            var configuration = _configurations[i];
            if (configuration.ScreenLayout == screenLayout)
            {
                if (screenIndex.HasValue)
                    _configurations[i] = new Configuration(screenLayout, screenIndex.Value);
                else
                    _configurations.RemoveAt(i);
                return;
            }
        }

        if (screenIndex.HasValue)
            _configurations.Add(new Configuration(screenLayout, screenIndex.Value));
    }

    public override string ToString()
    {
        var sb = StringBuilderCache.Acquire();

        foreach (var configuration in _configurations)
        {
            if (sb.Length > 0)
                sb.Append(';');
            sb.Append(configuration.ScreenLayout)
                .Append(':')
                .Append(configuration.Screen.ToString(CultureInfo.InvariantCulture));
        }

        return StringBuilderCache.GetStringAndRelease(sb);
    }

    private string GetScreenLayout()
    {
        var sb = StringBuilderCache.Acquire();

        foreach (var screen in GetAllScreens())
        {
            if (sb.Length > 0)
                sb.Append(',');

            var bounds = screen.Bounds;
            sb.Append(bounds.Y.ToString(CultureInfo.InvariantCulture))
                .Append('x')
                .Append(bounds.X.ToString(CultureInfo.InvariantCulture))
                .Append('-')
                .Append(bounds.Width.ToString(CultureInfo.InvariantCulture))
                .Append('x')
                .Append(bounds.Height.ToString(CultureInfo.InvariantCulture));
        }

        return StringBuilderCache.GetStringAndRelease(sb);
    }

    private IEnumerable<Screen> GetAllScreens()
    {
        return Screen.AllScreens.OrderBy(p => p.Bounds.X).ThenBy(p => p.Bounds.Y);
    }

    private record Configuration(string ScreenLayout, int Screen);
}
