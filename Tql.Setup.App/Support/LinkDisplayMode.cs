namespace Tql.Setup.App.Support;

[CLSCompliant(false)]
public enum LinkDisplayMode : uint
{
    Normal = NativeMethods.EShowWindowFlags.SW_NORMAL,
    Minimized = NativeMethods.EShowWindowFlags.SW_SHOWMINNOACTIVE,
    Maximized = NativeMethods.EShowWindowFlags.SW_MAXIMIZE
}
