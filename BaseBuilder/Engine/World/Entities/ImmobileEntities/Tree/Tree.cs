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
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.State.Resources;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.Tree
{
    public class Tree : SpriteSheetBuilding, Harvestable
    {
        protected TreeSize Size;
        protected TreeStyle Style;
        protected TreeColor Color;

        protected int WoodRemaining;

        public Tree() : base()
        {
        }

        public Tree(PointD2D position, CollisionMeshD2D collisionMesh, int id, List<Tuple<Rectangle, PointD2D>> sourceRectsToOffsetLocations, TreeSize size, TreeStyle style, TreeColor color) : base(position, collisionMesh, id, "roguelikeSheet_transparent", sourceRectsToOffsetLocations)
        {
            Size = size;
            Style = style;
            Color = color;
            WoodRemaining = (int)size + 1;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            ID = message.ReadInt32();
            Position = new PointD2D(message);
            Size = (TreeSize)message.ReadInt32();
            Style = (TreeStyle)message.ReadInt32();
            Color = (TreeColor)message.ReadInt32();
            WoodRemaining = message.ReadInt32();

            TasksFromMessage(gameState, message);


            var dataFromInfo = TreeUtils.GetCollisionMesh(Size, Style, Color);
            CollisionMesh = dataFromInfo.Item1;
            SourceRectsToOffsetLocations = dataFromInfo.Item2;
            SheetName = "roguelikeSheet_transparent";
        }

        public override void Write(NetOutgoingMessage message)
        {
            message.Write(ID);
            Position.Write(message);
            message.Write((int)Size);
            message.Write((int)Style);
            message.Write((int)Color);
            message.Write(WoodRemaining);

            WriteTasks(message);
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            return true;
        }

        public string GetHarvestNamePretty()
        {
            return "Wood";
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            var mat = Material.Wood;
            int amt = 1;

            if (!reciever.Inventory.HaveRoomFor(mat, amt))
                return;

            reciever.Inventory.AddMaterial(mat, amt);
            WoodRemaining -= amt;

            if(WoodRemaining <= 0)
            {
                sharedGameState.World.RemoveImmobileEntity(this);
            }
        }
    }
}
