using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace BaseBuilder.Screens.GameScreens
{
    public class GameBackgroundMusicComponent : MyGameComponent
    {
        // song, initial time
        protected Tuple<string, int>[] BackgroundSongs;

        protected SoundEffect CurrentSong;
        protected SoundEffectInstance CurrentSongInstance;
        protected Stopwatch CurrentSongStopwatch;

        protected bool SwitchSongsNext;

        protected int CurrentSongIndex;
        protected int CurrentSongLoopCounter;

        protected bool FadingIn;
        protected bool FadingOut;
        protected float FadePerMS;
        protected float CurrentVolume;

        protected bool AlreadyConsideredFadeOut;
        protected bool SwitchingSongs;

        protected Random random;

        public GameBackgroundMusicComponent(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) : base(content, graphics, graphicsDevice, spriteBatch)
        {
            BackgroundSongs = new Tuple<string, int>[]
            {
                Tuple.Create("Music/Alpha Dance", 1116),
                 Tuple.Create("Music/Cheerful Annoyance", 543),
                 Tuple.Create("Music/Drumming Sticks", 1016),
                 Tuple.Create("Music/Farm Frolics", 1516),
                 Tuple.Create("Music/Flowing Rocks", 1038),
                 Tuple.Create("Music/Game Over", 2270),
                 Tuple.Create("Music/German Virtue", 1174),
                 Tuple.Create("Music/Infinite Descent", 1749),
                 Tuple.Create("Music/Italian Mom", 1287),
                 Tuple.Create("Music/Mission Plausible", 1009),
                 Tuple.Create("Music/Mishief Stroll", 1090),
                 Tuple.Create("Music/Night at the Beach", 577),
                 Tuple.Create("Music/Polka Train", 707),
                 Tuple.Create("Music/Sad Descent", 3654),
                 Tuple.Create("Music/Sad Town", 2237),
                 Tuple.Create("Music/Space Cadet", 4037),
                 Tuple.Create("Music/Swinging Pants", 722),
                 Tuple.Create("Music/Time Driving", 2470),
                 Tuple.Create("Music/Wacky Waiting", 875)
            };

            CurrentSong = null;
            CurrentSongIndex = -1;
            CurrentSongLoopCounter = 0;
            FadingIn = false;
            FadingOut = false;
            FadePerMS = 0;
            CurrentVolume = 0;

            random = new Random();
        }

        public override void Draw(RenderContext context)
        {
        }

        public override void Update(SharedGameState sharedGameState, LocalGameState localGameState, int timeMS)
        {
            if(CurrentSong == null)
            {
                ChooseInitialSong();
                PlayNewlyChosenSong();
                return;
            }


            var timeLeftMS = (long)CurrentSong.Duration.TotalMilliseconds - CurrentSongStopwatch.ElapsedMilliseconds;
            if (!SwitchingSongs && timeLeftMS <= BackgroundSongs[CurrentSongIndex].Item2)
            {
                SwitchingSongs = DecideIfSwitchingNextSong();
                if (!SwitchingSongs)
                    LoopCurrentSong();

                return;
            }

            if(SwitchingSongs && timeLeftMS <= 0)
            {
                SwitchSong();
                PlayNewlyChosenSong();
                SwitchingSongs = false;
            }

            if(FadingIn)
            {
                HandleSongFadeIn(timeMS);
            }else if(FadingOut)
            {
                HandleSongFadeOut(timeMS);
            }else
            {
                ConsiderSongFadeOut(timeLeftMS);
            }
            
        }

        void ChooseInitialSong()
        {
            CurrentSongIndex = random.Next(BackgroundSongs.Length);
        }

        bool DecideIfSwitchingNextSong()
        {
            if(!AlreadyConsideredFadeOut)
            {
                var percSwitchChance = CurrentSong.Duration.TotalSeconds * (5.0 / 6.0) * CurrentSongLoopCounter;
                SwitchSongsNext = random.Next(100) <= percSwitchChance;
            }
            return SwitchSongsNext;
        }

        void SwitchSong()
        {
            var bannedIndex = CurrentSongIndex;
            var newChoice = random.Next(BackgroundSongs.Length - 1);

            if (newChoice >= bannedIndex)
                newChoice++;

            CurrentSongIndex = newChoice;
        }



        void PlayNewlyChosenSong()
        {
            CurrentSong = Content.Load<SoundEffect>(BackgroundSongs[CurrentSongIndex].Item1);
            CurrentSongInstance = CurrentSong.CreateInstance();
            CurrentSongStopwatch = new Stopwatch();

            CurrentVolume = 0;
            FadingIn = true;
            FadingOut = false;
            FadePerMS = 0.001f;
            CurrentSongLoopCounter = 0;

            CurrentSongInstance.Volume = CurrentVolume;
            CurrentSongInstance.Play();
            CurrentSongStopwatch.Start();

            AlreadyConsideredFadeOut = false;
        }

        void LoopCurrentSong()
        {
            CurrentSongLoopCounter++;

            CurrentSongInstance.Stop();
            CurrentSongInstance = CurrentSong.CreateInstance();

            CurrentVolume = 1.0f;
            CurrentSongInstance.Volume = CurrentVolume;
            CurrentSongInstance.Play();
            CurrentSongStopwatch.Restart();

            AlreadyConsideredFadeOut = false;
            FadingIn = false;
            FadingOut = false;
        }

        void HandleSongFadeIn(int timeMS)
        {
            CurrentVolume += FadePerMS * timeMS;

            if(CurrentVolume >= 1)
            {
                FadingIn = false;
                CurrentVolume = 1.0f;
            }

            CurrentSongInstance.Volume = CurrentVolume;
        }

        void HandleSongFadeOut(int timeMS)
        {
            CurrentVolume -= FadePerMS * timeMS;

            if (CurrentVolume <= 0)
            {
                FadingOut = false;
                CurrentVolume = 0.0f;
            }

            CurrentSongInstance.Volume = CurrentVolume;
        }

        void ConsiderSongFadeOut(long timeLeftMS)
        {
            if(!AlreadyConsideredFadeOut && SwitchSongsNext && timeLeftMS <= 1000)
            {
                FadingOut = true;
                FadePerMS = 1.0f / timeLeftMS;
            }
            else if (timeLeftMS <= 1000 && !AlreadyConsideredFadeOut)
            {
                var percSwitchChance = CurrentSong.Duration.TotalSeconds * (5.0 / 6.0) * CurrentSongLoopCounter;
                SwitchSongsNext = random.Next(100) <= percSwitchChance;

                if (SwitchSongsNext)
                {
                    FadingOut = true;
                    FadePerMS = 1.0f / (timeLeftMS);
                }

                AlreadyConsideredFadeOut = true;
            }
        }
    }
}
