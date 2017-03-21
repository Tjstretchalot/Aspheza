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
    public class Farm : ImmobileEntity, Container, Harvestable
    {
        protected static CollisionMeshD2D _CollisionMesh;

        protected int TimeUntilGownMS;
        GrowthState GrowthState;

        protected SpriteRenderer Renderer;
        static Rectangle EmptyDrawRec = new Rectangle(0, 0, 64, 68);
        static Rectangle PlantedDrawRec = new Rectangle(0, 69, 64, 68);
        static Rectangle CarrotHarvestDrawRec = new Rectangle(65, 53, 64, 84);
        static Rectangle WheatHarvestDrawRec = new Rectangle(130, 49, 64, 88);
        static Rectangle SugarcaneHarvestDrawRec = new Rectangle(195, 27, 64, 110);

        static Farm()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(4, 4) });
        }

        // This is used as a prop, it never has anything in it
        public EntityInventory Inventory { get; protected set; }

        public override string UnbuiltHoverText
        {
            get
            {
                return "Farm - used for growing crops";
            }
        }

        public Farm(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            _HoverText = "A rudimentary farm. It's currently not growing anything";
            CollisionMesh = _CollisionMesh;
            GrowthState = GrowthState.Empty;
            Renderer = new SpriteRenderer("Farms", EmptyDrawRec);
            Inventory = new EntityInventory(1);

            InitInventoryForNonnetworkableParts();
        }

        public Farm() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Farms", EmptyDrawRec);
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            GrowthState = (GrowthState)message.ReadInt32();
            TimeUntilGownMS = message.ReadInt32();
            Inventory = new EntityInventory(message);
            _HoverText = message.ReadString();

            InitInventoryForNonnetworkableParts();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            message.Write((int)GrowthState);
            message.Write(TimeUntilGownMS);
            Inventory.Write(message);
            message.Write(_HoverText);

            WriteTasks(message);
        }

        protected void InitInventoryForNonnetworkableParts()
        {
            Inventory.AcceptsMaterialFunc = IsASeed;
            Inventory.OnMaterialAdded += OnItemAdded;
        }


        protected bool IsASeed(Material mat)
        {
            return mat == Material.CarrotSeed || mat == Material.WheatSeed || mat == Material.Sugarcane;
        }

        protected void OnItemAdded(object sender, EventArgs args)
        {
            var mat = Inventory.MaterialAt(0).Item1;

            if (!IsASeed(mat))
                return;
            if (ReadyToHarvest(null))
                return;
            if (mat == Material.WheatSeed)
                PlantFarm(1);
            else if (mat == Material.CarrotSeed)
                PlantFarm(2);
            else if (mat == Material.Sugarcane)
                PlantFarm(3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plantOption">1 for Wheat, 2 for Carrot, 3 for Sugarcane</param>
        public void PlantFarm(int plantOption)
        {
            if(plantOption == 1)
            {
                _HoverText = "A rudimentary farm. It's currently growing wheat, but\nit's not done yet.";
                GrowthState = GrowthState.WheatPlanted;
                TimeUntilGownMS = 10000;
                Renderer.SourceRect = PlantedDrawRec;
            }
            else if (plantOption == 2)
            {
                _HoverText = "A rudimentary farm. It's currently growing carrots, but\nit's not done yet.";
                GrowthState = GrowthState.CarrotsPlanted;
                TimeUntilGownMS = 10000;
                Renderer.SourceRect = PlantedDrawRec;
            }
            else if (plantOption == 3)
            {
                _HoverText = "A rudimentary farm. It's currently growing sugarcane, but\nit's not done yet.";
                GrowthState = GrowthState.SugarcanePlanted;
                TimeUntilGownMS = 10000;
                Renderer.SourceRect = PlantedDrawRec;
            }
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            if(GrowthState == GrowthState.CarrotsPlanted || GrowthState == GrowthState.WheatPlanted || GrowthState == GrowthState.SugarcanePlanted)
            {
                TimeUntilGownMS -= timeMS;
                if (TimeUntilGownMS <= 0)
                {
                    if (GrowthState == GrowthState.WheatPlanted)
                    {
                        GrowthState = GrowthState.WheatHarvestable;
                        Renderer.SourceRect = WheatHarvestDrawRec;
                        _HoverText = "A rudimentary farm. It has some wheat\nwhich is ready to harvest.";
                    }
                    else if (GrowthState == GrowthState.CarrotsPlanted)
                    {
                        _HoverText = "A rudimentary farm. It has some carrots\nwhich are ready to harvest.";
                        GrowthState = GrowthState.CarrotsHarvestable;
                        Renderer.SourceRect = CarrotHarvestDrawRec;
                    }
                    else if (GrowthState == GrowthState.SugarcanePlanted)
                    {
                        _HoverText = "A rudimentary farm. It has some sugarcane\nwhich are ready to harvest.";
                        GrowthState = GrowthState.SugarcaneHarvestable;
                        Renderer.SourceRect = SugarcaneHarvestDrawRec;
                    }
                }
            }
        }

        protected void ClearFarm()
        {
            Inventory.RemoveMaterialAt(0);
            GrowthState = GrowthState.Empty;
            Renderer.SourceRect = EmptyDrawRec;
            _HoverText = "A rudimentary farm. It's currently not growing anything";
        }


        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            int endX;
            int endY;
            double worldWidth;
            double worldHeight;

            switch (GrowthState)
            {
                case GrowthState.WheatHarvestable:
                    worldWidth = 4;
                    worldHeight = 88.0 / 16;
                    endX = (int)(screenTopLeft.X);
                    endY = (int)(screenTopLeft.Y - worldHeight * (20.0 / 88) * context.Camera.Zoom);
                    break;
                case GrowthState.CarrotsHarvestable:
                    worldWidth = 4;
                    worldHeight = 84.0 / 16;
                    endX = (int)(screenTopLeft.X);
                    endY = (int)(screenTopLeft.Y - worldHeight * (16.0 / 84) * context.Camera.Zoom);
                    break;
                case GrowthState.SugarcaneHarvestable:
                    worldWidth = 4;
                    worldHeight = 110.0 / 16;
                    endX = (int)(screenTopLeft.X);
                    endY = (int)(screenTopLeft.Y - worldHeight * ((110.0 - 68.0) / 110) * context.Camera.Zoom);
                    break;
                case GrowthState.CarrotsPlanted:
                case GrowthState.WheatPlanted:
                case GrowthState.SugarcanePlanted:
                case GrowthState.Empty:
                    endX = (int)(screenTopLeft.X);
                    endY = (int)(screenTopLeft.Y);
                    worldWidth = 4;
                    worldHeight = 68.0 / 16;
                    break;
                default:
                    throw new InvalidProgramException();
            }

            Renderer.Render(context, endX, endY, worldWidth, worldHeight, overlay);
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            return GrowthState == GrowthState.CarrotsHarvestable || GrowthState == GrowthState.WheatHarvestable || GrowthState == GrowthState.SugarcaneHarvestable;
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            int numSeeds = RandomUtils.GetNetSafeRandom(sharedGameState, Position.GetHashCode()).Next(2) + 1;

            if(GrowthState == GrowthState.CarrotsHarvestable)
            {
                if (!reciever.Inventory.HaveRoomFor(Material.Carrot, 1))
                    return;

                reciever.Inventory.AddMaterial(Material.Carrot, 1);

                if (!reciever.Inventory.HaveRoomFor(Material.CarrotSeed, numSeeds))
                {
                    ClearFarm();
                    return;
                }

                reciever.Inventory.AddMaterial(Material.CarrotSeed, numSeeds);
            }
            else if(GrowthState == GrowthState.WheatHarvestable)
            {
                if (!reciever.Inventory.HaveRoomFor(Material.Wheat, 1))
                    return;

                reciever.Inventory.AddMaterial(Material.Wheat, 1);

                if (!reciever.Inventory.HaveRoomFor(Material.WheatSeed, numSeeds))
                {
                    ClearFarm();
                    return;
                }

                reciever.Inventory.AddMaterial(Material.WheatSeed, numSeeds);
            }
            else if (GrowthState == GrowthState.SugarcaneHarvestable)
            {
                if (!reciever.Inventory.HaveRoomFor(Material.Sugarcane, 1))
                    return;

                reciever.Inventory.AddMaterial(Material.Sugarcane, 1);

                if (!reciever.Inventory.HaveRoomFor(Material.Sugarcane, numSeeds))
                {
                    ClearFarm();
                    return;
                }

                reciever.Inventory.AddMaterial(Material.Sugarcane, numSeeds);
            }
            else
            {
                return;
            }

            ClearFarm();
        }

        public string GetHarvestNamePretty()
        {
            if (GrowthState == GrowthState.CarrotsHarvestable)
            {
                return "Carrots";
            }
            else if (GrowthState == GrowthState.WheatHarvestable)
            {
                return "Wheat";
            }
            else if (GrowthState == GrowthState.SugarcaneHarvestable)
            {
                return "Sugarcane";
            }
            else
            {
                return "Nothing";
            }
        }
    }
}
