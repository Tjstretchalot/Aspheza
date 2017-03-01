using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.ToolbarOverlays
{
    public class StorageBarnOverlay : ToolbarOverlay
    {
        protected InventoryOverlayComponent InventoryOverlayComp;

        public StorageBarnOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            const int invWidth = 356;
            const int invHeight = 146;

            Texture2D bkndTexture = new Texture2D(graphicsDevice, 1, 1); // TMP
            bkndTexture.SetData(new[] { Color.DimGray });

            var srcRects = new List<Rectangle>();
            for(int x = 0; x < (invWidth - 6) / 35; x++)
            {
                for(int y = 0; y < (invHeight - 6) / 35; y++)
                {
                    srcRects.Add(new Rectangle(3 + x * 35, 3 + y * 35, 32, 32));
                }
            }

            InventoryOverlayComp = new InventoryOverlayComponent(content, graphics, graphicsDevice, spriteBatch, /*content.Load<Texture2D>("name_of_bknd")*/bkndTexture,
                new Rectangle(ScreenLocation.X + Size.X / 2 - invWidth / 2, ScreenLocation.Y + Size.Y / 2 - invHeight / 2, invWidth, invHeight),
                srcRects);
        }
        protected override void DrawImpl(RenderContext context)
        {
            base.DrawImpl(context);

            InventoryOverlayComp.Draw(context);
            InventoryOverlayComp.DrawHoverText(context);
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            var res = base.HandleMouseState(sharedGameState, localGameState, netContext, last, current);

            res = InventoryOverlayComp.HandleMouseState(sharedGameState, localGameState, netContext, last, current) || res;

            return res;
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            base.Update(sharedGameState, localGameState, netContext, timeMS);

            InventoryOverlayComp.Update(sharedGameState, localGameState, netContext, timeMS);
        }

        protected override void UpdateToolbarEntity(Entity newEntity)
        {
            base.UpdateToolbarEntity(newEntity);

            var barn = (StorageBarn)newEntity;
            InventoryOverlayComp.SetInventory(barn);
        }

        protected override bool IsOverlayFor(Entity selected)
        {
            return typeof(StorageBarn).IsAssignableFrom(selected.GetType());
        }
    }
}
