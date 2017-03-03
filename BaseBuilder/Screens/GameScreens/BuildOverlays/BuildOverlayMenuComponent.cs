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
    public class BuildOverlayMenuComponent : MyGameComponent
    {
        /// <summary>
        /// The list of unbuilt entities that can be chosen from
        /// </summary>
        protected List<BuildOverlayMenuItem> MenuItems;

        /// <summary>
        /// The current index in menu items that is hovered, or -1 if
        /// nothing is currently hovered
        /// </summary>
        protected int HoveredIndex;

        /// <summary>
        /// The current index in menu items that is selected, or -1 if
        /// nothing is currently selected
        /// </summary>
        protected int SelectedIndex;


        /// <summary>
        /// The current selected unbuilt entity
        /// </summary>
        public UnbuiltImmobileEntity Current { get; protected set; }

        /// <summary>
        /// The camera for this menu.
        /// </summary>
        protected Camera MenuCamera;

        /// <summary>
        /// My own render context!
        /// </summary>
        protected RenderContext MyContext;

        /// <summary>
        /// The background texture
        /// </summary>
        protected Texture2D MenuBackgroundTexture;

        /// <summary>
        /// Where this menu is visually
        /// </summary>
        protected Rectangle MyVisualRect;

        /// <summary>
        /// The size of this menu if there were no scrollbar
        /// </summary>
        protected Rectangle MyRectIfNoScrollbar;

        /// <summary>
        /// The y offset caused by the scroll bar
        /// </summary>
        protected int ScrollBarYOffset;

        public BuildOverlayMenuComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            Init(new PointI2D(graphicsDevice.Viewport.Width - 300, 25), new PointI2D(250, graphicsDevice.Viewport.Height - 250), 2);

            MenuBackgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            MenuBackgroundTexture.SetData(new[] { Color.Gray });

            MyVisualRect = new Rectangle(ScreenLocation.X, ScreenLocation.Y, Size.X, Size.Y);

            HoveredIndex = -1;
            SelectedIndex = -1;

            MenuItems = new List<BuildOverlayMenuItem>{
                 new BarnBuildOverlayMenuItem(content, graphics, graphicsDevice, spriteBatch),
                 new FarmBuildOverlayMenuItem(content, graphics, graphicsDevice, spriteBatch),
                 new WaterMillOverlayMenuItem(content, graphics, graphicsDevice, spriteBatch),
                 new BakeryBuildOverlayMenuItem(content, graphics, graphicsDevice, spriteBatch),
                 new LibraryBuildOverlayMenuItem(content, graphics, graphicsDevice, spriteBatch),
                 new TempleBuildOverlayMenuItem(content, graphics, graphicsDevice, spriteBatch),
                 new TavernBuildOverlayMenuItem(content, graphics, graphicsDevice, spriteBatch)
            };

            MyRectIfNoScrollbar = new Rectangle(MyVisualRect.X, MyVisualRect.Y, MyVisualRect.Width, 5); // 5px padding on top

            foreach(var item in MenuItems)
            {
                item.Location = new PointI2D((int)(MyVisualRect.Width / 2 - item.VisualCollisionMesh.Width / 2), MyRectIfNoScrollbar.Height);

                MyRectIfNoScrollbar.Height += (int)(item.VisualCollisionMesh.Height + 3); // 3px padding between each
            }

            MyRectIfNoScrollbar.Height += 5; // 5px padding on bottom
        }

        public override void Draw(RenderContext context)
        {
            SpriteBatch.Draw(MenuBackgroundTexture, MyVisualRect, Color.White);

            for(int i = 0; i < MenuItems.Count; i++)
            {
                MenuItems[i].Render(MyVisualRect, ScrollBarYOffset, i == SelectedIndex);
            }
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
            for (int i = 0; i < MenuItems.Count; i++)
            {
                MenuItems[i].Update(timeMS, i == SelectedIndex);
            }
        }

        public void ClearSelection()
        {
            SelectedIndex = -1;
            Current = null;
        }

        public override bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame)
        {
            return false;
        }

        public override bool HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            if (!MyVisualRect.Contains(current.Position))
                return false;

            bool foundHover = false;
            if(HoveredIndex != -1)
            {
                if(MenuItems[HoveredIndex].ContainsMouse(MyVisualRect, ScrollBarYOffset, current.Position))
                {
                    foundHover = true;
                }
            }

            if(!foundHover)
            {
                for(int i = 0; i < MenuItems.Count; i++)
                {
                    if(i != HoveredIndex && MenuItems[i].ContainsMouse(MyVisualRect, ScrollBarYOffset, current.Position))
                    {
                        // Hover changed from HoveredIndex to i
                        HoveredIndex = i;
                        foundHover = true;
                        break;
                    }
                }
            }
            
            if(!foundHover && HoveredIndex != -1)
            {
                // Previously something was hovered, but now nothing is hovered.
                HoveredIndex = -1;
            }

            if(last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
            {
                if(HoveredIndex != -1 && !MenuItems[HoveredIndex].Selectable && SelectedIndex != -1)
                {
                    // Tried to select something that isn't selectable and had something selected, causing
                    // us to lose that selection
                    SelectedIndex = -1;
                    Current = null;
                }else if(SelectedIndex != HoveredIndex)
                {
                    // The selected item changed
                    SelectedIndex = HoveredIndex;

                    if (SelectedIndex == -1)
                        Current = null;
                    else
                        Current = MenuItems[SelectedIndex].CreateUnbuiltImmobileEntity(sharedGameState);
                }
            }

            if(last.ScrollWheelValue != current.ScrollWheelValue)
            {
                // Scrolling was requested
                
                var deltaScroll = (int)Math.Round((current.ScrollWheelValue - last.ScrollWheelValue) * 0.07);
                var desiredNewScrollY = ScrollBarYOffset + deltaScroll;

                // Can't scroll things to cause them to go down visually
                desiredNewScrollY = Math.Min(desiredNewScrollY, 0);

                // Can't scroll past the bottom
                desiredNewScrollY = Math.Max(desiredNewScrollY, -(MyRectIfNoScrollbar.Height - MyVisualRect.Height));

                ScrollBarYOffset = desiredNewScrollY;
            }

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            MenuBackgroundTexture.Dispose();
            MenuBackgroundTexture = null;
        }
    }
}
