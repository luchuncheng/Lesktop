namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("0000010F-0000-0000-C000-000000000046"), ComConversionLoss]
    public interface IAdviseSink
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteOnDataChange([In] ref tagFORMATETC pformatetc, [In, ComAliasName("MsHtmHstInterop.wireASYNC_STGMEDIUM")] IntPtr pStgmed);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteOnViewChange([In] uint dwAspect, [In] int lindex);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteOnRename([In, MarshalAs(UnmanagedType.Interface)] IMoniker pmk);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteOnSave();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteOnClose();
    }
}

