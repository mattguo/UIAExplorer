using System;
using Gtk;
using MonoDevelop.Components.Docking;
using Mono.Accessibility.UIAExplorer.UserInterface;
using Mono.Accessibility.UIAExplorer.UiaUtil;

namespace Mono.Accessibility.UIAExplorer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			GLib.ExceptionManager.UnhandledException += HandleGLibExceptionManagerUnhandledException;
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}

		static void HandleGLibExceptionManagerUnhandledException (GLib.UnhandledExceptionArgs args)
		{
			Exception exp = args.ExceptionObject as Exception;
			if (exp != null)
				Log.Error (exp.ToString ());
			else
				Log.Error ("Non-DotNet error: {0}", args.ExceptionObject.ToString ());
		}
	}
}
