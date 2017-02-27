using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.ToolbarOverlays
{
    /// <summary>
    /// Describes a basic toolbar overlay. This class is not meant to be a 
    /// component of the game itself.
    /// </summary>
    public abstract class ToolbarOverlay : MyGameComponent
    {
        /// <summary>
        /// The background texture to use
        /// </summary>
        protected Texture2D BackgroundTexture;

        /// <summary>
        /// The texture used for a complete progress bar
        /// </summary>
        protected Texture2D FullProgressBarTexture;
        
        /// <summary>
        /// The font used for everything
        /// </summary>
        protected SpriteFont Font;

        /// <summary>
        /// Where would the progress bar go if it were 100% progress
        /// </summary>
        protected Rectangle FullProgressBarRect;

        /// <summary>
        /// What texture do we draw over the progress bar texture for the parts
        /// that are not supposed to be active
        /// </summary>
        protected Texture2D IncompleteProgressBarOverlay;

        /// <summary>
        /// The entity that the toolbar is currently being displayed for
        /// </summary>
        protected Entity CurrentToolbarEntity;
        

        public ToolbarOverlay(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            const int height = 200;
            Init(new Engine.Math2D.PointI2D(0, graphicsDevice.Viewport.Height - height), new Engine.Math2D.PointI2D(graphicsDevice.Viewport.Width, height), 5);
            InitTextures();

            const int progBarWidth = 500;
            const int progBarHeight = 30;
            FullProgressBarRect = new Rectangle(ScreenLocation.X + Size.X / 2 - progBarWidth / 2, ScreenLocation.Y + Size.Y / 3 - progBarHeight / 2, progBarWidth, progBarHeight);
        }

        public override void Draw(RenderContext context)
        {
            if (CurrentToolbarEntity == null)
                return;

            Rectangle destRect = new Rectangle(ScreenLocation.X, ScreenLocation.Y, Size.X, Size.Y);

            SpriteBatch.Draw(BackgroundTexture, destinationRectangle: destRect);
            SpriteBatch.Draw(FullProgressBarTexture, destinationRectangle: FullProgressBarRect);
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            if(localGameState.SelectedEntity != CurrentToolbarEntity)
            {
                if (localGameState.SelectedEntity != null && IsOverlayFor(localGameState.SelectedEntity))
                    UpdateToolbarEntity(localGameState.SelectedEntity);
                else
                    UpdateToolbarEntity(null);
            }
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            if(CurrentToolbarEntity != null)
            {
                if (current.Position.Y >= ScreenLocation.Y)
                    return true;
            }

            return false;
        }

        protected virtual void UpdateToolbarEntity(Entity newEntity)
        {
            CurrentToolbarEntity = newEntity;
        }

        protected virtual void InitTextures()
        {
            BackgroundTexture = new Texture2D(GraphicsDevice, 1, 1);
            BackgroundTexture.SetData(new[] { Color.Black });

            FullProgressBarTexture = new Texture2D(GraphicsDevice, 1, 1);
            FullProgressBarTexture.SetData(new[] { Color.White });

            IncompleteProgressBarOverlay = new Texture2D(GraphicsDevice, 1, 1);
            IncompleteProgressBarOverlay.SetData(new[] { Color.Black });
            
            Font = Content.Load<SpriteFont>("Bitter-Regular");
        }

        public override void Dispose()
        {
            if (FullProgressBarTexture != null)
            {
                FullProgressBarTexture.Dispose();
                FullProgressBarTexture = null;
            }

            if (BackgroundTexture != null)
            {
                BackgroundTexture.Dispose();
                BackgroundTexture = null;
            }

            if(IncompleteProgressBarOverlay != null)
            {
                IncompleteProgressBarOverlay.Dispose();
                IncompleteProgressBarOverlay = null;
            }
        }

        /// <summary>
        /// Determines if this overlay applies for the specified selected entity
        /// </summary>
        /// <param name="selected">The selected entity</param>
        /// <returns>If this toolbar applies to it</returns>
        protected abstract bool IsOverlayFor(Entity selected);
    }
}
