namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), Guid("0000000F-0000-0000-C000-000000000046")]
    public interface IMoniker : IPersistStream
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteBindToObject([In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkToLeft, [In] ref Guid riidResult, [MarshalAs(UnmanagedType.IUnknown)] out object ppvResult);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Reduce([In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In] uint dwReduceHowFar, [In, Out, MarshalAs(UnmanagedType.Interface)] ref IMoniker ppmkToLeft, [MarshalAs(UnmanagedType.Interface)] out IMoniker ppmkReduced);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Enum([In] int fForward, [MarshalAs(UnmanagedType.Interface)] out IEnumMoniker ppenumMoniker);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsEqual([In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkOtherMoniker);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Hash(out uint pdwHash);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsRunning([In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkToLeft, [In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkNewlyRunning);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetTimeOfLastChange([In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkToLeft, out _FILETIME pfiletime);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Inverse([MarshalAs(UnmanagedType.Interface)] out IMoniker ppmk);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void CommonPrefixWith([In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkOther, [MarshalAs(UnmanagedType.Interface)] out IMoniker ppmkPrefix);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RelativePathTo([In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkOther, [MarshalAs(UnmanagedType.Interface)] out IMoniker ppmkRelPath);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetDisplayName([In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkToLeft, [MarshalAs(UnmanagedType.LPWStr)] out string ppszDisplayName);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void ParseDisplayName([In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc, [In, MarshalAs(UnmanagedType.Interface)] IMoniker pmkToLeft, [In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, out uint pchEaten, [MarshalAs(UnmanagedType.Interface)] out IMoniker ppmkOut);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void IsSystemMoniker(out uint pdwMksys);
    }
}

