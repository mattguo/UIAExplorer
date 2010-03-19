using System;
using System.Collections.Generic;
using Gtk;
using System.Text;

namespace IronPythonRepl
{
	public delegate void SelectCompletionHandler (string completion);
	public class CompletionWindow : Window
	{
		private string [] choices;
		private TreeView choiceList;
		private ListStore listStore;

		public CompletionWindow (string [] choices, int defaultIndex) :
			base (WindowType.Toplevel)
		{
			Decorated = false;

			choiceList = new TreeView ();
			ScrolledWindow scroll = new ScrolledWindow ();
			ModifyBg (StateType.Normal, new Gdk.Color (0, 0, 0));
			scroll.BorderWidth = 2;
			scroll.Add (choiceList);
			Add (scroll);
			var col = new TreeViewColumn ();
			col.Title = "";
			var cell = new CellRendererText ();
			col.PackStart (cell, true);
			col.AddAttribute (cell, "text", 0);
			choiceList.AppendColumn (col);

			choiceList.HeadersVisible = false;
			listStore = new Gtk.ListStore (typeof (string));
			choiceList.Model = listStore;
			TreeIter iter = new TreeIter();
			foreach (var choice in choices)
				iter = listStore.AppendValues (choice);
			// TODO auto calculate size
			this.SetSizeRequest (220, 180);
			choiceList.Selection.SelectPath (new TreePath (new int [] { defaultIndex }));
			Focus = choiceList;
		}

		public event SelectCompletionHandler SelectCompletion;

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			Console.WriteLine ("CompletionWindow.Key presed");
			switch (evnt.Key) {
				case Gdk.Key.Return:
				case Gdk.Key.Tab:
				case Gdk.Key.KP_Enter:
					OnSelectCompletion ();
					Close ();
					return true;
				case Gdk.Key.Escape:
					Close ();
					break;
			}
			return base.OnKeyPressEvent (evnt);
		}

		protected override bool OnFocusOutEvent (Gdk.EventFocus evnt)
		{
			Close ();
			return true;
		}

		private void Close ()
		{
			Console.WriteLine ("CompletionWindow.Destroy");
			this.Destroy ();
		}

		private void OnSelectCompletion ()
		{
			TreeIter selected;
			if (choiceList.Selection.GetSelected (out selected)) {
				string choice = listStore.GetValue (selected, 0) as string;
				if (choice != null && SelectCompletion != null)
					SelectCompletion (choice);
			}
		}
	}
}
