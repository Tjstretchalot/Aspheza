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

        protected bool Baking;
        protected int TimeUntilNextBakeCompletionMS;
        protected int NextAnimationTickMS;

        protected SpriteRenderer Renderer;
        static Rectangle SourceRec = new Rectangle(0, 0, 158, 114);

        /// <summary>
        /// Items to mill
        /// </summary>
        public EntityInventory Inventory { get; protected set; }
        public EntityInventory InventoryBaked { get; protected set; }

        public override string HoverText
        {
            get
            {
                var result = new StringBuilder();

                result.Append("Bakery - bakes flour into bread.");

                if (Baking)
                {
                    result.Append("\nIt has ");
                    result.Append(Inventory.MaterialAt(0).Item2);
                    result.Append(" bag");
                    if (Inventory.MaterialAt(0).Item2 > 1)
                        result.Append("s");

                    result.Append(" of flour ready to bake.");
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

        static Bakery()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D>{ new RectangleD2D(5, 3.5) });
        }

        public Bakery(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Bakery", SourceRec);

            Baking = false;
            Inventory = new EntityInventory(1, IsBakable);
            Inventory.SetDefaultStackSize(10);
            Inventory.OnMaterialAdded += OnItemAdded;
            InventoryBaked = new EntityInventory(1, IsBaked);
            InventoryBaked.SetDefaultStackSize(10);
        }

        public Bakery() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Bakery", SourceRec);
        }

        protected bool IsBakable(Material mat)
        {
            return mat == Material.Flour;
        }

        protected bool IsBaked(Material mat)
        {
            return mat == Material.Bread;
        }

        protected void OnItemAdded(object sender, EventArgs args)
        {
            var mat = Inventory.MaterialAt(0).Item1;

            if (mat != Material.Flour)
                return;

            if (!Baking)
            {
                Baking = true;
                TimeUntilNextBakeCompletionMS = 5000;
            }
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);
            
            if (Baking)
            {
                TimeUntilNextBakeCompletionMS -= timeMS;
                if (TimeUntilNextBakeCompletionMS <= 0)
                {
                    TimeUntilNextBakeCompletionMS = 5000;
                    if (InventoryBaked.HaveRoomFor(Material.Bread, 1))
                    {
                        Inventory.RemoveMaterial(Material.Flour, 1);
                        InventoryBaked.AddMaterial(Material.Bread, 1);

                        if (Inventory.GetAmountOf(Material.Bread) == 0)
                        {
                            Baking = false;
                        }
                    }
                }
            }
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);
            InventoryBaked = new EntityInventory(message);
            _HoverText = message.ReadString();

            Inventory.OnMaterialAdded += OnItemAdded;

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);
            InventoryBaked.Write(message);
            message.Write(_HoverText);

            WriteTasks(message);
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
            if (InventoryBaked.GetAmountOf(Material.Flour) == 0)
                return "Nothing";
            else
            {
                return "Bread";
            }
        }

    }
}
