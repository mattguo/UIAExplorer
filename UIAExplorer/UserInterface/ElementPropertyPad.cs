using System;
using System.Windows.Automation;
using System.Collections.Generic;
using Gtk;
using Mono.Accessibility.UIAExplorer.Discriptors;
using MonoDevelop.Components.PropertyGrid;

namespace Mono.Accessibility.UIAExplorer.UserInterface
{
	public class ElementPropertyPad : IDockPad
	{
		private AutomationElement element = null;
		private VBox box = null;
		private PropertyGrid grid = null;

		public ElementPropertyPad ()
		{
			grid = new PropertyGrid ();
			box = new VBox();
			box.PackStart (grid, true, true, 0);
			box.ShowAll ();
		}
		
		public AutomationElement AutomationElement
		{
			get { return element;}
			set {
				element = value;
				grid.CurrentObject = new AutomationElementDescriptor (element);
			}
		}

		#region IDockPad Members

		public string Title
		{
			get { return "Properties/Patterns"; }
		}

		public Widget Control
		{
			get { return box; }
		}

		#endregion
	}
}
