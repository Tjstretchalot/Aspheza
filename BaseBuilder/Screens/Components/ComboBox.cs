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
    public class ComboBox : IScreenComponent
    {
        public Point Center { get; set; }

        public Point Size { get; protected set; }

        /// <summary>
        /// Triggered after the selected combo box item changes
        /// </summary>
        public event EventHandler SelectedChanged;

        /// <summary>
        /// The currently selected combo box item, or null if none is 
        /// selected.
        /// </summary>
        public ComboBoxItem Selected
        {
            get
            {
                if (SelectedIndex == -1)
                    return null;

                return Items[SelectedIndex];
            }
        }
        /// <summary>
        /// The combo box items
        /// </summary>
        protected List<ComboBoxItem> Items;

        /// <summary>
        /// The currently selected index
        /// </summary>
        protected int SelectedIndex;

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
        public ComboBox(List<ComboBoxItem> items, Point suggestedSize)
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

        public void HandleMouseState(ContentManager content, MouseState last, MouseState current, ref bool handled)
        {
            var wasHandled = handled;
            Selected?.HandleMouseState(last, current, ref handled);
            
            for (int i = 0; i < Items.Count; i++)
            {
                if (i != SelectedIndex)
                {
                    Items[i].HandleMouseState(last, current, ref handled);
                }
            }

            if(!wasHandled && Expanded && !handled)
            {
                int sizeYMultiplier = Math.Min(Items.Count, 5);
                Rectangle visibleRect = new Rectangle(Center.X - Size.X / 2, Center.Y - Size.Y / 2, Size.X, Size.Y * (sizeYMultiplier + 1)); // fix black pixel

                if (visibleRect.Contains(current.Position))
                    handled = true;
            }
            
            if(!wasHandled && last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
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
                            break;
                        }
                    }else
                    {
                        if(Items[i].Hovered)
                        {
                            Expanded = !Expanded;
                        }
                    }
                }
            }else if(!wasHandled && Expanded && last.ScrollWheelValue != current.ScrollWheelValue)
            {
                handled = true;
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

                ScrollYOffset = desiredNewScrollY;
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
