using System;
using System.Collections.Generic;
using System.Text;

namespace ReplOverMdEditor
{
	class HistoryManager
	{
		private List<string> history = new List<string> ();
		private int cursor  = 0;

		public void AppendHistory (string cmd)
		{
			if (!string.IsNullOrEmpty (cmd)) {
				// put cmd to the end of the queue.
				history.Remove (cmd);
				history.Add (cmd);
				cursor = history.Count;
			}
		}

		public string Current
		{
			get
			{
				if (cursor >= 0 && cursor < history.Count)
					return history [cursor];
				else
					return null;
			}
		}

		public bool HistoryUp ()
		{
			if (cursor > 0) {
				cursor--;
				return true;
			} else {
				return false;
			}
		}

		public bool HistoryDown ()
		{
			if (cursor < history.Count - 1) {
				cursor++;
				return true;
			} else {
				return false;
			}
		}
	}
}

