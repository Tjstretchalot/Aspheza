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
    /// <summary>
    /// Describes a simple checkbox
    /// </summary>
    public class CheckBox : RadioButton
    {
        public CheckBox(Point center) : base(center)
        {
            PushedTextureName = "UI/blueSheet";
            UnpushedTextureName = "UI/greySheet";
            PushedSourceRect = new Rectangle(381, 36, 36, 36);
            UnpushedSourceRect = new Rectangle(148, 433, 36, 36);
            PushSoundEffectName = "UI/switch7";
            UnpushSoundEffectName = "UI/switch8";

            Center = center;
            Size = UnpushedSourceRect.Size;

            DrawRect = new Rectangle(-1, -1, 1, 1);
            _Hovered = false;
            _Pushed = false;
        }

        /// <summary>
        /// Of mouse not handled checkbox was click change checkbox state and set handled to true.
        /// </summary>
        /// <param name="content">The content manager.</param>
        /// <param name="last">The previous mouse state.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="handled">If the mouse has been handled.</param>
        /// <param name="scrollHandled">If the scroll wheel has been handled.</param>
        public override void HandleMouseState(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled)
        {
            var newHovered = !handled && DrawRect.Contains(current.Position);
            var justPressed = newHovered && (last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released);

            Hovered = newHovered;
            Pushed = justPressed ? !Pushed : Pushed;
            handled = handled || newHovered;
        }
    }
}
