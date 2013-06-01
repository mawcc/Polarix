// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace PolarixUpload
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSButton myFirstButton { get; set; }

		[Outlet]
		MyTreeScrollView treeScrollView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (myFirstButton != null) {
				myFirstButton.Dispose ();
				myFirstButton = null;
			}

			if (treeScrollView != null) {
				treeScrollView.Dispose ();
				treeScrollView = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
