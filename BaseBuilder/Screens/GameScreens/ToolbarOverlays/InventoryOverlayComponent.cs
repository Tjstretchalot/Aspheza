using BaseBuilder.Engine.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.ToolbarOverlays
{
    /// <summary>
    /// Unlike a typical overlay, the inventory overlay component is meant to be used inside
    /// other overlays. It draws an inventory (highly configurable from a texture). The components
    /// of the inventory are just renderables as far as this is concerned. 
    /// </summary>
    public class InventoryOverlayComponent : HoverTextComponent
    {
        /// <summary>
        /// The background texture for the inventory.
        /// </summary>
        protected Texture2D InventoryBackgroundTexture;

        /// <summary>
        /// Where InventoryBackgroundTexture is drawn
        /// </summary>
        protected Rectangle InventoryVisualLocation;

        /// <summary>
        /// 
        /// </summary>
        protected List<Rectangle> InventoryLocations;

        /// <summary>
        /// The items in the inventory
        /// </summary>
        protected Renderable[] InventoryLocationsToItems;

        /// <summary>
        /// The index that is currently hovered on. -1 for nothing
        /// </summary>
        protected int HoveredIndex;

        /// <summary>
        /// Where the mouse is positioned (should be used for drawing hover text, not calculating
        /// what hover text to draw)
        /// </summary>
        protected Point MousePosition;

        public InventoryOverlayComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            Texture2D bkndTexture, Rectangle invVisualLoc, List<Rectangle> invLocs) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            InventoryBackgroundTexture = bkndTexture;
            InventoryVisualLocation = invVisualLoc;
            InventoryLocations = invLocs;
            InventoryLocationsToItems = new Renderable[invLocs.Count];

            HoveredIndex = -1;
            
            Init(new PointI2D(invVisualLoc.X, invVisualLoc.Y), new PointI2D(invVisualLoc.Width, invVisualLoc.Height), 1);
        }

        public void SetItemAt(int index, Renderable renderable)
        {
            InventoryLocationsToItems[index] = renderable;
        }

        public override void Draw(RenderContext context)
        {
            SpriteBatch.Draw(InventoryBackgroundTexture, destinationRectangle: InventoryVisualLocation);

            for(int i = 0; i < InventoryLocations.Count; i++)
            {
                if (InventoryLocationsToItems[i] == null)
                    continue;

                Rectangle rect = InventoryLocations[i];
                InventoryLocationsToItems[i].Render(context, new PointI2D(rect.X + ScreenLocation.X, rect.Y + ScreenLocation.Y), Color.White);
            }
        }

        /// <summary>
        /// Draws the hover text appropriate for this component. Should be called after everything nearby is rendered!
        /// </summary>
        /// <param name="context"></param>
        public void DrawHoverText(RenderContext context)
        {
            if (HoveredIndex == -1)
                return;
            if (InventoryLocationsToItems[HoveredIndex] == null)
                return;

            base.HoverText = InventoryLocationsToItems[HoveredIndex].HoverText;
            base.HoverTextMouseLoc = MousePosition;

            base.Draw(context);
        }

        /// <summary>
        /// Determines if the inventory at the specified index contains the specified point
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="index">Index in inventory locations</param>
        /// <returns>If inventory locations at index contains point</returns>
        protected bool InventoryAtIndexContainsPoint(Point point, int index)
        {
            var inventoryRect = InventoryLocations[index];

            return point.X >= (inventoryRect.Left + ScreenLocation.X) && point.X <= (inventoryRect.Right + ScreenLocation.X) && point.Y >= (inventoryRect.Top + ScreenLocation.Y) && point.Y < +(inventoryRect.Bottom + ScreenLocation.Y);
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            MousePosition = current.Position;

            if(HoveredIndex != -1)
            {
                if (InventoryAtIndexContainsPoint(MousePosition, HoveredIndex))
                    return true;
            }

            for(var i = 0; i < InventoryLocations.Count; i++)
            {
                if (i == HoveredIndex)
                    continue;

                if(InventoryAtIndexContainsPoint(MousePosition, i))
                {
                    HoveredIndex = i;
                    return true;
                }
            }

            HoveredIndex = -1;
            return false;
        }
    }
}
