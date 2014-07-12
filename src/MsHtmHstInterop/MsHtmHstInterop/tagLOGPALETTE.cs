namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2), ComConversionLoss]
    public struct tagLOGPALETTE
    {
        public ushort palVersion;
        public ushort palNumEntries;
        [ComConversionLoss]
        public IntPtr palPalEntry;
    }
}

