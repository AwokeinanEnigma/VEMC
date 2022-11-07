using System;

namespace VEMC.Parts
{
	internal class MapPartConversionException : Exception
	{
		public override string Message
		{
			get
			{
				return this.message;
			}
		}
		public MapPartConversionException(string name, string key, Type expectedType)
		{
			this.message = string.Format("Error processing {0}: Cannot convert \"{1}\" to a {2}", name, key, expectedType.Name);
		}
		private string message;
	}
}
