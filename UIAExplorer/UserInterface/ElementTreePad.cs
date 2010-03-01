using System;
using System.Windows.Automation;
using AEIds = System.Windows.Automation.AutomationElementIdentifiers;
using Gtk;
using System.IO;
using System.Windows;
using System.Threading;
using Mono.Accessibility.UIAExplorer.UiaUtil;

namespace Mono.Accessibility.UIAExplorer.UserInterface
{
	public class ElementTreePad : IDockPad
	{
		public ElementTreePad ()
		{
			/* Name,
			 * ControlType,
			 * Child count
			 * AutomationId,
			 * Parent window's handle
             * Icon stock-Id
             * Is refreshing children of children needed.
			 */
			elementStore = new TreeStore (
				typeof (string), typeof (string),
				typeof (int), typeof (string),
				typeof (int), typeof (string),
				typeof (bool));

			elementTree = new TreeView ();
			Gtk.TreeViewColumn column = new Gtk.TreeViewColumn ();
			column.Title = "Name";
			var iconRenderer = new Gtk.CellRendererPixbuf ();
			column.PackStart (iconRenderer, false);
			column.AddAttribute (iconRenderer, "stock-id", 5);
			var nameRenderer = new Gtk.CellRendererText ();
			column.PackStart (nameRenderer, true);
			column.AddAttribute (nameRenderer, "text", 0);
			column.Resizable = true;
			elementTree.AppendColumn (column);

			column = elementTree.AppendColumn ("Type", new Gtk.CellRendererText (), "text", 1);
			column.Resizable = true;
			column = elementTree.AppendColumn ("Children", new Gtk.CellRendererText (), "text", 2);
			column.Resizable = true;
			elementTree.CursorChanged += (o, e) => OnSelectAutomationElement ();
			elementTree.RowExpanded += new RowExpandedHandler (treeRowExpanded);

			//TODO
			progress = new ProgressBar ();

			box = new VBox ();
			ScrolledWindow wnd = new ScrolledWindow ();
			wnd.Child = elementTree;
			box.PackStart (wnd, true, true, 0);
			box.PackStart (progress, false, false, 0);

			box.ShowAll ();
			InitElementTree ();
		}

		private void treeRowExpanded (object o, RowExpandedArgs args)
		{
			bool needRefreshingChildren = (bool) elementStore.GetValue (args.Iter, 6);
			if (needRefreshingChildren) {
				TreeIter iter;
				elementStore.IterNthChild (out iter, 0);
				do {
					var ae = GetCachedElementFromIter (iter);
					if (ae == null)
						continue;
					var childCount = ae.CachedChildren.Count;
					elementStore.SetValue (args.Iter, 2, childCount);
					elementStore.SetValue (args.Iter, 6, childCount > 0);
					InsertChildElements (ae, iter);
				} while (elementStore.IterNext (ref iter));
				elementStore.SetValue (args.Iter, 6, false);
			}
		}

		private AutomationElement GetCachedElementFromIter (TreeIter iter)
		{
			CacheRequest request = new CacheRequest ();
			request.TreeScope = TreeScope.Element | TreeScope.Children;
			request.Add (AutomationElement.NameProperty);
			request.Add (AutomationElement.ControlTypeProperty);
			request.Add (AutomationElement.AutomationIdProperty);
			request.Add (AutomationElement.NativeWindowHandleProperty);

			return GetElementFromIter (iter, request);
		}

		private AutomationElement GetElementFromIter (TreeIter iter, CacheRequest request)
		{
			AutomationElement ret = null;
			string automationId = (string) elementStore.GetValue (iter, 3);
			int handle = (int) elementStore.GetValue (iter, 4);
			var parentWindow = AutomationElement.FromHandle (new IntPtr (handle));
			if (parentWindow != null) {
				IDisposable requestHandle = null;
				if (request != null)
					requestHandle = request.Activate ();
				try {
					ret = parentWindow.FindFirst (TreeScope.Subtree,
						new PropertyCondition (AEIds.AutomationIdProperty, automationId));
				} finally {
					if (requestHandle != null)
						requestHandle.Dispose ();
				}
			}
			if (ret == null)
				Log.Warn ("AutomationElement for TreeIter is null, {0:X}, {1}, {2}",
					handle, automationId, parentWindow.Current.Name);
			return ret;
		}

		private void OnSelectAutomationElement ()
		{
			if (SelectAutomationElement != null) {
				AutomationElement ae = null;
				var selectedRows = elementTree.Selection.GetSelectedRows ();
				if (selectedRows.Length > 0) {
					TreeIter iter;
					if (elementStore.GetIter (out iter, selectedRows [0])) {
						string automationId = (string) elementStore.GetValue (iter, 3);
						int handle = (int) elementStore.GetValue (iter, 4);
						var parentWindow = AutomationElement.FromHandle (new IntPtr (handle));
						if (parentWindow != null) {
							ae = parentWindow.FindFirst (TreeScope.Subtree,
								new PropertyCondition (AEIds.AutomationIdProperty, automationId));
						}
						//?????
						if (ae == null)
							Log.Error ("ae is null, {0:X}, {1}, {2}", handle, automationId,
									  parentWindow.Current.Name);
					}
				}

				SelectAutomationElement (elementTree, new SelectAutomationElementArgs (ae));
			}
		}

		public event EventHandler<SelectAutomationElementArgs>
			SelectAutomationElement;

		public Widget Control
		{
			get { return box; }
		}

		public string Title
		{
			get { return "Element Tree"; }
		}

		public void InitElementTree ()
		{
			elementTree.Model = null;
			elementTree.Sensitive = false;
			progress.Fraction = 0.0;
			progress.Pulse ();
			progress.Text = "Loading";
			Thread thread = new Thread (new ParameterizedThreadStart (RefreshTreeNode));
			updateTreeNotify = new ThreadNotify (() => {
				elementTree.Model = elementStore;
				elementTree.Sensitive = true;
				progress.Fraction = 1.0;
				progress.Text = "Complete";
			});
			thread.Start (null);
		}

		private void RefreshTreeNode (object startNode)
		{
			//TODO ensure this method is never reentered
			TreePath path = startNode as TreePath;
			if (path == null) {
				elementStore.Clear ();
				CacheRequest request = new CacheRequest ();
				request.TreeScope = TreeScope.Element | TreeScope.Children;
				request.Add (AutomationElement.NameProperty);
				request.Add (AutomationElement.ControlTypeProperty);
				request.Add (AutomationElement.AutomationIdProperty);
				request.Add (AutomationElement.NativeWindowHandleProperty);

				Condition cond = new PropertyCondition (AEIds.ProcessIdProperty,
					System.Diagnostics.Process.GetCurrentProcess ().Id);
				cond = new AndCondition (Automation.ControlViewCondition, new NotCondition (cond));

				using (request.Activate ()) {
					var rootElements = AutomationElement.RootElement.FindAll (TreeScope.Children, cond);
					Application.Invoke ((o, e) => progress.Pulse ());
					double progressStep = 1.0 / rootElements.Count;
					foreach (AutomationElement child in rootElements) {
						int childCount = child.CachedChildren.Count;
						TreeIter iter = elementStore.AppendValues (
							GetNameDisplay (child.Cached.Name),
							GetControlTypeDisplay (child.Cached.ControlType),
							childCount,
							child.Cached.AutomationId,
							child.Cached.NativeWindowHandle,
							string.Empty,
							childCount > 0);
						InsertChildElements (child, iter);
						Application.Invoke ((o, e) => progress.Fraction += progressStep);
					}
				}
			} else {
				//???? TODO
			}
			updateTreeNotify.WakeupMain ();
		}

		private void InsertChildElements (AutomationElement parent, TreeIter iter)
		{
			int parentWindowHandle = (int) elementStore.GetValue (iter, 4);
			foreach (AutomationElement child in parent.CachedChildren) {
				object handleObj = child.GetCachedPropertyValue (
					AutomationElement.NativeWindowHandleProperty, true);
				int handle = (handleObj != AutomationElement.NotSupported) ?
					(int) handleObj : parentWindowHandle;
				elementStore.AppendValues (iter,
					GetNameDisplay (child.Cached.Name),
					GetControlTypeDisplay (child.Cached.ControlType),
					0,
					child.Cached.AutomationId,
					parentWindowHandle,
					string.Empty, true);
			}
		}

		private VBox box = null;
		private ProgressBar progress = null;
		private TreeView elementTree = null;
		private TreeStore elementStore = null;
		//???? probably need lock here
		private ThreadNotify updateTreeNotify = null;

		private static string GetNameDisplay (string elementName)
		{
			return string.Format ("\"{0}\"", elementName);
		}

		private static string GetControlTypeDisplay (ControlType ct)
		{
			return ct.ProgrammaticName.Substring ("ControlType.".Length);
		}
	}
}
