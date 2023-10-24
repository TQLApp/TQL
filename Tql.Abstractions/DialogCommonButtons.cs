namespace Tql.App.Services;

[Flags]
public enum DialogCommonButtons
{
    None = 0,
    OK = 1,
    Yes = 2,
    No = 4,
    Cancel = 8,
    Retry = 16,
    Close = 32,
}
