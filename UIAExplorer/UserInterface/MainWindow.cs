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
		private ElementTreePad rawTreePad = null;
		private ElementPropertyPad propPad = null;
		private ElementTestPad testPad = null;
		private ScriptingPad ipyScriptPad = null;
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
			rawTreePad.InitElementTree ();
		}

		private void InitDockFrame ()
		{
			rawTreePad = new ElementTreePad(TreeWalker.RawViewWalker);
			propPad = new ElementPropertyPad ();
			testPad = new ElementTestPad ();
			ipyScriptPad = new ScriptingPad ();

			rawTreePad.SelectAutomationElement += (o, e) => propPad.AutomationElement = e.AutomationElement;
			rawTreePad.SelectAutomationElement += (o, e) => testPad.AutomationElement = e.AutomationElement;
			rawTreePad.SelectAutomationElement += (o, e) => ipyScriptPad.AutomationElement = e.AutomationElement;
			rawTreePad.SelectAutomationElement += new EventHandler<SelectAutomationElementArgs> (ShowElementBound);

			dockFrame = new DockFrame();
			dockFrame.Homogeneous = false;

			DockItem testDockItem = dockFrame.AddItem ("elementTest");
			testDockItem.Behavior = DockItemBehavior.Sticky;
			testDockItem.Label = testPad.Title;
			testDockItem.Content = testPad.Control;
			testDockItem.DrawFrame = true;
			testDockItem.DefaultVisible = true;
			testDockItem.Visible = true;
			testDockItem.Expand = true;
			// TODO set stocked icon.
			//testDockItem.Icon = "";

			DockItem ipyScriptDockItem = dockFrame.AddItem ("ipyScripting");
			ipyScriptDockItem.DefaultLocation = "elementTest/Center";
			ipyScriptDockItem.Behavior = DockItemBehavior.Sticky;
			ipyScriptDockItem.Label = ipyScriptPad.Title;
			ipyScriptDockItem.Content = ipyScriptPad.Control;
			ipyScriptDockItem.DrawFrame = true;
			ipyScriptDockItem.DefaultVisible = true;
			ipyScriptDockItem.Visible = true;

			DockItem rawTreeDockItem = dockFrame.AddItem ("elementTree");
			rawTreeDockItem.DefaultLocation = "elementTest/Left";
			rawTreeDockItem.Behavior = DockItemBehavior.Locked;
			rawTreeDockItem.Label = rawTreePad.Title;
			rawTreeDockItem.Content = rawTreePad.Control;
			rawTreeDockItem.DrawFrame = true;
			rawTreeDockItem.DefaultVisible = true;
			rawTreeDockItem.Visible = true;
			rawTreeDockItem.DefaultWidth = 250;

			DockItem propertyDockItem = dockFrame.AddItem ("elementProperty");
			propertyDockItem.DefaultLocation = "elementTest/Right";
			propertyDockItem.Behavior = DockItemBehavior.Sticky;
			propertyDockItem.Label = propPad.Title;
			propertyDockItem.Content = propPad.Control;
			propertyDockItem.DrawFrame = true;
			propertyDockItem.DefaultVisible = true;
			propertyDockItem.Visible = true;
			propertyDockItem.DefaultWidth = 250;

			DockItem outputDockItem = dockFrame.AddItem ("output");
			outputDockItem.DefaultLocation = "elementTest/Bottom";
			outputDockItem.Behavior = DockItemBehavior.Sticky;
			outputDockItem.Label = "Output";
			outputDockItem.Content = new TextView ();
			outputDockItem.DrawFrame = true;
			outputDockItem.DefaultVisible = true;
			outputDockItem.Visible = true;

			dockFrame.CreateLayout( "uia-explorer", true );
			dockFrame.CurrentLayout = "uia-explorer";
			dockFrame.HandlePadding = 0;
			dockFrame.HandleSize = 6;
		}

		private void ShowElementBound (object sender, SelectAutomationElementArgs e)
		{
			Highlighter h = null;
			if (e.AutomationElement.Current.IsOffscreen)
				h = new Highlighter (-1, -1, -1, -1);
			else {
				var rect = e.AutomationElement.Current.BoundingRectangle;
				if (!rect.IsEmpty)
					h = new Highlighter (
						(int) rect.Left, (int) rect.Top, (int) rect.Width, (int) rect.Height);
				else
					h = new Highlighter (-1, -1, -1, -1);
			}
			h.Flash (1000);
		}

		public MainWindow () : base(Gtk.WindowType.Toplevel)
		{
			this.SetSizeRequest (800, 600);
			box = new VBox ();

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
			// TODO Load the saved layout at next startup.
			dockFrame.SaveLayouts (@"layout.txt");
			Application.Quit ();
			a.RetVal = true;
		}
	}
}