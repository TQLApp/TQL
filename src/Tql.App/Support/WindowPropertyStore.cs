using System.Runtime.InteropServices;
using Tql.App.Interop;

namespace Tql.App.Support;

internal class WindowPropertyStore : IDisposable
{
    private NativeMethods.IPropertyStore? _propertyStore;
    private bool _disposed;

    public WindowPropertyStore(IntPtr handle)
    {
        var guid = new Guid(NativeMethods.IID_IPropertyStore);

        NativeMethods.SHGetPropertyStoreForWindow(handle, ref guid, out _propertyStore);
    }

    ~WindowPropertyStore()
    {
        Dispose(false);
    }

    public string GetValue(PropertyStoreProperty property)
    {
        using (var propertyValue = new NativeMethods.PROPVARIANT())
        {
            _propertyStore!.GetValue(NativeMethods.GetPkey(property), propertyValue);

            return propertyValue.GetValue();
        }
    }

    public void SetValue(PropertyStoreProperty property, string value)
    {
        using (var propertyValue = new NativeMethods.PROPVARIANT())
        {
            propertyValue.SetValue(value);

            _propertyStore!.SetValue(NativeMethods.GetPkey(property), propertyValue);
        }
    }

    public void SetValue(PropertyStoreProperty property, bool value)
    {
        using (var propertyValue = new NativeMethods.PROPVARIANT())
        {
            propertyValue.SetValue(value);

            _propertyStore!.SetValue(NativeMethods.GetPkey(property), propertyValue);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_propertyStore != null)
            {
                Marshal.ReleaseComObject(_propertyStore);
                _propertyStore = null;
            }

            _disposed = true;
        }
    }
}

internal enum PropertyStoreProperty
{
    Title,
    AppUserModel_ID,
    AppUserModel_IsDestListSeparator,
    AppUserModel_RelaunchCommand,
    AppUserModel_RelaunchDisplayNameResource,
    AppUserModel_RelaunchIconResource,
    AppUserModel_PreventPinning
}
