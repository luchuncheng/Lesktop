namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;

    /*[ComImport, InterfaceType((short) 1), Guid("00000122-0000-0000-C000-000000000046")]
    public interface IDropTarget
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DragEnter([In, MarshalAs(UnmanagedType.Interface)] IDataObject pDataObj, [In] uint grfKeyState, [In] _POINTL pt, [In, Out] ref uint pdwEffect);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DragOver([In] uint grfKeyState, [In] _POINTL pt, [In, Out] ref uint pdwEffect);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void DragLeave();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
        void Drop([In, MarshalAs(UnmanagedType.Interface)] IDataObject pDataObj, [In] uint grfKeyState, [In] _POINTL pt, [In, Out] ref uint pdwEffect);
    }*/

	public struct POINTL
	{
		public int x;
		public int y;

		public POINTL(int x0, int y0)
		{
			x = x0;
			y = y0;
		}
	}

	[
		ComImport(),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
		GuidAttribute("00000122-0000-0000-C000-000000000046")
	]
	public interface IDropTarget
	{
		[PreserveSig]
		long DragEnter(
			[In]System.Runtime.InteropServices.ComTypes.IDataObject pDataObject,
			[In]int grfKeyState,
			[In] POINTL pt,
			[In, Out] ref uint pdwEffect
		);

		[PreserveSig]
		long DragOver(
			[In] int grfKeyState,
			[In] POINTL pt,
			[In, Out] ref uint pdwEffect);

		[PreserveSig]
		long DragLeave();

		[PreserveSig]
		long Drop(
			[In] System.Runtime.InteropServices.ComTypes.IDataObject pDataObject,
			[In] int grfKeyState,
			[In] POINTL pt,
			[In, Out] ref uint pdwEffect
		);
	}
}

