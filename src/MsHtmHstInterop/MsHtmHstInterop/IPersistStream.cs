namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("00000109-0000-0000-C000-000000000046"), InterfaceType((short) 1)]
    public interface IPersistStream : IPersist
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsDirty();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Load([In, MarshalAs(UnmanagedType.Interface)] IStream pstm);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Save([In, MarshalAs(UnmanagedType.Interface)] IStream pstm, [In] int fClearDirty);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetSizeMax(out _ULARGE_INTEGER pcbSize);
    }
}

