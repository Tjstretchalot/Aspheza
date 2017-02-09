using BaseBuilder.Engine.Math2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// A collection of utility functions for creating ui elements.
    /// </summary>
    public class UIUtils
    {
        public enum ButtonColor
        {
            Blue, Red, Yellow, Green, Grey
        }

        public enum ButtonSize
        {
            Medium
        }
        
        private static Color ReallyDarkGray;

        private static Dictionary<Tuple<ButtonColor, ButtonSize>, XElement> SpritesToTextureAtlases;
        
        /// <summary>
        /// Sets this utility class up for use. Requires
        /// loading some files.
        /// </summary>
        public static void Load()
        {
            ReallyDarkGray = new Color(55, 55, 55);

            SpritesToTextureAtlases = new Dictionary<Tuple<ButtonColor, ButtonSize>, XElement>();
            LoadTextureAtlas("UI/blueSheet.xml", ButtonColor.Blue, ButtonSize.Medium);
            LoadTextureAtlas("UI/yellowSheet.xml", ButtonColor.Yellow, ButtonSize.Medium);
            LoadTextureAtlas("UI/greenSheet.xml", ButtonColor.Green, ButtonSize.Medium);
            LoadTextureAtlas("UI/redSheet.xml", ButtonColor.Red, ButtonSize.Medium);
            LoadTextureAtlas("UI/greySheet.xml", ButtonColor.Grey, ButtonSize.Medium);
        }

        private static void LoadTextureAtlas(string spriteSheet, ButtonColor buttonColor, ButtonSize buttonSize)
        {
            SpritesToTextureAtlases.Add(Tuple.Create(buttonColor, buttonSize), XElement.Load("Content/" + spriteSheet));
        }
        
        private static Rectangle LoadRectangleFromSheetNameAndElementName(ButtonColor color, ButtonSize size, string elementName)
        {
            var atlas = SpritesToTextureAtlases[Tuple.Create(color, size)];
            foreach(var node in atlas.Descendants("SubTexture"))
            {
                var isCorrectElement = node.Attributes("name").First().Value.Equals(elementName);

                if (!isCorrectElement)
                    continue;
                
                return new Rectangle(
                    int.Parse(node.Attribute("x").Value),
                    int.Parse(node.Attribute("y").Value),
                    int.Parse(node.Attribute("width").Value),
                    int.Parse(node.Attribute("height").Value)
                    );
            }

            throw new InvalidProgramException($"Sheet for button color={color} size={size} has no subtexture named {elementName}");
        }

        private static string GetColorNameFromButtonColor(ButtonColor color)
        {
            switch(color)
            {
                case ButtonColor.Blue:
                    return "blue";
                case ButtonColor.Red:
                    return "red";
                case ButtonColor.Yellow:
                    return "yellow";
                case ButtonColor.Green:
                    return "green";
                case ButtonColor.Grey:
                    return "grey";
                default:
                    throw new InvalidProgramException($"Unknown button color {color}");
            }
        }

        private static string GetSizeNameFromButtonSize(ButtonSize size)
        {
            switch(size)
            {
                case ButtonSize.Medium:
                    return "medium";
                default:
                    throw new InvalidProgramException($"Unknown button size {size}");
            }
        }

        private static Color GetTextColorFromButtonColor(ButtonColor color)
        {
            return ReallyDarkGray;
        }
        
        private static void LoadButtonSheetInfo(ButtonColor color, ButtonSize size, out Color textColor, out string sheetName, out Rectangle unhovUnpress, out Rectangle hovUnpress, out Rectangle hovPress)
        {
            if (size != ButtonSize.Medium)
                throw new NotImplementedException($"Weird button size {size}");

            textColor = GetTextColorFromButtonColor(color);
            var colorName = GetColorNameFromButtonColor(color);
            var sizeName = GetSizeNameFromButtonSize(size);

            sheetName = "UI/" + colorName + "Sheet";
            unhovUnpress = LoadRectangleFromSheetNameAndElementName(color, size, colorName + "_button_unhov_unpress_" + sizeName + ".png"); // blue 01
            hovUnpress = LoadRectangleFromSheetNameAndElementName(color, size, colorName + "_button_hov_unpress_" + sizeName + ".png"); // blue 05
            hovPress = LoadRectangleFromSheetNameAndElementName(color, size, colorName + "_button_hov_press_" + sizeName + ".png"); // blue 03
        }

        private static void UpdateButtonGraphicsFromSheet(Button button, ButtonColor color, ButtonSize size)
        {
            Color textColor;
            string sheetName;
            Rectangle unhovUnpress, hovUnpress, hovPress;
            LoadButtonSheetInfo(color, size, out textColor, out sheetName, out unhovUnpress, out hovUnpress, out hovPress);

            button.UnhoveredUnpressedButtonSpriteName = sheetName;
            button.HoveredUnpressedButtonSpriteName = sheetName;
            button.HoveredPressedButtonSpriteName = sheetName;

            button.UnhoveredUnpressedTextColor = textColor;
            button.HoveredUnpressedTextColor = textColor;
            button.HoveredPressedTextColor = textColor;

            button.UnhoveredUnpressedSourceRect = unhovUnpress;
            button.HoveredUnpressedSourceRect = hovUnpress;
            button.HoveredPressedSourceRect = hovPress;

            button._Location = null;
            button._TextDestinationVec = null;
        }

        private static Button CreateButtonFromSheet(PointI2D center, string text, ButtonColor color, ButtonSize size)
        {
            Color textColor;
            string sheetName;
            Rectangle unhovUnpress, hovUnpress, hovPress;
            LoadButtonSheetInfo(color, size, out textColor, out sheetName, out unhovUnpress, out hovUnpress, out hovPress);

            return new Button(text, "KenVector Future", textColor, textColor, textColor,
                center, sheetName, sheetName, sheetName, unhovUnpress, hovUnpress, hovPress, 
                "MouseEnter", "MouseLeave", "ButtonPress", "ButtonUnpress");
        }
        
        public static Button CreateButton(PointI2D center, string text, ButtonColor color, ButtonSize size)
        {
            return CreateButtonFromSheet(center, text, color, size);
        }

        public static void SetButton(Button button, ButtonColor color, ButtonSize size)
        {
            UpdateButtonGraphicsFromSheet(button, color, size);
        }
    }
}
