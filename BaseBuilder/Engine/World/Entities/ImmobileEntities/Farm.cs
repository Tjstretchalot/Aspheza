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

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class Farm : ImmobileEntity, Container, Harvestable
    {
        protected static PolygonD2D _CollisionMesh;
        protected SpriteRenderer Renderer;

        protected int TimeUntilGownMS;
        GrowthState GrowthState;
        static Rectangle EmptyDrawRec = new Rectangle(0, 0, 64, 68);
        static Rectangle PlantedDrawRec = new Rectangle(0, 69, 64, 68);
        static Rectangle CarrotHarvestDrawRec = new Rectangle(65, 53, 64, 84);
        static Rectangle WheatHarvestDrawRec = new Rectangle(130, 49, 64, 88);
        
        static Farm()
        {
            _CollisionMesh = new RectangleD2D(4, 4);
        }

        // This is used as a prop, it never has anything in it
        public EntityInventory Inventory { get; protected set; }

        public Farm(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            GrowthState = GrowthState.Empty;
            Renderer = new SpriteRenderer("Farms", EmptyDrawRec);
            Inventory = new EntityInventory(1, IsASeed);

            Inventory.OnMaterialAdded += OnItemAdded;
        }

        public Farm() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Farms", EmptyDrawRec);
        }

        public override void Update(UpdateContext context)
        {
            base.Update(context);
            if(Selected)
            {
                PlantFarm(1);
            }
        }

        protected bool IsASeed(Material mat)
        {
            return mat == Material.CarrotSeed || mat == Material.WheatSeed;
        }

        protected void OnItemAdded(object sender, EventArgs args)
        {
            var mat = Inventory.MaterialAt(0).Item1;

            if (!IsASeed(mat))
                return;

            if (mat == Material.WheatSeed)
                PlantFarm(1);
            else if (mat == Material.CarrotSeed)
                PlantFarm(2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plantOption">1 for Wheat, 2 for Beans</param>
        public void PlantFarm(int plantOption)
        {
            if(plantOption == 1)
            {
                GrowthState = GrowthState.WheatPlanted;
                TimeUntilGownMS = 10000;
                Renderer.SourceRect = PlantedDrawRec;
            }
            else if (plantOption == 2)
            {
                GrowthState = GrowthState.CarrotsPlanted;
                TimeUntilGownMS = 10000;
                Renderer.SourceRect = PlantedDrawRec;
            }
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            if(GrowthState == GrowthState.CarrotsPlanted || GrowthState == GrowthState.WheatPlanted)
            {
                TimeUntilGownMS -= timeMS;
                if (TimeUntilGownMS <= 0)
                {
                    if (GrowthState == GrowthState.WheatPlanted)
                    {
                        GrowthState = GrowthState.WheatHarvestable;
                        Renderer.SourceRect = WheatHarvestDrawRec;
                    }
                    else if (GrowthState == GrowthState.CarrotsPlanted)
                    {
                        GrowthState = GrowthState.CarrotsHarvestable;
                        Renderer.SourceRect = CarrotHarvestDrawRec;
                    }
                }
            }
        }

        protected void ClearFarm()
        {
            GrowthState = GrowthState.Empty;
            Renderer.SourceRect = EmptyDrawRec;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            GrowthState = (GrowthState)message.ReadInt32();
            Inventory = new EntityInventory(message);

            Inventory.OnMaterialAdded += OnItemAdded;

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            message.Write((int)GrowthState);
            Inventory.Write(message);

            WriteTasks(message);
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
                case GrowthState.CarrotsPlanted:
                case GrowthState.WheatPlanted:
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
            return GrowthState == GrowthState.CarrotsHarvestable || GrowthState == GrowthState.WheatHarvestable;
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            Material mat;
            int amt;

            if(GrowthState == GrowthState.CarrotsHarvestable)
            {
                mat = Material.Carrot;
                amt = 1;
            }else if(GrowthState == GrowthState.WheatHarvestable)
            {
                mat = Material.Wheat;
                amt = 1;
            }else
            {
                return;
            }

            if (!reciever.Inventory.HaveRoomFor(mat, amt))
                return;

            reciever.Inventory.AddMaterial(mat, amt);
            ClearFarm();
        }

        public string GetHarvestNamePretty()
        {
            if(GrowthState == GrowthState.CarrotsHarvestable)
            {
                return "Carrots";
            }else if(GrowthState == GrowthState.WheatHarvestable)
            {
                return "Wheat";
            }else
            {
                return "Nothing";
            }
        }
    }
}
