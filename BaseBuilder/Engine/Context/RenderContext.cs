﻿using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Context
{
    /// <summary>
    /// Contains everything that you might need to render stuff.
    /// </summary>
    public class RenderContext
    {
        public ContentManager Content;
        public GraphicsDeviceManager Graphics;
        public GraphicsDevice GraphicsDevice;
        public SpriteBatch SpriteBatch;
        public Camera Camera;
        public SpriteFont DefaultFont;
        public bool CollisionDebug;

        // should avoid using this
        public SharedGameState __SharedGameState;
    }
}
