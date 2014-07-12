namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("00000116-0000-0000-C000-000000000046"), InterfaceType((short) 1)]
    public interface IOleInPlaceFrame : IOleInPlaceUIWindow
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetMenu([In, ComAliasName("MsHtmHstInterop.wireHMENU")] ref _RemotableHandle hmenuShared, [In, ComAliasName("MsHtmHstInterop.wireHGLOBAL")] ref _userHGLOBAL holemenu, [In, ComAliasName("MsHtmHstInterop.wireHWND")] ref _RemotableHandle hwndActiveObject);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void SetStatusText([In, MarshalAs(UnmanagedType.LPWStr)] string pszStatusText);
       [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void TranslateAccelerator([In] ref tagMSG lpmsg, [In] ushort wID);
    }
}

