using System;
using Gtk;
using MonoDevelop.Components.Docking;
using Mono.Accessibility.UIAExplorer.UserInterface;

namespace Mono.Accessibility.UIAExplorer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}
