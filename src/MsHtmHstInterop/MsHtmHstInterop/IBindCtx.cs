namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("0000000E-0000-0000-C000-000000000046"), InterfaceType((short) 1)]
    public interface IBindCtx
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RegisterObjectBound([In, MarshalAs(UnmanagedType.IUnknown)] object punk);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RevokeObjectBound([In, MarshalAs(UnmanagedType.IUnknown)] object punk);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ReleaseBoundObjects();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteSetBindOptions([In] ref tagBIND_OPTS2 pbindopts);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteGetBindOptions([In, Out] ref tagBIND_OPTS2 pbindopts);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetRunningObjectTable([MarshalAs(UnmanagedType.Interface)] out IRunningObjectTable pprot);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RegisterObjectParam([In, MarshalAs(UnmanagedType.LPWStr)] string pszKey, [In, MarshalAs(UnmanagedType.IUnknown)] object punk);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetObjectParam([In, MarshalAs(UnmanagedType.LPWStr)] string pszKey, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnumObjectParam([MarshalAs(UnmanagedType.Interface)] out IEnumString ppenum);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RevokeObjectParam([In, MarshalAs(UnmanagedType.LPWStr)] string pszKey);
    }
}

