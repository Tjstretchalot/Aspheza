using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Screens.GameScreens
{
    /// <summary>
    /// Shows the resources in the upper-right
    /// </summary>
    public class ResourceOverlay : MyGameComponent
    {
        private MaterialManager MatManager;

        public ResourceOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Init(new PointI2D(graphicsDevice.Viewport.Width - 100, 5), new PointI2D(95, 16), 3);
        }

        public override void Draw(RenderContext context)
        {
            if (MatManager == null)
                return;
            RenderMaterial(context, ScreenLocation.X, ScreenLocation.Y, Material.GoldOre);
        }

        private void RenderMaterial(RenderContext context, int x, int y, Material mat)
        {
            mat.Render(context, new PointD2D(x, y), Color.White);

            var str = $"{MatManager.AmountOf(mat)}";
            var strHeight = context.DefaultFont.MeasureString(str).Y;
            context.SpriteBatch.DrawString(context.DefaultFont, str, new Vector2(x + 36, y + 16 - (strHeight / 2)), Color.White);
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            MatManager = sharedGameState.Resources;
        }
    }
}
