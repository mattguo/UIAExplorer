using System;

namespace Mono.Accessibility.UIAExplorer.UserInterface
{
	public static class Message
	{
		public static void Error (string format, params object[] args)
		{
			RunModalDialog (Gtk.MessageType.Error, format, args);
		}

		public static void Warn (string format, params object[] args)
		{
			RunModalDialog (Gtk.MessageType.Warning, format, args);
		}

		public static void Info (string format, params object[] args)
		{
			RunModalDialog (Gtk.MessageType.Info, format, args);
		}

		private static void RunModalDialog (Gtk.MessageType type, string format, params object[] args)
		{
			Gtk.MessageDialog md =
				new Gtk.MessageDialog (null, Gtk.DialogFlags.Modal | Gtk.DialogFlags.DestroyWithParent,
					type, Gtk.ButtonsType.Ok, format, args);
			md.Run ();
			md.Destroy ();
		}
	}
}
