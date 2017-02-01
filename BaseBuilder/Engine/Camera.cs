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
            // the world unit polygon starts at WorldTopLeft and is VisibleWorldWidth x VisibleWorldHeight.
            // it consists of 4 lines:
            //   (WorldTopLeft.X, WorldTopLeft.Y) to (WorldTopLeft.X + VisibleWorldWidth, WorldTopLeft.Y)
            //   (WorldTopLeft.X + VisibleWorldWidth, WorldTopLeft.Y) to (WorldTopLeft.X + VisibleWorldWidth, WorldTopLeft.Y + VisibleWorldHeight)
            //   (WorldTopLeft.X + VisibleWorldWidth, WorldTopLeft.Y + VisibleWorldHeight) to (WorldTopLeft.X, WorldTopLeft.Y + VisibleWorldHeight)
            //   (WorldTopLeft.X, WorldTopLeft.Y + VisibleWorldHeight) to (WorldTopLeft.X, WorldTopLeft.Y)

            var min = double.MaxValue;
            var max = double.MinValue;

            // Line 1
            var px1 = WorldTopLeft.X;
            var py1 = WorldTopLeft.Y;
            var px2 = WorldTopLeft.X + VisibleWorldWidth;
            var py2 = WorldTopLeft.Y;

            var start = axis.DeltaX * (px1 + (shift == null ? 0 : shift.X)) + axis.DeltaY * (py1 + (shift == null ? 0 : shift.Y));
            var end = axis.DeltaX * (px2 + (shift == null ? 0 : shift.X)) + axis.DeltaY * (py2 + (shift == null ? 0 : shift.Y));

            min = Math.Min(start, end);
            max = Math.Max(start, end);

            // Line 2
            px1 = WorldTopLeft.X + VisibleWorldWidth;
            py1 = WorldTopLeft.Y;
            px2 = WorldTopLeft.X + VisibleWorldWidth;
            py2 = WorldTopLeft.Y + VisibleWorldHeight;

            start = axis.DeltaX * (px1 + (shift == null ? 0 : shift.X)) + axis.DeltaY * (py1 + (shift == null ? 0 : shift.Y));
            end = axis.DeltaX * (px2 + (shift == null ? 0 : shift.X)) + axis.DeltaY * (py2 + (shift == null ? 0 : shift.Y));

            min = Math.Min(start, end);
            max = Math.Max(start, end);

            // Line 3
            px1 = WorldTopLeft.X + VisibleWorldWidth;
            py1 = WorldTopLeft.Y + VisibleWorldHeight;
            px2 = WorldTopLeft.X;
            py2 = WorldTopLeft.Y + VisibleWorldHeight;

            start = axis.DeltaX * (px1 + (shift == null ? 0 : shift.X)) + axis.DeltaY * (py1 + (shift == null ? 0 : shift.Y));
            end = axis.DeltaX * (px2 + (shift == null ? 0 : shift.X)) + axis.DeltaY * (py2 + (shift == null ? 0 : shift.Y));

            min = Math.Min(start, end);
            max = Math.Max(start, end);

            // Line 4
            px1 = WorldTopLeft.X;
            py1 = WorldTopLeft.Y + VisibleWorldHeight;
            px2 = WorldTopLeft.X;
            py2 = WorldTopLeft.Y;

            start = axis.DeltaX * (px1 + (shift == null ? 0 : shift.X)) + axis.DeltaY * (py1 + (shift == null ? 0 : shift.Y));
            end = axis.DeltaX * (px2 + (shift == null ? 0 : shift.X)) + axis.DeltaY * (py2 + (shift == null ? 0 : shift.Y));

            min = Math.Min(start, end);
            max = Math.Max(start, end);

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
        
    }
}
