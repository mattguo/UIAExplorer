using System;
using System.Collections.Generic;
using System.Text;
using Mono.TextEditor;
using System.Reflection;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;

namespace ReplOverMdEditor
{
	public class DlrShell : ReplShellBase
	{
		public DlrShell (string mimeType, bool redirectStdout, bool redirectStderr)
			: base (redirectStdout, redirectStderr)
		{
			Document.MimeType = mimeType;
		}

		public void InitRuntime (ScriptEngine engine, Assembly [] assembliesToLoad, string initScript)
		{
			this.engine = engine;
			scope = engine.CreateScope ();
			engine.Runtime.IO.SetOutput (new GuiStream (x => Info (x)), Encoding.UTF8);
			engine.Runtime.IO.SetErrorOutput (new GuiStream (x => Error (x)), Encoding.UTF8);
			foreach (var assembly in assembliesToLoad)
				engine.Runtime.LoadAssembly (assembly);
			RunScript (initScript, SourceCodeKind.Statements);
			repl.NewLines += new NewLinesHandler (repl_NewLines);
			repl.StartNewInput ();
		}

		void repl_NewLines (ReplEditMode sender, int startLine, int lineCount)
		{
			repl.RepaintPrompts ();
			//if (Caret.Offset == Document.Length && Document.GetLine (startLine).EditableLength == 0) {
			if (Caret.Offset == Document.Length) {
				var lastLine2 = Document.GetLine (Caret.Line - 1);
				var text2 = Document.GetTextAt (lastLine2.Offset, lastLine2.EditableLength).TrimEnd ();
				var lastLine1 = Document.GetLine (Caret.Line);
				var text1 = Document.GetTextAt (lastLine1.Offset, lastLine1.EditableLength).TrimEnd ();
				if (!text2.EndsWith (":") && !text2.StartsWith ("\t")) {
					var script = repl.Script.TrimEnd ();
					RunScript (script, SourceCodeKind.InteractiveCode);
					repl.AppendHistory (script);
					repl.StartNewInput ();
				}
			}
		}

		private bool RunScript (string script, SourceCodeKind srcKind)
		{
			var source = engine.CreateScriptSourceFromString (script, srcKind);
			bool isSuccessful = false;
			try {
				source.Execute (scope);
				isSuccessful = true;
			} catch (Exception e) {
				Error (e.Message);
			}
			ApplyPendingMarkers ();
			return isSuccessful;
		}

		#region Fields

		private ScriptEngine engine = null;
		private ScriptScope scope = null;

		#endregion
	}
}
