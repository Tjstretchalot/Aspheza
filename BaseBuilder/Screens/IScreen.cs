using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens
{
    /// <summary>
    /// Describes the characteristics of a screen
    /// </summary>
    public interface IScreen
    {
        /// <summary>
        /// Updates the screen. 
        /// </summary>
        /// <param name="deltaMS">Time in milliseconds since the last call to Update</param>
        void Update(int deltaMS);

        /// <summary>
        /// Draws the screen.
        /// </summary>
        void Draw();
    }
}
