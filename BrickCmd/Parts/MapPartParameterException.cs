using System;

namespace VEMC.Parts
{
	internal class MapPartParameterException : Exception
	{
		public override string Message
		{
			get
			{
				return this.message;
			}
		}
		public MapPartParameterException(string name, string parameter, Type type)
		{
			this.message = string.Format("Error processing {0}: \"{1}\" ({2}) is invalid", name, parameter, type.Name);
		}
		private string message;
	}
}
