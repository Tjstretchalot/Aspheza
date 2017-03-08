﻿using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Logic.Orders;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Screens.GameScreens;
using BaseBuilder.Screens.GameScreens.TaskOverlays;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BaseBuilder.Engine.Math2D.Double.MathUtilsD2D;

namespace BaseBuilder.Engine.Logic
{
    /// <summary>
    /// The local game logic converts player actions (such as clicking a unit) into modifications to
    /// the local game state (the selected unit). Some local game modifications will propagate through
    /// networking into the shared game state, but this class never directly modifies anything except
    /// for the local game state.
    /// </summary>
    public class LocalGameLogic
    {
        protected Dictionary<Entity, TaskMenuOverlay> TaskComponents;

        protected ContentManager content;
        GraphicsDeviceManager graphics;
        GraphicsDevice graphicsDevice;
        SpriteBatch spriteBatch;
        
        protected int minCameraZoom;
        protected double cameraSpeed;
        protected double cameraZoomSpeed;
        protected double cameraPartialZoom;
        protected PointD2D cameraPartialTopLeft;
        
        MouseState mouseCurr;
        MouseState? mouseLast;
        KeyboardState keyboardCurr;
        KeyboardState? keyboardLast;
        List<Keys> keysPressedReusable;

        public LocalGameLogic(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            this.content = content;
            this.graphics = graphics;
            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;

            cameraSpeed = 0.03;
            minCameraZoom = 32;
            cameraZoomSpeed = 0.01;

            cameraPartialZoom = 8;

            cameraPartialTopLeft = new PointD2D(0, 0);
            keysPressedReusable = new List<Keys>();

            TaskComponents = new Dictionary<Entity, TaskMenuOverlay>();
        }

        public void AddComponent(LocalGameState localGameState, IMyGameComponent component)
        {
            LogicUtils.BinaryInsert(localGameState.Components, component, (c1, c2) => c1.Z - c2.Z); // ascending order
        }

        public void RemoveComponent(LocalGameState localGameState, IMyGameComponent component)
        {
            localGameState.Components.Remove(component);
        }

        public void UpdateComponentZ(LocalGameState localGameState, IMyGameComponent component)
        {
            RemoveComponent(localGameState, component);
            AddComponent(localGameState, component);
        }
        
        /// <summary>
        /// <para>Loop through the game components in order of descending Z and give them a chance to act on the
        /// mouse and keyboard state. Components may specify that they have acted on the state of the mouse
        /// and/or keyboard, in which no further components will be able to act on the part they have handled,
        /// and no other portions of the program should either.</para>
        /// </summary>
        /// <param name="sharedGameState">The shared game state</param>
        /// <param name="localGameState">The local game state</param>
        /// <param name="elapsedMS">The elapsed time</param>
        /// <param name="keyboardHandled"></param>
        /// <param name="mouseHandled"></param>
        protected void UpdateComponents(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int elapsedMS, ref bool keyboardHandled, ref bool mouseHandled)
        {
            if (!mouseLast.HasValue && !keyboardLast.HasValue)
            {
                return;
            }

            if (mouseLast.HasValue && keyboardLast.HasValue)
            {
                var keysCurr = keyboardCurr.GetPressedKeys();

                keysPressedReusable.Clear();
                keysPressedReusable.AddRange(keyboardLast.Value.GetPressedKeys());
                keysPressedReusable.RemoveAll((k) => keysCurr.Contains(k));
                for (int i = localGameState.Components.Count - 1; i >= 0; i--)
                {
                    var comp = localGameState.Components[i];
                    if (!mouseHandled && comp.HandleMouseState(sharedGameState, localGameState, netContext, mouseLast.Value, mouseCurr))
                        mouseHandled = true;
                    if (!keyboardHandled && comp.HandleKeyboardState(sharedGameState, localGameState, netContext, keyboardLast.Value, keyboardCurr, keysPressedReusable))
                        keyboardHandled = true;

                    if (mouseHandled && keyboardHandled)
                        return;
                }
            }

            foreach(var comp in localGameState.Components)
            {
                comp.Update(sharedGameState, localGameState, netContext, elapsedMS);
            }
        }

        /// <summary>
        /// Loop through every entity and update their current task for graphics/audio.
        /// </summary>
        /// <param name="sharedGameState">Shared game state</param>
        /// <param name="localGameState">Local game state</param>
        /// <param name="content">Content</param>
        public void UpdateTasks(SharedGameState sharedGameState, LocalGameState localGameState, ContentManager content)
        {
            foreach(var ent in sharedGameState.World.MobileEntities)
            {
                ent.CurrentTask?.Update(content, sharedGameState, localGameState);
            }

            foreach (var ent in sharedGameState.World.ImmobileEntities)
            {
                ent.CurrentTask?.Update(content, sharedGameState, localGameState);
            }
        }

        /// <summary>
        /// If an entity is selected and the user right clicks this will issue a move order and handle the mouse.
        /// </summary>
        /// <param name="sharedGameState">Shared game staet</param>
        /// <param name="localGameState">Local game state</param>
        /// <param name="netContext">Net context</param>
        /// <param name="elapsedMS">Elapsed time since last frame</param>
        /// <param name="keyboardHandled">If the keyboard has already been handeld</param>
        /// <param name="mouseHandled">If the mouse has already been handled. Move orders require the mouse not to be handled and do handle the mouse.</param>
        protected void CheckForMoveOrder(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int elapsedMS, ref bool keyboardHandled, ref bool mouseHandled)
        {
            if (!mouseLast.HasValue || mouseHandled)
            {
                return;
            }

            var world = sharedGameState.World;
            var camera = localGameState.Camera;

            if ((mouseCurr.RightButton != mouseLast.Value.RightButton) && (mouseLast.Value.RightButton == ButtonState.Pressed))
            {
                double mousePixelX = mouseCurr.Position.X, mousePixelY = mouseCurr.Position.Y;
                double mouseWorldX, mouseWorldY;
                camera.WorldLocationOfPixel(mousePixelX, mousePixelY, out mouseWorldX, out mouseWorldY);
                mouseWorldX = (int)mouseWorldX;
                mouseWorldY = (int)mouseWorldY;
                var mouseWorld = new PointD2D(mouseWorldX, mouseWorldY);

                if (localGameState.SelectedEntity != null && typeof(MobileEntity).IsAssignableFrom(localGameState.SelectedEntity.GetType()))
                {
                    mouseHandled = true;

                    if(EpsilonEqual(localGameState.SelectedEntity.Position.X, mouseWorldX) && EpsilonEqual(localGameState.SelectedEntity.Position.Y, mouseWorldY))
                    {
                        SFXUtils.PlaySAXJingle(content);
                        return;
                    }

                    var tiles = new List<PointI2D>();
                    localGameState.SelectedEntity.CollisionMesh.TilesIntersectedAt(mouseWorld, tiles);
                    foreach(var tile in tiles)
                    {
                        if (sharedGameState.Reserved.Contains(tile))
                        {
                            SFXUtils.PlaySAXJingle(content);
                            return;
                        }
                    }

                    foreach(var ent in sharedGameState.World.GetEntitiesAtLocation(localGameState.SelectedEntity.CollisionMesh, mouseWorld))
                    {
                        if(ent != localGameState.SelectedEntity)
                        {
                            SFXUtils.PlaySAXJingle(content);
                            return;
                        }
                    }

                    //Console.WriteLine($"Issue move order to enitity ID {localGameState.SelectedEntity.ID} To ({mouseWorldX} {mouseWorldY}).");
                    if (!keyboardCurr.IsKeyDown(Keys.LeftShift))
                    {
                        var cancelTasksOrder = netContext.GetPoolFromPacketType(typeof(CancelTasksOrder)).GetGamePacketFromPool() as CancelTasksOrder;
                        cancelTasksOrder.EntityID = localGameState.SelectedEntity.ID;
                        localGameState.Orders.Add(cancelTasksOrder);
                    }
                    var taskOrder = netContext.GetPoolFromPacketType(typeof(IssueTaskOrder)).GetGamePacketFromPool() as IssueTaskOrder;
                    taskOrder.Entity = localGameState.SelectedEntity;
                    taskOrder.Task = new EntityMoveTask(localGameState.SelectedEntity as MobileEntity, new PointI2D((int)mouseWorldX, (int)mouseWorldY));
                    localGameState.Orders.Add(taskOrder);
                }
            }
        }

        /// <summary>
        /// Determines where e1 should go to get to e2
        /// </summary>
        /// <param name="e1">The moving entity</param>
        /// <param name="e2">The not moving entity</param>
        /// <returns>Location that if e1 goes to he will be adjacent to e2</returns>
        public static PointI2D FindDestination(SharedGameState gameState, MobileEntity e1, Thing e2)
        {
            /*
             * Imagine you had two rectangles. Clearly, the only viable locations are either
             * ones that are e1.Height above e2, e1.Height below e2, e1.Width left of e2, or
             * e1.Width right of e2.
             * 
             * This can be generalized to all polygons by projections upon normal axes like so:
             * 
             * ALL potential locations can be found by:
             * 
             * For each line = a line in e1 or e2,
             *   normal = normal of line
             *   
             *   p1 = e1 projected onto normal
             *   p2 = e2 projected onto normal
             *   
             *   pl1 = e1 projected onto line
             *   pl2 = e2 projected onto line
             *   
             *   We are going to construct a line that will be parallel to line and go 
             *   through (p2.Min - p1.length). The lower end of this line will be the same
             *   as line, and the upper end will be the same as line.
             *   
             *   That is to say, we can find a line of potential points by shifting line to
             *   p2.Min - p1.Length 
             *   
             *   Similiarly,
             *   
             *   We can find a line of potential points by shifting line to p2.Max + p1.Length
             */

            PointI2D best = null;
            double bestDistance = -1;

            Action<PointI2D> foundChoice = (p) =>
            {
                if (best != null && bestDistance <= 0)
                    return;

                if(e1.CollisionMesh.Intersects(CollisionMeshD2D.UnitSquare, e1.Position, p))
                {
                    best = p;
                    bestDistance = 0;
                    return;
                }

                var dist = Math.Abs(e1.Position.X - p.X) + Math.Abs(e1.Position.Y - p.Y);

                if(best == null || dist < bestDistance)
                {
                    best = p;
                    bestDistance = dist;
                }
            };

            Func<PointI2D, bool> isValid = (p) =>
            {
                if (gameState.Reserved.Contains(p))
                    return false;

                // Top left
                if (p.X < e2.CollisionMesh.Left + e2.Position.X && p.Y < e2.CollisionMesh.Top + e2.Position.Y)
                    return false;

                // Top right
                if (p.X >= e2.CollisionMesh.Right + e2.Position.X && p.Y < e2.CollisionMesh.Top + e2.Position.Y)
                    return false;

                // Bottom left
                if (p.X < e2.CollisionMesh.Left + e2.Position.X && p.Y >= e2.CollisionMesh.Bottom + e2.Position.Y)
                    return false;

                // Bottom right
                if (p.X >= e2.CollisionMesh.Right + e2.Position.X && p.Y >= e2.CollisionMesh.Bottom + e2.Position.Y)
                    return false;

                foreach (var e in gameState.World.GetEntitiesAtLocation(e1.CollisionMesh, p, true))
                {
                    if (e != e1)
                        return false;
                }

                return true;
            };

            Action<FiniteLineD2D, PointD2D> checkLine = (testLine, shift) =>
            {
                foreach(var pt in testLine.Shift(shift.X, shift.Y).GetTilesIntersected())
                {
                    if (isValid(pt))
                    {
                        foundChoice(pt);
                    }
                }
            };

            Action<FiniteLineD2D, PointD2D> tryLine = (line, shift) =>
            {
                shift = (shift == null ? PointD2D.Origin : shift);

                var movingProjectedOnNormal = e1.CollisionMesh.BoundingBox.ProjectOntoAxis(line.Normal.UnitVector); // UNSHIFTED, DOESNT REQUIRE SHIFT
                var nmovinProjectedOnNormal = e2.CollisionMesh.BoundingBox.ProjectOntoAxis(line.Normal.UnitVector, e2.Position); // SHIFTED

                var movingProjectedOnNormalAsLine = movingProjectedOnNormal.AsFiniteLineD2D(); // UNSHIFTED, DOESNT REQUIRE SHIFT
                var nmovinProjectedOnNormalAsLine = nmovinProjectedOnNormal.AsFiniteLineD2D(); // SHIFTED

                var movingProjectedOnLine = e1.CollisionMesh.BoundingBox.ProjectOntoAxis(line.Axis.UnitVector); // UNSHIFTED, DOESNT REQUIRE SHIFT
                var nmovinProjectedOnLine = e2.CollisionMesh.BoundingBox.ProjectOntoAxis(line.Axis.UnitVector, e2.Position); // SHIFTED


                var linePointInNormal = OneDimensionalLine.DistanceOnAxis(line.Normal.UnitVector, line.Start, shift); // SHIFTED
                
                var destinationLinePointInNormal = nmovinProjectedOnNormal.Min - movingProjectedOnNormal.Length; // SHIFTED - UNSHIFTED = SHIFTED
                var shiftRequiredInNormal = destinationLinePointInNormal - linePointInNormal; // SHIFTED - SHIFTED = SHIFTED
                var shiftRequiredAsLine = new OneDimensionalLine(line.Normal.UnitVector, 0, shiftRequiredInNormal).AsFiniteLineD2D(); // SHIFTED
                var shiftRequiredAsVector = (shiftRequiredAsLine.End - shiftRequiredAsLine.Start).AsVectorD2D(); // SHIFTED
                var slightlyTowardsUsOnNormalVector = line.Normal.UnitVector.Scale(Math.Sign(-shiftRequiredInNormal) * 0.05);

                var resultingLine = new FiniteLineD2D(line.Start + shift + shiftRequiredAsVector, line.End + shift + shiftRequiredAsVector); // SHIFT REQUIRED ON line.Start + line.End -> SHIFTED
                checkLine(resultingLine, slightlyTowardsUsOnNormalVector.AsPointD2D()); // NO SHIFT NECESSARY; ALREADY SHIFTED; BUT WE NEED IT NOT TO BE AN EVEN POINT

                destinationLinePointInNormal = nmovinProjectedOnNormal.Max + movingProjectedOnNormal.Length; // SHIFTED - SHIFTED = SHIFTED
                shiftRequiredInNormal = destinationLinePointInNormal - linePointInNormal; // SHIFTED - SHIFTED = SHIFTED
                shiftRequiredAsLine = new OneDimensionalLine(line.Normal.UnitVector, 0, shiftRequiredInNormal).AsFiniteLineD2D(); // SHIFTED
                shiftRequiredAsVector = (shiftRequiredAsLine.End - shiftRequiredAsLine.Start).AsVectorD2D(); // SHIFTED
                slightlyTowardsUsOnNormalVector = line.Normal.UnitVector.Scale(Math.Sign(-shiftRequiredInNormal) * 0.05);


                resultingLine = new FiniteLineD2D(line.Start + shift + shiftRequiredAsVector, line.End + shift + shiftRequiredAsVector); // SHIFT REQUIRED ON line.Start + line.End -> SHIFTED
                checkLine(resultingLine, slightlyTowardsUsOnNormalVector.AsPointD2D()); // NO SHIFT NECESSARY; ALREADY SHIFTED
            };

            var goodEnoughDistance = 0;// e1.CollisionMesh.MinDistanceTo(e2.CollisionMesh, e1.Position, e2.Position);
            foreach(var line in e2.CollisionMesh.BoundingBox.Lines)
            {
                tryLine(line, e2.Position);
                if (best != null && bestDistance <= goodEnoughDistance)
                    return best;
            }

            return best;
        }

        /// <summary>
        /// If the user has a CaveManWorker selected and right clicks on gold ore, then he is trying to have
        /// the worker dig ore. For right now, this requires that the worker is already close enough to the ore.
        /// </summary>
        /// <param name="sharedGameState">Shared state</param>
        /// <param name="localGameState">Local state</param>
        /// <param name="netContext">Net context</param>
        /// <param name="elapsedMS">Time since last frame in ms</param>
        /// <param name="keyboardHandled">If the keyboard was already handled</param>
        /// <param name="mouseHandled">If the mouse was already handled. Dig orders require mouse is not handled and handle the mouse</param>
        protected void CheckForDigGoldOreOrder(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int elapsedMS, ref bool keyboardHandled, ref bool mouseHandled)
        {
            if (mouseHandled)
                return;

            var worker = localGameState.SelectedEntity as CaveManWorker;
            var vein = localGameState.HoveredEntity as GoldOre;
            if (worker == null || vein == null)
                return;

            if (mouseLast.Value.RightButton != ButtonState.Pressed || mouseCurr.RightButton != ButtonState.Released)
                return;

            if (!keyboardCurr.IsKeyDown(Keys.LeftShift))
            {
                var cancelTasksOrder = netContext.GetPoolFromPacketType(typeof(CancelTasksOrder)).GetGamePacketFromPool() as CancelTasksOrder;
                cancelTasksOrder.EntityID = localGameState.SelectedEntity.ID;
                localGameState.Orders.Add(cancelTasksOrder);
            }

            mouseHandled = true;

            IssueTaskOrder order;
            if (!worker.CollisionMesh.Intersects(vein.CollisionMesh, worker.Position, vein.Position, false))
            {
                order = netContext.GetPoolFromPacketType(typeof(IssueTaskOrder)).GetGamePacketFromPool() as IssueTaskOrder;
                order.Entity = worker;
                order.Task = new EntityMoveTask(worker, FindDestination(sharedGameState, worker, vein));
                localGameState.Orders.Add(order);
                
            }

            order = netContext.GetPoolFromPacketType(typeof(IssueTaskOrder)).GetGamePacketFromPool() as IssueTaskOrder;
            order.Entity = worker;
            order.Task = new EntityMineGoldTask(worker, vein);
            localGameState.Orders.Add(order);
        }

        /// <summary>
        /// If the player has a worker selected and right clicks a harvestable that is ready to harvest,
        /// harvest the thing
        /// </summary>
        /// <param name="sharedGameState"></param>
        /// <param name="localGameState"></param>
        /// <param name="netContext"></param>
        /// <param name="elapsedMS"></param>
        /// <param name="keyboardHandled"></param>
        /// <param name="mouseHandled"></param>
        protected void CheckForHarvestOrder(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int elapsedMS, ref bool keyboardHandled, ref bool mouseHandled)
        {
            if (mouseHandled)
                return;
            var worker = localGameState.SelectedEntity as CaveManWorker;
            var farm = localGameState.HoveredEntity as Harvestable;
            if (worker == null || farm == null || !farm.ReadyToHarvest(sharedGameState))
                return;

            if (mouseLast.Value.RightButton != ButtonState.Pressed || mouseCurr.RightButton != ButtonState.Released)
                return;

            if (!keyboardCurr.IsKeyDown(Keys.LeftShift))
            {
                var cancelTasksOrder = netContext.GetPoolFromPacketType(typeof(CancelTasksOrder)).GetGamePacketFromPool() as CancelTasksOrder;
                cancelTasksOrder.EntityID = localGameState.SelectedEntity.ID;
                localGameState.Orders.Add(cancelTasksOrder);
            }

            mouseHandled = true;

            IssueTaskOrder order;
            if (!worker.CollisionMesh.Intersects(farm.CollisionMesh, worker.Position, farm.Position, false))
            {
                order = netContext.GetPoolFromPacketType(typeof(IssueTaskOrder)).GetGamePacketFromPool() as IssueTaskOrder;
                order.Entity = worker;
                order.Task = new EntityMoveTask(worker, FindDestination(sharedGameState, worker, farm));
                localGameState.Orders.Add(order);

            }

            order = netContext.GetPoolFromPacketType(typeof(IssueTaskOrder)).GetGamePacketFromPool() as IssueTaskOrder;
            order.Entity = worker;
            order.Task = new EntityHarvestTask(worker, farm, 5000);
            localGameState.Orders.Add(order);
        }
        /// <summary>
        /// Checks if an entity was selected this frame. An entity is selected when the user left-clicks while
        /// HoveredEntity is not null.
        /// </summary>
        /// <param name="sharedGameState">The shared game state</param>
        /// <param name="localGameState">The local game state</param>
        /// <param name="elapsedMS">The time in milliseconds since last frame</param>
        /// <param name="keyboardHandled">If the keyboard has already been handled</param>
        /// <param name="mouseHandled">
        /// If the mouse has already been handled. Selecting an entity requires the mouse
        /// is not already handled and does handle the mouse
        /// </param>
        protected void CheckForSelect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int elapsedMS, ref bool keyboardHandled, ref bool mouseHandled)
        {
            if (!mouseLast.HasValue || mouseHandled)
            {
                return;
            }

            if ((mouseCurr.LeftButton != mouseLast.Value.LeftButton) && (mouseLast.Value.LeftButton == ButtonState.Pressed))
            {
                var entity = localGameState.HoveredEntity;
                if (entity == null)
                {
                    if (localGameState.SelectedEntity == null)
                    {
                        return;
                    }

                    localGameState.SelectedEntity.Selected = false;
                    localGameState.SelectedEntity = entity;

                    foreach(var kvp in TaskComponents)
                    {
                        RemoveComponent(localGameState, kvp.Value);
                    }
                    TaskComponents.Clear();
                    return;
                }

                var entityAsMobile = entity as MobileEntity;
                if(entityAsMobile != null && entity != localGameState.SelectedEntity)
                {
                    SFXUtils.PlaySAXJingle(content);

                    if(!TaskComponents.ContainsKey(entityAsMobile))
                    {
                        var comp = new TaskMenuOverlay(content, graphics, graphicsDevice, spriteBatch, localGameState, netContext, entityAsMobile);
                        AddComponent(localGameState, comp);
                        TaskComponents.Add(entityAsMobile, comp);
                    }
                }

                mouseHandled = true;
                if (localGameState.SelectedEntity == null)
                {
                    localGameState.SelectedEntity = entity;
                    entity.Selected = true;
                } else
                {
                    localGameState.SelectedEntity.Selected = false;
                    localGameState.SelectedEntity = entity;
                    entity.Selected = true;
                }
            }
        }
        
        /// <summary>
        /// Updates localGameState.HoveredEntity. An entity is hovered if the mouse is contained within an entity.
        /// </summary>
        /// <param name="sharedGameState">The shared game state</param>
        /// <param name="localGameState">The local game state</param>
        /// <param name="elapsedMS">Elapsed time since last frame in milliseconds</param>
        /// <param name="keyboardHandled">If the keyboard has already been handled</param>
        /// <param name="mouseHandled">If the mouse has already been handled. Hovering requires the mouse was not handled but does not handle the mouse</param>
        protected void CheckForHovering(SharedGameState sharedGameState, LocalGameState localGameState, int elapsedMS, ref bool keyboardHandled, ref bool mouseHandled)
        {
            if (!mouseLast.HasValue || mouseHandled)
            {
                localGameState.HoveredEntity = null;
                return;
            }

            var world = sharedGameState.World;
            var camera = localGameState.Camera;
            
            double mousePixelX = mouseCurr.Position.X, mousePixelY = mouseCurr.Position.Y;
            double mouseWorldX, mouseWorldY;
            camera.WorldLocationOfPixel(mousePixelX, mousePixelY, out mouseWorldX, out mouseWorldY);

            if (mouseWorldX < 0 || mouseWorldY < 0 || mouseWorldX >= world.TileWidth || mouseWorldY >= world.TileHeight)
            {
                localGameState.HoveredEntity = null;
                return;
            }

            var entity = world.GetEntityAtLocation(new PointD2D(mouseWorldX, mouseWorldY));
            
            localGameState.HoveredEntity = entity;
        }

        /// <summary>
        /// Toggles localGameState.CollisionDebug with the Alt+D command. 
        /// </summary>
        /// <param name="sharedGameState">The shared game state</param>
        /// <param name="localGameState">The local game stae</param>
        /// <param name="elapsedMS">The elapsed time in milliseconds</param>
        /// <param name="keyboardHandled">If the keyboard has already been handled; this requires the keyboard is not handled but does not handle the keyboard</param>
        /// <param name="mouseHandled">If the mouse has already been handled</param>
        protected void CheckForCollisionDebugUpdate(SharedGameState sharedGameState, LocalGameState localGameState, int elapsedMS, ref bool keyboardHandled, ref bool mouseHandled)
        {
            if (!keyboardLast.HasValue || keyboardHandled)
            {
                return;
            }

            if ((keyboardCurr.IsKeyDown(Keys.D) != keyboardLast.Value.IsKeyDown(Keys.D)) && !keyboardLast.Value.IsKeyDown(Keys.D) && (keyboardCurr.IsKeyDown(Keys.LeftAlt) || keyboardCurr.IsKeyDown(Keys.RightAlt)))
            {
                localGameState.CollisionDebug = !localGameState.CollisionDebug;
            }
        }
        
        /// <summary>
        /// Updates the camera based on the arrow keys.
        /// </summary>
        /// <param name="sharedGameState">The shared game state.</param>
        /// <param name="localGameState">The local game state</param>
        /// <param name="elapsedMS">Time in milliseconds elapsed since last frame</param>
        /// <param name="keyboardHandled">If the keyboard was already handled. Updating the camera requires the keyboard was not handled, but does not handle the keyboard</param>
        /// <param name="mouseHandled">If the mouse was already handled</param>
        protected virtual void UpdateCamera(SharedGameState sharedGameState, LocalGameState localGameState, int elapsedMS, ref bool keyboardHandled, ref bool mouseHandled)
        {
            if (!mouseLast.HasValue)
            {
                return;
            }

            var world = sharedGameState.World;
            var camera = localGameState.Camera;

            if (!keyboardHandled)
            {
                if (keyboardCurr.IsKeyDown(Keys.Left))
                    cameraPartialTopLeft.X = Math.Max(0, cameraPartialTopLeft.X - cameraSpeed * elapsedMS);
                if (keyboardCurr.IsKeyDown(Keys.Right))
                    cameraPartialTopLeft.X = Math.Min(world.TileWidth - camera.VisibleWorldWidth - 1, cameraPartialTopLeft.X + cameraSpeed * elapsedMS);

                if (keyboardCurr.IsKeyDown(Keys.Up))
                    cameraPartialTopLeft.Y = Math.Max(0, cameraPartialTopLeft.Y - cameraSpeed * elapsedMS);
                if (keyboardCurr.IsKeyDown(Keys.Down))
                    cameraPartialTopLeft.Y = Math.Min(world.TileHeight - camera.VisibleWorldHeight - 1, cameraPartialTopLeft.Y + cameraSpeed * elapsedMS);
            }

            if (!mouseHandled && mouseCurr.ScrollWheelValue != mouseLast.Value.ScrollWheelValue)
            {
                var delta = mouseCurr.ScrollWheelValue - mouseLast.Value.ScrollWheelValue;

                var oldZoom = cameraPartialZoom;
                var oldWorldWidth = camera.VisibleWorldWidth;
                var oldWorldHeight = camera.VisibleWorldHeight;
                var newZoom = cameraPartialZoom + delta * cameraZoomSpeed;

                newZoom = Math.Min(newZoom, minCameraZoom);
                newZoom = Math.Max(Math.Max(newZoom, Math.Ceiling(camera.ScreenLocation.Width / world.TileWidth)), 8); // looks bad less than 8

                double mousePixelX = mouseCurr.Position.X, mousePixelY = mouseCurr.Position.Y;
                double mouseWorldX, mouseWorldY;
                camera.WorldLocationOfPixel(mousePixelX, mousePixelY, out mouseWorldX, out mouseWorldY);

                cameraPartialZoom = newZoom;

                camera.Zoom = Math.Round(cameraPartialZoom);

                double newMouseWorldX, newMouseWorldY;
                camera.WorldLocationOfPixel(mousePixelX, mousePixelY, out newMouseWorldX, out newMouseWorldY);

                cameraPartialTopLeft.X -= (newMouseWorldX - mouseWorldX);
                cameraPartialTopLeft.Y -= (newMouseWorldY - mouseWorldY);

                cameraPartialTopLeft.X = Math.Max(cameraPartialTopLeft.X, 0);
                cameraPartialTopLeft.Y = Math.Max(cameraPartialTopLeft.Y, 0);
                cameraPartialTopLeft.X = Math.Min(cameraPartialTopLeft.X, world.TileWidth - camera.VisibleWorldWidth - 1);
                cameraPartialTopLeft.Y = Math.Min(cameraPartialTopLeft.Y, world.TileHeight - camera.VisibleWorldHeight - 1);
            }

            // the goal is that the camera can move in increments of one pixel. One pixel = (1 world unit / Zoom).
            
            int pixelTopLeftX = (int)Math.Round(cameraPartialTopLeft.X * camera.Zoom);
            int pixelTopLeftY = (int)Math.Round(cameraPartialTopLeft.Y * camera.Zoom);
            camera.WorldTopLeft.X = pixelTopLeftX / camera.Zoom;
            camera.WorldTopLeft.Y = pixelTopLeftY / camera.Zoom;
        }

        /// <summary>
        /// Converts user input into modifications in the local game state.
        /// </summary>
        /// <param name="sharedGameState">The shared game state for reference</param>
        /// <param name="localGameState">The local game state, for reference and modification.</param>
        public void HandleUserInput(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int elapsedMS)
        {
            mouseCurr = Mouse.GetState();
            keyboardCurr = Keyboard.GetState();

            bool keyboardHandled = false, mouseHandled = false;
            
            UpdateComponents(sharedGameState, localGameState, netContext, elapsedMS, ref keyboardHandled, ref mouseHandled);
            CheckForCollisionDebugUpdate(sharedGameState, localGameState, elapsedMS, ref keyboardHandled, ref mouseHandled);
            CheckForHovering(sharedGameState, localGameState, elapsedMS, ref keyboardHandled, ref mouseHandled);
            CheckForSelect(sharedGameState, localGameState, netContext, elapsedMS, ref keyboardHandled, ref mouseHandled);
            UpdateCamera(sharedGameState, localGameState, elapsedMS, ref keyboardHandled, ref mouseHandled);
            CheckForDigGoldOreOrder(sharedGameState, localGameState, netContext, elapsedMS, ref keyboardHandled, ref mouseHandled);
            CheckForHarvestOrder(sharedGameState, localGameState, netContext, elapsedMS, ref keyboardHandled, ref mouseHandled);
            CheckForMoveOrder(sharedGameState, localGameState, netContext, elapsedMS, ref keyboardHandled, ref mouseHandled);

            mouseLast = mouseCurr;
            keyboardLast = keyboardCurr;
        }

        public void CenterCamera(SharedGameState sharedGameState, LocalGameState localGameState)
        {
            cameraPartialTopLeft = new PointD2D(sharedGameState.World.TileWidth / 2 - localGameState.Camera.VisibleWorldWidth / 2, sharedGameState.World.TileHeight / 2 - localGameState.Camera.VisibleWorldHeight / 2);

            int pixelTopLeftX = (int)Math.Round(cameraPartialTopLeft.X * localGameState.Camera.Zoom);
            int pixelTopLeftY = (int)Math.Round(cameraPartialTopLeft.Y * localGameState.Camera.Zoom);
            localGameState.Camera.WorldTopLeft.X = pixelTopLeftX / localGameState.Camera.Zoom;
            localGameState.Camera.WorldTopLeft.Y = pixelTopLeftY / localGameState.Camera.Zoom;
        }
    }
}
