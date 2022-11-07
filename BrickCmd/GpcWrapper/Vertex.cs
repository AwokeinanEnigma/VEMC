using System;

namespace GpcWrapper
{
	public struct Vertex
	{
		public Vertex(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"(",
				this.X.ToString(),
				",",
				this.Y.ToString(),
				")"
			});
		}
		public double X;
		public double Y;
	}
}
