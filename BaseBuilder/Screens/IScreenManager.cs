using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens
{
    /// <summary>
    /// Handles transitioning between screens.
    /// </summary>
    public interface IScreenManager
    {
        /// <summary>
        /// The current screen
        /// </summary>
        IScreen CurrentScreen { get; }

        /// <summary>
        /// Transitions from the current screen to the new screen, using the
        /// specified transition.
        /// </summary>
        /// <param name="newScreen">The new screen</param>
        /// <param name="transition">The transition to use</param>
        void TransitionTo(IScreen newScreen, IScreenTransition transition);

        /// <summary>
        /// Updates the screen manager.
        /// </summary>
        /// <param name="deltaMS">The time since the last call to update, in milliseconds</param>
        void Update(int deltaMS);

        /// <summary>
        /// Draws the current screen or transition as appropriate.
        /// </summary>
        void Draw();
    }
}
