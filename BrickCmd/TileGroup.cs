using System;
using System.Collections.Generic;

namespace VEMC
{
	internal struct TileGroup
	{
		public TileGroup(TileGroup group)
		{
			this.tiles = new List<TileGrouper.TileData>(group.tiles);
			this.depth = group.depth;
			this.x = group.x;
			this.y = group.y;
			this.originX = group.originX;
			this.originY = group.originY;
			this.width = group.width;
			this.height = group.height;
			rainaway = group.rainaway;
		}
		public List<TileGrouper.TileData> tiles;
		public int depth;
		public int x;
		public int y;
		public int originX;
		public int originY;
		public int width;
		public int height;

		public int rainaway;
	}
}
