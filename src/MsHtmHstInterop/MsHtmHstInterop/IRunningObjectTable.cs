namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("00000010-0000-0000-C000-000000000046"), InterfaceType((short) 1)]
    public interface IRunningObjectTable
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Register([In] uint grfFlags, [In, MarshalAs(UnmanagedType.IUnknown)] object punkObject, [In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkObjectName, out uint pdwRegister);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Revoke([In] uint dwRegister);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsRunning([In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkObjectName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetObject([In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkObjectName, [MarshalAs(UnmanagedType.IUnknown)] out object ppunkObject);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void NoteChangeTime([In] uint dwRegister, [In] ref _FILETIME pfiletime);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTimeOfLastChange([In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkObjectName, out _FILETIME pfiletime);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnumRunning([MarshalAs(UnmanagedType.Interface)] out IEnumMoniker ppenumMoniker);
    }
}

