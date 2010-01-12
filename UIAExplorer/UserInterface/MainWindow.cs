using System;
using System.ComponentModel;
using Gtk;
using MonoDevelop.Components;
using MonoDevelop.Components.Docking;
using MonoDevelop.Components.PropertyGrid;
using System.Windows.Automation;


namespace Mono.Accessibility.UIAExplorer.UserInterface
{	
	class MainWindow : Gtk.Window
	{
		DockFrame dockFrame = null;
		ElementTreePad treePad = null;
		private bool keepBelow = false;

		private MenuBar CreateMainMenuBar ()
		{
			AccelGroup accel_group = new AccelGroup ();
			this.AddAccelGroup (accel_group);
			MenuBar menubar = new MenuBar ();
			MenuItem item = null;

			item = new MenuItem("_File");
			item.Submenu = CreateFileMenu(accel_group);
			menubar.Append(item);

			item = new MenuItem("_View");
			menubar.Append(item);
			item.Submenu = CreateViewMenu(accel_group);

			item = new MenuItem("_Tools");
			menubar.Append(item);
			item.Submenu = CreateToolsMenu(accel_group);

			return menubar;
		}

		private Menu CreateViewMenu(AccelGroup accel_group)
		{
			Menu menu = new Menu ();
			//todo, Pad view
			//todo, property selection view
			MenuItem item = new MenuItem("Toggle Keep Below");
			item.Activated += delegate(object sender, EventArgs e) {
				keepBelow = !keepBelow;
				this.KeepBelow = keepBelow;
			};
			menu.Append (item);
			return menu;
		}

		private Menu CreateFileMenu(AccelGroup accel_group)
		{
			Menu menu = new Menu ();
			MenuItem item = new MenuItem ("E_xit");
			item.Activated += delegate { Application.Quit (); };
			menu.Append (item);
			return menu;
		}

		private Menu CreateToolsMenu(AccelGroup accel_group)
		{
			Menu menu = new Menu();
			MenuItem item = new MenuItem("Refresh");
			
			item.AddAccelerator ("activate", accel_group, (int)Gdk.Key.r, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			item.Activated += delegate(object sender, EventArgs e) {
				treePad.InitElementTree ();
			};
			menu.Append(item);
			return menu;
		}

		private void InitDockFrame ()
		{
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

			treePad = new ElementTreePad();
			left.Label = treePad.Title;
			
			left.Content = treePad.Control;

			DockItem right = dockFrame.AddItem("elementProperty");
			right.DefaultWidth = 250;
			right.Behavior = DockItemBehavior.CantClose;
			right.DefaultLocation = "Document/Right";
			right.DefaultVisible = true;
			right.Visible = true;
			right.Label = "Element Property";
			right.DrawFrame = true;

			PropertyGrid grid = new PropertyGrid();
			//AutomationElementDescriptor d = new AutomationElementDescriptor(AutomationElement.RootElement);
			//grid.CurrentObject = d;
			treePad.SelectAutomationElement += (o, e) => grid.CurrentObject = 
				new Mono.Accessibility.UIAExplorer.Discriptors.AutomationElementDescriptor(e.AutomationElement);

			grid.ShowAll();

			right.Content = grid;
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
			this.SetSizeRequest(800, 600);
			VBox box1 = new VBox();

			MenuBar menubar = CreateMainMenuBar ();
			box1.PackStart (menubar, false, false, 0);

			InitDockFrame();
			box1.PackStart (dockFrame, true, true, 0);

			this.Child = box1;
			this.Title = "UIA Explorer";

			this.DeleteEvent += OnDeleteEvent;

			this.WindowPosition = WindowPosition.Center;
			this.Maximize();
			this.ShowAll();
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
	}
}