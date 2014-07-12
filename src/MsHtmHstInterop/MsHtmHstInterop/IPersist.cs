namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("0000010C-0000-0000-C000-000000000046"), InterfaceType((short) 1)]
    public interface IPersist
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void GetClassID(out Guid pClassID);
    }
}

