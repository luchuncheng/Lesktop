namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct _COSERVERINFO
    {
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszName;
        [ComConversionLoss]
        public IntPtr pAuthInfo;
        public uint dwReserved2;
    }
}

