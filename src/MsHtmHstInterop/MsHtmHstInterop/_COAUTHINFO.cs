namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct _COAUTHINFO
    {
        public uint dwAuthnSvc;
        public uint dwAuthzSvc;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwszServerPrincName;
        public uint dwAuthnLevel;
        public uint dwImpersonationLevel;
        [ComConversionLoss]
        public IntPtr pAuthIdentityData;
        public uint dwCapabilities;
    }
}

