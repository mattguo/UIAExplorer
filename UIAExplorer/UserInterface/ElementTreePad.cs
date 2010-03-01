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

		enum TreeStoreColumn
		{
			AutomationElement = 0,
			Name = 1,
			ControlType = 2,
			ChildCount = 3,
			IconStockId = 4,
			IsChildUpdateNeeded = 5
		}

		#endregion

		#region Private Fields

		private VBox box = null;
		private ProgressBar progress = null;
		private TreeView elementTree = null;
		private TreeStore elementStore = null;
		//???? probably need lock here
		private ThreadNotify updateTreeNotify = null;
		private PerformanceMonitor perfMon = new PerformanceMonitor ();

		#endregion

		#region Constructor

		public ElementTreePad ()
		{
			elementStore = new TreeStore (
				typeof (AutomationElement),
				typeof (string),
				typeof (string),
				typeof (int),
				typeof (string),
				typeof (bool));

			elementTree = new TreeView ();
			Gtk.TreeViewColumn column = new Gtk.TreeViewColumn ();
			column.Title = "Name";
			var iconRenderer = new Gtk.CellRendererPixbuf ();
			column.PackStart (iconRenderer, false);
			column.AddAttribute (iconRenderer, "stock-id", (int) TreeStoreColumn.IconStockId);
			var nameRenderer = new Gtk.CellRendererText ();
			column.PackStart (nameRenderer, true);
			column.AddAttribute (nameRenderer, "text", (int) TreeStoreColumn.Name);
			column.Resizable = true;
			elementTree.AppendColumn (column);

			column = elementTree.AppendColumn ("Type", new Gtk.CellRendererText (), "text", (int) TreeStoreColumn.ControlType);
			column.Resizable = true;
			column = elementTree.AppendColumn ("Children", new Gtk.CellRendererText (), "text", (int) TreeStoreColumn.ChildCount);
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

		#region Private Methods

		private void treeRowExpanded (object o, RowExpandedArgs args)
		{
			bool updateChildren = (bool) elementStore.GetValue (args.Iter, 
				(int) TreeStoreColumn.IsChildUpdateNeeded);
			if (updateChildren) {
				TreeIter iter;
				elementStore.IterNthChild (out iter, args.Iter, 0);
				do {
					perfMon.TimerStart ("step 0");
					AutomationElement ae = (AutomationElement)
						elementStore.GetValue (iter, (int) TreeStoreColumn.AutomationElement);
					if (ae == null)
						continue;
					perfMon.TimerEnd ();
					perfMon.TimerStart ("step 1");
					AutomationElementCollection children = ae.FindAll (
						TreeScope.Children, Condition.TrueCondition);
					perfMon.TimerEnd ();
					elementStore.SetValue (iter,
						(int) TreeStoreColumn.ChildCount,
						children.Count);
					perfMon.TimerStart ("child insert");
					InsertChildElements (ae, iter, children);
					perfMon.TimerEnd ();
				} while (elementStore.IterNext (ref iter));
				elementStore.SetValue (args.Iter,
					(int) TreeStoreColumn.IsChildUpdateNeeded, false);
			}
		}

		private void OnSelectAutomationElement ()
		{
			if (SelectAutomationElement != null) {
				AutomationElement ae = null;
				var selectedRows = elementTree.Selection.GetSelectedRows ();
				if (selectedRows.Length > 0) {
					TreeIter iter;
					if (elementStore.GetIter (out iter, selectedRows [0]))
						ae = (AutomationElement)
							elementStore.GetValue (iter, (int) TreeStoreColumn.AutomationElement);
				}

				SelectAutomationElement (elementTree, new SelectAutomationElementArgs (ae));
			}
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
				
				foreach (AutomationElement topLevel in rootElements) {
					AutomationElementCollection children = topLevel.FindAll (
						TreeScope.Children, Condition.TrueCondition);
					TreeIter iter = elementStore.AppendValues (
						topLevel,
						StringFormatter.Format (topLevel.Current.Name, 32),
						StringFormatter.Format (topLevel.Current.ControlType),
						children.Count,
						string.Empty,
						true);
					InsertChildElements (topLevel, iter, children);
					Application.Invoke ((o, e) => progress.Fraction += progressStep);
				}
			} else {
				//???? TODO Refresh Tree Part
			}
			updateTreeNotify.WakeupMain ();
		}

		private void InsertChildElements (AutomationElement parent, TreeIter iter, AutomationElementCollection children)
		{
			foreach (AutomationElement child in children) {
				elementStore.AppendValues (iter,
					child,
					StringFormatter.Format (child.Current.Name, 32),
					StringFormatter.Format (child.Current.ControlType),
					0,
					string.Empty,
					true);
			}
		}

		#endregion

		#region Public Members

		public event EventHandler<SelectAutomationElementArgs> SelectAutomationElement;

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

		#endregion
	}
}
