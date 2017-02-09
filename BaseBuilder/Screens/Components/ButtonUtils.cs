using BaseBuilder.Engine.Math2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.Components
{
    public class ButtonUtils
    {
        public static Button CreateSmallButton(PointI2D center, string text)
        {
            var sourceY = 95;
            var sourceHeight = 50;
            var sourceWidth = 128;
            return new Button(text, "Arial", center, "ButtonSmall",
                new Rectangle(112, sourceY, sourceWidth, sourceHeight),
                new Rectangle(247, sourceY, sourceWidth, sourceHeight),
                new Rectangle(381, sourceY, sourceWidth, sourceHeight), "MouseEnter", "MouseLeave", "ButtonPress", "ButtonUnpress");
        }
    }
}
