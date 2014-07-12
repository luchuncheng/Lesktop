namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct _tagOLECMDTEXT
    {
        public uint cmdtextf;
        public uint cwActual;
        public uint cwBuf;
        [ComConversionLoss]
        public IntPtr rgwz;
    }
}

