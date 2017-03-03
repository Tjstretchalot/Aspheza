using System;
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
using BaseBuilder.Engine.State.Resources;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class StorageBarn : ImmobileEntity, Directional, Container
    {
        protected static Dictionary<Direction, Tuple<Rectangle, CollisionMeshD2D>> DirectionToSourceRectAndCollisionMesh;

        static StorageBarn()
        {
            CollisionMeshD2D collisionMeshHorizontal = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(100 / 16, 86 / 16) });
            CollisionMeshD2D collisionMeshVertical = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(74 / 16, 116 / 16) });

            var sourceRectLeft = new Rectangle(0, 0, 100, 86);
            var sourceRectRight = new Rectangle(102, 0, 100, 86);
            var sourceRectUp = new Rectangle(0, 87, 74, 116);
            var sourceRectDown = new Rectangle(76, 87, 74, 116);

            DirectionToSourceRectAndCollisionMesh = new Dictionary<Direction, Tuple<Rectangle, CollisionMeshD2D>>{
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

        public override string UnbuiltHoverText
        {
            get
            {
                return "Barn - holds stuff.";
            }
        }

        public EntityInventory Inventory { get; set; }
        protected SpriteRenderer Renderer;

        public StorageBarn(PointD2D position, int id, Direction direction) : base(position, null, id)
        {
            Direction = direction;
            Inventory = new EntityInventory(40);
            Inventory.SetDefaultStackSize(10);
            _HoverText = "Storage Barn\nHolds a lot of stuff";
        }

        /// <summary>
        /// For from message
        /// </summary>
        public StorageBarn() : base()
        {
            _HoverText = "Storage Barn\nHolds a lot of stuff";
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
