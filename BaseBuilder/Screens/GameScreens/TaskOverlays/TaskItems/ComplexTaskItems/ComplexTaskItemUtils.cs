using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State.Resources;
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

        /// <summary>
        /// Sets up the specified component to have a label, either vertically or horizontally.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="label">The label</param>
        /// <param name="component">The component to label</param>
        /// <param name="vertical">True if the label should be above, false if the label should be to the right</param>
        /// <returns>The wrapped component</returns>
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

                result.Children.Add(component);
                result.Children.Add(Wrap(new Text(new Point(0, 0), label, context.DefaultFont, Color.Black)));

                return result;
            }
        }

        /// <summary>
        /// Creates some padding of the specified width and height
        /// </summary>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <returns>padding component</returns>
        public static ITaskItemComponent CreatePadding(int width, int height)
        {
            return new PaddingTaskItemComponent(width, height);
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
        /// Create a combo box that has all of the materials in it.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="redraw">Redraw</param>
        /// <param name="redrawAndReload">Redraw and reload</param>
        /// <returns>Material combo box</returns>
        public static ComboBox<Material> CreateMaterialComboBox(RenderContext context, EventHandler redraw, EventHandler redrawAndReload)
        {
            var result = new ComboBox<Material>(MaterialComboBoxItem.AllMaterialsWithFont(context.DefaultFont), new Point(200, 34));
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

        /// <summary>
        /// Creates a checkbox and sets it up to redraw and reload when
        /// pushed.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="redraw">Redraw</param>
        /// <param name="redrawAndReload">Redraw and reload</param>
        /// <returns>the checkbox</returns>
        public static CheckBox CreateCheckBox(RenderContext context, EventHandler redraw, EventHandler redrawAndReload)
        {
            var result = new CheckBox(new Point(0, 0));

            result.PushedChanged += redrawAndReload;

            return result;
        }

        /// <summary>
        /// Creates a radio button and sets it up to redraw and reload when pushed
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="redraw">Redraw</param>
        /// <param name="redrawAndReload">Redraw and reload</param>
        /// <returns>The radio button</returns>
        public static RadioButton CreateRadioButton(RenderContext context, EventHandler redraw, EventHandler redrawAndReload)
        {
            var result = new RadioButton(new Point(0, 0));

            result.PushedChanged += redrawAndReload;

            return result;
        }

        /// <summary>
        /// Groups up the specified radio buttons such that only one can be pushed at onec
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="redraw">Redraw</param>
        /// <param name="redrawAndReload">Redraw and reload</param>
        /// <param name="buttonsWeak">Buttons to group</param>
        public static void GroupRadioButtons(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, params WeakReference<RadioButton>[] buttonsWeak)
        {
            EventHandler handler = null;
            handler = (sender, args) =>
            {
                var buttonPressed = sender as RadioButton;
                if (!buttonPressed.Pushed)
                    return;

                foreach(var buttonWeak in buttonsWeak)
                {
                    RadioButton buttonStrong;
                    if(buttonWeak.TryGetTarget(out buttonStrong))
                    {
                        if(!ReferenceEquals(buttonStrong, buttonPressed))
                        {
                            buttonStrong.Pushed = false;
                        }
                    }
                }
            };
            foreach(var buttonWeak in buttonsWeak)
            {
                RadioButton buttonStrong;
                if (!buttonWeak.TryGetTarget(out buttonStrong))
                    throw new InvalidProgramException("button already disposed!");

                buttonStrong.PushedChanged += handler;
            }
        }

        /// <summary>
        /// Groups the buttons such that only one can be pushed at a time. Holds
        /// only a weak reference.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="redraw">Redraw</param>
        /// <param name="redrawAndReload">Redraw and reload</param>
        /// <param name="buttons">The buttons</param>
        public static void GroupRadioButtons(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, params RadioButton[] buttons)
        {
            GroupRadioButtons(context, redraw, redrawAndReload, buttons.Select((but) => new WeakReference<RadioButton>(but)).ToArray());
        }

        /// <summary>
        /// Sets up the radio button to unhide the thing when radioButton.Pushed == pushed
        /// </summary>
        /// <param name="radioButtonWeak">The radio button</param>
        /// <param name="thingWeak">The thing to toggle hidden</param>
        /// <param name="pushed">The state to unhide the thing</param>
        public static void SetupRadioButtonHiddenToggle(WeakReference<RadioButton> radioButtonWeak, WeakReference<ITaskItemComponent> thingWeak, bool pushed = true)
        {
            RadioButton radioButtonStrong;

            if (!radioButtonWeak.TryGetTarget(out radioButtonStrong))
                throw new InvalidProgramException("Radio button is already lost!");

            EventHandler handler = null;
            handler = (sender, args) =>
            {
                RadioButton radioButtonStrong2;
                if (!radioButtonWeak.TryGetTarget(out radioButtonStrong2))
                {
                    return;
                }

                ITaskItemComponent thingStrong;
                if (!thingWeak.TryGetTarget(out thingStrong))
                {
                    radioButtonStrong2.PushedChanged -= handler;   
                    return;
                }

                thingStrong.Hidden = radioButtonStrong2.Pushed != pushed;
            };

            radioButtonStrong.PushedChanged += handler;
        }

        /// <summary>
        /// As if you called SetupRadioButtonHiddenToggle(new WeakReference(checkbox), new WeakReference(thing), pushed)
        /// </summary>
        /// <param name="button">The radio button</param>
        /// <param name="thing">The thing</param>
        /// <param name="pushed">The state to unhide the thing</param>
        public static void SetupRadioButtonHiddenToggle(RadioButton button, ITaskItemComponent thing, bool pushed = true)
        {
            SetupRadioButtonHiddenToggle(new WeakReference<RadioButton>(button), new WeakReference<ITaskItemComponent>(thing), pushed);
        }

        /// <summary>
        /// Creates a text field and ensures it redraws when necessary
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="redraw">Redraw</param>
        /// <param name="redrawAndReload">Redraw and reload</param>
        /// <returns>The textfield</returns>
        public static TextField CreateTextField(RenderContext context, EventHandler redraw, EventHandler redrawAndReload)
        {
            var result = UIUtils.CreateTextField(new Point(0, 0), new Point(150, 30));

            result.FocusGained += redraw;
            result.FocusLost += redraw;
            result.TextChanged += redraw;

            return result;
        }
    }
}
