﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.State;
using Lidgren.Network;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class StorageBarn : ImmobileEntity, Directional
    {
        protected static Dictionary<Direction, Tuple<Rectangle, PolygonD2D>> DirectionToSourceRectAndCollisionMesh;

        static StorageBarn()
        {
            PolygonD2D collisionMeshHorizontal = new RectangleD2D(6, 3);
            PolygonD2D collisionMeshVertical = new RectangleD2D(3, 6);

            var sourceRectLeft = new Rectangle(0, 0, 96, 48);
            var sourceRectRight = new Rectangle(96, 0, 96, 48);
            var sourceRectUp = new Rectangle(0, 48, 48, 96);
            var sourceRectDown = new Rectangle(48, 48, 48, 96);

            DirectionToSourceRectAndCollisionMesh = new Dictionary<Direction, Tuple<Rectangle, PolygonD2D>>{
                { Direction.Down, Tuple.Create(sourceRectDown, collisionMeshVertical) },
                { Direction.Up, Tuple.Create(sourceRectUp, collisionMeshVertical) },
                { Direction.Left, Tuple.Create(sourceRectLeft, collisionMeshHorizontal) },
                { Direction.Right, Tuple.Create(sourceRectRight, collisionMeshHorizontal) }
            };
        }

        protected Direction _Direction;
        public Direction Direction
        {
            get
            {
                return _Direction;
            }
            set
            {
                var srcRectAndCollMesh = DirectionToSourceRectAndCollisionMesh[value];
                CollisionMesh = srcRectAndCollMesh.Item2;
                Renderer = new SpriteRenderer("StorageBarn", srcRectAndCollMesh.Item1);
                _Direction = value;
            }
        }

        public EntityInventory Inventory;
        protected SpriteRenderer Renderer;

        public StorageBarn(PointD2D position, int id, Direction direction) : base(position, null, id)
        {
            Direction = direction;
            Inventory = new EntityInventory(40);
        }

        /// <summary>
        /// For from message
        /// </summary>
        public StorageBarn() : base()
        {

        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Direction = (Direction)message.ReadInt32();
            Inventory = new EntityInventory(message);

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            message.Write((int)Direction);
            Inventory.Write(message);

            WriteTasks(message);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, (int)(CollisionMesh.Right - CollisionMesh.Left), (int)(CollisionMesh.Bottom - CollisionMesh.Top), overlay);
        }
    }
}
