using System;
using System.Collections.Generic;
using System.Text;
using Mono.TextEditor;
using System.Reflection;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;

namespace ReplOverMdEditor
{
	class Shell : TextEditor
	{
		protected ReplEditMode repl;

		public Shell ()
		{
			Document.MimeType = "text/x-python";
			var options = GetTextEditorData ().Options;
			options.ShowLineNumberMargin = false;
			options.ShowFoldMargin = true;
			options.ShowTabs = true;
			options.ShowIconMargin = true;
			options.ShowSpaces = true;
			options.AutoIndent = false;
			options.IndentationSize = 4;
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
			repl = new ReplEditMode (this);
			CurrentMode = repl;
			repl.NewLines += new NewLinesHandler (repl_NewLines);
			repl.StartNewInput ();
		}

		private void Output (string str, OutputTextMarker marker)
		{
			int startLine = Caret.Line;
			InsertAtCaret (str);
			for (int i = startLine; i <= Caret.Line; i++)
				stagedMarkers [i] = marker;
		}

		private void Info (string str)
		{
			Output (str, OutputTextMarker.Info);
			Console.Write (str);
		}

		private void Error (string str)
		{
			Output (str, OutputTextMarker.Error);
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
					RunScript (repl.Script, SourceCodeKind.InteractiveCode);
					repl.StartNewInput ();
				}
			}
		}

		private void RunScript (string script, SourceCodeKind srcKind)
		{
			var source = engine.CreateScriptSourceFromString (script, srcKind);
			try {
				source.Execute (scope);
			} catch (Exception e) {
				Error (e.Message);
			}
			foreach (var pair in stagedMarkers)
				Document.AddMarker (pair.Key, pair.Value);
			stagedMarkers.Clear ();
		}

		#region Fields

		private ScriptEngine engine = null;
		private ScriptScope scope = null;
		private Dictionary<int, OutputTextMarker> stagedMarkers = new Dictionary<int,OutputTextMarker>();
		// TODO private HistoryManager history = new HistoryManager ();

		#endregion
	}
}
