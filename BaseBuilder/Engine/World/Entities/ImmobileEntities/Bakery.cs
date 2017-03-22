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

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class Bakery : ImmobileEntity, Container, Harvestable
    {
        protected static CollisionMeshD2D _CollisionMesh;
        protected static Rectangle SourceRec = new Rectangle(0, 0, 158, 114);

        public override string HoverText
        {
            get
            {
                var result = new StringBuilder();

                result.Append("Bakery - bakes flour, sugar, and eggs into bread.");

                if (Inventory.GetAmountOf(Material.Flour) > 0)
                {
                    result.Append("\nIt has ");
                    result.Append(Inventory.GetAmountOf(Material.Flour));
                    result.Append(" bag");
                    if (Inventory.GetAmountOf(Material.Flour) > 1)
                        result.Append("s");

                    result.Append(" of flour ready to bake with.");
                }
                if (Inventory.GetAmountOf(Material.Sugar) > 0)
                {
                    result.Append("\nIt has ");
                    result.Append(Inventory.GetAmountOf(Material.Sugar));
                    result.Append(" cube");
                    if (Inventory.GetAmountOf(Material.Sugar) > 1)
                        result.Append("s");

                    result.Append(" of sugar ready to bake with.");
                }
                if (Inventory.GetAmountOf(Material.Egg) > 0)
                {
                    result.Append("\nIt has ");
                    result.Append(Inventory.GetAmountOf(Material.Egg));
                    result.Append(" egg");
                    if (Inventory.GetAmountOf(Material.Egg) > 1)
                        result.Append("s");

                    result.Append(" ready to bake with.");
                }


                var millMat = InventoryBaked.MaterialAt(0);
                if (millMat != null)
                {
                    result.Append("\nIt has ").Append(millMat.Item2).Append(" loaf");
                    if (InventoryBaked.MaterialAt(0).Item2 > 1)
                        result.Append("s");
                    result.Append(" of bread ready for pickup.");

                    if (millMat.Item2 == InventoryBaked.GetStackSizeFor(millMat.Item1))
                    {
                        result.Append("\nIt cannot hold any more bread.");
                    }
                }


                return result.ToString();
            }
        }
        
        protected int TimeUntilNextBakeCompletionMS;

        protected SpriteRenderer Renderer;

        /// <summary>
        /// Items to bake
        /// </summary>
        public EntityInventory Inventory { get; protected set; }
        public EntityInventory InventoryBaked { get; protected set; }

        static Bakery()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D>{ new RectangleD2D(5, 3.5) });
        }

        public Bakery(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Bakery", SourceRec);
            
            Inventory = new EntityInventory(3);
            Inventory.SetDefaultStackSize(10);
            InventoryBaked = new EntityInventory(1);
            InventoryBaked.SetDefaultStackSize(10);
            InitInventoryForNonnetworkableParts();
        }

        public Bakery() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Bakery", SourceRec);
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);
            InventoryBaked = new EntityInventory(message);
            TimeUntilNextBakeCompletionMS = message.ReadInt32();

            InitInventoryForNonnetworkableParts();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);
            InventoryBaked.Write(message);
            message.Write(TimeUntilNextBakeCompletionMS);

            WriteTasks(message);
        }

        protected void InitInventoryForNonnetworkableParts()
        {
            Inventory.AcceptsMaterialFunc = IsBakable;
            Inventory.OnMaterialAdded += OnItemAdded;

            InventoryBaked.AcceptsMaterialFunc = IsBaked;
        }

        protected bool IsBakable(Material mat)
        {
            return mat == Material.Flour || mat == Material.Sugar || mat == Material.Egg;
        }

        protected bool IsBaked(Material mat)
        {
            return mat == Material.Bread;
        }

        protected bool HaveMaterials()
        {
            return (Inventory.GetAmountOf(Material.Flour) >= 1 && Inventory.GetAmountOf(Material.Sugar) >= 1 && Inventory.GetAmountOf(Material.Egg) >= 1);
        }

        protected void OnItemAdded(object sender, EntityInventory.InventoryChangedEventArgs args)
        {      
            if (HaveMaterials())
            {
                TimeUntilNextBakeCompletionMS = 5000;
            }
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            TimeUntilNextBakeCompletionMS -= timeMS;
            if (TimeUntilNextBakeCompletionMS <= 0)
            {
                TimeUntilNextBakeCompletionMS = 5000;
                if (InventoryBaked.HaveRoomFor(Material.Bread, 1) && HaveMaterials())
                {
                    Inventory.RemoveMaterial(Material.Flour, 1);
                    Inventory.RemoveMaterial(Material.Sugar, 1);
                    Inventory.RemoveMaterial(Material.Egg, 1);
                    InventoryBaked.AddMaterial(Material.Bread, 1);
                }
            }
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 156 / 32.0, 114 / 32.0, overlay);
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            return (InventoryBaked.GetAmountOf(Material.Bread) != 0);
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            var breadAmount = InventoryBaked.GetAmountOf(Material.Bread);

            var amountRecieved = reciever.Inventory.AddMaterial(Material.Bread, breadAmount);
            InventoryBaked.RemoveMaterial(Material.Bread, amountRecieved);
        }

        public string GetHarvestNamePretty()
        {
            if (InventoryBaked.GetAmountOf(Material.Bread) == 0)
            {
                return "Nothing";
            }
            else
            {
                return "Bread";
            }
        }

    }
}
