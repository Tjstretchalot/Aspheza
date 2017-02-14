using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// This class simply draws a grey panel of a certain size. The grey
    /// panel should be used as the background for UI elements, which is itself
    /// on a white background or the game background.
    /// </summary>
    public class GreyPanel : IResizableComponent
    {
        private const int SAMPLES_SQRT = 3; // this number must be odd and increases the number of operations quadratically
        private const int RADIUS = 15 * SAMPLES_SQRT;
        private const int INNER_BORDER = 4 * SAMPLES_SQRT;
        private const int OUTER_BORDER = 2 * SAMPLES_SQRT;

        /// <summary>
        /// The actual location
        /// </summary>
        protected Rectangle _Location;

        /// <summary>
        /// Where the grey panel is located
        /// </summary>
        protected Rectangle Location
        {
            get
            {
                return _Location;
            }
            set
            {
                if (value.Width != _Location.Width || value.Height != _Location.Height)
                {
                    Texture?.Dispose();
                    Texture = null;
                }

                _Location = value;
            }
        }

        /// <summary>
        /// The center of this panel.
        /// </summary>
        public Point Center
        {
            get
            {
                return _Location.Center;
            }

            set
            {
                _Location.X = value.X - Size.X / 2;
                _Location.Y = value.Y - Size.Y / 2;
            }
        }

        /// <summary>
        /// The size of this panel.
        /// </summary>
        public Point Size
        {
            get
            {
                return _Location.Size;
            }
        }

        public Point MinSize
        {
            get { return Point.Zero; }
        }

        public Point MaxSize
        {
            get { return UIUtils.MaxPoint; }
        }
        
        /// <summary>
        /// The texture. 
        /// </summary>
        protected Texture2D Texture;

        /// <summary>
        /// Sets up a grey panel at the specified location
        /// </summary>
        /// <param name="location">The location of the grey panel</param>
        public GreyPanel(Rectangle location)
        {
            _Location = location;
        }

        /// <summary>
        /// Resizes this panel to the specified size.
        /// </summary>
        /// <param name="newSize">The new size of this panel</param>
        public void Resize(Point newSize)
        {
            Location = new Rectangle(Center.X - newSize.X / 2, Center.Y - newSize.Y / 2, newSize.X, newSize.Y);
        }

        /// <summary>
        /// Draws the grey panel
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="graphics">Graphics</param>
        /// <param name="graphicsDevice">Graphics device</param>
        /// <param name="spriteBatch">Sprite batch</param>
        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            if (Texture == null)
                InitTexture(content, graphics, graphicsDevice, spriteBatch);

            spriteBatch.Draw(Texture, destinationRectangle: Location);
        }

        public void Update(ContentManager content, int deltaMS)
        {
        }

        protected virtual void InitTexture(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            // Create a rounded rectangle with a center color of (238, 238, 238)
            // a 2px wide border of white (255, 255, 255) and another border of (153, 153, 153) that's also 2px wide

            var colors = CreateScaledTexture(content, graphics, graphicsDevice, spriteBatch);
            InitTextureByAntialiasingColors(content, graphics, graphicsDevice, spriteBatch, colors);
        }

        private Color[] CreateScaledTexture(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            var textureWidth = Location.Width * SAMPLES_SQRT;
            var textureHeight = Location.Height * SAMPLES_SQRT;

            var colors = new Color[textureWidth * textureHeight];

            var counter = 0;

            var centerColor = new Color(238, 238, 238);
            var firstBorderColor = new Color(255, 255, 255);
            var secondBorderColor = new Color(153, 153, 153);
            var outsideBorderColor = new Color(0, 0, 0, 0);

            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    if (x < OUTER_BORDER || x >= textureWidth - OUTER_BORDER || y < OUTER_BORDER || y >= textureHeight - OUTER_BORDER)
                    {
                        colors[counter] = secondBorderColor;
                    }
                    else if (x < INNER_BORDER || x >= textureWidth - INNER_BORDER || y < INNER_BORDER || y >= textureHeight - INNER_BORDER)
                    {
                        colors[counter] = firstBorderColor;
                    }
                    else
                    {
                        colors[counter] = centerColor;
                    }

                    counter++;
                }
            }

            // round corners
            RoundCorner(true, true, colors, textureWidth, textureHeight, outsideBorderColor, secondBorderColor, firstBorderColor);
            RoundCorner(true, false, colors, textureWidth, textureHeight, outsideBorderColor, secondBorderColor, firstBorderColor);
            RoundCorner(false, true, colors, textureWidth, textureHeight, outsideBorderColor, secondBorderColor, firstBorderColor);
            RoundCorner(false, false, colors, textureWidth, textureHeight, outsideBorderColor, secondBorderColor, firstBorderColor);

            return colors;
        }

        private void RoundCorner(bool top, bool left, Color[] colors, int textureWidth, int textureHeight, Color outsideBorderColor, Color secondBorderColor, Color firstBorderColor)
        {
            var yStart = top ? 0 : textureHeight - RADIUS;
            var yEnd = top ? RADIUS : textureHeight;
            var xStart = left ? 0 : textureWidth - RADIUS;
            var xEnd = left ? RADIUS : textureWidth;

            var point = new Point(left ? RADIUS : textureWidth - RADIUS, top ? RADIUS : textureHeight - RADIUS);

            var cutoffOne = (RADIUS - INNER_BORDER) * (RADIUS - INNER_BORDER);
            var cutoffTwo = (RADIUS - OUTER_BORDER) * (RADIUS - OUTER_BORDER);
            var cutoffThree = RADIUS * RADIUS;
            for (var y = yStart; y < yEnd; y++)
            {
                for (var x = xStart; x < xEnd; x++)
                {
                    var distSqToPoint = (x - point.X) * (x - point.X) + (y - point.Y) * (y - point.Y);
                    var index = y * textureWidth + x;

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
        private void InitTextureByAntialiasingColors(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Color[] colors)
        {

            Func<int, int, Color> pointToColor = (x, y) => colors[y * (Location.Width * SAMPLES_SQRT) + x];

            // Now we're going to anti-alias down to the true size

            var actColors = new Color[Location.Width * Location.Height];

            for (var y = 0; y < Location.Height; y++)
            {
                for (var x = 0; x < Location.Width; x++)
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

                    actColors[y * Location.Width + x] = AverageColor(surrounding);
                }
            }

            Texture = new Texture2D(graphicsDevice, Location.Width, Location.Height);
            Texture.SetData(actColors);
        }

        private static Color AverageColor(List<Color> colors)
        {
            int sumR = 0, sumG = 0, sumB = 0, sumA = 0;

            foreach(var color in colors)
            {
                sumR += color.R;
                sumG += color.G;
                sumB += color.B;
                sumA += color.A;
            }

            return new Color(sumR / colors.Count, sumG / colors.Count, sumB / colors.Count, sumA / colors.Count);
        }
    }
}
