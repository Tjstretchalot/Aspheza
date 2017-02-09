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
    /// <summary>
    /// Describes a one-line text field.
    /// </summary>
    public class TextField : IScreenComponent
    {
        public Rectangle Location
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Text;

        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            
        }

        public void Update(ContentManager content, int deltaMS)
        {
            throw new NotImplementedException();
        }
    }
}
