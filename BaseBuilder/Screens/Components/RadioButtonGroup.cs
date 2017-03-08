using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// A radio button group just manages a group of radio buttons so they
    /// act like radio buttons (no more than 1 pressed at a time). 
    /// </summary>
    public class RadioButtonGroup
    {
        /// <summary>
        /// The buttons in the group
        /// </summary>
        public ICollection<RadioButton> Buttons;

        /// <summary>
        /// Sets up a radio button group, but does not attach to it.
        /// </summary>
        /// <param name="buttons">The buttons in the group</param>
        public RadioButtonGroup(ICollection<RadioButton> buttons)
        {
            Buttons = buttons;
        }

        /// <summary>
        /// Attaches this radio button group to the buttons
        /// </summary>
        public void Attach()
        {
            foreach(var but in Buttons)
            {
                but.PushedChanged += ResetOthers;
            }
        }

        /// <summary>
        /// Detaches this button group from the buttons
        /// </summary>
        public void Detach()
        {
            foreach(var but in Buttons)
            {
                but.PushedChanged -= ResetOthers;
            }
        }

        void ResetOthers(object sender, EventArgs args)
        {
            var button = sender as RadioButton;
            if (button == null)
                throw new ArgumentNullException(nameof(sender));

            if (!button.Pushed)
                return;

            foreach(var but in Buttons)
            {
                if (!ReferenceEquals(but, button))
                    but.Pushed = false;
            }
        }
    }
}
