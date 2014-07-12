namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _STGMEDIUM_UNION
    {
        public uint tymed;
        public __MIDL_IAdviseSink_0003 u;
    }
}

