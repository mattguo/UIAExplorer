using System;
using System.Text;
using System.Windows.Automation;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	public static class StringFormatter
	{
		// This method requires the input AutomationElement supports GetCurrentPropertyValue (i.e. not cached)
		public static string Format (AutomationElement element)
		{
			string name = element.Current.Name;
			return string.Format("\"{0}\", {1}, {2}",
				name,
				StringFormatter.Format (element.Current.ControlType),
				StringFormatter.Format (element.GetRuntimeId ()));
		}

		public static string Format (ControlType ctype)
		{
			string fullName = ctype.ProgrammaticName;
			int lastDotIndex = fullName.LastIndexOf('.');
			return fullName.Substring(lastDotIndex + 1);
		}

		public static string Format (Array arr)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("[");
			foreach (object item in arr)
				sb.AppendFormat ("{0}, ", item);
			if (arr.Length > 0)
				sb.Remove (sb.Length - 2, 2);
			sb.Append ("]");
			return sb.ToString ();
		}
	}
}
