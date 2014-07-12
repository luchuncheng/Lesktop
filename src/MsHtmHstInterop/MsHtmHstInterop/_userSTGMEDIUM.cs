namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _userSTGMEDIUM
    {
        public _STGMEDIUM_UNION __MIDL_0003;
        [MarshalAs(UnmanagedType.IUnknown)]
        public object pUnkForRelease;
    }
}

