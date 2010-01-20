using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Mono.Accessibility.UIAExplorer.Discriptors
{
	public class ParameterSetDescriptor : CustomTypeDescriptor
	{
		public ParameterSetDescriptor (MethodInfo method)
		{
			methodName = method.Name;
			foreach (ParameterInfo para in method.GetParameters ()) {
				parameters.Add (new ParameterDescriptor (para.Name, para.ParameterType, para.IsOut));
			}
		}

		private string methodName;

		private List<ParameterDescriptor> parameters = new List<ParameterDescriptor> ();
		public List<ParameterDescriptor> Parameters
		{
			get { return parameters; }
		}

		public override string GetClassName ()
		{
			return GetComponentName ();
		}

		public override string GetComponentName ()
		{
			return string.Format ("Parameters of \"{0}\"", methodName);
		}
		
		public override PropertyDescriptorCollection GetProperties ()
		{
			return new PropertyDescriptorCollection (parameters.ToArray ());
		}
	}
}
