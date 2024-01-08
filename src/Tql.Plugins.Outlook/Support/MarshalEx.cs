using Windows.Win32;

namespace Tql.Plugins.Outlook.Support;

// Taken from https://stackoverflow.com/questions/58010510.

internal static class MarshalEx
{
    public static object GetActiveObject(string progID)
    {
        Guid clsid;

        try
        {
            PInvoke.CLSIDFromProgIDEx(progID, out clsid).ThrowOnFailure();
        }
        catch (Exception)
        {
            PInvoke.CLSIDFromProgID(progID, out clsid).ThrowOnFailure();
        }

        unsafe
        {
            PInvoke.GetActiveObject(in clsid, null, out var obj).ThrowOnFailure();

            return obj;
        }
    }
}
