﻿using BaseBuilder.Engine.Utility;
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
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.Logic;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.World.Entities;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferResultDeciders;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors;

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
        /// The current container entity that we are showing
        /// </summary>
        protected Container BaseEntity;

        /// <summary>
        /// The current inventory that we are showing
        /// </summary>
        protected EntityInventory BaseInventory;

        /// <summary>
        /// The index that is currently hovered on. -1 for nothing
        /// </summary>
        protected int HoveredIndex;

        /// <summary>
        /// What item index are we currently "carrying"/dragging. -1 for nothing
        /// </summary>
        protected int CarryingIndex;

        /// <summary>
        /// The offset for rendering the carrying index
        /// </summary>
        protected Point CarryingOffset;

        /// <summary>
        /// Where the mouse is positioned (should be used for drawing hover text, not calculating
        /// what hover text to draw)
        /// </summary>
        protected Point MousePosition;

        // Prevent flickering
        protected int HideIndex;
        protected int HideIndexTimeRemaining;

        public InventoryOverlayComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            Texture2D bkndTexture, Rectangle invVisualLoc, List<Rectangle> invLocs) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            InventoryBackgroundTexture = bkndTexture;
            InventoryVisualLocation = invVisualLoc;
            InventoryLocations = invLocs;
            InventoryLocationsToItems = new Renderable[invLocs.Count];

            HoveredIndex = -1;
            CarryingIndex = -1;
            HideIndex = -1;
            HideIndexTimeRemaining = -1;
            
            Init(new PointI2D(invVisualLoc.X, invVisualLoc.Y), new PointI2D(invVisualLoc.Width, invVisualLoc.Height), 1);
        }

        public void SetInventory(Container container, EntityInventory inventory = null)
        {
            BaseEntity = container;
            if (container != null)
            {
                if (inventory != null)
                    BaseInventory = inventory;
                else
                    BaseInventory = container.Inventory;
            }
            else
                BaseInventory = null;
            for (int i = 0; i < InventoryLocationsToItems.Length; i++)
            {
                InventoryLocationsToItems[i] = (container == null ? null : new InventoryComponentWrapper(BaseInventory, i));
            }
        }

        public override void Draw(RenderContext context)
        {
            SpriteBatch.Draw(InventoryBackgroundTexture, destinationRectangle: InventoryVisualLocation);

            for(int i = 0; i < InventoryLocations.Count; i++)
            {
                if (InventoryLocationsToItems[i] == null || i == CarryingIndex || i == HideIndex)
                    continue;

                Rectangle rect = InventoryLocations[i];
                InventoryLocationsToItems[i].Render(context, new PointI2D(rect.X + ScreenLocation.X, rect.Y + ScreenLocation.Y), Color.White);
            }

            if(CarryingIndex != -1)
            {
                InventoryLocationsToItems[CarryingIndex].Render(context, new PointI2D(MousePosition.X + CarryingOffset.X, MousePosition.Y + CarryingOffset.Y), Color.White);
            }
        }

        /// <summary>
        /// Draws the hover text appropriate for this component. Should be called after everything nearby is rendered!
        /// </summary>
        /// <param name="context"></param>
        public void DrawHoverText(RenderContext context)
        {
            if (HoveredIndex == -1 || CarryingIndex != -1 || HoveredIndex == HideIndex)
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

        protected void HandleHover(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            if (handled)
                return;

            MousePosition = current.Position;

            if (HoveredIndex != -1)
            {
                if (InventoryAtIndexContainsPoint(MousePosition, HoveredIndex))
                {
                    handled = true;
                    return;
                }
            }

            for (var i = 0; i < InventoryLocations.Count; i++)
            {
                if (i == HoveredIndex)
                    continue;

                if (InventoryAtIndexContainsPoint(MousePosition, i))
                {
                    HoveredIndex = i;
                    handled = true;
                    return;
                }
            }

            HoveredIndex = -1;
        }

        protected void HandleCarry(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            if(CarryingIndex != -1 && InventoryLocationsToItems[CarryingIndex] == null)
            {
                CarryingIndex = -1;
                handled = true;
                return;
            }

            if (CarryingIndex != -1)
            {
                var matTup = BaseInventory.MaterialAt(CarryingIndex);
                if (matTup == null)
                {
                    CarryingIndex = -1;
                    handled = true;
                    return;
                }
            }

            if (CarryingIndex == -1 && HoveredIndex != -1 && current.LeftButton == ButtonState.Pressed)
            {
                if (InventoryLocationsToItems[HoveredIndex] == null)
                {
                    handled = true;
                    return;
                }

                var mat = BaseInventory.MaterialAt(HoveredIndex);
                if (mat == null)
                {
                    handled = true;
                    return;
                }

                CarryingIndex = HoveredIndex;
                CarryingOffset = new Point(InventoryLocations[CarryingIndex].Left + ScreenLocation.X - current.Position.X, InventoryLocations[CarryingIndex].Top + ScreenLocation.Y - current.Position.Y);
                handled = true;
                return;
            }
            else if (CarryingIndex != -1 && current.LeftButton == ButtonState.Released)
            {
                double mouseWorldX, mouseWorldY;
                localGameState.Camera.WorldLocationOfPixel(current.Position.X, current.Position.Y, out mouseWorldX, out mouseWorldY);

                if (mouseWorldX < 0 || mouseWorldY < 0 || mouseWorldX >= sharedGameState.World.TileWidth || mouseWorldY >= sharedGameState.World.TileHeight)
                {
                    CarryingIndex = -1;
                    handled = true;
                    return;
                }

                var cont = sharedGameState.World.GetEntityAtLocation(new PointD2D(mouseWorldX, mouseWorldY)) as Container;

                if (cont == null) {
                    CarryingIndex = -1;
                    handled = true;
                    return;
                }

                var cancelTasksOrder = netContext.GetPoolFromPacketType(typeof(CancelTasksOrder)).GetGamePacketFromPool() as CancelTasksOrder;
                cancelTasksOrder.EntityID = BaseEntity.ID;
                localGameState.Orders.Add(cancelTasksOrder);

                if (!BaseEntity.CollisionMesh.Intersects(cont.CollisionMesh, BaseEntity.Position, cont.Position) && !BaseEntity.CollisionMesh.MinDistanceShorterThan(cont.CollisionMesh, 0.8, BaseEntity.Position, cont.Position))
                {
                    var mob = BaseEntity as MobileEntity;

                    if (mob != null)
                    {
                        var dest = LocalGameLogic.FindDestination(sharedGameState, mob, cont);

                        if (dest == null)
                        {
                            CarryingIndex = -1;
                            handled = true;
                            return;
                        } else
                        {
                            var issueMoveOrder = netContext.GetPoolFromPacketType(typeof(IssueTaskOrder)).GetGamePacketFromPool() as IssueTaskOrder;
                            issueMoveOrder.Entity = mob;
                            issueMoveOrder.Task = new EntityMoveTask(mob, dest);
                            localGameState.Orders.Add(issueMoveOrder);
                        }
                    }
                    else
                    {
                        CarryingIndex = -1;
                        handled = true;
                        return;
                    }
                }

                var tup = BaseInventory.MaterialAt(CarryingIndex);
                var mat = tup.Item1;
                var amt = tup.Item2;
                if (!cont.Inventory.HaveRoomFor(mat, amt))
                {
                    CarryingIndex = -1;
                    handled = true;
                    return;
                }
                
                var pool = netContext.GetPoolFromPacketType(typeof(IssueTaskOrder));
                var order = pool.GetGamePacketFromPool() as IssueTaskOrder;
                order.Entity = (Entity)BaseEntity; // BaseEntity, cont, CarryingIndex
                order.Task = new EntityTransferItemTask(BaseEntity.ID, false, new TransferTargetByID(cont.ID), new List<ITransferRestrictor> { new MaterialRestriction(mat, true), new QuantityRestriction(mat, amt) }, new ItemsTransferedResultDecider(null, 1, EntityTaskStatus.Failure));
                localGameState.Orders.Add(order);

                HideIndex = CarryingIndex;
                HideIndexTimeRemaining = 100;
                CarryingIndex = -1;
                handled = true;
                return;
            }
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext context, int timeMS)
        {
            base.Update(sharedGameState, localGameState, context, timeMS);

            if(HideIndexTimeRemaining > 0)
            {
                HideIndexTimeRemaining -= timeMS;
                if(HideIndexTimeRemaining <= 0)
                {
                    HideIndex = -1;
                }
            }
        }

        public override void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            HandleHover(sharedGameState, localGameState, netContext, last, current, ref handled, ref scrollHandled);
            HandleCarry(sharedGameState, localGameState, netContext, last, current, ref handled, ref scrollHandled);
        }
    }
}
