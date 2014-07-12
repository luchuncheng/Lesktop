namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _userFLAG_STGMEDIUM
    {
        public int ContextFlags;
        public int fPassOwnership;
        public _userSTGMEDIUM Stgmed;
    }
}

