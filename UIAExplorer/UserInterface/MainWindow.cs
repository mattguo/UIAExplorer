using System;
using System.ComponentModel;
using Gtk;
using MonoDevelop.Components;
using MonoDevelop.Components.Docking;
using MonoDevelop.Components.PropertyGrid;
using System.Windows.Automation;
using System.IO;
using Mono.Accessibility.UIAExplorer.UiaUtil;

namespace Mono.Accessibility.UIAExplorer.UserInterface
{	
	class MainWindow : Gtk.Window
	{
		private DockFrame dockFrame = null;
		private ElementTreePad treePad = null;
		private ElementPropertyPad propPad = null;
		private VBox box = null;
		private VBox headBox = null;
		private Statusbar statusbar = null;
		private UIManager uim = null;

		private void OnKeepBelow (object sender, EventArgs args)
		{
			ToggleAction action = (ToggleAction)sender;
			this.KeepBelow = action.Active;
		}

		private void RefreshTree (object sender, EventArgs e)
		{
			treePad.InitElementTree ();
		}

		private void InitDockFrame ()
		{
			treePad = new ElementTreePad();
			propPad = new ElementPropertyPad ();
			treePad.SelectAutomationElement += (o, e) => propPad.AutomationElement = e.AutomationElement;
			treePad.SelectAutomationElement += (o, e) => {
				if (!e.AutomationElement.Current.IsOffscreen) {
					var rect = e.AutomationElement.Current.BoundingRectangle;
					Highlighter h = new Highlighter (
						(int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
					h.Flash (1000);
				}
			};

			dockFrame = new DockFrame();
			dockFrame.Homogeneous = false;

			DockItem doc_item = dockFrame.AddItem("Document");
			doc_item.Behavior = DockItemBehavior.Locked;
			doc_item.Expand = true;
			doc_item.DrawFrame = false;
			doc_item.Label = "Documentos";
			doc_item.Content = new Label("Leave for scripting/testing areas");
			doc_item.DefaultVisible = true;
			doc_item.Visible = true;

			DockItem left = dockFrame.AddItem("elementTree");
			left.DefaultWidth = 250;
			left.Behavior = DockItemBehavior.CantClose;
			left.DefaultLocation = "Document/Left";
			left.DefaultVisible = true;
			left.Visible = true;
			left.DrawFrame = true;
			left.Label = treePad.Title;
			left.Content = treePad.Control;

			DockItem right = dockFrame.AddItem("elementProperty");
			right.DefaultWidth = 250;
			right.Behavior = DockItemBehavior.CantClose;
			right.DefaultLocation = "Document/Right";
			right.DefaultVisible = true;
			right.Visible = true;
			right.DrawFrame = true;
			right.Content = propPad.Control;
			right.Label = propPad.Title;
			right.Icon = "gtk-close";

			DockItem rb = dockFrame.AddItem("outputPad");
			rb.Behavior = DockItemBehavior.CantClose;
			rb.DefaultLocation = "Document/Bottom";
			rb.DefaultVisible = true;
			rb.Visible = true;
			rb.Label = "Output";
			rb.DrawFrame = true;
			rb.Content = new TextView();
			rb.Icon = "gtk-new";
		
			dockFrame.CreateLayout( "uia-explorer", true );
			dockFrame.CurrentLayout = "uia-explorer";
			dockFrame.HandlePadding = 0;
			dockFrame.HandleSize = 10;
		}

		public MainWindow () : base(Gtk.WindowType.Toplevel)
		{
			this.SetSizeRequest (800, 600);
			box = new VBox ();
			
			//????
			//todo, Pad view
			//todo, property selection view
			ActionEntry[] entries = new ActionEntry[] {
				new ActionEntry ("fileMenu", null, "_File", null, null, null),
				new ActionEntry ("exit", Stock.Quit, "E_xit", "<Alt>F4", null, (o,e) => Application.Quit ()),
				new ActionEntry ("viewMenu", null, "_View", null, null, null),
				new ActionEntry ("toolsMenu", null, "_Tools", null, null, null),
				new ActionEntry ("refreshTree", Stock.Refresh, "Refresh Entire _Tree", "<Control>R", "",
					RefreshTree)
			};

			ToggleActionEntry[] toggleEntries = new ToggleActionEntry[] {
				new ToggleActionEntry ("keepBelow", null, "Window Keep Below", "<control>B",
					"Toggle keep current window below, so that the AT client won't cover the target application",
					OnKeepBelow, false)
			};

			var group = new ActionGroup ("UiaExplorerActions");
			group.Add (entries);
			group.Add (toggleEntries);

			uim = new UIManager ();
			uim.InsertActionGroup (group, 0);
			//???? I guess so, is it right?
			this.AddAccelGroup (uim.AccelGroup);
			uim.AddWidget += new AddWidgetHandler (OnWidgetAdd);
			uim.AddUiFromResource ("Mono.Accessibility.UIAExplorer.UserInterface.MainMenu.xml");
			uim.AddUiFromResource ("Mono.Accessibility.UIAExplorer.UserInterface.Toolbar.xml");

//			MenuBar menubar = CreateMainMenuBar ();
//			box.PackStart (menubar, false, false, 0);
			headBox = new VBox ();
			box.PackStart (headBox, false, true, 0);

			InitDockFrame();
			box.PackStart (dockFrame, true, true, 0);
			
			statusbar = new Statusbar ();
			statusbar.Push (0, "hehe:");
			box.PackStart (statusbar, false, false, 0);

			this.Child = box;
			this.Title = "UIA Explorer";

			this.DeleteEvent += OnDeleteEvent;

			this.WindowPosition = WindowPosition.Center;
			this.Maximize();
			this.ShowAll();
		}

		private void OnWidgetAdd (object obj, AddWidgetArgs args)
		{
			//string widgetName = args.Widget.Name;
			headBox.PackStart (args.Widget, false, true, 0);
		}
		
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
	}
}