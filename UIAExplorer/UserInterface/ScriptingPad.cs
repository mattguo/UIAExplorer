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
	public class ScriptingPad : IDockPad
	{
		#region Private Fields

		private AutomationElement element = null;
		private VBox box = null;
		private IronPythonRepl.Shell ipyShell = null;

		public AutomationElement AutomationElement
		{
			get { return element; }
			set
			{
				element = value;
				ipyShell.SetVariable ("acc", element);
			}
		}

		#endregion

		#region Constructor

		public ScriptingPad ()
		{
			box = new VBox ();
			ipyShell = new IronPythonRepl.Shell ();
			box.PackStart (ipyShell, true, true, 0);
			box.ShowAll ();
		}

		#endregion

		#region IDockPad Members

		public Widget Control
		{
			get { return box; }
		}

		public string Title
		{
			get { return "Script"; }
		}

		#endregion
	}
}
