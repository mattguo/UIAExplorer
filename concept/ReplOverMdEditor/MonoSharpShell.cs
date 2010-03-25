using System;
using Mono.CSharp;
using System.Reflection;
using System.IO;

namespace ReplOverMdEditor
{
	public class MonoSharpShell : ReplShellBase
	{
		public MonoSharpShell (bool redirectStdout, bool redirectStderr)
			: base (redirectStdout, redirectStderr)
		{
			Document.MimeType = "text/x-csharp";
		}

		public void InitRuntime (Assembly [] assembliesToLoad, string initScript)
		{
			foreach (var assembly in assembliesToLoad)
				Evaluator.Run (String.Format ("LoadAssembly (\"{0}\");", assembly.FullName));
			Evaluator.Run (initScript);
			repl.NewLines += new NewLinesHandler (repl_NewLines);
			Info ("Welcome to C# Programming");
			ApplyPendingMarkers ();
			repl.StartNewInput ();
		}

		void repl_NewLines (ReplEditMode sender, int startLine, int lineCount)
		{
			repl.RepaintPrompts ();
			if (Caret.Offset == Document.Length) {
				var script = repl.Script.TrimEnd ();
				if (Evaluate (script)){
					repl.AppendHistory (script);
					repl.StartNewInput ();
				}
			}
		}

		public bool Evaluate (string s)
		{
			string res = null;
			object result;
			bool result_set;
			StringWriter errorwriter = new StringWriter ();
			Evaluator.MessageOutput = errorwriter;
			
			try {
				res = Evaluator.Evaluate (s, out result, out result_set);
			} catch (Exception e){
				Error (e.ToString ());
				ApplyPendingMarkers ();
				return true;
			}

			// Partial input
			if (res != null){
				return false;
			}

			string error = errorwriter.ToString ();
			if (error.Length > 0) {
				Error (error);
			} else if (result_set) {
				Info (result.ToString ());
			}
			ApplyPendingMarkers ();
			return true;
		}
	}
}

