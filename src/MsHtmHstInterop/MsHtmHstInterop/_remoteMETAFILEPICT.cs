namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct _remoteMETAFILEPICT
    {
        public int mm;
        public int xExt;
        public int yExt;
        [ComConversionLoss]
        public IntPtr hMF;
    }
}

