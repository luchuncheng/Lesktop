namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _GDI_OBJECT
    {
        public uint ObjectType;
        public __MIDL_IAdviseSink_0002 u;
    }
}

