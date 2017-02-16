using System;
using System.Collections.Generic;
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
    }
}
