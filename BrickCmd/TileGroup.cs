using System.Collections.Generic;

namespace VEMC
{
    internal struct TileGroup
    {
        public TileGroup(TileGroup group)
        {
            tiles = new List<TileGrouper.TileData>(group.tiles);
            depth = group.depth;
            x = group.x;
            y = group.y;
            originX = group.originX;
            originY = group.originY;
            width = group.width;
            height = group.height;
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
