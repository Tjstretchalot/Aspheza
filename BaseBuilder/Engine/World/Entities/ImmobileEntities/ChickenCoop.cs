using BaseBuilder.Engine.World.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.State.Resources;
using Lidgren.Network;
using BaseBuilder.Engine.Math2D;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class ChickenCoop : ImmobileEntity, Container, Harvestable
    {
        protected static CollisionMeshD2D _CollisionMesh;
        protected static List<Tuple<PointI2D, Direction>> _PreferredAdjacentPoints;

        static ChickenCoop()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> {
                new RectangleD2D(226 / 32.0, 32 / 32.0, 8 / 32.0, 0),
                new RectangleD2D(8 / 32.0, 236 / 32.0),
                new RectangleD2D(54 / 32.0, 32 / 32.0, 8 / 32.0, 204 / 32.0),
                new RectangleD2D(54 / 32.0, 32 / 32.0, 180 / 32.0, 204 / 32.0),
                new RectangleD2D(8 / 32.0, 236 / 32.0, 234 / 32.0, 0 / 32.0),
                new RectangleD2D(226 / 32.0, 108 / 32.0, 8 / 32.0, 33 / 32.0),
            });

            _PreferredAdjacentPoints = new List<Tuple<PointI2D, Direction>>
            {
                Tuple.Create(new PointI2D(2, 5), Direction.Up),
                Tuple.Create(new PointI2D(3, 5), Direction.Up),
                Tuple.Create(new PointI2D(4, 5), Direction.Up),
                Tuple.Create(new PointI2D(5, 5), Direction.Up),
                Tuple.Create(new PointI2D(6, 5), Direction.Up),
            };
        }

        public override List<Tuple<PointI2D, Direction>> PreferredAdjacentPoints
        {
            get
            {
                return _PreferredAdjacentPoints;
            }
        }

        public override string HoverText
        {
            get
            {
                var result = new StringBuilder("Chicken Coop - Holds up to 4 chickens and produces eggs");

                if(Inventory.GetCount() > 0)
                {
                    result.Append("\n");
                    result.Append("  It currently has ").Append(Inventory.GetCount()).Append(" chickens.");
                }

                if(InventoryProducts.GetCount() > 0)
                {
                    result.Append("\n");
                    result.Append("  There are ").Append(InventoryProducts.GetCount()).Append(" eggs available for harvest");
                }

                return result.ToString();
            }
        }
        public EntityInventory Inventory { get; set; }
        public EntityInventory InventoryProducts;

        protected SpriteRenderer Renderer;

        protected int TimeToEggWithOneChickenMS;
        protected int TimeToNextEgg;

        public ChickenCoop(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            Renderer = new SpriteRenderer("ChickenCoop", new Rectangle(0, 0, 242, 236));

            Inventory = new EntityInventory(1);
            Inventory.SetDefaultStackSize(4);

            InventoryProducts = new EntityInventory(1);
            InventoryProducts.SetDefaultStackSize(10);

            InitInventoriesForNonnetworkableParts();

            TimeToEggWithOneChickenMS = 5000;
        }

        public ChickenCoop() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("ChickenCoop", new Rectangle(0, 0, 242, 236));
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);
            InventoryProducts = new EntityInventory(message);

            TimeToEggWithOneChickenMS = message.ReadInt32();
            TimeToNextEgg = message.ReadInt32();

            InitInventoriesForNonnetworkableParts();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);
            InventoryProducts.Write(message);

            message.Write(TimeToEggWithOneChickenMS);
            message.Write(TimeToNextEgg);

            WriteTasks(message);
        }

        protected void InitInventoriesForNonnetworkableParts()
        {
            Inventory.AcceptsMaterialFunc = InputAllowed;
            Inventory.OnMaterialAdded += OnItemAdded;
        }

        protected void OnItemAdded(object sender, EventArgs args)
        {
            TimeToNextEgg = TimeToEggWithOneChickenMS / Inventory.GetCount();
        }

        protected int InputAllowed(Material material, int amt)
        {
            return IsChicken(material) ? amt : 0;
        }

        protected bool IsChicken(Material material)
        {
            return material == Material.Chicken;
        }

        protected bool IsEgg(Material material)
        {
            return material == Material.Egg;
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            if(Inventory.GetCount() > 0)
            {
                TimeToNextEgg -= timeMS;

                if(TimeToNextEgg <= 0)
                {
                    TimeToNextEgg = TimeToEggWithOneChickenMS / Inventory.GetCount();

                    InventoryProducts.AddMaterial(Material.Egg, 1);
                }
            }
        }
        public string GetHarvestNamePretty()
        {
            return "Eggs";
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            return InventoryProducts.GetCount() > 0;
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 242 / 32.0, 236 / 32.0, overlay);
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            int numGiven = reciever.Inventory.AddMaterial(Material.Egg, InventoryProducts.GetCount());

            InventoryProducts.RemoveMaterial(Material.Egg, numGiven);
        }
    }
}
