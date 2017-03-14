using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems
{
    /// <summary>
    /// Lays out components horizontally based on the specified alignment and spacing.
    /// </summary>
    public class HorizontalFlowTaskItemComponent : TaskItemComponentAsLayoutManager
    {
        public enum HorizontalAlignmentMode
        {
            /// <summary>
            /// Aligns components such that their tops are all at the same place 
            /// and the center of the entire flow is at the suggested center.
            /// </summary>
            TopAlignSuggested,

            /// <summary>
            /// Aligns components such that their tops are all at the same place and
            /// the center of the entire flow is at the true center
            /// </summary>
            TopAlignWidth,

            /// <summary>
            /// Aligns components such that their centers are all at the same place and
            /// the center of the flow is at the suggested center
            /// </summary>
            CenterAlignSuggested,

            /// <summary>
            /// Aligns components such that their centers are all at the same place and the
            /// center of the flow is at the true center
            /// </summary>
            CenterAlignWidth,

            /// <summary>
            /// Aligns components such that their bottoms are all at the same place and the
            /// center of the flow is at the suggested center
            /// </summary>
            BottomAlignSuggested,

            /// <summary>
            /// Aligns components such that their bottoms are all at the same place and the 
            /// center of the flow is at the true center
            /// </summary>
            BottomAlignWidth
        }

        public HorizontalAlignmentMode Alignment;
        public int Spacing;

        public HorizontalFlowTaskItemComponent(HorizontalAlignmentMode alignment, int spacing)
        {
            Alignment = alignment;
            Spacing = spacing;
        }

        public override int GetRequiredHeight(RenderContext context)
        {
            int result = 0;

            foreach(var child in Children)
            {
                result = Math.Max(result, child.GetRequiredHeight(context));
            }

            return result;
        }

        public override int GetRequiredWidth(RenderContext context)
        {
            int result = 0;
            bool first = true;

            foreach(var child in Children)
            {
                if (first)
                    first = false;
                else
                    result += Spacing;

                result += child.GetRequiredWidth(context);
            }

            return result;
        }

        public override void Layout(RenderContext context, int suggestedCenterX, int width, ref int height)
        {
            var requiredHeight = GetRequiredHeight(context);
            int x;

            switch(Alignment)
            {
                case HorizontalAlignmentMode.BottomAlignSuggested:
                case HorizontalAlignmentMode.CenterAlignSuggested:
                case HorizontalAlignmentMode.TopAlignSuggested:
                    x = suggestedCenterX - GetRequiredWidth(context) / 2;
                    break;
                case HorizontalAlignmentMode.BottomAlignWidth:
                case HorizontalAlignmentMode.CenterAlignWidth:
                case HorizontalAlignmentMode.TopAlignWidth:
                    x = width / 2 - GetRequiredWidth(context) / 2;
                    break;
                default:
                    throw new InvalidProgramException($"Unknown alignment {Alignment}");
            }

            bool first = true;
            
            foreach(var child in Children)
            { 
                var childHeight = child.GetRequiredHeight(context);
                var childWidth = child.GetRequiredWidth(context);
                if (first)
                    first = false;
                else
                    x += Spacing;

                int y;
                switch(Alignment)
                {
                    case HorizontalAlignmentMode.BottomAlignWidth:
                    case HorizontalAlignmentMode.BottomAlignSuggested:
                        y = height + requiredHeight - childHeight / 2;
                        break;
                    case HorizontalAlignmentMode.CenterAlignWidth:
                    case HorizontalAlignmentMode.CenterAlignSuggested:
                        y = height + requiredHeight / 2;
                        break;
                    case HorizontalAlignmentMode.TopAlignSuggested:
                    case HorizontalAlignmentMode.TopAlignWidth:
                        y = height + childHeight / 2;
                        break;
                    default:
                        throw new InvalidProgramException($"Unknown alignment {Alignment}");
                }

                var oldHeight = height;
                height = y - childHeight / 2;
                child.Layout(context, x + childWidth / 2, width, ref height);
                height = oldHeight;
                x += childWidth;
            }

            height += requiredHeight;
        }
    }
}
