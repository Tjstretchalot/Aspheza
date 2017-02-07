namespace BaseBuilder.Screens
{
    /// <summary>
    /// Describes a transition between one screen and another screen
    /// </summary>
    public interface IScreenTransition
    {
        /// <summary>
        /// Updates this transition to reflect the progress the transition should display.
        /// </summary>
        /// <remarks>
        /// At progress=0 the screen should be the original screen exactly, and at progress=100 the screen should
        /// be the new screen exactly.
        /// </remarks>
        /// <param name="progress">A number between 0 and 1, 0 being 0% complete and 1 being 100% complete.</param>
        void Update(double progress);

        /// <summary>
        /// Draws the screen using the progress from the last call to Update, or 0
        /// if Update has not been called.
        /// </summary>
        void Draw();
    }
}