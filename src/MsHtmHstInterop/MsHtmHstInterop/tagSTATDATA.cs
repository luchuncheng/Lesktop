namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct tagSTATDATA
    {
        public tagFORMATETC formatetc;
        public uint advf;
        [MarshalAs(UnmanagedType.Interface)]
        public IAdviseSink pAdvSink;
        public uint dwConnection;
    }
}

