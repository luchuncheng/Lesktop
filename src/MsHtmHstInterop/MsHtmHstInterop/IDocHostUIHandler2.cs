namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F6D0-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType((short) 1)]
    public interface IDocHostUIHandler2 : IDocHostUIHandler
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetOverrideKeyPath([MarshalAs(UnmanagedType.LPWStr)] out string pchKey, [In] uint dw);
    }
}

