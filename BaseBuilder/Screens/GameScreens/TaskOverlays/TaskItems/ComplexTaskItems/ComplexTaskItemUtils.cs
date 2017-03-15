using BaseBuilder.Engine.Context;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems.ComplexTaskItems
{
    /// <summary>
    /// A collection of utility functions for complex task items
    /// </summary>
    public class ComplexTaskItemUtils
    {
        /// <summary>
        /// Wraps all of the specified screen components, losing their type information.
        /// </summary>
        /// <param name="comps">The components</param>
        /// <returns>The wrapped components</returns>
        public static List<ITaskItemComponent> WrapAll(params IScreenComponent[] comps)
        {
            var result = new List<ITaskItemComponent>();

            foreach(var comp in comps)
            {
                result.Add(Wrap(comp));
            }

            return result;
        }

        /// <summary>
        /// Wraps the specified component without losing its type information
        /// </summary>
        /// <typeparam name="T">The type of the screen component so it may be preserved</typeparam>
        /// <param name="component">The component to wrap</param>
        /// <returns>The wrapped component</returns>
        public static TaskItemComponentFromScreenComponent<T> Wrap<T>(T component) where T:IScreenComponent
        {
            return new TaskItemComponentFromScreenComponent<T>(component);
        }

        /// <summary>
        /// Unwraps the weak reference to the wrapped component, throwing an exception
        /// if the weak reference is lost or the component is disposed.
        /// </summary>
        /// <typeparam name="T1">The type of component</typeparam>
        /// <param name="weak">The weak reference</param>
        /// <returns>Unwrapped component</returns>
        /// <exception cref="InvalidProgramException">If the weak reference is lost</exception>
        /// <exception cref="InvalidProgramException">If the component is disposed</exception>
        public static T1 Unwrap<T1>(WeakReference<TaskItemComponentFromScreenComponent<T1>> weak) where T1:IScreenComponent
        {
            TaskItemComponentFromScreenComponent<T1> strong;
            if (!weak.TryGetTarget(out strong))
                throw new InvalidProgramException("Weak reference lost!");

            return Unwrap(strong);
        }

        /// <summary>
        /// Unwraps the specified component
        /// </summary>
        /// <typeparam name="T1">The type of wrapped component</typeparam>
        /// <param name="strong">The strong reference</param>
        /// <returns>The component</returns>
        /// <exception cref="InvalidProgramException">If the component is disposed</exception>
        public static T1 Unwrap<T1>(TaskItemComponentFromScreenComponent<T1> strong) where T1:IScreenComponent
        {
            if (strong.Disposed)
                throw new InvalidProgramException("Component is disposed!");

            return strong.Component;
        }

        public static ITaskItemComponent Label(RenderContext context, string label, ITaskItemComponent component, bool vertical = true)
        {
            if(vertical)
            {
                var result = new VerticalFlowTaskItemComponent(VerticalFlowTaskItemComponent.VerticalAlignmentMode.LeftAlignSuggested, 3);

                result.Children.Add(Wrap(new Text(new Point(0, 0), label, context.DefaultFont, Color.Black)));
                result.Children.Add(component);

                return result;
            }else
            {
                var result = new HorizontalFlowTaskItemComponent(HorizontalFlowTaskItemComponent.HorizontalAlignmentMode.CenterAlignSuggested, 3);

                result.Children.Add(Wrap(new Text(new Point(0, 0), label, context.DefaultFont, Color.Black)));
                result.Children.Add(component);

                return result;
            }
        }

        /// <summary>
        /// Creates a combo box of the specified type hooked to redraw with the specified 
        /// event handler
        /// </summary>
        /// <typeparam name="T1">The type of the combo box</typeparam>
        /// <param name="context">The context</param>
        /// <param name="redraw">Redraw handler</param>
        /// <param name="redrawAndReload">Redraw and reload handler</param>
        /// <param name="options">Options for combo box, name + T1</param>
        /// <returns>The constructed combo box</returns>
        public static ComboBox<T1> CreateComboBox<T1>(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, params Tuple<string, T1>[] options)
        {
            List<ComboBoxItem<T1>> items = new List<ComboBoxItem<T1>>();

            foreach(var opt in options)
            {
                items.Add(new ComboBoxItem<T1>(context.DefaultFont, opt.Item1, opt.Item2));
            }

            var result = new ComboBox<T1>(items, new Point(200, 34));
            result.Selected = null;
            result.HoveredChanged += redraw;
            result.ScrollChanged += redraw;
            result.ExpandedChanged += redraw;
            result.SelectedChanged += (sender, tmp) => redrawAndReload(sender, EventArgs.Empty);
            return result;
        }

        /// <summary>
        /// Setup the combo box to toggle the thing to not hidden when choice is selected 
        /// and to hidden if choice is not selected
        /// </summary>
        /// <typeparam name="T1">The type of the combo box</typeparam>
        /// <param name="boxWeak">The weak reference to the combo box</param>
        /// <param name="choice">The choice that thing should not be hidden for</param>
        /// <param name="thingToToggleWeak">The thing</param>
        public static void SetupComboBoxHiddenToggle<T1>(WeakReference<ComboBox<T1>> boxWeak, T1 choice, WeakReference<ITaskItemComponent> thingToToggleWeak)
        {
            ComboBox<T1> boxStrong;
            if(!boxWeak.TryGetTarget(out boxStrong))
            {
                throw new InvalidProgramException("Weak reference already lost");
            }

            ComboBoxSelectedChangedEventHandler<T1> handler = null;
            handler = (sender, oldSelected) =>
            {
                ComboBox<T1> boxStrong2;
                ITaskItemComponent thingToToggleStrong;
                if (!thingToToggleWeak.TryGetTarget(out thingToToggleStrong))
                {
                    if(boxWeak.TryGetTarget(out boxStrong2))
                    {
                        boxStrong2.SelectedChanged -= handler;
                    }
                    return;
                }

                var comparer = EqualityComparer<T1>.Default;

                if (oldSelected != null && comparer.Equals(oldSelected.Value, choice))
                {
                    thingToToggleStrong.Hidden = true;
                    return;
                }

                if (!boxWeak.TryGetTarget(out boxStrong2))
                {
                    return;
                }

                if (boxStrong2.Selected == null)
                    return;

                if(comparer.Equals(boxStrong2.Selected.Value, choice))
                {
                    thingToToggleStrong.Hidden = false;
                    return;
                }
            };

            boxStrong.SelectedChanged += handler;
        }

        /// <summary>
        /// Wraps the box and thing in weak references and calls that version of this functino.
        /// </summary>
        /// <typeparam name="T1">the type of the combo box</typeparam>
        /// <param name="box">the box</param>
        /// <param name="choice">the choice</param>
        /// <param name="thing">the thing</param>
        public static void SetupComboBoxHiddenToggle<T1>(ComboBox<T1> box, T1 choice, ITaskItemComponent thing)
        {
            SetupComboBoxHiddenToggle(new WeakReference<ComboBox<T1>>(box), choice, new WeakReference<ITaskItemComponent>(thing));
        }
    }
}
