namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("C4D244B0-D43E-11CF-893B-00AA00BDCE1A"), InterfaceType((short) 1)]
    public interface IDocHostShowUI
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ShowMessage([In, ComAliasName("MsHtmHstInterop.wireHWND")] ref _RemotableHandle hwnd, [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrText, [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrCaption, [In] uint dwType, [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrHelpFile, [In] uint dwHelpContext, [ComAliasName("MsHtmHstInterop.LONG_PTR")] out int plResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ShowHelp([In, ComAliasName("MsHtmHstInterop.wireHWND")] ref _RemotableHandle hwnd, [In, MarshalAs(UnmanagedType.LPWStr)] string pszHelpFile, [In] uint uCommand, [In] uint dwData, [In] tagPOINT ptMouse, [Out, MarshalAs(UnmanagedType.IDispatch)] object pDispatchObjectHit);
    }
}

