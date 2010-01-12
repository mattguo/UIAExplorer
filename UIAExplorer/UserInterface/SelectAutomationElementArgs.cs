using System;
using System.Windows.Automation;

namespace Mono.Accessibility.UIAExplorer.UserInterface
{
	public class SelectAutomationElementArgs : EventArgs
	{
		public SelectAutomationElementArgs(AutomationElement ae)
		{
			this.AutomationElement = ae;
		}

		public AutomationElement AutomationElement { get; protected set; }
	}

}
