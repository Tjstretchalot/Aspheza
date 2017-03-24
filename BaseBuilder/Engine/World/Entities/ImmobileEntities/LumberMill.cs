using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class LumberMill : ImmobileEntity, Container, Harvestable
    {
        protected static CollisionMeshD2D _CollisionMesh;

        protected bool MillingWood;
        protected int TimeUntilNextMillingCompletionMS;
        protected int NextAnimationTickMS;

        protected SpriteRenderer Renderer;
        static Rectangle SourceRec = new Rectangle(0, 0, 164, 204);

        /// <summary>
        /// Items to mill
        /// </summary>
        public EntityInventory Inventory { get; protected set; }
        public EntityInventory InventoryLumber { get; protected set; }

        public override string HoverText
        {
            get
            {
                var result = new StringBuilder();

                result.Append("Lumbermill - Cuts raw wood into lumber.");

                if (MillingWood)
                {
                    result.Append("\nIt has ").Append(Inventory.MaterialAt(0).Item2).Append(" log");
                    if (Inventory.MaterialAt(0).Item2 > 1)
                        result.Append("s");

                    result.Append(" of wood ready to mill.");
                }

                var millMat = InventoryLumber.MaterialAt(0);
                if (millMat != null)
                {
                    result.Append("\nIt has ").Append(millMat.Item2).Append(" stack");
                    if (InventoryLumber.MaterialAt(0).Item2 > 1)
                        result.Append("s");
                    result.Append(" of lumber ready for pickup.");

                    if (millMat.Item2 == InventoryLumber.GetStackSizeFor(millMat.Item1))
                    {
                        result.Append("\nIt cannot hold any more lumber.");
                    }
                }


                return result.ToString();
            }
        }

        static LumberMill()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(5.125, 5, 0, 1.375),
                new PolygonD2D(new List<PointD2D> { new PointD2D(0.4375, 1.375), new PointD2D(0.4375, 0.9375), new PointD2D(1.4375, 0), new PointD2D(2.4375, 0.9375), new PointD2D(2.4375, 1.375) }) });
        }

        public LumberMill(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("LumberMill", SourceRec);

            MillingWood = false;
            Inventory = new EntityInventory(1);
            Inventory.SetDefaultStackSize(10);
            InventoryLumber = new EntityInventory(1);
            InventoryLumber.SetDefaultStackSize(10);
            InitInventoryForNonnetworkableParts();
        }

        public LumberMill() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("LumberMill", SourceRec);
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);
            InventoryLumber = new EntityInventory(message);
            TimeUntilNextMillingCompletionMS = message.ReadInt32();
            _HoverText = message.ReadString();

            InitInventoryForNonnetworkableParts();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);
            InventoryLumber.Write(message);
            message.Write(TimeUntilNextMillingCompletionMS);
            message.Write(_HoverText);

            WriteTasks(message);
        }

        protected void InitInventoryForNonnetworkableParts()
        {
            Inventory.AcceptsMaterialFunc = AcceptsMaterial;
            Inventory.OnMaterialAdded += OnItemAdded;
        }

        protected int AcceptsMaterial(Material mat, int amt)
        {
            return (mat == Material.Wood) ? amt : 0;
        }

        protected bool IsWoodMilled(Material mat)
        {
            return mat == Material.Lumber;
        }

        protected void OnItemAdded(object sender, EventArgs args)
        {
            var mat = Inventory.MaterialAt(0).Item1;

            if (mat != Material.Wood)
                return;

            if (!MillingWood)
            {
                MillingWood = true;
                TimeUntilNextMillingCompletionMS = 5000;
            }
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            if (MillingWood)
            {
                TimeUntilNextMillingCompletionMS -= timeMS;
                if (TimeUntilNextMillingCompletionMS <= 0)
                {
                    TimeUntilNextMillingCompletionMS = 5000;
                    if (InventoryLumber.HaveRoomFor(Material.Lumber, 1))
                    {
                        Inventory.RemoveMaterial(Material.Wood, 1);
                        InventoryLumber.AddMaterial(Material.Lumber, 1);

                        if (Inventory.GetAmountOf(Material.Wood) == 0)
                        {
                            MillingWood = false;
                        }
                    }
                }
            }
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 164 / 32.0, 204 / 32.0, overlay);
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            return (InventoryLumber.GetAmountOf(Material.Lumber) != 0);
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            var lumberAmount = InventoryLumber.GetAmountOf(Material.Lumber);

            var amountRecieved = reciever.Inventory.AddMaterial(Material.Lumber, lumberAmount);
            InventoryLumber.RemoveMaterial(Material.Lumber, amountRecieved);
        }

        public string GetHarvestNamePretty()
        {
            if (InventoryLumber.GetAmountOf(Material.Lumber) == 0)
            {
                return "Nothing";
            }
            else
            {
                return "Lumber";
            }
        }

    }
}
