namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("B722BCCB-4E68-101B-A2BC-00AA00404770"), InterfaceType((short) 1)]
    public interface IOleCommandTarget
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void QueryStatus([In] ref Guid pguidCmdGroup, [In] uint cCmds, [In, Out] ref _tagOLECMD prgCmds, [In, Out] ref _tagOLECMDTEXT pCmdText);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Exec([In] ref Guid pguidCmdGroup, [In] uint nCmdID, [In] uint nCmdexecopt, [In, MarshalAs(UnmanagedType.Struct)] ref object pvaIn, [In, Out, MarshalAs(UnmanagedType.Struct)] ref object pvaOut);
    }
}

