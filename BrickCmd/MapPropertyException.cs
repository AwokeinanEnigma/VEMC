using System;

namespace VEMC
{
	internal class MapPropertyException : Exception
	{
		public string Property { get; private set; }
		public MapPropertyException(string property) : base("Map property \"" + property + "\" is missing.")
		{
			this.Property = property;
		}
		public MapPropertyException(string property, Exception innerException) : base("Map property \"" + property + "\" is missing.", innerException)
		{
			this.Property = property;
		}
	}
}
