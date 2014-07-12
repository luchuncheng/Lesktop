namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct _tagOLECMD
    {
        public uint cmdID;
        public uint cmdf;
    }
}

