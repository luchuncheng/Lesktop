using System;
using System.Runtime.InteropServices;

namespace Client
{
	[ComImport(),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	GuidAttribute("3050f3f0-98b5-11cf-bb82-00aa00bdce0b")]
	interface ICustomDoc
	{
		[PreserveSig]
		void SetUIHandler(MsHtmHstInterop.IDocHostUIHandler pUIHandler);
	}

}