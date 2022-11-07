using System;

namespace VEMC
{
	internal class MapLayerException : Exception
	{
		public string Layer { get; private set; }
		public MapLayerException(string layer) : base("Map layer \"" + layer + "\" is missing.")
		{
			this.Layer = layer;
		}
		public MapLayerException(string layer, Exception innerException) : base("Map layer \"" + layer + "\" is missing.", innerException)
		{
			this.Layer = layer;
		}
	}
}
