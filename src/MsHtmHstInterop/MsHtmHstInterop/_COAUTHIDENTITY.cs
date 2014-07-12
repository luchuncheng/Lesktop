namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct _COAUTHIDENTITY
    {
        [ComConversionLoss]
        public IntPtr User;
        public uint UserLength;
        [ComConversionLoss]
        public IntPtr Domain;
        public uint DomainLength;
        [ComConversionLoss]
        public IntPtr Password;
        public uint PasswordLength;
        public uint Flags;
    }
}

