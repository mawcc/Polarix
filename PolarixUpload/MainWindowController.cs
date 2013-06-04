using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace PolarixUpload
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		#region Constructors
		
		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();

			var outline = treeScrollView.Subviews.First ().Subviews.First () as NSOutlineView;
			var dataSource = new TreeDataSource ();
			outline.DataSource = dataSource;

			myFirstButton.Activated += MyFirstButton_Click;
			treeScrollView.RegisterForDraggedTypes (new string[]{"NSFilenamesPboardType"});
		}

		private void MyFirstButton_Click (object sender, EventArgs e)
		{
			var panel = new NSOpenPanel ();
			panel.CanChooseFiles = false;
			panel.CanChooseDirectories = true;
			var result = panel.RunModal ();
			if (result == 1) {
				var root = new Directory (panel.Directory);
				root.Children.Add (new Directory (root.Value + "/sub1"));
				root.Children [0].Children.Add (new Directory(root.Value + "/sub1/subsub1"));
				root.Children.Add (new Directory(root.Value + "/sub2"));
				var outline = treeScrollView.Subviews.First ().Subviews.First () as NSOutlineView;
				var dataSource = outline.DataSource as TreeDataSource;
				dataSource.Directories.Add (root);
				outline.ReloadData ();
			}
		}
		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}
	}

	[Register("MyTreeScrollView")]
	class MyTreeScrollView : NSScrollView
	{
		public MyTreeScrollView (IntPtr handle) : base (handle)
		{
		}

		public override NSDragOperation DraggingUpdated (NSDraggingInfo sender)
		{
			//The ImageBrowserView uses this method to set the icon during dragging. Regardless
			//of what we return the view will send a moveitems message to the datasource.
			//so it is best to not display the copy icon.

			//Console.WriteLine ("Drag Delegate received 'DraggingUpdated'");
			NSObject obj = sender.DraggingSource;
			if (obj != null && obj.Equals (this)) {
				return NSDragOperation.Move;
			}
			return NSDragOperation.Copy;
		}

		public override bool PerformDragOperation (NSDraggingInfo sender)
		{
			NSPasteboard pb = sender.DraggingPasteboard;
			NSArray data = null;
			if (pb.Types.Contains (NSPasteboard.NSFilenamesType))
				data = pb.GetPropertyListForType (NSPasteboard.NSFilenamesType) as NSArray;
			if (data != null) {
				for (int i = 0; i < data.Count; i++) {
					string path = (string)NSString.FromHandle (data.ValueAt ((uint)i));
					Console.WriteLine ("From pasteboard Item {0} = {1}", i, path);

					var url = NSUrl.FromFilename (path);
					var baseDir = url.Path;
					var rootDir = baseDir.Substring (0, baseDir.LastIndexOf ('/'));
					var d = new Directory (baseDir);
					ScanDirectories (rootDir, baseDir, d);

					var outlineView = Subviews [0].Subviews [0] as NSOutlineView;
					((TreeDataSource)outlineView.DataSource).Directories.Add (d);
					outlineView.ReloadData ();

					return true;
				}
			}
			return false;
		}

		private void ScanDirectories (string rootDirectory, string currentDirectory, Directory tree)
		{
			var subDirs = System.IO.Directory.GetDirectories (currentDirectory);
			foreach (var subDir in subDirs) {
				var d = new Directory (subDir);
				tree.Children.Add (d);
				ScanDirectories (rootDirectory, subDir, d);
			}
		}
	}

	class TreeDataSource : NSOutlineViewDataSource
	{
		/// The list of persons (top level)
		public List<Directory> Directories 
		{
			get;
			set;
		}

		// Constructor
		public TreeDataSource ()
		{
			// Create the Persons list
			Directories = new List<Directory> ();
		}

		public override int GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			// If the item is not null, return the child count of our item
			if (item != null)
				return (item as Directory).Children.Count;
			// Its null, that means its asking for our root element count.
			return Directories.Count ();
		}

		public override NSObject GetObjectValue (NSOutlineView outlineView, NSTableColumn forTableColumn, NSObject byItem)
		{
			// Is it null? (It really shouldnt be...)
			if (byItem != null) {
				// Jackbot, typecast to our Person object
				var p = ((Directory)byItem);
				// Get the table column identifier
//				var ident = forTableColumn.Identifier.ToString();
				// We return the appropriate information for each column
//				if (ident == "Name") {
				return (NSString)p.Name;
//				}
			}
			// Oh well.. errors dont have to be THAT depressing..
			return (NSString)"Not enough jQuery";
		}

		public override NSObject GetChild (NSOutlineView outlineView, int childIndex, NSObject ofItem)
		{
			// If the item is null, it's asking for a root element. I had serious trouble figuring this out...
			if (ofItem == null)
				return Directories [childIndex];
			// Return the child its asking for.
			return (NSObject)((ofItem as Directory).Children [childIndex]);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			// Straight forward - it wants to know if its expandable.
			if (item == null)
				return false;
			return (item as Directory).Children.Count > 0;
		}
	}

	public class Directory : NSObject
	{
		public string Name 
		{
			get;
			set;
		}

		public string Value 
		{
			get;
			set;
		}

		public List<Directory> Children 
		{
			get;
			set;
		}

		public Directory (string path)
		{
			Name = path.Substring (path.LastIndexOf('/') + 1);
			Value = path;
			Children = new List<Directory> ();
		}
	}
}

