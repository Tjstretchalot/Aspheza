﻿using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.State.Resources;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public class CaveManWorker : MobileEntity, Container
    {
        private SpriteSheetAnimationRenderer AnimationRenderer;
        private const double SpeedConst = 0.005;
        private static CollisionMeshD2D _CollisionMesh;

        static private Dictionary<string, Tuple<string, List<Rectangle>>> StringToSourceRecList = new Dictionary<string, Tuple<string, List<Rectangle>>>()
        {
            { "DownMove", new Tuple<string, List<Rectangle>> ( "CaveManWorker/CaveManWorker 32x32", new List<Rectangle> { new Rectangle(0, 0, 32, 32), new Rectangle(32, 0, 32, 32), new Rectangle(64, 0, 32, 32), new Rectangle(96, 0, 32, 32) } ) },
            { "UpMove", new Tuple<string, List<Rectangle>> ( "CaveManWorker/CaveManWorker 32x32", new List<Rectangle> { new Rectangle(0, 32, 32, 32), new Rectangle(32, 32, 32, 32), new Rectangle(64, 32, 32, 32), new Rectangle(96, 32, 32, 32) } ) },
            { "RightMove", new Tuple<string, List<Rectangle>> ( "CaveManWorker/CaveManWorker 32x32", new List<Rectangle> { new Rectangle(0, 64, 32, 32), new Rectangle(32, 64, 32, 32), new Rectangle(64, 64, 32, 32), new Rectangle(96, 64, 32, 32) } ) },
            { "LeftMove",  new Tuple<string, List<Rectangle>> ( "CaveManWorker/CaveManWorker 32x32", new List<Rectangle> { new Rectangle(0, 96, 32, 32), new Rectangle(32, 96, 32, 32), new Rectangle(64, 96, 32, 32), new Rectangle(96, 96, 32, 32) } ) },
        };

        public EntityInventory Inventory { get; set; }

        static CaveManWorker()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(1, 1) });
        }

        public CaveManWorker(PointD2D position, int id) : base(position, _CollisionMesh, id, SpeedConst)
        {
            Inventory = new EntityInventory(6);
            Inventory.SetStackSizeFor(Material.Sapling, 5);
            AnimationRenderer = new SpriteSheetAnimationRenderer(StringToSourceRecList);
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public CaveManWorker() : base()
        {
            AnimationRenderer = new SpriteSheetAnimationRenderer(StringToSourceRecList);
            CollisionMesh = _CollisionMesh;
            SpeedUnitsPerMS = SpeedConst;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);
            AnimationRenderer.FromMessage(message);

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);
            AnimationRenderer.Write(message);

            WriteTasks(message);
        }

        public override void OnMove(SharedGameState sharedState, int timeMS, double dx, double dy)
        {
            AnimationRenderer.UpdateSprite(sharedState, timeMS, dx, dy);
        }

        public override void OnStop(SharedGameState sharedState)
        {
            AnimationRenderer.MoveComplete(sharedState);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            AnimationRenderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, (int)(CollisionMesh.Right - CollisionMesh.Left), (int)(CollisionMesh.Bottom - CollisionMesh.Top), overlay);
        }
    }
}
