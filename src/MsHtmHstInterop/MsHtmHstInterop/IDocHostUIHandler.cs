namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, ComConversionLoss, InterfaceType((short) 1), Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A")]
    public interface IDocHostUIHandler
    {
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint ShowContextMenu([In] uint dwID, [In] ref tagPOINT ppt, [In, MarshalAs(UnmanagedType.IUnknown)] object pcmdtReserved, [In, MarshalAs(UnmanagedType.IDispatch)] object pdispReserved);
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint GetHostInfo([In, Out] ref _DOCHOSTUIINFO pInfo);
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint ShowUI([In] uint dwID, [In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceActiveObject pActiveObject, [In, MarshalAs(UnmanagedType.Interface)] IOleCommandTarget pCommandTarget, [In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceFrame pFrame, [In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceUIWindow pDoc);
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint HideUI();
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint UpdateUI();
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint EnableModeless([In] int fEnable);
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint OnDocWindowActivate([In] int fActivate);
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint OnFrameWindowActivate([In] int fActivate);
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint ResizeBorder([In] ref tagRECT prcBorder, [In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceUIWindow pUIWindow, [In] int fRameWindow);
        
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		uint TranslateAccelerator(ref tagMSG lpmsg, uint pguidCmdGroup, uint nCmdID);
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint GetOptionKeyPath([MarshalAs(UnmanagedType.LPWStr)] out string pchKey, [In] uint dw);
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint GetDropTarget([In, MarshalAs(UnmanagedType.Interface)] IDropTarget pDropTarget, [MarshalAs(UnmanagedType.Interface)] out IDropTarget ppDropTarget);
		
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint GetExternal([MarshalAs(UnmanagedType.IDispatch)] out object ppDispatch);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint TranslateUrl([In] uint dwTranslate, [In] ref ushort pchURLIn, [Out] IntPtr ppchURLOut);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint FilterDataObject([In, MarshalAs(UnmanagedType.Interface)] IDataObject pDO, [MarshalAs(UnmanagedType.Interface)] out IDataObject ppDORet);
    }
}

