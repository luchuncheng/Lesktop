namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType((short) 1), ComConversionLoss, Guid("0000010E-0000-0000-C000-000000000046")]
    public interface IDataObject
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteGetData([In] ref tagFORMATETC pformatetcIn, [Out, ComAliasName("MsHtmHstInterop.wireSTGMEDIUM")] IntPtr pRemoteMedium);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteGetDataHere([In] ref tagFORMATETC pformatetc, [In, Out, ComAliasName("MsHtmHstInterop.wireSTGMEDIUM")] IntPtr pRemoteMedium);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void QueryGetData([In] ref tagFORMATETC pformatetc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetCanonicalFormatEtc([In] ref tagFORMATETC pformatectIn, out tagFORMATETC pformatetcOut);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void RemoteSetData([In] ref tagFORMATETC pformatetc, [In, ComAliasName("MsHtmHstInterop.wireFLAG_STGMEDIUM")] IntPtr pmedium, [In] int fRelease);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnumFormatEtc([In] uint dwDirection, [MarshalAs(UnmanagedType.Interface)] out IEnumFORMATETC ppenumFormatEtc);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DAdvise([In] ref tagFORMATETC pformatetc, [In] uint advf, [In, MarshalAs(UnmanagedType.Interface)] IAdviseSink pAdvSink, out uint pdwConnection);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DUnadvise([In] uint dwConnection);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void EnumDAdvise([MarshalAs(UnmanagedType.Interface)] out IEnumSTATDATA ppenumAdvise);
    }
}

