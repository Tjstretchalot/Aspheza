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
    public class WaterMill : ImmobileEntity, Container, Harvestable
    {
        protected static CollisionMeshD2D _CollisionMesh;

        protected bool Milling;
        protected int TimeUntilNextMillCompletionMS;
        protected int NextAnimationTickMS;

        protected SpriteRenderer Renderer;
        protected int CurrentAnimationLocation;
        static List<Rectangle> AnimationRecs = new List<Rectangle> { new Rectangle(0, 0, 180, 100), new Rectangle(180, 0, 180, 100), new Rectangle(360, 0, 180, 100) };

        /// <summary>
        /// Items to mill
        /// </summary>
        public EntityInventory Inventory { get; protected set; } 
        public EntityInventory InventoryMilled { get; protected set; }

        public override string UnbuiltHoverText
        {
            get
            {
                return "Water mill - mills wheat into flour.";
            }
        }

        public override string HoverText
        {
            get
            {
                var result = new StringBuilder();

                result.Append("Water mill - mills wheat into flour.");

                if (Milling)
                {
                    result.Append("\nIt has ");
                    result.Append(Inventory.MaterialAt(0).Item2);
                    result.Append(" bushel");
                    if (Inventory.MaterialAt(0).Item2 > 1)
                        result.Append("s");

                    result.Append(" of wheat ready to mill.");
                }

                var millMat = InventoryMilled.MaterialAt(0);
                if(millMat != null)
                {
                    result.Append("\nIt has ").Append(millMat.Item2).Append(" bag");
                    if (InventoryMilled.MaterialAt(0).Item2 > 1)
                        result.Append("s");
                    result.Append(" of flour ready for pickup.");

                    if(millMat.Item2 == InventoryMilled.GetStackSizeFor(millMat.Item1))
                    {
                        result.Append("\nIt cannot hold any more flour.");
                    }
                }
                

                return result.ToString();
            }
        }

        static WaterMill()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(11, 6) });
        }
        
        public WaterMill(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            CurrentAnimationLocation = 0;
            Renderer = new SpriteRenderer("WaterMill", AnimationRecs[0]);

            Milling = false;
            Inventory = new EntityInventory(1, IsMillable);
            Inventory.SetDefaultStackSize(10);
            Inventory.OnMaterialAdded += OnItemAdded;
            InventoryMilled = new EntityInventory(1, IsMilled);
            InventoryMilled.SetDefaultStackSize(10);
        }

        public WaterMill() : base()
        {
            CollisionMesh = _CollisionMesh;
            CurrentAnimationLocation = 0;
            Renderer = new SpriteRenderer("WaterMill", AnimationRecs[0]);
        }

        protected bool IsMillable(Material mat)
        {
            return mat == Material.Wheat;
        }

        protected bool IsMilled(Material mat)
        {
            return mat == Material.Flour;
        }
        
        protected void OnItemAdded(object sender, EventArgs args)
        {
            var mat = Inventory.MaterialAt(0).Item1;

            if (mat != Material.Wheat)
                return;

            if(!Milling)
            {
                Milling = true;
                TimeUntilNextMillCompletionMS = 5000;
            }
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            NextAnimationTickMS -= timeMS;
            if(NextAnimationTickMS <= 0)
            {
                CurrentAnimationLocation = (CurrentAnimationLocation + 1) % AnimationRecs.Count;
                Renderer.SourceRect = AnimationRecs[CurrentAnimationLocation];
                NextAnimationTickMS = 250;
            }

            if(Milling)
            {
                TimeUntilNextMillCompletionMS -= timeMS;
                if(TimeUntilNextMillCompletionMS <= 0)
                {
                    TimeUntilNextMillCompletionMS = 5000;
                    if (InventoryMilled.HaveRoomFor(Material.Flour, 1))
                    {
                        Inventory.RemoveMaterial(Material.Wheat, 1);
                        InventoryMilled.AddMaterial(Material.Flour, 1);

                        if (Inventory.GetAmountOf(Material.Wheat) == 0)
                        {
                            Milling = false;
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
            InventoryMilled = new EntityInventory(message);
            _HoverText = message.ReadString();

            Inventory.OnMaterialAdded += OnItemAdded;

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);
            InventoryMilled.Write(message);
            message.Write(_HoverText);

            WriteTasks(message);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 180 / 16.0, 100 / 16.0, overlay);
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            return (InventoryMilled.GetAmountOf(Material.Flour) != 0);
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            var flourAmount = InventoryMilled.GetAmountOf(Material.Flour);

            var amountRecieved = reciever.Inventory.AddMaterial(Material.Flour, flourAmount);
            InventoryMilled.RemoveMaterial(Material.Flour, amountRecieved);
        }

        public string GetHarvestNamePretty()
        {
            if (InventoryMilled.GetAmountOf(Material.Flour) == 0)
                return "Nothing";
            else
            {
                return "Flour";
            }
        }

    }
}
