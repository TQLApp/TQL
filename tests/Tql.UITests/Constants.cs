namespace Tql.UITests;

internal static class Constants
{
    public const string AppName = "Tql.App";
    public const string WinAppDriverEndpoint = "http://127.0.0.1:4723";
    public static readonly TimeSpan ResetTimeout = TimeSpan.FromSeconds(10);
    public const string DefaultLanguage = "en";
    public const string DefaultQuickStartState =
        "{\"Step\":\"Dismissed\",\"SelectedTool\":null,\"CompletedTools\":[]}";
    public const string DefaultEnvironment = "ManualTesting";

    // public const string DefaultPath = @"%LOCALAPPDATA%\Programs\TQL\Tql.App.exe";
    public const string DefaultPath =
        @"C:\Projects\Tql\src\Tql.App\bin\Debug\net8.0-windows\Tql.App.exe";

    public static readonly TimeSpan OpenMainWindowTimeout = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan OpenConfigurationWindowTimeout = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan CloseOpenWindowsTimeout = TimeSpan.FromSeconds(3);
}
