namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct _DOCHOSTUIINFO
    {
        public uint cbSize;
        public uint dwFlags;
        public uint dwDoubleClick;
        [ComConversionLoss]
        public IntPtr pchHostCss;
        [ComConversionLoss]
        public IntPtr pchHostNS;
    }
}

