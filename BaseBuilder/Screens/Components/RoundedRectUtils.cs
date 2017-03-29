using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// This class creates rounded rectangles with a double border
    /// </summary>
    public class RoundedRectUtils
    {
        private const int SAMPLES_SQRT = 3; // this number must be odd and increases the number of operations for the upper-left corner quadratically

        public static Texture2D CreateRoundedRect(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, 
            int width, int height, Color centerColor, Color secondBorderColor, Color firstBorderColor, int radius, int innerBorder, int outerBorder)
        {
            var radiusSc = radius * SAMPLES_SQRT;
            var innerBorderSc = innerBorder * SAMPLES_SQRT;
            var outerBorderSc = outerBorder * SAMPLES_SQRT;
            
            var outsideBorderColor = new Color(0, 0, 0, 0);
            var upperLeftCornerScaled = CreateScaledUpperLeftCorner(content, graphics, graphicsDevice, width, height, centerColor, outsideBorderColor, secondBorderColor,
                firstBorderColor, radiusSc, innerBorderSc, outerBorderSc);
            var upperLeftCorner = AntialiasColors(content, graphics, graphicsDevice, upperLeftCornerScaled, radius, radius);

            var result = new Color[width * height];
            int counter = 0;
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    if (x < outerBorder || x >= width - outerBorder || y < outerBorder || y >= height - outerBorder)
                    {
                        result[counter] = secondBorderColor;
                    }
                    else if (x < innerBorder || x >= width - innerBorder || y < innerBorder || y >= height - innerBorder)
                    {
                        result[counter] = firstBorderColor;
                    }
                    else
                    {
                        result[counter] = centerColor;
                    }

                    counter++;
                }
            }

            for(int y = 0; y < radius; y++)
            {
                for(int x = 0; x < radius; x++)
                {
                    var color = upperLeftCorner[y * radius + x];
                    result[y * width + x] = color;
                    result[(height - y - 1) * width + x] = color;
                    result[(height - y - 1) * width + (width - x - 1)] = color;
                    result[y * width + (width - x - 1)] = color;
                }
            }

            var texture = new Texture2D(graphicsDevice, width, height);
            texture.SetData(result);
            return texture;
        }

        
        public static Color[] CreateScaledUpperLeftCorner(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice,
            int finalWidth, int finalHeight, Color centerColor, Color outsideBorderColor, Color secondBorderColor, Color firstBorderColor, int radius,
            int innerBorder, int outerBorder)
        {
            var textureWidth = finalWidth * SAMPLES_SQRT;
            var textureHeight = finalHeight * SAMPLES_SQRT;
            var scaledRadius = radius;
            Color[] result = new Color[scaledRadius * scaledRadius];
            int counter = 0;
            for(int y = 0; y < scaledRadius; y++)
            {
                for(int x = 0; x < scaledRadius; x++)
                {
                    if (x < outerBorder || y < outerBorder)
                    {
                        result[counter] = secondBorderColor;
                    }
                    else if (x < innerBorder || y < innerBorder)
                    {
                        result[counter] = firstBorderColor;
                    }
                    else
                    {
                        result[counter] = centerColor;
                    }

                    counter++;
                }
            }

            RoundUpperLeftCorner(result, radius, outsideBorderColor, secondBorderColor, firstBorderColor, radius, innerBorder, outerBorder);

            return result;
        }
        
        public static void RoundUpperLeftCorner(Color[] colors, int arrWidth, Color outsideBorderColor, Color secondBorderColor,
            Color firstBorderColor, int radius, int innerBorder, int outerBorder)
        {
            var yStart = 0;
            var yEnd = radius;
            var xStart = 0;
            var xEnd = radius;

            var point = new Point(radius, radius);

            var cutoffOne = (radius - innerBorder) * (radius - innerBorder);
            var cutoffTwo = (radius - outerBorder) * (radius - outerBorder);
            var cutoffThree = radius * radius;
            for (var y = yStart; y < yEnd; y++)
            {
                for (var x = xStart; x < xEnd; x++)
                {
                    var distSqToPoint = (x - point.X) * (x - point.X) + (y - point.Y) * (y - point.Y);
                    var index = y * arrWidth + x;

                    if (distSqToPoint > cutoffThree)
                    {
                        colors[index] = outsideBorderColor;
                    }
                    else if (distSqToPoint > cutoffTwo)
                    {
                        colors[index] = secondBorderColor;
                    }
                    else if (distSqToPoint > cutoffOne)
                    {
                        colors[index] = firstBorderColor;
                    }

                }
            }
        }

        /// <summary>
        /// Super-sampling anti-aliasing. Not exactly ideal but it works.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="graphics"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="colors"></param>
        public static Color[] AntialiasColors(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, Color[] colors, int finalWidth, int finalHeight)
        {
            Func<int, int, Color> pointToColor = (x, y) => colors[y * (finalWidth * SAMPLES_SQRT) + x];

            // Now we're going to anti-alias down to the true size

            var actColors = new Color[finalWidth * finalHeight];

            for (var y = 0; y < finalHeight; y++)
            {
                for (var x = 0; x < finalWidth; x++)
                {
                    var scaledY = y * SAMPLES_SQRT + (SAMPLES_SQRT / 2); // int math flooring is intended
                    var scaledX = x * SAMPLES_SQRT + (SAMPLES_SQRT / 2);

                    // avg the actual point and the surrounding points. If the samples sqrt is 3, then the 
                    // colors has 9x as many colors and that is the number averages together.

                    var surrounding = new List<Color>();

                    for (var tmpX = scaledX - (SAMPLES_SQRT / 2); tmpX <= scaledX + (SAMPLES_SQRT / 2); tmpX++)
                    {
                        for (var tmpY = scaledY - (SAMPLES_SQRT / 2); tmpY <= scaledY + (SAMPLES_SQRT / 2); tmpY++)
                        {
                            surrounding.Add(pointToColor(tmpX, tmpY));
                        }
                    }

                    actColors[y * finalWidth + x] = AverageColor(surrounding);
                }
            }

            return actColors;
        }

        public static Color AverageColor(List<Color> colors)
        {
            int sumR = 0, sumG = 0, sumB = 0, sumA = 0;

            foreach (var color in colors)
            {
                if(color.A != 0)
                {
                    sumR += color.R;
                    sumG += color.G;
                    sumB += color.B;
                    sumA += color.A;
                }
            }

            return new Color(sumR / colors.Count, sumG / colors.Count, sumB / colors.Count, sumA / colors.Count);
        }
    }
}
