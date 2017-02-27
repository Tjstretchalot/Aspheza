using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;

namespace BaseBuilder.Screens.GameScreens
{
    public class SFXUtils
    {
        private static Random random;
        
        static SFXUtils()
        {
            random = new Random();
        }

        public static void PlaySAXJingle(ContentManager content)
        {
            int ind = random.Next(17);

            string indStr;
            if (ind < 10)
                indStr = $"0{ind}";
            else
                indStr = $"{ind}";

            var sfxName = $"Sounds/jingles_SAX{indStr}";

            content.Load<SoundEffect>(sfxName).Play();
        }
    }
}