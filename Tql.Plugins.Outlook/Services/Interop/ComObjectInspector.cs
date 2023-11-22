using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Tql.Plugins.Outlook.Services.Interop;

internal static class ComObjectInspector
{
    public static Guid? GetTypeId(dynamic comObject)
    {
        if (comObject is IDispatch dispatch)
        {
            dispatch.GetTypeInfo(0, 0, out var typeInfo);
            typeInfo.GetTypeAttr(out IntPtr attributes);

            var typeAttr = Marshal.PtrToStructure<TYPEATTR>(attributes);

            return typeAttr.guid;
        }

        return null;
    }
}
