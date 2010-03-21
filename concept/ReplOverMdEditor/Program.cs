using System;
using System.Collections.Generic;
using System.Text;
using Gtk;

namespace ReplOverMdEditor
{
	class Program
	{
		static void Main (string [] args)
		{
			Application.Init ();
			var wnd = new MainWindow ();
			wnd.ShowAll ();
			Application.Run ();
		}
	}
}
