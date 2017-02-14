using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// Describes a component that is based on a single texture that is
    /// unscaled.
    /// </summary>
    public class TextureComponent : IScreenComponent
    {
        /// <summary>
        /// The texture that represents this component
        /// </summary>
        protected Texture2D Texture;

        /// <summary>
        /// The actual location of this component
        /// </summary>
        protected Rectangle _Location;

        /// <summary>
        /// Gets or sets the location of this component. The width and
        /// height must match the texture width and height.
        /// </summary>
        public Rectangle Location
        {
            get
            {
                return _Location;
            }

            set
            {
                if (value.Width != Texture.Width || value.Height != Texture.Height)
                    throw new InvalidOperationException($"Mismatching sizes, texture is {Texture.Width} x {Texture.Height} but tried to set size to {value.Width} x {value.Height}");

                _Location = value;
            }
        }

        public Point Center
        {
            get
            {
                return _Location.Center;
            }

            set
            {
                _Location.X = value.X - Size.X / 2;
                _Location.Y = value.Y - Size.Y / 2;
            }
        }

        public Point Size
        {
            get
            {
                return _Location.Size;
            }
        }

        /// <summary>
        /// Initialize a new texture component
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="location">The location of this component</param>
        public TextureComponent(Texture2D texture, Rectangle location)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            Texture = texture;
            Location = location;
        }

        public virtual void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, destinationRectangle: _Location);
        }

        public virtual void Update(ContentManager content, int deltaMS)
        {
        }
    }
}
