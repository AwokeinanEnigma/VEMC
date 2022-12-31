// Decompiled with JetBrains decompiler

using fNbt;
using System.Collections.Generic;
using System.Drawing;
using TiledSharp;

namespace VEMC
{
    internal static class TilesetDatBuilder
    {
        public static NbtFile Build(TmxTileset tmxTileset, OptimizedTileset optTileset)
        {
            NbtFile nbtFile = new NbtFile();
            NbtCompound rootTag = nbtFile.RootTag;
            rootTag.Name = tmxTileset.Name;
            int width = optTileset.Width;
            rootTag.Add(new NbtInt("w", width));
            rootTag.Add(new NbtByteArray("img", optTileset.IndexedImage));
            rootTag.Add(CreatePaletteTag(optTileset.Palette));
            rootTag.Add(CreateAnimationTag(tmxTileset, optTileset));
            return nbtFile;
        }

        private static NbtList CreatePaletteTag(Color[] colors)
        {
            int[] numArray = new int[colors.Length];
            for (int index = 0; index < numArray.Length; ++index)
            {
                numArray[index] = colors[index].ToArgb();
            }

            return new NbtList("pal", new List<NbtIntArray>()
      {
        new NbtIntArray(numArray)
      }, NbtTagType.IntArray);
        }

        private static NbtCompound CreateAnimationTag(
          TmxTileset tmxTileset,
          OptimizedTileset optTileset)
        {
            NbtCompound animData = new NbtCompound("spr");
            int actualId = 0;
            foreach (TmxTilesetTile tile in tmxTileset.Tiles)
            {
                if (tile.Properties.ContainsKey("animid"))
                {
                    // Get the animation properties from the tile's properties
                    int animId = (int)tile.Properties.TryGetDecimal("animid");
                    int frameCount = (int)tile.Properties.TryGetDecimal("frames");
                    int verticalFrameSkip = (int)tile.Properties.TryGetDecimal("vFrameSkip");
                    int horizontalFrameSkip = (int)tile.Properties.TryGetDecimal("hFrameSkip");
                    float speed = (float)tile.Properties.TryGetDecimal("speed");

                    // Calculate the number of tiles per row in the tileset image
                    int tilesPerRow = tmxTileset.Image.Width / tmxTileset.TileWidth;

                    // Calculate the row of the current tile in the tileset image
                    int tileRow = tile.Id / tilesPerRow;

                    // Create an array to store the indices of the frames in the optimized tileset
                    int[] frameIndices = new int[frameCount];

                    // Flag to check if all frames are the same
                    bool allFramesSame = true;

                    // Iterate through the frames and add their indices to the array
                    for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                    {
                        // Calculate the GID of the frame
                        int frameGid = tmxTileset.FirstGid + tile.Id + frameIndex * horizontalFrameSkip;

                        // Calculate the row of the frame in the tileset image
                        int frameRow = frameGid / tilesPerRow;

                        // Calculate the GID of the frame in the optimized tileset
                        int oldGid = frameGid + (frameRow - tileRow) * (tilesPerRow * verticalFrameSkip);

                        // Add the index of the frame to the array
                        frameIndices[frameIndex] = optTileset.Translate(oldGid);

                        // Update the allFramesSame flag
                        allFramesSame &= (frameIndex == 0 || frameIndices[frameIndex - 1] == frameIndices[frameIndex]);
                    }

                    // Check if all frames are the same
                    if (!allFramesSame)
                    {
                        // Add the actual animation ID to the tile's properties
                        tile.Properties.Add("actualid", actualId.ToString());

                        // Create a compound tag to store the animation data
                        NbtCompound animCompound = new NbtCompound(actualId.ToString())
            {
                new NbtIntArray("d", frameIndices),
                new NbtList("spd", new List<NbtFloat>()
                {
                    new NbtFloat(speed)
                })
            };

                        // Add the animation data to the animData compound
                        animData.Add(animCompound);

                        // Increment the animation ID
                        actualId++;
                    }
                }
            }

            // Return the animData compound
            return animData;
        }
    }
}
