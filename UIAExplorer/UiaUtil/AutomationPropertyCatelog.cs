using System;
using System.Collections.Generic;
using System.Windows.Automation;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	public class AutomationPropertyCatelog
	{
		public AutomationPropertyCatelog (string name)
		{
			this.Name = name;
		}

		public string Name { get; set; }

		//key: property, value: is property shown in the right-side grid.
		private List<AutomationProperty> properties =
			new List<AutomationProperty>();
		public List<AutomationProperty> Properties
		{
			get { return properties; }
		}
	}
}
