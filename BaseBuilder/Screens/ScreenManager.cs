using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens
{
    public class ScreenManager : IScreenManager
    {
        protected IScreen _CurrentScreen;
        public IScreen CurrentScreen
        {
            get
            {
                return _CurrentScreen;
            }
        }

        protected int? CurrentTransitionMSSoFar;
        protected int? CurrentTransitionMSTotal;
        protected IScreenTransition CurrentTransition;
        protected IScreen NewScreen;
        protected bool? TransitionComplete;

        protected Dictionary<string, string> SavedFields;

        public ScreenManager()
        {
        }

        public void SetInitialScreen(IScreen screen)
        {
            _CurrentScreen = screen;
            CurrentTransitionMSSoFar = null;
            CurrentTransitionMSTotal = null;
        }

        public void Draw()
        {
            if(CurrentTransition != null)
            {
                CurrentTransition.Draw();
            }else
            {
                _CurrentScreen.Draw();
            }
        }

        public void TransitionTo(IScreen newScreen, IScreenTransition transition, int transitionTimeMS)
        {
            CurrentTransition = transition;
            NewScreen = newScreen;
            CurrentTransitionMSSoFar = 0;
            CurrentTransitionMSTotal = transitionTimeMS;
        }

        public void Update(int deltaMS)
        {
            if(TransitionComplete.HasValue && TransitionComplete.Value)
            {
                CurrentTransition.Finished();

                _CurrentScreen = NewScreen;

                CurrentTransition = null;
                CurrentTransitionMSSoFar = null;
                CurrentTransitionMSTotal = null;
                TransitionComplete = null;
                NewScreen = null;
            }

            if(CurrentTransition != null)
            {
                CurrentTransitionMSSoFar += deltaMS;

                var progress = ((double)CurrentTransitionMSSoFar.Value) / CurrentTransitionMSTotal.Value;
                if (progress >= 1) {
                    progress = 1;
                    TransitionComplete = true;
                }
                CurrentTransition.Update(progress);
            }else
            {
                _CurrentScreen.Update(deltaMS);
            }
        }

        protected static string GetConfigFolder()
        {
            return Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "My Games", "BaseBuilderCustom", "ScreenConfig");
        }

        public static void LoadConfig(Dictionary<string, string> dictionaryWithDefaultsSet, string screenConfigFileName)
        {
            var file = Path.Combine(GetConfigFolder(), screenConfigFileName);

            if (!File.Exists(file))
                return;
            
            using (var reader = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                int count = reader.ReadInt32();
                for (int n = 0; n < count; n++)
                {
                    var key = reader.ReadString();
                    var value = reader.ReadString();
                    
                    dictionaryWithDefaultsSet[key] = value;
                }
            }
        }

        public static void SaveConfig(Dictionary<string, string> dictionary, string screenConfigFileName)
        {
            var folder = GetConfigFolder();
            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var file = Path.Combine(GetConfigFolder(), screenConfigFileName);
            
            using (var writer = new BinaryWriter(File.Open(file, FileMode.Create)))
            {
                writer.Write(dictionary.Count);
                foreach (var kvp in dictionary)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
            }
        }
    }
}
