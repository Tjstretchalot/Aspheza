using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Screens.Components;
using BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems;
using BaseBuilder.Screens.GComponents.ScrollableComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.Components.ScrollableComponents
{
    /// <summary>
    /// A collection of utility functions for complex task items
    /// </summary>
    public class ScrollableComponentUtils
    {
        /// <summary>
        /// Wraps all of the specified screen components, losing their type information.
        /// </summary>
        /// <param name="comps">The components</param>
        /// <returns>The wrapped components</returns>
        public static List<IScrollableComponent> WrapAll(params IScreenComponent[] comps)
        {
            var result = new List<IScrollableComponent>();

            foreach (var comp in comps)
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
        public static ScrollableComponentFromScreenComponent<T> Wrap<T>(T component) where T : IScreenComponent
        {
            return new ScrollableComponentFromScreenComponent<T>(component);
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
        public static T1 Unwrap<T1>(WeakReference<ScrollableComponentFromScreenComponent<T1>> weak) where T1 : IScreenComponent
        {
            return Unwrap(MakeStrong(weak));
        }

        /// <summary>
        /// Unwraps the specified component
        /// </summary>
        /// <typeparam name="T1">The type of wrapped component</typeparam>
        /// <param name="strong">The strong reference</param>
        /// <returns>The component</returns>
        /// <exception cref="InvalidProgramException">If the component is disposed</exception>
        public static T1 Unwrap<T1>(ScrollableComponentFromScreenComponent<T1> strong) where T1 : IScreenComponent
        {
            if (strong.Disposed)
                throw new InvalidProgramException("Component is disposed!");

            return strong.Component;
        }

        /// <summary>
        /// Makes the weak reference strong, throwing an exception if the
        /// weak reference is lost
        /// </summary>
        /// <typeparam name="T1">The referenced type</typeparam>
        /// <param name="weak">Weak reference</param>
        /// <returns>Strong reference</returns>
        public static T1 MakeStrong<T1>(WeakReference<T1> weak) where T1 : class
        {
            T1 strong;
            if (!weak.TryGetTarget(out strong))
                throw new InvalidProgramException("Weak reference lost!");

            return strong;
        }

        /// <summary>
        /// Tries to get the task item component from the weak reference, returning false
        /// if the weak reference is lost OR the underlying component is disposed.
        /// </summary>
        /// <typeparam name="T1">Referenced screen component type</typeparam>
        /// <param name="weakWrapped">The wrapped delegate</param>
        /// <param name="value">The variable to set</param>
        /// <returns>If the value was set, false otherwise</returns>
        public static bool TryGetWrapped<T1>(WeakReference<T1> weakWrapped, out T1 value) where T1 : class, IScrollableComponent
        {
            value = default(T1);

            T1 strong;
            if (!weakWrapped.TryGetTarget(out strong))
                return false;

            if (strong.Disposed)
                return false;

            value = strong;
            return true;
        }

        /// <summary>
        /// Sets up the specified component to have a label, either vertically or horizontally.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="label">The label</param>
        /// <param name="component">The component to label</param>
        /// <param name="vertical">True if the label should be above, false if the label should be to the right</param>
        /// <returns>The wrapped component</returns>
        public static IScrollableComponent Label(RenderContext context, string label, IScrollableComponent component, bool vertical = true)
        {
            if (vertical)
            {
                var result = new VerticalFlowScrollableComponent(VerticalFlowScrollableComponent.VerticalAlignmentMode.LeftAlignSuggested, 3);

                result.Children.Add(Wrap(new Text(new Point(0, 0), label, context.DefaultFont, Color.Black)));
                result.Children.Add(component);

                return result;
            } else
            {
                var result = new HorizontalFlowScrollableComponent(HorizontalFlowScrollableComponent.HorizontalAlignmentMode.CenterAlignSuggested, 3);

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
        public static IScrollableComponent CreatePadding(int width, int height)
        {
            return new PaddingScrollableComponent(width, height);
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

            foreach (var opt in options)
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
        public static void SetupComboBoxHiddenToggle<T1>(WeakReference<ComboBox<T1>> boxWeak, T1 choice, WeakReference<IScrollableComponent> thingToToggleWeak)
        {
            ComboBox<T1> boxStrong;
            if (!boxWeak.TryGetTarget(out boxStrong))
            {
                throw new InvalidProgramException("Weak reference already lost");
            }

            if (boxStrong.Selected == null || EqualityComparer<T1>.Default.Equals(boxStrong.Selected.Value, choice))
            {
                IScrollableComponent thingStrong;

                if (TryGetWrapped(thingToToggleWeak, out thingStrong))
                {
                    thingStrong.Hidden = true;
                }
            }

            ComboBoxSelectedChangedEventHandler<T1> handler = null;
            handler = (sender, oldSelected) =>
            {
                ComboBox<T1> boxStrong2;
                IScrollableComponent thingToToggleStrong;
                if (!thingToToggleWeak.TryGetTarget(out thingToToggleStrong) || thingToToggleStrong.Disposed)
                {
                    if (boxWeak.TryGetTarget(out boxStrong2))
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

                if (comparer.Equals(boxStrong2.Selected.Value, choice))
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
        public static void SetupComboBoxHiddenToggle<T1>(ComboBox<T1> box, T1 choice, IScrollableComponent thing)
        {
            SetupComboBoxHiddenToggle(new WeakReference<ComboBox<T1>>(box), choice, new WeakReference<IScrollableComponent>(thing));
        }

        /// <summary>
        /// Create text with default font and the specified text thats white
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="text">Text</param>
        /// <param name="cache">If it should be cached</param>
        /// <returns>Text</returns>
        public static Text CreateText(RenderContext context, string text, bool cache = false)
        {
            return new Text(new Point(0, 0), text, context.DefaultFont, Color.White, cache);
        }

        /// <summary>
        /// Create texture with one times zoom loaded from the content manager with
        /// the specified texure name
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="textureName">TextureName</param>
        /// <param name="size">Size, if null will be set to the size of the texture.</param>
        /// <param name="disposeRequired">If the texture component should dispose the texture.</param>
        /// <returns>The TextureComponent</returns>
        public static TextureComponent CreateTexture(RenderContext context, string textureName, PointI2D size = null, bool disposeRequired = false)
        {
            var texture = context.Content.Load<Texture2D>(textureName);
            if (size == null)
                size = new PointI2D(texture.Width, texture.Height);
            return new TextureComponent(texture, new Rectangle(0, 0, size.X, size.Y), disposeRequired);
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
        public static void SetupRadioButtonHiddenToggle(WeakReference<RadioButton> radioButtonWeak, WeakReference<IScrollableComponent> thingWeak, bool pushed = true)
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

                IScrollableComponent thingStrong;
                if (!thingWeak.TryGetTarget(out thingStrong))
                {
                    radioButtonStrong2.PushedChanged -= handler;   
                    return;
                }

                thingStrong.Hidden = radioButtonStrong2.Pushed != pushed;
            };

            radioButtonStrong.PushedChanged += handler;

            MakeStrong(thingWeak).Hidden = radioButtonStrong.Pushed != pushed;
        }

        /// <summary>
        /// As if you called SetupRadioButtonHiddenToggle(new WeakReference(checkbox), new WeakReference(thing), pushed)
        /// </summary>
        /// <param name="button">The radio button</param>
        /// <param name="thing">The thing</param>
        /// <param name="pushed">The state to unhide the thing</param>
        public static void SetupRadioButtonHiddenToggle(RadioButton button, IScrollableComponent thing, bool pushed = true)
        {
            SetupRadioButtonHiddenToggle(new WeakReference<RadioButton>(button), new WeakReference<IScrollableComponent>(thing), pushed);
        }

        /// <summary>
        /// Creates a button that will redraw and reload correctly
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="redraw">Redraw handler</param>
        /// <param name="redrawAndReload">Redraw and reload handler</param>
        /// <param name="text">The text on the button</param>
        /// <param name="color">The color of the button</param>
        /// <returns>The button</returns>
        public static Button CreateButton(RenderContext context, EventHandler redraw, EventHandler redrawAndReload, string text, UIUtils.ButtonColor color = UIUtils.ButtonColor.Blue)
        {
            var result = UIUtils.CreateButton(new Point(0, 0), text, color, UIUtils.ButtonSize.Medium);

            result.HoveredChanged += redraw;
            result.PressedChanged += redraw;
            result.PressReleased += redrawAndReload;

            return result;
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
