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
		public MainWindow ()
			: base (WindowType.Toplevel)
		{
			VBox box = new VBox ();
			ScrolledWindow scroll = new ScrolledWindow ();
			Shell shell = new Shell ();
			
			scroll.Add (shell);
			box.PackStart (scroll, true, true, 0);

			Button clearBtn = new Button ();
			clearBtn.Label = "Clear";
			clearBtn.Clicked += delegate {
				shell.ClearText ();
				Focus = shell;
			};
			box.PackStart (clearBtn, false, false, 0);
			Add (box);
			Show ();
			Focus = shell;
			SetSizeRequest (640, 480);
			this.DeleteEvent += new DeleteEventHandler (MainWindow_DeleteEvent);

			Assembly [] assemblies = new Assembly [] {
				typeof (Gtk.Window).Assembly,
				typeof (Gdk.Window).Assembly,
				typeof (Atk.Action).Assembly,
				typeof (GLib.Object).Assembly
			};
			string script = @"
from System import *
from Gtk import *
print 'Welcome to IronPython programming!!!'";

			shell.InitRuntime (Python.CreateEngine (), assemblies, script);
		}

		void MainWindow_DeleteEvent (object o, DeleteEventArgs args)
		{
			Application.Quit ();
		}
	}
}
