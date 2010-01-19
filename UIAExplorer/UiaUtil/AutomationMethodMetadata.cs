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

			//GridItemPattern, no method

			//GridPattern
			patternMethods.Add (GridPattern.Pattern,
				new MethodInfo [] {
					typeof(GridPattern).GetMethod("GetItem")
				});

			//InvokePattern
			patternMethods.Add (InvokePattern.Pattern,
				new MethodInfo [] {
					typeof(InvokePattern).GetMethod("Invoke")
				});

			//MultipleViewPattern
			patternMethods.Add (MultipleViewPattern.Pattern,
				new MethodInfo [] {
					typeof(MultipleViewPattern).GetMethod("GetViewName"),
					typeof(MultipleViewPattern).GetMethod("SetCurrentView")
				});

			//RangeValuePattern
			patternMethods.Add (RangeValuePattern.Pattern,
				new MethodInfo [] {
					typeof(RangeValuePattern).GetMethod("SetValue")
				});

			//ScrollItemPattern
			patternMethods.Add (ScrollItemPattern.Pattern,
				new MethodInfo [] {
					typeof(ScrollItemPattern).GetMethod("ScrollIntoView")
				});
			
			//ScrollPattern
			patternMethods.Add (ScrollPattern.Pattern,
				new MethodInfo [] {
					typeof(ScrollPattern).GetMethod("Scroll"),
					typeof(ScrollPattern).GetMethod("ScrollHorizontal"),
					typeof(ScrollPattern).GetMethod("ScrollVertical"),
					typeof(ScrollPattern).GetMethod("SetScrollPercent"),
				});

			//SelectionItemPattern
			patternMethods.Add (SelectionItemPattern.Pattern,
				new MethodInfo [] {
					typeof(SelectionItemPattern).GetMethod("AddToSelection"),
					typeof(SelectionItemPattern).GetMethod("RemoveFromSelection"),
					typeof(SelectionItemPattern).GetMethod("Select")
				});
			
			//SelectionPattern, no method

			//TableItemPattern, no method
			
			//TablePattern, no method
			patternMethods.Add (TablePattern.Pattern,
				new MethodInfo [] {
					typeof(TablePattern).GetMethod("GetItem")
				});

			//TextPattern
			patternMethods.Add (TextPattern.Pattern,
				new MethodInfo [] {
					typeof(TextPattern).GetMethod("GetSelection"),
					typeof(TextPattern).GetMethod("GetVisibleRanges"),
					typeof(TextPattern).GetMethod("RangeFromChild"),
					typeof(TextPattern).GetMethod("RangeFromPoint")
				});

			//TogglePattern
			patternMethods.Add (TogglePattern.Pattern,
				new MethodInfo [] {
					typeof(TogglePattern).GetMethod("Toggle")
				});

			//TransformPattern
			patternMethods.Add (TransformPattern.Pattern,
				new MethodInfo [] {
					typeof(TransformPattern).GetMethod("Move"),
					typeof(TransformPattern).GetMethod("Resize"),
					typeof(TransformPattern).GetMethod("Rotate"),
				});

			//ValuePattern
			patternMethods.Add (ValuePattern.Pattern,
				new MethodInfo [] {
					typeof(ValuePattern).GetMethod("SetValue")
				});

			//WindowPattern
			patternMethods.Add (WindowPattern.Pattern,
				new MethodInfo [] {
					typeof(WindowPattern).GetMethod("Close"),
					typeof(WindowPattern).GetMethod("SetWindowVisualState"),
					typeof(WindowPattern).GetMethod("WaitForInputIdle")
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
