namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct tagFORMATETC
    {
        [ComAliasName("MsHtmHstInterop.wireCLIPFORMAT"), ComConversionLoss]
        public IntPtr cfFormat;
        [ComConversionLoss]
        public IntPtr ptd;
        public uint dwAspect;
        public int lindex;
        public uint tymed;
    }
}

