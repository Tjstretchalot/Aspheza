using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Screens.GameScreens
{
    /// <summary>
    /// This component allows the user to chat.
    /// </summary>
    public class ChatOverlay : MyGameComponent
    {
        /// <summary>
        /// A list of (message, gameTimeMessageWasSent) from the shared game state
        /// </summary>
        private List<Tuple<string, int>> RecentMessages;

        public ChatOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Init(new PointI2D(0, (graphicsDevice.Viewport.Height * 2) / 3), new PointI2D(graphicsDevice.Viewport.Width / 3, graphicsDevice.Viewport.Height / 3), 1);
        }

        public override void Draw(RenderContext context)
        {
            if (RecentMessages == null || RecentMessages.Count == 0)
                return;

            var font = context.DefaultFont;
            var lineHeight = font.LineSpacing;

            int x = ScreenLocation.X + 3;
            int y = ScreenLocation.Y + Size.Y - lineHeight;
            
            for(int i = 0; i < RecentMessages.Count && y >= ScreenLocation.Y + lineHeight; i++)
            {
                context.SpriteBatch.DrawString(font, RecentMessages[i].Item1, new Vector2(x, y), Color.White);
                y -= lineHeight;
            }
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext context, int timeMS)
        {
            RecentMessages = sharedGameState.RecentMessages;
        }
    }
}
