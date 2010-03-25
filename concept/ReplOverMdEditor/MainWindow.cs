using System;
using System.Collections.Generic;
using System.Text;
using Gtk;
using IronPython.Hosting;
using System.Reflection;

namespace ReplOverMdEditor
{
	class MainWindow: Window
	{
		private Assembly [] assemblies;
		private Notebook notebook;

		public MainWindow ()
			: base (WindowType.Toplevel)
		{
			assemblies = new Assembly [] {
				typeof (Gtk.Window).Assembly,
				typeof (Gdk.Window).Assembly,
				typeof (Atk.Action).Assembly,
				typeof (GLib.Object).Assembly
			};

			var box = new VBox ();
			notebook = new Notebook ();
			notebook.ChangeCurrentPage += delegate {
				Focus = CurrentShell;
			};

			notebook.AppendPage (CreateMonoSharpShell (), new Label ("    C#    "));
			notebook.AppendPage (CreateIronPythonShell (), new Label ("IronPython"));

			box.PackStart (notebook, true, true, 0);
//			box.PackStart (CreateMonoSharpShell (), true, true, 0);
//			box.PackStart (CreateIronPythonShell (), true, true, 0);

			Button clearBtn = new Button ();
			clearBtn.SetSizeRequest (80, 25);
			clearBtn.Label = "Clear Text";
			clearBtn.Clicked += delegate {
				var currentShell = CurrentShell;
				currentShell.ClearText ();
				Focus = currentShell;
			};
			box.PackStart (clearBtn, false, false, 0);
			Add (box);
			SetSizeRequest (640, 480);
			Show ();
			Focus = CurrentShell;

			this.DeleteEvent += new DeleteEventHandler (MainWindow_DeleteEvent);
		}

		Widget CreateMonoSharpShell ()
		{
			MonoSharpShell shell = new MonoSharpShell (true, true);
			string script = "using System; using System.Collections.Generic; using Gtk;";
			shell.InitRuntime (assemblies, script);
			ScrolledWindow scroll = new ScrolledWindow ();
			scroll.Add (shell);
			return scroll;
		}

		Widget CreateIronPythonShell ()
		{
			DlrShell shell = new DlrShell ("text/x-python", true, true);
			string script = @"
from System import *
from Gtk import *
print 'Welcome to IronPython programming!!!'";
			shell.InitRuntime (Python.CreateEngine (), assemblies, script);
			ScrolledWindow scroll = new ScrolledWindow ();
			scroll.Add (shell);
			return scroll;
		}

		ReplShellBase CurrentShell
		{
			get {
				return (ReplShellBase) ((ScrolledWindow) notebook.CurrentPageWidget).Child;
			}
		}

		void MainWindow_DeleteEvent (object o, DeleteEventArgs args)
		{
			Application.Quit ();
		}
	}
}
