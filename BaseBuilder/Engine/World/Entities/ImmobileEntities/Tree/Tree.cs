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

        protected bool GrowthTimeNeedsRandoming;
        protected int GrowthTimeRemainingMS;

        public Tree() : base()
        {
        }

        public Tree(PointD2D position, int id, TreeSize size, TreeStyle style, TreeColor color) : base(position, null, id, null, null)
        {
            Size = size;
            Style = style;
            Color = color;

            GrowthTimeNeedsRandoming = true;

            SizeStyleOrColorChanged();
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            ID = message.ReadInt32();
            Position = new PointD2D(message);
            Size = (TreeSize)message.ReadInt32();
            Style = (TreeStyle)message.ReadInt32();
            Color = (TreeColor)message.ReadInt32();
            GrowthTimeRemainingMS = message.ReadInt32();
            GrowthTimeNeedsRandoming = message.ReadBoolean();

            TasksFromMessage(gameState, message);

            SizeStyleOrColorChanged();
        }

        public override void Write(NetOutgoingMessage message)
        {
            message.Write(ID);
            Position.Write(message);
            message.Write((int)Size);
            message.Write((int)Style);
            message.Write((int)Color);
            message.Write(GrowthTimeRemainingMS);
            message.Write(GrowthTimeNeedsRandoming);

            WriteTasks(message);
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            if(GrowthTimeNeedsRandoming)
            {
                GrowthTimeRemainingMS = RandomUtils.GetNetSafeRandom(sharedState, Position.GetHashCode()).Next(75000) + 45000;
                GrowthTimeNeedsRandoming = false;
            }

            GrowthTimeRemainingMS -= timeMS;

            if(GrowthTimeRemainingMS <= 0)
            {
                GrowthTimeNeedsRandoming = true;

                switch(Size)
                {
                    case TreeSize.Sapling:
                        Size = TreeSize.Small;
                        SizeStyleOrColorChanged();
                        break;
                    case TreeSize.Small:
                        var tileAbove = sharedState.World.TileAt((int)Position.X, (int)(Position.Y - 1));
                        bool growFailed = !tileAbove.Ground;
                        if(!growFailed)
                        {
                            foreach(var e in sharedState.World.GetEntitiesAtLocation(CollisionMeshD2D.UnitSquare, tileAbove.Position))
                            {
                                growFailed = true;
                                break;
                            }
                        }

                        if (!growFailed)
                        {
                            Size = TreeSize.Large;
                            SizeStyleOrColorChanged();
                            Position.Y -= 1;
                            sharedState.World.UpdateTileCollisions(this);
                        }
                        break;
                    case TreeSize.Large:
                        Size = TreeSize.Sapling;
                        SizeStyleOrColorChanged();
                        Position.Y += 1;
                        sharedState.World.UpdateTileCollisions(this);
                        break;
                }

            }
        }

        protected void SizeStyleOrColorChanged()
        {
            var dataFromInfo = TreeUtils.GetCollisionMesh(Size, Style, Color);
            CollisionMesh = dataFromInfo.Item1;
            SourceRectsToOffsetLocations = dataFromInfo.Item2;

            if (Size == TreeSize.Sapling)
                SheetName = "materials";
            else
                SheetName = "roguelikeSheet_transparent";
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            if(Size == TreeSize.Small || Size == TreeSize.Large)
                return true;

            return false;
        }

        public string GetHarvestNamePretty()
        {
            return "Wood";
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            switch(Size)
            {
                case TreeSize.Sapling:
                    return;
                case TreeSize.Small:
                    if (!reciever.Inventory.HaveRoomFor(Tuple.Create(Material.Sapling, 2), Tuple.Create(Material.Wood, 1)))
                        return;

                    reciever.Inventory.AddMaterial(Material.Sapling, 2);
                    reciever.Inventory.AddMaterial(Material.Wood, 1);
                    sharedGameState.World.RemoveImmobileEntity(this);
                    break;
                case TreeSize.Large:
                    if (!reciever.Inventory.HaveRoomFor(Material.Wood, 1))
                        return;

                    reciever.Inventory.AddMaterial(Material.Wood, 1);
                    Size = TreeSize.Small;
                    Position.Y += 1;
                    SizeStyleOrColorChanged();
                    sharedGameState.World.UpdateTileCollisions(this);
                    break;
                default:
                    throw new InvalidProgramException();
            }
        }
    }
}
