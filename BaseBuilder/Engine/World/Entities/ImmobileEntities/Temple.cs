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
using BaseBuilder.Engine.World.Entities.Utilities.Animations;
using Microsoft.Xna.Framework.Content;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class Temple : ImmobileEntity
    {
        protected static CollisionMeshD2D _CollisionMesh;

        protected SpriteRenderer Renderer;
        static Rectangle SourceRec = new Rectangle(0, 0, 100, 132);

        static Temple()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(3, 4) });
        }

        public Temple(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Temple", SourceRec);
            _HoverText = "Temple";

        }

        public Temple() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Temple", SourceRec);
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
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 100 / 32.0, 132 / 32.0, overlay);
        }

        public override SpriteSheetAnimationRenderer GetInprogressRenderable(ContentManager content)
        {
            return new AnimationRendererBuilder(content)
                .BeginAnimation(null, AnimationType.Idle, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 132 - 20, height: 20, topLeftDif: new PointD2D(0, -112))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.Unbuilt, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 132 - 20, height: 20, topLeftDif: new PointD2D(0, -112))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltThirty, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 132 - 59, height: 59, topLeftDif: new PointD2D(0, -73))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltSixty, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 132 - 99, height: 99, topLeftDif: new PointD2D(0, -33))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltNinety, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 0, height: 132, topLeftDif: new PointD2D(0, 0))
                .EndAnimation()
                .Build();
        }
    }
}
