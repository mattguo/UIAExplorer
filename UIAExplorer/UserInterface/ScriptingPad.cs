using System;
using System.Windows.Automation;
using AEIds = System.Windows.Automation.AutomationElementIdentifiers;
using Gtk;
using System.IO;
using System.Windows;
using System.Threading;
using Mono.Accessibility.UIAExplorer.UiaUtil;
using System.Collections.Generic;
using System.Reflection;

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

			string title = "IronPython Shell" + Environment.NewLine +
				"Type 'help()' for help." + Environment.NewLine +
				"Enter statements or expressions below.\n";

			Assembly [] assemblies = new Assembly [] {
				Assembly.Load ("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
				Assembly.Load ("WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"),
				Assembly.Load ("UIAutomationClient, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"),
				Assembly.Load ("UIAutomationTypes, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")
			};

			var scriptStream = Assembly.GetExecutingAssembly ().GetManifestResourceStream (
				"Mono.Accessibility.UIAExplorer.InitScript.py");
			StreamReader sr = new StreamReader (scriptStream);
			string initScript = sr.ReadToEnd ();
			sr.Dispose ();
			scriptStream.Dispose ();

			ipyShell = new IronPythonRepl.Shell (assemblies, initScript, title);
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
			get { return "Ipy Script"; }
		}

		#endregion
	}
}
