namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct _FLAGGED_BYTE_BLOB
    {
        public uint fFlags;
        public uint clSize;
        [ComConversionLoss]
        public IntPtr abData;
    }
}

