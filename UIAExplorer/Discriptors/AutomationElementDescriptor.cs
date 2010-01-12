using System;
using System.Windows.Automation;
using System.Collections.Generic;
using System.ComponentModel;
using Mono.Accessibility.UIAExplorer.UiaUtil;

namespace Mono.Accessibility.UIAExplorer.Discriptors
{
	public class AutomationElementDescriptor : CustomTypeDescriptor
	{
		private AutomationElement element = null;
		public AutomationElementDescriptor (AutomationElement element)
		{
			this.element = element;
		}

		public AutomationElement Element
		{
			get { return element; }
		}

		public override string GetClassName ()
		{
			return typeof(AutomationElement).FullName;
		}
		
		public override string GetComponentName ()
		{
			if (element == null)
				return "(null)";
			return element.Current.Name;
		}

		public override PropertyDescriptorCollection GetProperties ()
		{
			if (element == null)
				return new PropertyDescriptorCollection (new PropertyDescriptor [0]);

			//????? Currently GetSupportedProperties is not supported.
			List<PropertyDescriptor> discriptors = new List<PropertyDescriptor>();
			List<AutomationPattern> supportedPatterns = new List<AutomationPattern> ();
			foreach (AutomationPattern pattern in element.GetSupportedPatterns ()) {
				discriptors.Add(new PatternAvailableIndicator (pattern));
				supportedPatterns.Add (pattern);
				foreach (var property in AutomationPropertyMetadata.GetPatternProperties (pattern))
					discriptors.Add(new AutomationPropertyDescriptor(property));
			}

//			var supportedProperties = element.GetSupportedProperties();
//			foreach (AutomationProperty prop in supportedProperties)
//			{
//				var pattern = AutomationPropertyMetadata.GetPropertyPattern (prop);
//				if (pattern == null || supportedPatterns.Contains (pattern))
//					if (AutomationPropertyMetadata.PropertMetadata.ContainsKey(prop))
//						discriptors.Add(new AutomationPropertyDescriptor(prop));
//			}

			foreach (var catelog in AutomationPropertyMetadata.PredefinedCatelogs) {
				foreach (var property in catelog.Properties) {
					if (element.GetCurrentPropertyValue (property, true) != AutomationElement.NotSupported)
						discriptors.Add(new AutomationPropertyDescriptor(property));
				}
			}

			return new PropertyDescriptorCollection (discriptors.ToArray());
		}

		public override int GetHashCode()
		{
			if (element == null)
				return -1;
			return element.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var target = obj as AutomationElementDescriptor;
			if (target != null)
				return element.Equals(target.element);
			else
				return false;
		}
	}
}