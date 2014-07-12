namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("00000117-0000-0000-C000-000000000046")]
    public interface IOleInPlaceActiveObject : IOleWindow
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OnFrameWindowActivate([In] int fActivate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void OnDocWindowActivate([In] int fActivate);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteResizeBorder([In] ref tagRECT prcBorder, [In] ref Guid riid, [In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceUIWindow pUIWindow, [In] int fFrameWindow);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnableModeless([In] int fEnable);
    }
}

