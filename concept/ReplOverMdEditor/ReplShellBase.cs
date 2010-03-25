using System;
using Mono.TextEditor;
using System.Collections.Generic;
using System.IO;

namespace ReplOverMdEditor
{
	public class ReplShellBase : TextEditor
	{
		public ReplShellBase ()
			: this (false, false)
		{
		}

		public ReplShellBase (bool redirectStdout, bool redirectStderr)
		{
			var options = GetTextEditorData ().Options;
			options.ShowLineNumberMargin = false;
			options.ShowFoldMargin = true;
			options.ShowTabs = true;
			options.ShowIconMargin = true;
			options.ShowSpaces = true;
			options.AutoIndent = false;
			options.IndentationSize = 4;

			repl = new ReplEditMode (this);
			CurrentMode = repl;

			if (redirectStdout) {
				var stdoutStream = new GuiStream (x => Info (x));
				StreamWriter stdoutWriter = new StreamWriter (stdoutStream);
				stdoutWriter.AutoFlush = true;
				Console.SetOut (stdoutWriter);
			}

			if (redirectStderr) {
				var errorStream = new GuiStream (x => Info (x));
				StreamWriter errorWriter = new StreamWriter (errorStream);
				errorWriter.AutoFlush = true;
				Console.SetError (errorWriter);
			}
		}

		public void Output (string str, OutputTextMarker marker)
		{
			int startLine = Caret.Line;
			InsertAtCaret (str);
			for (int i = startLine; i <= Caret.Line; i++)
				pendingMarkers [i] = marker;
		}

		public void Info (string str)
		{
			Output (str, OutputTextMarker.Info);
		}

		public void Error (string str)
		{
			Output (str, OutputTextMarker.Error);
		}

		public void ClearText ()
		{
			repl.ClearText ();
		}

		protected void ApplyPendingMarkers ()
		{
			foreach (var pair in pendingMarkers)
				Document.AddMarker (pair.Key, pair.Value);
			pendingMarkers.Clear ();
		}

		protected ReplEditMode repl;
		private Dictionary<int, OutputTextMarker> pendingMarkers = new Dictionary<int,OutputTextMarker>();
	}
}

