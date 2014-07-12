namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4), ComConversionLoss]
    public struct tagMSG
    {
        [ComConversionLoss, ComAliasName("MsHtmHstInterop.wireHWND")]
        public IntPtr hwnd;
        public uint message;
        [ComAliasName("MsHtmHstInterop.UINT_PTR")]
        public uint wParam;
        [ComAliasName("MsHtmHstInterop.LONG_PTR")]
        public int lParam;
        public uint time;
        public tagPOINT pt;
    }
}

