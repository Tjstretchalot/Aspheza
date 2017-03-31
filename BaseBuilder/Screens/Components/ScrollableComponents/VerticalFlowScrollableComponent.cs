using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Screens.Components.ScrollableComponents;

namespace BaseBuilder.Screens.Components.ScrollableComponents
{
    /// <summary>
    /// Lays children out vertically based on a specified alignment mode, spacing,
    /// and horizontal padding
    /// </summary>
    public class VerticalFlowScrollableComponent : ScrollableComponentAsLayoutManager
    {
        public enum VerticalAlignmentMode
        {
            /// <summary>
            /// All components have there center placed at the suggested x.
            /// </summary>
            CenteredSuggested,

            /// <summary>
            /// All components are centered based on the width provided ("truely centered")
            /// </summary>
            CenteredWidth,

            /// <summary>
            /// All components are left-aligned to where the widest components
            /// left where the widest component is centered at the suggested
            /// location.
            /// </summary>
            LeftAlignSuggested,

            /// <summary>
            /// All components are left-aligned to where the widest components
            /// left where the widest component is truly centered.
            /// </summary>
            LeftAlignWidth,

            /// <summary>
            /// All components are right-aligned to where the widest components
            /// right where the widest component is centered at the suggested
            /// location
            /// </summary>
            RightAlignSuggested,

            /// <summary>
            /// All components are right-aligned to where the widest components
            /// right where the widest component is truly centered
            /// </summary>
            RightAlignWidth
        }

        /// <summary>
        /// Alignment mode
        /// </summary>
        public VerticalAlignmentMode Alignment;

        /// <summary>
        /// Vertical spacing between components
        /// </summary>
        public int Spacing;

        /// <summary>
        /// Creates a new vertical flow
        /// </summary>
        /// <param name="alignment">How children will be aligned</param>
        /// <param name="spacing">The spacing between children</param>
        public VerticalFlowScrollableComponent(VerticalAlignmentMode alignment, int spacing)
        {
            Alignment = alignment;
            Spacing = spacing;
        }

        public override int GetRequiredHeight(RenderContext context)
        {
            int result = 0;
            bool first = true;

            foreach(var child in Children)
            {
                if (child.Hidden)
                    continue;
                
                if (first)
                    first = false;
                else
                    result += Spacing;

                result += child.GetRequiredHeight(context);
            }

            return result;
        }

        public override int GetRequiredWidth(RenderContext context)
        {
            int result = 0;
            
            foreach(var child in Children)
            {
                if (child.Hidden)
                    continue;

                result = Math.Max(result, child.GetRequiredWidth(context));
            }

            return result;
        }

        public override void Layout(RenderContext context, int suggestedCenterX, int width, ref int height)
        {
            int requiredWidth = GetRequiredWidth(context);
            bool first = true;

            foreach(var child in Children)
            {
                if (child.Hidden)
                    continue;

                if (first)
                    first = false;
                else
                    height += Spacing;

                switch(Alignment)
                {
                    case VerticalAlignmentMode.CenteredSuggested:
                        child.Layout(context, suggestedCenterX, width, ref height);
                        break;
                    case VerticalAlignmentMode.CenteredWidth:
                        child.Layout(context, width / 2, width, ref height);
                        break;
                    case VerticalAlignmentMode.LeftAlignSuggested:
                        child.Layout(context, (suggestedCenterX - requiredWidth / 2 + child.GetRequiredWidth(context) / 2), width, ref height);
                        break;
                    case VerticalAlignmentMode.LeftAlignWidth:
                        child.Layout(context, (width / 2 - requiredWidth / 2 + child.GetRequiredWidth(context) / 2), width, ref height);
                        break;
                    case VerticalAlignmentMode.RightAlignSuggested:
                        child.Layout(context, (suggestedCenterX + requiredWidth / 2 - child.GetRequiredWidth(context) / 2), width, ref height);
                        break;
                    case VerticalAlignmentMode.RightAlignWidth:
                        child.Layout(context, (width / 2 + requiredWidth / 2 - child.GetRequiredWidth(context) / 2), width, ref height);
                        break;
                    default:
                        throw new InvalidProgramException($"Unknown alignment mode {Alignment}");
                }
            }
        }
    }
}
