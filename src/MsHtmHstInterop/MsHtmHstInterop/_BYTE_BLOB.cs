namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct _BYTE_BLOB
    {
        public uint clSize;
        [ComConversionLoss]
        public IntPtr abData;
    }
}

