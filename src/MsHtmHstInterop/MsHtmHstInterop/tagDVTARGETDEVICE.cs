namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct tagDVTARGETDEVICE
    {
        public uint tdSize;
        public ushort tdDriverNameOffset;
        public ushort tdDeviceNameOffset;
        public ushort tdPortNameOffset;
        public ushort tdExtDevmodeOffset;
        [ComConversionLoss]
        public IntPtr tdData;
    }
}

