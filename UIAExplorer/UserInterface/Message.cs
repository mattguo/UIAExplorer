using System;

namespace Mono.Accessibility.UIAExplorer.UserInterface
{
	public static class Message
	{
		public static void Error (string format, params object[] args)
		{
			RunModalDialog ("Error", format, args);
		}

		public static void Warn (string format, params object[] args)
		{
			RunModalDialog ("Warning", format, args);
		}

		public static void Info (string format, params object[] args)
		{
			RunModalDialog ("Info", format, args);
		}

		private static void RunModalDialog (string title, string format, params object[] args)
		{
			Gtk.Dialog dlg = new Gtk.Dialog ("UIA Explorer - " + title, null,
			                                 Gtk.DialogFlags.Modal | Gtk.DialogFlags.DestroyWithParent);
			var text = new Gtk.TextView ();
			if (args.Length > 0)
				format = string.Format (format, args);
			text.Buffer.Text = format;
			var scroll = new Gtk.ScrolledWindow ();
			scroll.Add (text);
			dlg.AddButton ("Close", Gtk.ResponseType.Close);
			dlg.VBox.PackStart (scroll, true, true, 0);
			dlg.SetSizeRequest (500, 500);
			scroll.ShowAll ();
			dlg.Run ();
			dlg.Destroy ();
		}
	}
}
