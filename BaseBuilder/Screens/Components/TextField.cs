using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilder.Screens.Components.TextFields
{
    public class TextField : IScreenComponent
    {
        public Rectangle Location
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }

        public void Update(ContentManager content, int deltaMS)
        {
            throw new NotImplementedException();
        }
    }
}
