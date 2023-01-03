using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TiledSharp;

namespace VEMC
{
    internal class TilesetOptimizer
    {
        public static OptimizedTileset Optimize(TmxTileset tset)
        {
            // Initialize an OptimizedTileset object
            OptimizedTileset optimizedTileset = new OptimizedTileset();

            // Load the image file into the optimizedTileset object and get the size of the image
            Point imageSize = TilesetOptimizer.LoadImage(tset.Image.Source, tset.Image.Trans, out List<Color> colors, out byte[] pixelColors);
            int imageWidth = imageSize.X;
            int imageHeight = imageSize.Y;

            // Check that the dimensions of the image are multiples of 8
            if (imageWidth % 8 != 0 || imageHeight % 8 != 0)
            {
                string message = string.Format("The dimensions of the tileset image \"{0}\" are not multiples of {1}. Tiles are {1}x{1}, so this size doesn't make sense. Expand the tileset image to the next multiple of {1} and try again.", tset.Image.Source, 8);
                throw new MapBuildException(message);
            }

            // Initialize a dictionary to store the unique tile data and a list to store the tile data
            IDictionary<int, int> uniqueTileData = new Dictionary<int, int>();
            IList<byte[]> tileData = new List<byte[]>();
            int nextTileId = 1;

            // Iterate through the tiles in the image
            for (int y = 0; y < imageHeight; y += 8)
            {
                for (int x = 0; x < imageWidth; x += 8)
                {
                    // Calculate the tile's first global identifier (gid)
                    int tileGid = tset.FirstGid + (y / 8 * (imageWidth / 8)) + (x / 8);

                    // Initialize an array to store the tile data
                    byte[] tileDataArray = new byte[64];
                    int dataIndex = 0;

                    // Iterate through the pixels in the tile and add their colors to the tile data array
                    for (int tileY = y; tileY < y + 8; tileY++)
                    {
                        for (int tileX = x; tileX < x + 8; tileX++)
                        {
                            int pixelIndex = tileY * imageWidth + tileX;
                            if (pixelIndex < pixelColors.Length)
                            {
                                tileDataArray[dataIndex] = pixelColors[pixelIndex];
                            }
                            dataIndex++;
                        }
                    }

                    // Calculate the hash of the tile data
                    int tileDataHash = Utility.Hash(tileDataArray);

                    // Check if the tile data has already been added to the unique tile data dictionary
                    if (!uniqueTileData.ContainsKey(tileDataHash))
                    {
                        // Add the tile data to the unique tile data dictionary and the tile data list
                        uniqueTileData.Add(tileDataHash, nextTileId);
                        optimizedTileset.TranslationTable.Add(tileGid, nextTileId);
                        tileData.Add(tileDataArray);
                        nextTileId++;
                    }
                    else
                    {
                        // Add the tile gid to the translation table using the existing tile id
                        optimizedTileset.TranslationTable.Add(tileGid, uniqueTileData[tileDataHash]);
                    }
                }
            }

            optimizedTileset.Palette = colors.ToArray();

            // Declare variable for the size of the next tile
            int nextTileSize = nextTileId * 8 * 8;

            // Declare variable to store the size of the optimized tileset
            int optimizedTilesetSize = 0;

            // Find the size of the optimized tileset that can hold all the tiles
            // Iterate through all the possible sizes in the TILESET_SIZES array
            for (int i = 0; i < TilesetOptimizer.TILESET_SIZES.Length; i++)
            {
                // If the size of the next tile fits in the current size of the optimized tileset
                if (nextTileSize < TilesetOptimizer.TILESET_SIZES[i] * TilesetOptimizer.TILESET_SIZES[i])
                {
                    // Store the size of the optimized tileset
                    optimizedTilesetSize = TilesetOptimizer.TILESET_SIZES[i];
                    // Break out of the loop
                    break;
                }
            }

            // If a suitable size for the optimized tileset was found
            if (optimizedTilesetSize > 0)
            {
                // Declare a byte array to store the indexed image data for the optimized tileset
                byte[] indexedImageData = new byte[optimizedTilesetSize * optimizedTilesetSize];

                // Iterate through all the tiles in the tileData array except for the last one
                for (int i = 0; i < nextTileId - 1; i++)
                {
                    // Calculate the x position of the tile in the optimized tileset
                    int xPos = i * 8 % optimizedTilesetSize;
                    // Calculate the y position of the tile in the optimized tileset
                    int yPos = i * 8 / optimizedTilesetSize * 8;

                    // Iterate through all the pixels in the tile
                    for (int j = 0; j < 64; j++)
                    {
                        // Calculate the x position of the pixel within the tile
                        int pixelXPos = j % 8;
                        // Calculate the y position of the pixel within the tile
                        int pixelYPos = j / 8;
                        // Calculate the index of the pixel in the indexedImageData array
                        int pixelIndex = (yPos + pixelYPos) * optimizedTilesetSize + (xPos + pixelXPos);
                        // Set the pixel value in the indexedImageData array
                        indexedImageData[pixelIndex] = tileData[i][j];
                    }
                }

                // Set the width and indexed image data of the optimized tileset object
                optimizedTileset.Width = optimizedTilesetSize;
                optimizedTileset.IndexedImage = indexedImageData;

                // Return the optimized tileset object
                return optimizedTileset;
            }

            throw new InvalidOperationException(string.Format("The tileset is too large to be stored in a {0}x{0} pixel image.", TilesetOptimizer.TILESET_SIZES[TilesetOptimizer.TILESET_SIZES.Length - 1]));
        }
        private static Point LoadImage(string file, TmxColor transColor, out List<Color> colors, out byte[] pixelcols)
        {
            // Load the image file into a Bitmap object
            Bitmap bitmap = new Bitmap(file);

            // Create a copy of the Bitmap in 32bppARGB format
            Bitmap bitmap2 = bitmap.Clone(new Rectangle(Point.Empty, bitmap.Size), PixelFormat.Format32bppArgb);

            // Initialize lists for colors and pixel colors
            colors = new List<Color>();
            pixelcols = new byte[bitmap2.Width * bitmap2.Height];

            // Lock the bitmap data for reading
            Rectangle rect = new Rectangle(0, 0, bitmap2.Width, bitmap2.Height);
            BitmapData bitmapData = bitmap2.LockBits(rect, ImageLockMode.ReadOnly, bitmap2.PixelFormat);

            // Copy the pixel data into a byte array
            IntPtr scan = bitmapData.Scan0;
            int num = Math.Abs(bitmapData.Stride) * bitmap2.Height;
            byte[] array = new byte[num];
            Marshal.Copy(scan, array, 0, num);

            // Unlock the bitmap data
            bitmap2.UnlockBits(bitmapData);

            // Check that the image is in 32bppARGB format
            if (bitmap2.PixelFormat == PixelFormat.Format32bppArgb)
            {
                // Iterate through the pixel data and add the colors to the list
                for (int i = 0; i < num; i += 4)
                {
                    Color color = Color.FromArgb(array[i + 3], array[i + 2], array[i + 1], array[i]);
                    if (!colors.Contains(color))
                    {
                        colors.Add(color);
                    }
                    pixelcols[i / 4] = (byte)colors.IndexOf(color);
                }

                // Check if there are more than 256 colors in the image
                if (colors.Count > 256)
                {
                    ConsoleColor foregroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Warning: There are more than 256 colors in \"{0}\"", Path.GetFileName(file));
                    Console.ForegroundColor = foregroundColor;
                }

                // Check if the transparent color is in the list of colors
                Color transparentColor = Color.FromArgb(255, transColor.R, transColor.G, transColor.B);
                int transparentColorIndex = colors.IndexOf(transparentColor);
                if (transparentColorIndex >= 0)
                {
                    // Set the transparent color to transparent
                    colors[transparentColorIndex] = Color.Transparent;
                }

                // Return the size of the image
                return new Point(bitmap2.Width, bitmap2.Height);
            }
            else
            {
                // Throw an exception if the image is not in 32bppARGB format
                throw new MapBuildException("Tileset image is not 32bppARGB, but it should be at this point. Bug Enigma!");
            }
        }
        private const int TILE_SIZE = 8;
        private static readonly int[] TILESET_SIZES = new int[]
        {
            32,
            64,
            128,
            256,
            384,
            480,
            512,
            640,
            768,
            1024
        };
    }
}
