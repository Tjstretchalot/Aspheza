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

namespace BaseBuilder.Screens.GameScreens.ToolbarOverlays
{
    public class CaveManWorkerOverlay : ToolbarOverlay
    {
        public CaveManWorkerOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
        }

        protected override bool IsOverlayFor(Entity selected)
        {
            return typeof(CaveManWorker).IsAssignableFrom(selected.GetType());
        }
    }
}
