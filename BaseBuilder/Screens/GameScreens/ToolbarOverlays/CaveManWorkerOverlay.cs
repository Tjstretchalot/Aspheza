using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;
using BaseBuilder.Engine.Utility;

namespace BaseBuilder.Screens.GameScreens.ToolbarOverlays
{
    public class CaveManWorkerOverlay : ToolbarOverlay
    {
        protected InventoryOverlayComponent InventoryOverlayComp;

        public CaveManWorkerOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            const int invWidth = 216;
            const int invHeight = 38;

            Texture2D bkndTexture = new Texture2D(graphicsDevice, 1, 1); // TMP
            bkndTexture.SetData(new[] { Color.DimGray });
            InventoryOverlayComp = new InventoryOverlayComponent(content, graphics, graphicsDevice, spriteBatch, /*content.Load<Texture2D>("name_of_bknd")*/bkndTexture,
                new Rectangle(ScreenLocation.X + Size.X / 2 - invWidth / 2, ScreenLocation.Y + (Size.Y * 2) / 3, invWidth, invHeight),
                new List<Rectangle>
                {
                    new Rectangle(3, 3, 32, 32),
                    new Rectangle(38, 3, 32, 32),
                    new Rectangle(73, 3, 32, 32),
                    new Rectangle(108, 3, 32, 32),
                    new Rectangle(143, 3, 32, 32),
                    new Rectangle(178, 3, 32, 32),
                });
        }


        protected override void DrawImpl(RenderContext context)
        {
            base.DrawImpl(context);
            
            InventoryOverlayComp.Draw(context);
            InventoryOverlayComp.DrawHoverText(context);
        }

        public override void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            base.HandleMouseState(sharedGameState, localGameState, netContext, last, current, ref handled, ref scrollHandled);

            if (CurrentToolbarEntity == null)
            {
                return;
            }

            InventoryOverlayComp.HandleMouseState(sharedGameState, localGameState, netContext, last, current, ref handled, ref scrollHandled);

        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            base.Update(sharedGameState, localGameState, netContext, timeMS);

            InventoryOverlayComp.Update(sharedGameState, localGameState, netContext, timeMS);
        }

        protected override void UpdateToolbarEntity(Entity newEntity)
        {
            base.UpdateToolbarEntity(newEntity);

            var caveman = (CaveManWorker)newEntity;
            InventoryOverlayComp.SetInventory(caveman); // works if null
        }

        protected override bool IsOverlayFor(Entity selected)
        {
            return typeof(CaveManWorker).IsAssignableFrom(selected.GetType());
        }
    }
}
