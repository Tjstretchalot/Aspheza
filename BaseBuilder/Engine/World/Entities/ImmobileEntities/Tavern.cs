using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.State.Resources;
using System.IO;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class Tavern : ImmobileEntity
    {
        protected static CollisionMeshD2D _CollisionMesh;

        protected SpriteRenderer Renderer;
        static Rectangle SourceRec = new Rectangle(0, 0, 218, 212);
                
        static Tavern()
        {
            double scale = 1 / 32.0;
            var pixelPolys = new List<List<PointD2D>>
            {
                new List<PointD2D> { new PointD2D(0, 90), new PointD2D(30, 60), new PointD2D(33, 60), new PointD2D(52, 78), new PointD2D(52, 168), new PointD2D(0, 168) },
                new List<PointD2D> { new PointD2D(52, 80), new PointD2D(102, 80), new PointD2D(102, 168), new PointD2D(52, 168) },
                new List<PointD2D> { new PointD2D(102, 80), new PointD2D(120, 60), new PointD2D(154, 90), new PointD2D(154, 168), new PointD2D(102, 168) },
                new List<PointD2D> { new PointD2D(154, 0), new PointD2D(218, 0), new PointD2D(218, 212), new PointD2D(154, 212) },

                new List<PointD2D> { new PointD2D(60, 168), new PointD2D(60, 174), new PointD2D(70, 186), new PointD2D(102, 181), new PointD2D(102, 168) }
            };

            var worldPolys = new List<PolygonD2D>();
            foreach(var poly in pixelPolys)
            {
                var worldList = new List<PointD2D>();
                foreach(var pixelP in poly)
                {
                    worldList.Add(new PointD2D(pixelP.X * scale, pixelP.Y * scale));
                }

                worldPolys.Add(new PolygonD2D(worldList));
            }

            _CollisionMesh = new CollisionMeshD2D(worldPolys);
        }

        public Tavern(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Tavern", SourceRec);
            _HoverText = "Tavern";
            
        }

        public Tavern() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Tavern", SourceRec);
        }
        
        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            _HoverText = message.ReadString();
            
            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            message.Write(_HoverText);

            WriteTasks(message);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 218 / 32.0, 212 / 32.0, overlay);
        }

        public override SpriteSheetAnimationRenderer GetInprogressRenderable(ContentManager content)
        {
            const int width = 218;
            const int height = 212;
            const string img = "Temple";
            return new AnimationRendererBuilder(content)
                .BeginAnimation(null, AnimationType.Idle, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: height - (int)(height * 0.1), height: (int)(height * 0.1), topLeftDif: new PointD2D(0, (int)(height * 0.1) - height))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.Unbuilt, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: height - (int)(height * 0.1), height: (int)(height * 0.1), topLeftDif: new PointD2D(0, (int)(height * 0.1) - height))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltThirty, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: height - (int)(height * 0.3), height: (int)(height * 0.3), topLeftDif: new PointD2D(0, (int)(height * 0.3) - height))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltSixty, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: height - (int)(height * 0.6), height: (int)(height * 0.6), topLeftDif: new PointD2D(0, (int)(height * 0.6) - height))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltNinety, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: 0, height: height, topLeftDif: new PointD2D(0, 0))
                .EndAnimation()
                .Build();
        }
    }
}
