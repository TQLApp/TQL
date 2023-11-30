using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Tql.Plugins.Outlook.Services.Interop;

[
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("00020400-0000-0000-C000-000000000046")
]
internal interface IDispatch
{
    void GetTypeInfoCount(out uint pctinfo);
    void GetTypeInfo(uint iTInfo, uint lcid, out ITypeInfo ppTInfo);
    void GetIDsOfNames(ref Guid riid, string[] rgszNames, uint cNames, uint lcid, int[] rgDispId);
    void Invoke(
        int dispIdMember,
        ref Guid riid,
        uint lcid,
        ushort wFlags,
        ref DISPPARAMS pDispParams,
        out object pVarResult,
        ref EXCEPINFO pExcepInfo,
        out uint puArgErr
    );
}
