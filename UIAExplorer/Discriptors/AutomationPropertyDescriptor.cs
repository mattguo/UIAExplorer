using System;
using System.Windows.Automation;
using System.Collections.Generic;
using System.ComponentModel;
using Mono.Accessibility.UIAExplorer.UiaUtil;

namespace Mono.Accessibility.UIAExplorer.Discriptors
{
	public class AutomationPropertyDescriptor : PropertyDescriptor
	{
		private AutomationProperty property = null;

		public AutomationPropertyDescriptor(
			AutomationProperty property)
			: base (property.ProgrammaticName, new Attribute[0])
		{
			this.property = property;
		}
		
		private AutomationPropertyMetadata PropertyMetadata
		{
			get {
				return AutomationPropertyMetadata.GetMetadata(property);
			}
		}

		public override object GetValue (object component)
		{
			object val = ((AutomationElementDescriptor)component).Element.GetCurrentPropertyValue(property);

			if (val == AutomationElement.NotSupported)
				return "Not Supported";
			if (val == null) {
				Log.Error ("There shall not be any value equals to null.");
				return "(null)";
			}
			else if (val is bool)
				return val;
			else if (val is ControlType)
				return StringFormatter.Format ((ControlType) val);
			else if (val is AutomationElement)
				return StringFormatter.Format ((AutomationElement) val);
			else if (val is Array)
				return StringFormatter.Format ((Array) val);
			else
				return val.ToString ();
		}

		public override bool IsReadOnly {
			get {
				return true;
			}
		}
		
		public override string Category {
			get {
				return PropertyMetadata.Catelog.Name;
			}
		}

		public override string Description {
			get {
				return PropertyMetadata.Description;
			}
		}
		public override bool IsBrowsable
		{
			get
			{
				return PropertyMetadata.Browsable;
			}
		}

		public override string DisplayName {
			get {
				return PropertyMetadata.DisplayName;
			}
		}

		public override Type PropertyType {
			get {
				// TODO currently we return every property as string or bool.
				Type type = null;
				try
				{
					type = AutomationPropertyMetadata.PropertMetadata[property].Type;
				}
				catch (Exception)
				{
					Console.Error.WriteLine("Can't get the type of {0}", property.ProgrammaticName);
					throw;
				}
				if (type != typeof (bool))
					type = typeof (string);
				return type;
			}
		}

		public override Type ComponentType {
			get {
				return typeof (AutomationElementDescriptor);
			}
		}

		public override void SetValue (object component, object value)
		{
			//throw new System.NotImplementedException ();
		}

		public override void ResetValue (object component)
		{
			//throw new System.NotImplementedException();
		}

		public override bool CanResetValue (object component)
		{
			return false;
		}

		public override bool ShouldSerializeValue (object component)
		{
			return true;
		}

		public override int GetHashCode()
		{
			return property.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var target = obj as AutomationPropertyDescriptor;
			if (target != null)
				return property.Equals(target.property);
			else
				return false;
		}
	}
}
