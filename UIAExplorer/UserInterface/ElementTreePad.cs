using System;
using System.Windows.Automation;
using AEIds = System.Windows.Automation.AutomationElementIdentifiers;
using Gtk;
using System.IO;
using System.Windows;
using System.Threading;
using Mono.Accessibility.UIAExplorer.UiaUtil;
using System.Collections.Generic;

namespace Mono.Accessibility.UIAExplorer.UserInterface
{
	public class ElementTreePad : IDockPad
	{
		#region Inner Types

		enum ElementTreeStoreColumn
		{
			Name = 0,
			ControlType = 1,
			ChildCount = 2,
			RuntimeId = 3,
			ParentWindowHandle = 4,
			IconStockId = 5,
			IsChildUpdateNeeded = 6
		}

		#endregion

		#region Constructor

		public ElementTreePad ()
		{
			elementStore = new TreeStore (
				typeof (string), typeof (string),
				typeof (int), typeof (int []),
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

		#endregion

		#region Private Members
		#endregion

		#region Public Members
		#endregion
		

		private void treeRowExpanded (object o, RowExpandedArgs args)
		{
			bool needRefreshingChildren = (bool) elementStore.GetValue (args.Iter, 6);
			if (needRefreshingChildren) {
				TreeIter iter;
				elementStore.IterNthChild (out iter, args.Iter, 0);
				do {
					var runtimeId = (int [])elementStore.GetValue (iter, (int) ElementTreeStoreColumn.RuntimeId);
					var name = elementStore.GetValue (iter, (int) ElementTreeStoreColumn.Name);
					TStart (name + ", " + StringFormatter.Format (runtimeId));
					var ae = GetElementFromIter (iter);
					if (ae == null)
						continue;
					TEnd ();
					TStart ("step 1");
					AutomationElementCollection children = ae.FindAll (
						TreeScope.Children, Condition.TrueCondition);
					var childCount = children.Count;
					//elementStore.SetValue (iter, 2, childCount);
					//elementStore.SetValue (iter, 6, childCount > 0);
					TEnd ();
					TStart ("child insert");
					InsertChildElements (ae, iter, children);
					TEnd ();
				} while (elementStore.IterNext (ref iter));
				elementStore.SetValue (args.Iter, 6, false);
			}
		}

		private AutomationElement GetElementFromIter (TreeIter iter)
		{
			AutomationElement ret = null;
			var runtimeId = (int []) elementStore.GetValue (iter, 3);
			int handle = (int) elementStore.GetValue (iter, 4);
			var parentWindow = AutomationElement.FromHandle (new IntPtr (handle));
			if (parentWindow != null)
				ret = parentWindow.FindFirst (TreeScope.Subtree,
					new PropertyCondition (AEIds.RuntimeIdProperty, runtimeId));
			if (ret == null)
				Log.Warn ("AutomationElement for TreeIter is null, {0:X}, {1}, {2}",
					handle, StringFormatter.Format (runtimeId), parentWindow.Current.Name);
			return ret;
		}

		private void OnSelectAutomationElement ()
		{
			if (SelectAutomationElement != null) {
				AutomationElement ae = null;
				var selectedRows = elementTree.Selection.GetSelectedRows ();
				if (selectedRows.Length > 0) {
					TreeIter iter;
					if (elementStore.GetIter (out iter, selectedRows [0]))
						ae = GetElementFromIter (iter);
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
				Condition cond = new PropertyCondition (AEIds.ProcessIdProperty,
					System.Diagnostics.Process.GetCurrentProcess ().Id);
				cond = new AndCondition (Automation.ControlViewCondition, new NotCondition (cond));

				var rootElements = AutomationElement.RootElement.FindAll (TreeScope.Children, cond);
				Application.Invoke ((o, e) => progress.Pulse ());
				double progressStep = 1.0 / rootElements.Count;
				foreach (AutomationElement child in rootElements) {
					AutomationElementCollection childrenOfChild = child.FindAll (
						TreeScope.Children, Condition.TrueCondition);
					int childCount = childrenOfChild.Count;
					TreeIter iter = elementStore.AppendValues (
						GetNameDisplay (child.Current.Name),
						GetControlTypeDisplay (child.Current.ControlType),
						childCount,
						child.GetRuntimeId (),
						child.Current.NativeWindowHandle,
						string.Empty,
						childCount > 0);
					InsertChildElements (child, iter, childrenOfChild);
					Application.Invoke ((o, e) => progress.Fraction += progressStep);
				}
			} else {
				//???? TODO
			}
			updateTreeNotify.WakeupMain ();
		}

		private void InsertChildElements (AutomationElement parent, TreeIter iter, AutomationElementCollection children)
		{
			int parentWindowHandle = (int) elementStore.GetValue (iter, 4);
			foreach (AutomationElement child in children) {
				object handleObj = child.GetCurrentPropertyValue (
					AEIds.NativeWindowHandleProperty, true);
				int handle = (handleObj != AutomationElement.NotSupported) ?
					(int) handleObj : parentWindowHandle;
				elementStore.AppendValues (iter,
					GetNameDisplay (child.Current.Name),
					GetControlTypeDisplay (child.Current.ControlType),
					0,
					child.GetRuntimeId (),
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

		#region Tick Calc

		DateTime tick;
		string tickMessage;

		private void TStart (string message)
		{
			tickMessage = message;
			tick = DateTime.Now;
		}

		private void TEnd ()
		{
			double sec =  (DateTime.Now - tick).TotalSeconds;
			Console.WriteLine ("{0}: {1} seconds", tickMessage, sec);
		}

		#endregion
	}
}
