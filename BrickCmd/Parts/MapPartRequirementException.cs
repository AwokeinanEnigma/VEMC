using System;

namespace VEMC.Parts
{
	internal class MapPartRequirementException : Exception
	{
		public override string Message
		{
			get
			{
				return this.message;
			}
		}
		public MapPartRequirementException(string name, string key)
		{
			this.message = string.Format("Error processing {0}: \"{1}\" is required", name, key);
		}
		private string message;
	}
}
