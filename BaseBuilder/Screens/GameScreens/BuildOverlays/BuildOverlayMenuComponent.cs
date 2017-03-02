using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.Utilities;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    /// <summary>
    /// This draws the menu for the build overlay and is not used as a standalone
    /// game component.
    /// </summary>
    public class BuildOverlayMenuComponent : HoverTextComponent
    {
        /// <summary>
        /// The list of unbuilt entities that can be chosen from
        /// </summary>
        protected List<Tuple<UnbuiltImmobileEntity, int>> ChoicesAndYSpacing;

        /// <summary>
        /// The camera for this menu.
        /// </summary>
        protected Camera MenuCamera;

        /// <summary>
        /// My own render context!
        /// </summary>
        protected RenderContext MyContext;

        /// <summary>
        /// The currently selected index
        /// </summary>
        protected int CurrentIndex;

        public UnbuiltImmobileEntity Current
        {
            get
            {
                if (CurrentIndex == -1)
                    return null;

                return ChoicesAndYSpacing[CurrentIndex].Item1;
            }
        }

        /// <summary>
        /// The background texture
        /// </summary>
        protected Texture2D MenuBackgroundTexture;

        public BuildOverlayMenuComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Init(new PointI2D(graphicsDevice.Viewport.Width - 200, 25), new PointI2D(200, graphicsDevice.Viewport.Height - 250), 2);

            MenuBackgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            MenuBackgroundTexture.SetData(new[] { Color.Gray });

            MenuCamera = new Camera(new PointD2D(0, 0), new RectangleD2D(Size.X, Size.Y, ScreenLocation.X, ScreenLocation.Y), 16);
            CurrentIndex = -1;
        }

        public override void Draw(RenderContext context)
        {
            if (ChoicesAndYSpacing == null)
                return;

            MyContext.Camera = MenuCamera;
            MyContext.Content = context.Content;
            MyContext.Graphics = context.Graphics;
            MyContext.GraphicsDevice = context.GraphicsDevice;
            MyContext.SpriteBatch = context.SpriteBatch;
            MyContext.DefaultFont = context.DefaultFont;
            MyContext.CollisionDebug = context.CollisionDebug;

            SpriteBatch.Draw(MenuBackgroundTexture, new Rectangle(ScreenLocation.X, ScreenLocation.Y, Size.X, Size.Y), Color.White);
            
            PointD2D drawPoint = new PointD2D(MenuCamera.ScreenLocation.Left, MenuCamera.ScreenLocation.Top + 5);
            var mouseState = Mouse.GetState();
            HoverText = null;
            for (int i = 0; i < ChoicesAndYSpacing.Count; i++)
            {
                double trueWidth = (ChoicesAndYSpacing[i].Item1.CollisionMesh.Right - ChoicesAndYSpacing[i].Item1.CollisionMesh.Left) * MenuCamera.Zoom;
                drawPoint.X = ScreenLocation.X + Size.X / 2 - trueWidth / 2;
                if (mouseState.Position.X >= drawPoint.X && mouseState.Position.X <= drawPoint.X + trueWidth
                    && mouseState.Position.Y >= drawPoint.Y && mouseState.Position.Y <= drawPoint.Y + ChoicesAndYSpacing[i].Item2)
                {
                    HoverText = ChoicesAndYSpacing[i].Item1.UnbuiltHoverText;

                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        CurrentIndex = i;
                    }
                }


                ChoicesAndYSpacing[i].Item1.Render(MyContext, drawPoint, CurrentIndex == i ? Color.Azure : Color.White);
                drawPoint.Y += ChoicesAndYSpacing[i].Item2;
            }

            HoverTextMouseLoc = mouseState.Position;
            base.Draw(context);
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            if (ChoicesAndYSpacing == null)
            {
                ChoicesAndYSpacing = new List<Tuple<UnbuiltImmobileEntity, int>>
                {
                      Tuple.Create((UnbuiltImmobileEntity)new UnbuiltImmobileEntityAsDelegator(() => new StorageBarn(new PointD2D(0, 0), sharedGameState.GetUniqueEntityID(), Direction.Right)), 74 + 3 + 5),
                      Tuple.Create((UnbuiltImmobileEntity)new UnbuiltImmobileEntityAsDelegator(() => new Farm(new PointD2D(0, 0), sharedGameState.GetUniqueEntityID())), 64 + 5),
                      Tuple.Create((UnbuiltImmobileEntity)new UnbuiltImmobileEntityAsDelegator(() => new WaterMill(new PointD2D(0, 0), sharedGameState.GetUniqueEntityID())), 100),
                };
            }
        }
    
    
        public override void Dispose()
        {
            base.Dispose();

            MenuBackgroundTexture.Dispose();
            MenuBackgroundTexture = null;
        }
    }
}
