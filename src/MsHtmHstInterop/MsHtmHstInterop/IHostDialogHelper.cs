namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("53DEC138-A51E-11D2-861E-00C04FA35C89")]
    public interface IHostDialogHelper
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ShowHTMLDialog([ComAliasName("MsHtmHstInterop.wireHWND")] ref _RemotableHandle hwndParent, [MarshalAs(UnmanagedType.Interface)] IMoniker pmk, [MarshalAs(UnmanagedType.Struct)] ref object pvarArgIn, ref ushort pchOptions, [MarshalAs(UnmanagedType.Struct)] ref object pvarArgOut, [MarshalAs(UnmanagedType.IUnknown)] object punkHost);
    }
}

