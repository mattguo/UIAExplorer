using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mono.Accessibility.UIAExplorer.Discriptors
{
	public class ParameterDescriptor : PropertyDescriptor
	{
		public ParameterDescriptor (string name, Type type, bool isIn, bool isOut)
			: base (name, new Attribute [0])
		{
			this.ParameterName = name;
			this.ParameterType = type;
			this.IsIn = isIn;
			this.IsOut = isOut;
			this.ParameterValue = DefaultForType (type);
		}

		public string ParameterName { get; private set; }
		public Type ParameterType { get; private set; }
		public object ParameterValue { get; private set; }
		public bool IsIn { get; private set; }
		public bool IsOut { get; private set; }
		
		private static object DefaultForType(Type targetType)
		{
			return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
		}

		public override object GetValue (object component)
		{
			return ParameterValue;
		}
		
		public override bool IsReadOnly {
			get {
				return !IsIn;
			}
		}

		public override bool IsBrowsable {
			get {
				return true;
			}
		}

		public override string Category {
			get {
				return "Parameters";
			}
		}

		public override string DisplayName {
			get {
				return ParameterName;
			}
		}

		public override Type PropertyType {
			get {
				return ParameterType;
			}
		}

		public override Type ComponentType {
			get {
				return typeof (ParameterSetDescriptor);
			}
		}

		public override void SetValue (object component, object value)
		{
			ParameterValue = value;
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
			return false;
		}
	}
}
