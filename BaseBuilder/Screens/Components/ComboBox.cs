using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.Components
{
    public class ComboBox <T1> : IScreenComponent
    {
        public Point Center { get; set; }

        public Point Size { get; protected set; }

        /// <summary>
        /// Triggered after the selected combo box item changes
        /// </summary>
        public event EventHandler SelectedChanged;

        /// <summary>
        /// Triggered when the hovered item changes.
        /// </summary>
        public event EventHandler HoveredChanged;

        /// <summary>
        /// Triggered when this combo box is expanded or minimized
        /// </summary>
        public event EventHandler ExpandedChanged;

        /// <summary>
        /// Triggered when the scrolling for this combo box changes
        /// </summary>
        public event EventHandler ScrollChanged;

        /// <summary>
        /// The currently selected combo box item, or null if none are
        /// selected.
        /// </summary>
        public ComboBoxItem<T1> Selected
        {
            get
            {
                if (SelectedIndex == -1)
                    return null;

                return Items[SelectedIndex];
            }
        }

        /// <summary>
        /// The currently hovered combo box item, or null if none are
        /// hovered.
        /// </summary>
        public ComboBoxItem<T1> Hovered
        {
            get
            {
                if (HoveredIndex == -1)
                    return null;

                return Items[HoveredIndex];
            }
        }

        /// <summary>
        /// The combo box items
        /// </summary>
        protected List<ComboBoxItem<T1>> Items;

        /// <summary>
        /// The currently selected index
        /// </summary>
        protected int SelectedIndex;

        /// <summary>
        /// The currently hovered index or -1
        /// </summary>
        protected int HoveredIndex;

        /// <summary>
        /// If the combo box is currently expanded
        /// </summary>
        protected bool Expanded;

        /// <summary>
        /// The texture to use if nothing is selected
        /// </summary>
        protected Texture2D UnselectedTexture;

        /// <summary>
        /// The y offset that is appled to the combo box items caused by the scrolling 
        /// that the user has done.
        /// </summary>
        protected int ScrollYOffset;

        /// <summary>
        /// If we've initialized ourself with a graphics device
        /// </summary>
        protected bool Initialized;

        /// <summary>
        /// Black pixel texture
        /// </summary>
        protected Texture2D BlackPixel;

        /// <summary>
        /// Initializes the combo box with the specified items
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="suggestedSize">The suggested size of this combo box. May be expanded if necessary</param>
        public ComboBox(List<ComboBoxItem<T1>> items, Point suggestedSize)
        {
            Items = items;
            SelectedIndex = 0;
            Expanded = false;

            Size = suggestedSize;
        }

        public void EnsureInitialized(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
            if (Initialized)
                return;

            if(BlackPixel == null)
            {
                BlackPixel = new Texture2D(graphicsDevice, 1, 1);
                BlackPixel.SetData(new[] { Color.Black });
            }

            foreach(var item in Items)
            {
                item.Initialize(content, graphics, graphicsDevice);
                Size = new Point(Math.Max(Size.X, item.MinSize.X), Math.Max(Size.Y, item.MinSize.Y));
            }

            foreach(var item in Items)
            {
                item.Size = new Point(Size.X, Size.Y);
            }

            Initialized = true;
        }

        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
            EnsureInitialized(content, graphics, graphicsDevice);

            if(UnselectedTexture == null)
            {
                UnselectedTexture = new Texture2D(graphicsDevice, 1, 1);
                UnselectedTexture.SetData(new[] { Color.DarkBlue });
            }

            var rect = new Rectangle(Center.X - Size.X / 2, Center.Y - Size.Y / 2, Size.X, Size.Y);
            Selected?.PreDraw(content, graphics, graphicsDevice, rect, 0, 0); 
            if (Expanded)
            {
                int sizeYMultiplier = Math.Min(Items.Count, 5);
                Rectangle visibleRect = new Rectangle(Center.X - Size.X / 2, Center.Y + Size.Y / 2, Size.X, Size.Y * sizeYMultiplier);
                visibleRect.Y += 1;
                var y = 0;
                for(int i = 0; i < Items.Count; i++)
                {
                    if (i == SelectedIndex)
                        continue;

                    if (y + ScrollYOffset + Size.Y >= 0)
                        Items[i].PreDraw(content, graphics, graphicsDevice, visibleRect, y, ScrollYOffset);
                    else
                        Items[i].SkippingDraw();

                    y += Size.Y + 1;

                    if (y + ScrollYOffset >= visibleRect.Height)
                        break;
                }
            }else
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (i == SelectedIndex)
                        continue;
                    Items[i].SkippingDraw();
                }
                }
        }

        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            var rect = new Rectangle(Center.X - Size.X / 2, Center.Y - Size.Y / 2, Size.X, Size.Y);
            if (Selected != null)
            {
                Selected.Draw(content, graphics, graphicsDevice, spriteBatch, rect, 0, 0);
            }
            else
            {
                spriteBatch.Draw(UnselectedTexture, rect, Color.White);
            }

            if (Expanded)
            {
                int sizeYMultiplier = Math.Min(Items.Count, 5);
                Rectangle visibleRect = new Rectangle(Center.X - Size.X / 2, Center.Y + Size.Y / 2, Size.X, Size.Y * sizeYMultiplier);

                var tmpRect = new Rectangle(visibleRect.X, visibleRect.Y, visibleRect.Width, 1);
                spriteBatch.Draw(BlackPixel, tmpRect, Color.White);

                visibleRect.Y += 1;

                int y = 0;

                for(int i = 0; i < Items.Count; i++)
                {
                    if (i == SelectedIndex)
                        continue;

                    if(y + ScrollYOffset + Size.Y >= 0)
                        Items[i].Draw(content, graphics, graphicsDevice, spriteBatch, visibleRect, y, ScrollYOffset);
                    
                    y += Size.Y;
                    tmpRect.Y = visibleRect.Y + y + ScrollYOffset;
                    if (y + ScrollYOffset >= 0 && y + ScrollYOffset <= visibleRect.Height)
                        spriteBatch.Draw(BlackPixel, tmpRect, Color.White);
                    y += 1;



                    if (y + ScrollYOffset >= visibleRect.Height)
                        break;
                }
            }
        }

        public void HandleKeyboardState(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled)
        {
        }

        public void HandleMouseState(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            var wasHandled = handled;
            var foundHovered = false;
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].HandleMouseState(last, current, ref handled);

                if(Items[i].Hovered && HoveredIndex != i)
                {
                    foundHovered = true;
                    HoveredIndex = i;
                    HoveredChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            if(!foundHovered)
            {
                HoveredIndex = -1;
                HoveredChanged?.Invoke(this, EventArgs.Empty);
            }

            bool containMouse = false;
            if(Expanded)
            {
                int sizeYMultiplier = Math.Min(Items.Count, 5);
                containMouse = new Rectangle(Center.X - Size.X / 2, Center.Y - Size.Y / 2, Size.X, Size.Y * (sizeYMultiplier + 1)).Contains(current.Position);
            }else
            {
                containMouse = new Rectangle(Center.X - Size.X / 2, Center.Y - Size.Y / 2, Size.X, Size.Y).Contains(current.Position);
            }

            if(!wasHandled && Expanded && !handled && containMouse)
            {
                handled = true;
            }
            
            if(!wasHandled && containMouse && last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
            {
                handled = true;
                for(int i = 0; i < Items.Count; i++)
                {
                    if(i != SelectedIndex)
                    {
                        if(Items[i].Hovered)
                        {
                            SelectedIndex = i;
                            SelectedChanged?.Invoke(null, EventArgs.Empty);
                            Expanded = false;
                            ExpandedChanged?.Invoke(null, EventArgs.Empty);
                            break;
                        }
                    }else
                    {
                        if(Items[i].Hovered)
                        {
                            Expanded = !Expanded;
                            ExpandedChanged?.Invoke(null, EventArgs.Empty);
                        }
                    }
                }
            }else if(!wasHandled && Expanded && containMouse && !scrollHandled && last.ScrollWheelValue != current.ScrollWheelValue)
            {
                scrollHandled = true;
                // Scrolling was requested

                var deltaScroll = (int)Math.Round((current.ScrollWheelValue - last.ScrollWheelValue) * 0.07);
                var desiredNewScrollY = ScrollYOffset + deltaScroll;

                // Can't scroll things to cause them to go down visually
                desiredNewScrollY = Math.Min(desiredNewScrollY, 0);

                // Can't scroll past the bottom
                int ourScrollableSizeUnscrollable = (Size.Y + 1) * Items.Count;
                if (Selected != null)
                    ourScrollableSizeUnscrollable -= (Size.Y + 1);

                int ourVisibleSize = Size.Y * Math.Min(Items.Count - (Selected != null ? 1 : 0), 5);

                var allowedScrollY = ourVisibleSize - ourScrollableSizeUnscrollable;
                desiredNewScrollY = Math.Max(desiredNewScrollY, allowedScrollY);

                if (ScrollYOffset != desiredNewScrollY)
                {
                    ScrollYOffset = desiredNewScrollY;
                    ScrollChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        public void Update(ContentManager content, int deltaMS)
        {
            foreach(var item in Items)
            {
                item.Update(deltaMS);
            }
        }
        
        public void Dispose()
        {
            foreach(var item in Items)
            {
                item.Dispose();
            }

            UnselectedTexture?.Dispose();
            UnselectedTexture = null;

            BlackPixel?.Dispose();
            BlackPixel = null;
        }
    }
}
