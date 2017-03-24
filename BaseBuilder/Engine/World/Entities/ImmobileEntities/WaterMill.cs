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

        static List<Rectangle> AnimationRecs = new List<Rectangle> { new Rectangle(0, 0, 180, 100), new Rectangle(180, 0, 180, 100), new Rectangle(360, 0, 180, 100) };

        public override string UnbuiltHoverText
        {
            get
            {
                return "Water mill - mills either wheat into flour or sugarcane into sugar.";
            }
        }

        public override string HoverText
        {
            get
            {
                var result = new StringBuilder();

                result.Append("Water mill - mills either wheat into flour or sugarcane into sugar.");

                if (Milling)
                {
                    result.Append("\nIt has ");
                    result.Append(Inventory.MaterialAt(0).Item2);
                    result.Append(" bushel");
                    if (Inventory.MaterialAt(0).Item2 > 1)
                        result.Append("s");

                    if (Inventory.GetAmountOf(Material.Sugarcane) != 0)
                        result.Append(" of sugarcane ready to mill.");
                    else if (Inventory.GetAmountOf(Material.Wheat) != 0)
                        result.Append(" of wheat ready to mill.");
                }

                var millMat = InventoryMilled.MaterialAt(0);
                if (millMat != null)
                {
                    result.Append("\nIt has ").Append(millMat.Item2);

                    if (millMat.Item1 == Material.Sugar)
                        result.Append(" cube");
                    else if (millMat.Item1 == Material.Flour)
                        result.Append(" bag");

                    if (InventoryMilled.MaterialAt(0).Item2 > 1)
                        result.Append("s");

                    if (millMat.Item1 == Material.Sugar)
                        result.Append(" of sugar ready for pickup.");
                    else if (millMat.Item1 == Material.Flour)
                        result.Append(" of flour ready for pickup.");

                    if (InventoryMilled.GetAmountOf(Material.Sugar) == 10)
                        result.Append("\nIt cannot hold any more sugar.");
                    else if (InventoryMilled.GetAmountOf(Material.Flour) == 10)
                        result.Append("\nIt cannot hold any more flour.");
                }


                return result.ToString();
            }
        }

        static WaterMill()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(11, 6) });
        }

        /// <summary>
        /// Items to mill
        /// </summary>
        public EntityInventory Inventory { get; protected set; }
        public EntityInventory InventoryMilled { get; protected set; }
        
        protected bool Milling;
        protected int TimeUntilNextMillCompletionMS;
        protected int NextAnimationTickMS;

        protected SpriteRenderer Renderer;
        protected int CurrentAnimationLocation;

        public WaterMill(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            CurrentAnimationLocation = 0;
            Renderer = new SpriteRenderer("WaterMill", AnimationRecs[0]);

            Milling = false;
            Inventory = new EntityInventory(1);
            Inventory.SetDefaultStackSize(10);
            InventoryMilled = new EntityInventory(1);
            InventoryMilled.SetDefaultStackSize(10);

            InitInventoryForNonnetworkableParts();
        }

        public WaterMill() : base()
        {
            CollisionMesh = _CollisionMesh;
            CurrentAnimationLocation = 0;
            Renderer = new SpriteRenderer("WaterMill", AnimationRecs[0]);
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);
            InventoryMilled = new EntityInventory(message);
            message.Write(Milling);
            message.Write(TimeUntilNextMillCompletionMS);

            InitInventoryForNonnetworkableParts();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);
            InventoryMilled.Write(message);
            Milling = message.ReadBoolean();
            TimeUntilNextMillCompletionMS = message.ReadInt32();

            WriteTasks(message);
        }

        protected void InitInventoryForNonnetworkableParts()
        {
            Inventory.AcceptsMaterialFunc = AcceptsMaterial;
            Inventory.OnMaterialAdded += OnItemAdded;
        }

        protected int AcceptsMaterial(Material mat, int amt)
        {
            return IsMillable(mat) ? amt : 0;
        }

        protected bool IsMillable(Material mat)
        {
            if (Inventory.GetCount() == 0 && InventoryMilled.GetCount() == 0)
                return mat == Material.Wheat || mat == Material.Sugarcane;
            else if (Inventory.GetAmountOf(Material.Sugarcane) != 0 || InventoryMilled.GetAmountOf(Material.Sugar) != 0)
                return mat == Material.Sugarcane;
            else if (Inventory.GetAmountOf(Material.Wheat) != 0 || InventoryMilled.GetAmountOf(Material.Flour) != 0)
                return mat == Material.Wheat;
            throw new InvalidOperationException("IsMillable cannot determine current processing.");
        }

        protected bool IsMilled(Material mat)
        {
            if (Inventory.GetCount() == 0 && InventoryMilled.GetCount() == 0)
                return mat == Material.Flour || mat == Material.Sugar;
            else if (Inventory.GetAmountOf(Material.Sugarcane) != 0 || InventoryMilled.GetAmountOf(Material.Sugar) != 0)
                return mat == Material.Sugar;
            else if (Inventory.GetAmountOf(Material.Wheat) != 0 || InventoryMilled.GetAmountOf(Material.Flour) != 0)
                return mat == Material.Flour;
            throw new InvalidOperationException("IsMilled cannot determine current processing.");
        }

        protected void OnItemAdded(object sender, EventArgs args)
        {
            var mat = Inventory.MaterialAt(0).Item1;

            if (mat != Material.Wheat && mat != Material.Sugarcane)
                return;

            if (!Milling)
            {
                Milling = true;
                TimeUntilNextMillCompletionMS = 5000;
            }
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            NextAnimationTickMS -= timeMS;
            if (NextAnimationTickMS <= 0)
            {
                CurrentAnimationLocation = (CurrentAnimationLocation + 1) % AnimationRecs.Count;
                Renderer.SourceRect = AnimationRecs[CurrentAnimationLocation];
                NextAnimationTickMS = 250;
            }

            if (Milling)
            {
                TimeUntilNextMillCompletionMS -= timeMS;
                if (TimeUntilNextMillCompletionMS <= 0)
                {
                    TimeUntilNextMillCompletionMS = 5000;

                    if (Inventory.GetAmountOf(Material.Sugarcane) != 0 || InventoryMilled.GetAmountOf(Material.Sugar) != 0)
                    {
                        if (InventoryMilled.HaveRoomFor(Material.Sugar, 1))
                        {
                            Inventory.RemoveMaterial(Material.Sugarcane, 1);
                            InventoryMilled.AddMaterial(Material.Sugar, 1);

                            if (Inventory.GetAmountOf(Material.Sugarcane) == 0)
                            {
                                Milling = false;
                            }
                        }
                    }
                    else if (Inventory.GetAmountOf(Material.Wheat) != 0 || InventoryMilled.GetAmountOf(Material.Flour) != 0)
                    {
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
                    else
                        throw new InvalidOperationException("SimulateTimePassing cannot determine current processing.");
                }
            }
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 180 / 16.0, 100 / 16.0, overlay);
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            return InventoryMilled.GetCount() != 0;
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            var milledInventory = InventoryMilled.MaterialAt(0);

            var amountRecieved = reciever.Inventory.AddMaterial(milledInventory.Item1, milledInventory.Item2);
            InventoryMilled.RemoveMaterial(milledInventory.Item1, amountRecieved);
        }

        public string GetHarvestNamePretty()
        {
            if (InventoryMilled.GetAmountOf(Material.Flour) != 0)
                return "Flour";
            else if (InventoryMilled.GetAmountOf(Material.Sugar) != 0)
                return "Sugar";
            else
                return "Nothing";
        }
    }
}
