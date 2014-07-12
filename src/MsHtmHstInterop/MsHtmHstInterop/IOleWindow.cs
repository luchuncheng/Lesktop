namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, ComConversionLoss, Guid("00000114-0000-0000-C000-000000000046"), InterfaceType((short) 1)]
    public interface IOleWindow
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetWindow([Out, ComAliasName("MsHtmHstInterop.wireHWND")] IntPtr phwnd);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ContextSensitiveHelp([In] int fEnterMode);
    }
}

