using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Automation;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	public static class AutomationMethodMetadata
	{
		private static Dictionary<AutomationPattern, MethodInfo[]> patternMethods;

		static AutomationMethodMetadata ()
		{
			patternMethods = new Dictionary<AutomationPattern, MethodInfo[]> ();
			
			//DockPattern
			patternMethods.Add (DockPattern.Pattern,
				new MethodInfo [] {
					typeof(DockPattern).GetMethod("SetDockPosition")
				});
			
			//ExpandCollapsePattern
			patternMethods.Add (ExpandCollapsePattern.Pattern,
				new MethodInfo [] {
					typeof(ExpandCollapsePattern).GetMethod("Expand"),
					typeof(ExpandCollapsePattern).GetMethod("Collapse")
				});
			
			// TODO more patterns... 
			
			//InvokePattern
			patternMethods.Add (InvokePattern.Pattern,
				new MethodInfo [] {
					typeof(InvokePattern).GetMethod("Invoke")
				});
		}

		public static MethodInfo [] GetPatternMethods (AutomationPattern pattern)
		{
			try {
				return patternMethods [pattern];
			} catch {
				return new MethodInfo [0];
			}
		}
	}
}
