using BaseBuilder.Engine.Math2D.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine
{
    /// <summary>
    /// Handles operations relating to the fact that the camera can move.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// The top-left of the camera's world position
        /// </summary>
        public PointD2D WorldTopLeft;

        /// <summary>
        /// Where the camera is located on the screen. 
        /// </summary>
        /// <example>
        /// If the game is in fullscreen at 1920x1080 and the entire screen is showing the 
        /// world, then the camera would be a 1920x1080 rectangle starting at (0,0).
        /// 
        /// If the game is in fullscreen at 1920x1080 but the world is being previewed at
        /// the bottom-right quarter, the camera would be a 480x270 rectangle starting at
        /// (1440, 810)
        /// </example>
        public RectangleD2D ScreenLocation;

        /// <summary>
        /// The width of the camera in world units.
        /// </summary>
        public double VisibleWorldWidth
        {
            get
            {
                return ScreenLocation.Width / Zoom;
            }
        }

        /// <summary>
        /// The height of the camera in world units.
        /// </summary>
        public double VisibleWorldHeight
        {
            get
            {
                return ScreenLocation.Height / Zoom;
            }
        }

        /// <summary>
        /// <para>Zoom is the number of pixels that one world unit corresponds to.</para>
        /// 
        /// <para>Zoom can also be thought of as the ratio of screen units to world units.</para>
        /// </summary>
        /// <example>
        /// If Zoom is 16, a (world) unit square will be represented as 16px x 16px.
        /// </example>
        public double Zoom;

        /// <summary>
        /// Initializes a camera with the specified world offset and camera screen
        /// location.
        /// </summary>
        /// <param name="worldTopLeft">The top-left of the camera's world position</param>
        /// <param name="screenLocation">The location of the camera on the screen, in pixels</param>
        /// <param name="zoom">The number of pixels 1 world unit corresponds to</param>
        public Camera(PointD2D worldTopLeft, RectangleD2D screenLocation, double zoom)
        {
            WorldTopLeft = worldTopLeft;
            ScreenLocation = screenLocation;
            Zoom = zoom;
        }

        /// <summary>
        /// Determines if the specified world point is visible on this camera.
        /// </summary>
        /// <param name="p">The world point</param>
        /// <returns>True if it is visible on this camera, false otherwise</returns>
        public bool IsVisible(PointD2D worldPoint)
        {
            return worldPoint.X >= WorldTopLeft.X 
                && worldPoint.Y >= WorldTopLeft.Y
                && worldPoint.X <= (WorldTopLeft.X + VisibleWorldWidth) 
                && worldPoint.Y <= (WorldTopLeft.Y + VisibleWorldHeight);
        }

        /// <summary>
        /// Projects the camera onto the specified axis in world units.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="shift">The position of this camera, null for the origin.</param>
        /// <returns>The projection of this camera onto the specified axis.</returns>
        protected OneDimensionalLine ProjectOntoAxis(VectorD2D axis, PointD2D shift = null)
        {
            // The world unit polygon starts at WorldTopLeft and is VisibleWorldWidth x VisibleWorldHeight.
            // Using this projection function is preferable to creating a polygon so that it isn't absurdly
            // slow to change the zoom or camera position, which would require recalculating the polygon.
            // it consists of 4 vertices:
            //   (WorldTopLeft.X, WorldTopLeft.Y)
            //   (WorldTopLeft.X + VisibleWorldWidth, WorldTopLeft.Y)
            //   (WorldTopLeft.X + VisibleWorldWidth, WorldTopLeft.Y + VisibleWorldHeight)
            //   (WorldTopLeft.X, WorldTopLeft.Y + VisibleWorldHeight)
            
            var min = double.MaxValue;
            var max = double.MinValue;
            
            var px1 = WorldTopLeft.X;
            var py1 = WorldTopLeft.Y;

            var px2 = WorldTopLeft.X + VisibleWorldWidth;
            var py2 = WorldTopLeft.Y;

            var px3 = WorldTopLeft.X + VisibleWorldWidth;
            var py3 = WorldTopLeft.Y + VisibleWorldHeight;

            var px4 = WorldTopLeft.X;
            var py4 = WorldTopLeft.Y + VisibleWorldHeight;

            var val = PointD2D.DotProduct(axis.DeltaX, axis.DeltaY, px1, py1, null, shift);
            min = Math.Min(min, val);
            max = Math.Max(max, val);

            val = PointD2D.DotProduct(axis.DeltaX, axis.DeltaY, px2, py2, null, shift);
            min = Math.Min(min, val);
            max = Math.Max(max, val);

            val = PointD2D.DotProduct(axis.DeltaX, axis.DeltaY, px3, py3, null, shift);
            min = Math.Min(min, val);
            max = Math.Max(max, val);

            val = PointD2D.DotProduct(axis.DeltaX, axis.DeltaY, px3, py3, null, shift);
            min = Math.Min(min, val);
            max = Math.Max(max, val);

            return new OneDimensionalLine(axis, min, max);
        }

        /// <summary>
        /// Determines if the specified polygon is visible on this camera.
        /// </summary>
        /// <param name="worldPoly">The polygon</param>
        /// <param name="polyPosition">The position of the polygon, null for the origin</param>
        /// <returns>True if the polygon is atleast partially visible on this camera</returns>
        public bool IsVisible(PolygonD2D worldPoly, PointD2D polyPosition = null)
        {
            return worldPoly.Intersects(
                ProjectOntoAxis, new List<VectorD2D> { VectorD2D.X_AXIS, VectorD2D.Y_AXIS }, null, polyPosition, true
                );
        }

        /// <summary>
        /// Determines what pixel x corresponds with the specified world x position
        /// on this camera.
        /// </summary>
        /// <param name="worldX">World x position</param>
        /// <returns>Pixel x position</returns>
        public double PixelLocationOfWorldX(double worldX)
        {
            return worldX * Zoom - (WorldTopLeft.X * Zoom) + ScreenLocation.Left;
        }

        /// <summary>
        /// Determines what pixel y corresponds with the specified world y position
        /// on this camera.
        /// </summary>
        /// <param name="worldY">World y position</param>
        /// <returns>Pixel y position</returns>
        public double PixelLocationOfWorldY(double worldY)
        {
            return worldY * Zoom - (WorldTopLeft.Y * Zoom) + ScreenLocation.Top;
        }

        /// <summary>
        /// Determines what pixel position corresponds with the specified world position.
        /// </summary>
        /// <param name="world">World position</param>
        /// <returns>Corresponding pixel position</returns>
        public PointD2D PixelLocationOfWorld(PointD2D world)
        {
            return new PointD2D(PixelLocationOfWorldX(world.X), PixelLocationOfWorldY(world.Y));
        }

        /// <summary>
        /// Determines what pixel position corresponds with the specified world position.
        /// </summary>
        /// <param name="worldX">The world x.</param>
        /// <param name="worldY">The world y.</param>
        /// <param name="pixelX">The variable to set the corresponding pixel x to.</param>
        /// <param name="pixelY">The variable to set the corresponding pixel y to.</param>
        public void PixelLocationOfWorld(double worldX, double worldY, out double pixelX, out double pixelY)
        {
            pixelX = PixelLocationOfWorldX(worldX);
            pixelY = PixelLocationOfWorldY(worldY);
        }

        /// <summary>
        /// Determines what world x position corresponds with the specified pixel x position.
        /// </summary>
        /// <param name="pixelX">The pixel x</param>
        /// <returns>The corresponding world x</returns>
        public double WorldLocationOfPixelX(double pixelX)
        {
            return pixelX / Zoom + WorldTopLeft.X - ScreenLocation.Left / Zoom;
        }

        /// <summary>
        /// Determines what world y position corresponds with the specified pixel y position.
        /// </summary>
        /// <param name="pixelY">The pixel y</param>
        /// <returns>The corresponding world y</returns>
        public double WorldLocationOfPixelY(double pixelY)
        {
            return pixelY / Zoom + WorldTopLeft.Y - ScreenLocation.Top / Zoom;
        }

        /// <summary>
        /// Determines the world position corresponding with the specified pixel position.
        /// </summary>
        /// <param name="pixel">The pixel position</param>
        /// <returns>The world position</returns>
        public PointD2D WorldLocationOfPixel(PointD2D pixel)
        {
            return new PointD2D(WorldLocationOfPixelX(pixel.X), WorldLocationOfPixelY(pixel.Y));
        }

        /// <summary>
        /// Determines the world position corresponding with the specified pixel position.
        /// </summary>
        /// <param name="pixelX">The pixel x position</param>
        /// <param name="pixelY">The pixel y position</param>
        /// <param name="worldX">The variable to set the corresponding world x position to</param>
        /// <param name="worldY">The variable to set the corresponding world y position to</param>
        public void WorldLocationOfPixel(double pixelX, double pixelY, out double worldX, out double worldY)
        {
            worldX = WorldLocationOfPixelX(pixelX);
            worldY = WorldLocationOfPixelY(pixelY);
        }
    }
}
