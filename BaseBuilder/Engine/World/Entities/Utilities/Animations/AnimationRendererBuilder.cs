﻿using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities.Animations
{
    public class AnimationRendererBuilder
    {
        const string Unset = "kljhlkjh";
        class UnbuiltAnimationFrame
        {
            public Texture2D Texture;
            public SoundEffect SoundEffect;
            public Rectangle SourceRec;
            public PointD2D TopLeftDif;
            public int DisplayTimeMS;
        }

        class UnbuiltAnimation
        {
            public AnimationType Type;
            public List<UnbuiltAnimationFrame> Frames;
            public Direction? Direction;
            public int BeginFrame;
            public int CycleStartFrame;

            public UnbuiltAnimation()
            {
                Frames = new List<UnbuiltAnimationFrame>();
            }
        }
        
        ContentManager Content;

        List<UnbuiltAnimation> CompletedAnimations;
        UnbuiltAnimation CurrentAnimation;
        int CurrentAnimationMajorAxis;
        int CurrentAnimationDefaultSpacing;
        int CurrentAnimationMinorAxis;
        bool CurrentAnimationHorizontal;
        int CurrentAnimationDefaultWidth;
        int CurrentAnimationDefaultHeight;
        PointD2D CurrentAnimtionDefaultTopLeftDif;
        int CurrentAnimationDefaultDisplayTime;
        string CurrentAnimationDefualtSound;
        string CurrentAnimationDefaultSourceTexture;


        public AnimationRendererBuilder(ContentManager content)
        {
            Content = content;
            CompletedAnimations = new List<UnbuiltAnimation>();
        }

        public AnimationRendererBuilder BeginAnimation(Direction? dir, AnimationType type, PointD2D defualtTopLeftDif = null, int defualtDisplayTime = 250, string defaultSourceTexture = null, int defualtSpacing = 32,
                            int yLocation = 0, int cycleStartLocation = 0, int startLocation = 0, int defualtWidth = 32, int defaultHeight = 32, bool horizontalOrintation = true,
                            string defualtSound = null)
        {
            if (CurrentAnimation != null)
                throw new InvalidOperationException($"Begin animation called before ending previous animation: check for matching BeginAnimation and EndAnimation CurrentAnimationDefaultSourceTexture={CurrentAnimationDefaultSourceTexture}");

            CurrentAnimation = new UnbuiltAnimation();
            CurrentAnimation.Direction = dir;
            CurrentAnimation.Type = type;
            CurrentAnimation.BeginFrame = startLocation;
            CurrentAnimation.CycleStartFrame = cycleStartLocation;

            CurrentAnimationDefaultSourceTexture = defaultSourceTexture;
            CurrentAnimationMajorAxis = 0;
            CurrentAnimationDefaultSpacing = defualtSpacing;
            CurrentAnimationMinorAxis = yLocation;
            CurrentAnimationDefaultWidth = defualtWidth;
            CurrentAnimationDefaultHeight = defaultHeight;
            CurrentAnimtionDefaultTopLeftDif = defualtTopLeftDif;
            CurrentAnimationDefaultDisplayTime = defualtDisplayTime;
            CurrentAnimationHorizontal = true;

            CurrentAnimationDefualtSound = defualtSound;

            return this;
        }
        public AnimationRendererBuilder EndAnimation()
        {
            if (CurrentAnimation == null)
                throw new InvalidOperationException($"End animation called before starting an animation: check for matching BeginAnimation and EndAnimation CurrentAnimationDefaultSourceTexture={CurrentAnimationDefaultSourceTexture}");
            if (CurrentAnimation.Frames.Count == 0)
                throw new InvalidOperationException($"End animation called before adding an animation frame: check for AddFrame call CurrentAnimationDefaultSourceTexture={CurrentAnimationDefaultSourceTexture}");

            CompletedAnimations.Add(CurrentAnimation);
            CurrentAnimation = null;

            return this;
        }

        public AnimationRendererBuilder AddFrame(bool addToBasePosition = false, int x = -1, int y = -1, int height = -1, int width = -1, string sound = Unset, string sourceTexture = Unset,
                      PointD2D topLeftDif = null, int displayTime = -1, int spacing = -1)
        {
            var frame = new UnbuiltAnimationFrame();

            if (sourceTexture.Equals(Unset))
            {
                if (CurrentAnimationDefaultSourceTexture == null)
                    throw new ArgumentException("sourceTexture is null without a defualt source texture set", nameof(sourceTexture));
                sourceTexture = CurrentAnimationDefaultSourceTexture;
            }
            frame.Texture = Content.Load<Texture2D>(sourceTexture);

            if (sound.Equals(Unset))
            {
                sound = CurrentAnimationDefualtSound;
            }
            if (sound == null)
                frame.SoundEffect = null;
            else
                frame.SoundEffect = Content.Load<SoundEffect>(sound);

            if (x == -1)
            {
                x = CurrentAnimationMajorAxis;
            }
            if (y == -1)
            {
                y = CurrentAnimationMinorAxis;
            }
            if (height == -1)
            {
                height = CurrentAnimationDefaultHeight;
            }
            if (width == -1)
            {
                width = CurrentAnimationDefaultWidth;
            }
            if (addToBasePosition)
            {
                if (x != -1)
                {
                    x += CurrentAnimationMajorAxis;
                }
                if (y != -1)
                {
                    y += CurrentAnimationMinorAxis;
                }
            }
            frame.SourceRec = new Rectangle(x, y, height, width);

            if (topLeftDif == null)
            {
                if (CurrentAnimtionDefaultTopLeftDif == null)
                    throw new ArgumentException("topLeftDif is null without a defualt set", nameof(topLeftDif));
                topLeftDif = CurrentAnimtionDefaultTopLeftDif;
            }
            frame.TopLeftDif = topLeftDif;

            if (displayTime == -1)
            {
                displayTime = CurrentAnimationDefaultDisplayTime;
            }
            frame.DisplayTimeMS = displayTime;

            CurrentAnimation.Frames.Add(frame);
            if (CurrentAnimationHorizontal)
                if (spacing == -1)
                    spacing = CurrentAnimationDefaultSpacing;
            CurrentAnimationMajorAxis += spacing;

            return this;
        }
        
        public SpriteSheetAnimationRenderer2 Build()
        {
            var dic = new Dictionary<AnimationType, List<Animation2>>();
            var typeList = new List<AnimationType>() { AnimationType.Idle, AnimationType.Moving, AnimationType.Chopping, AnimationType.Logging };
            foreach (var currType in typeList)
            {
                var currList = new List<Animation2>();
                for (int i = CompletedAnimations.Count - 1; i >= 0; i--)
                {
                    if (CompletedAnimations[i].Type == currType)
                    {
                        var frameList = new List<AnimationFrame2>();
                        foreach (var frame in CompletedAnimations[i].Frames)
                            frameList.Add(new AnimationFrame2(frame.Texture, frame.SoundEffect, frame.SourceRec, frame.TopLeftDif, frame.DisplayTimeMS));
                        currList.Add(new Animation2(frameList, CompletedAnimations[i].Direction, CompletedAnimations[i].CycleStartFrame, CompletedAnimations[i].BeginFrame));
                        CompletedAnimations.RemoveAt(i);
                    }
                }
                dic.Add(currType, currList);
            }
            return new SpriteSheetAnimationRenderer2(dic);
        }
    }
}
