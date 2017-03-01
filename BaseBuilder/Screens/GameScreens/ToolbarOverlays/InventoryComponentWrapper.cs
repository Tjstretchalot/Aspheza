using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.World.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;

namespace BaseBuilder.Screens.GameScreens.ToolbarOverlays
{
    /// <summary>
    /// Wraps a part of an EntityInventory as a Renderable. Draws nothing if the 
    /// slot is unused
    /// </summary>
    public class InventoryComponentWrapper : Renderable
    {
        private EntityInventory Inventory;
        private int MyIndex;

        public string HoverText
        {
            get
            {
                if (Inventory.MaterialAt(MyIndex) == null)
                    return null;

                return Inventory.MaterialAt(MyIndex).Item1.HoverText;
            }
        }


        public InventoryComponentWrapper(EntityInventory inventory, int myIndex)
        {
            Inventory = inventory;
            MyIndex = myIndex;
        }

        public void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            var matAt = Inventory.MaterialAt(MyIndex);

            if (matAt == null)
                return;

            matAt.Item1.Render(context, screenTopLeft, overlay);

            var str = $"{matAt.Item2}";

            var strSize = context.DefaultFont.MeasureString(str);

            context.SpriteBatch.DrawString(context.DefaultFont, str, new Vector2((int)(screenTopLeft.X + 32 - strSize.X - 2), (int)(screenTopLeft.Y + 32 - strSize.Y - 2)), Color.White);
        }
    }
}
