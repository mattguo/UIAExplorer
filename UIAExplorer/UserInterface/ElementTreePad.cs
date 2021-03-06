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
			IsChildUpdateNeeded = 5,
			Available = 6
		}

		#endregion

		#region Private Fields

		private VBox box = null;
		private ProgressBar progress = null;
		private TreeView elementTree = null;
		private TreeStore elementStore = null;
		private TreeWalker treeWalker;
		private PerformanceMonitor perfMon = new PerformanceMonitor ();

		//need to barrier and lock elementStore

		#endregion

		#region Constructor

		public ElementTreePad (TreeWalker treeWalker)
		{
			this.treeWalker = treeWalker;

			elementStore = new TreeStore (
				typeof (AutomationElement),
				typeof (string),
				typeof (string),
				typeof (int),
				typeof (string),
				typeof (bool),
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
			elementTree.ButtonReleaseEvent += new ButtonReleaseEventHandler (elementTree_ButtonReleaseEvent);

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
			//TODO put expansion in a new thread, or the progress won't have any effect.
			if (!CheckElementAvailability (args.Iter))
				return;
			bool updateChildren = (bool) elementStore.GetValue (args.Iter, 
				(int) TreeStoreColumn.IsChildUpdateNeeded);
			if (updateChildren) {
				TreeIter iter;
				var childCount = elementStore.IterNChildren (args.Iter);
				if (childCount == 0)
					return;
				double progressStep = 1.0 / (double) childCount;
				ResetProgress (true);
				elementStore.IterNthChild (out iter, args.Iter, 0);
				do {
					perfMon.TimerStart ("step 0");
					AutomationElement ae = (AutomationElement)
						elementStore.GetValue (iter, (int) TreeStoreColumn.AutomationElement);
					if (ae == null)
						continue;
					perfMon.TimerEnd ();
					perfMon.TimerStart ("step 1");
					var children = GetChildElements (ae);
					perfMon.TimerEnd ();
					elementStore.SetValue (iter,
						(int) TreeStoreColumn.ChildCount,
						children.Length);
					perfMon.TimerStart ("child insert");
					InsertChildElements (ae, iter, children);
					perfMon.TimerEnd ();
					AccumulateProgress (progressStep, "");
				} while (elementStore.IterNext (ref iter));
				SetProgress (1.0, "Completed");
				elementStore.SetValue (args.Iter,
					(int) TreeStoreColumn.IsChildUpdateNeeded, false);
			}
		}

		private void OnSelectAutomationElement ()
		{
			var selectedElement = SelectedAutomationElement;
			if (selectedElement != null)
				SelectAutomationElement (elementTree,
					new SelectAutomationElementArgs (selectedElement));
		}

		private AutomationElement SelectedAutomationElement	{
			get	{
				var selectedRows = elementTree.Selection.GetSelectedRows ();
				if (selectedRows.Length > 0) {
					TreeIter iter;
					if (elementStore.GetIter (out iter, selectedRows [0])) {
						if (CheckElementAvailability (iter)) {
							return (AutomationElement)
								elementStore.GetValue (iter, (int) TreeStoreColumn.AutomationElement);
						}
					}
				}
				return null;
			}
		}
		
		//[GLib.ConnectBefore]
		private void elementTree_ButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			if (args.Event.Button == 3U) {
				//right button
				//Gtk.Menu jBox = new Gtk.Menu ();
				//Gtk.MenuItem MenuItem1 = new MenuItem ("new job");
				//jBox.Add (MenuItem1);
				//jBox.ShowAll ();
				//jBox.Popup ();

				//MessageDialog dlg = new MessageDialog (MainWindow.Instace, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "{0}",
				//    SelectedAutomationElement.Current.Name);
				//dlg.Run ();
			}
		}

		private bool CheckElementAvailability (TreeIter iter)
		{
			bool avaiable = (bool) elementStore.GetValue (iter, (int) TreeStoreColumn.Available);
			if (avaiable) {
				var ae = (AutomationElement)
					elementStore.GetValue (iter, (int) TreeStoreColumn.AutomationElement);
				try {
					ae.GetCurrentPropertyValue (AEIds.NameProperty);
				} catch (ElementNotAvailableException) {
					avaiable = false;
					DisableElement (iter);
				}
			}
			return avaiable;
		}

		private void DisableElement (TreeIter rootIter)
		{
			elementStore.SetValue (rootIter, (int) TreeStoreColumn.Available, false);
			elementStore.SetValue (rootIter, (int) TreeStoreColumn.IsChildUpdateNeeded, false);
			elementStore.SetValue (rootIter, (int) TreeStoreColumn.IconStockId, "invalid");
			TreeIter iter;
			if (elementStore.IterNthChild (out iter, rootIter, 0)) {
				do {
					DisableElement (iter);
				} while (elementStore.IterNext (ref iter));
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

				var rootElements = GetChildElements (AutomationElement.RootElement, cond);
				ResetProgress(true);
				
				double progressStep = 1.0 / rootElements.Length;
				
				foreach (AutomationElement topLevel in rootElements) {
					var children = GetChildElements (topLevel);
					var controlType = topLevel.Current.ControlType;
					TreeIter iter = elementStore.AppendValues (
						topLevel,
						StringFormatter.Format (topLevel.Current.Name, 32),
						StringFormatter.Format (controlType),
						children.Length,
						IconUtil.GetStockName (controlType),
						true,
						true);
					InsertChildElements (topLevel, iter, children);
					AccumulateProgress (progressStep, "");
				}
			} else {
				//???? TODO Refresh Tree Part
			}
			Application.Invoke ((o, e) => {
				elementTree.Model = elementStore;
				elementTree.Sensitive = true;
			});
			SetProgress (1.0, "Completed");
		}

		private void InsertChildElements (AutomationElement parent, TreeIter iter, AutomationElement [] children)
		{
			foreach (AutomationElement child in children) {
				var controlType = child.Current.ControlType;
				elementStore.AppendValues (iter,
					child,
					StringFormatter.Format (child.Current.Name, 32),
					StringFormatter.Format (controlType),
					0,
					IconUtil.GetStockName (controlType),
					true,
					true);
			}
		}

		private AutomationElement [] GetChildElements (AutomationElement parent)
		{
			return GetChildElements (parent, null);
		}

		private AutomationElement [] GetChildElements (AutomationElement parent, Condition cond)
		{
			TreeWalker walker = null;
			if (cond != null && cond != Condition.TrueCondition) {
				Condition combinedCond = new AndCondition (cond, treeWalker.Condition);
				walker = new TreeWalker (combinedCond);
			} else
				walker = treeWalker;
			List<AutomationElement> children = new List<AutomationElement> ();
			var child = walker.GetFirstChild (parent);
			while (child != null) {
				children.Add (child);
				child = walker.GetNextSibling (child);
			}
			return children.ToArray ();
		}

		private void ResetProgress (bool pulse)
		{
			Application.Invoke ((o, e) => {
				progress.Fraction = 0.0;
				if (pulse)
					progress.Pulse ();
			});
		}

		private void SetProgress (double fraction, string text)
		{
			Application.Invoke ((o, e) => { progress.Fraction = fraction; progress.Text = text; });
		}

		private void AccumulateProgress (double change, string text)
		{
			Application.Invoke ((o, e) => { progress.Fraction += change; progress.Text = text; });
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
			get {
				if (treeWalker.Condition == Automation.RawViewCondition)
					return "Raw View";
				else if (treeWalker.Condition == Automation.ControlViewCondition)
					return "Control View";
				else if (treeWalker.Condition == Automation.ContentViewCondition)
					return "Content View";
				else
					return "Custom View";
			}
		}

		public void InitElementTree ()
		{
			elementTree.Model = null;
			elementTree.Sensitive = false;
			progress.Fraction = 0.0;
			progress.Pulse ();
			progress.Text = "Loading";
			Thread thread = new Thread (new ParameterizedThreadStart (RefreshTreeNode));
			thread.Start (null);
		}

		#endregion
	}
}
