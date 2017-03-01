﻿using System;
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

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class Farm : ImmobileEntity
    {
        protected static PolygonD2D _CollisionMesh;
        protected SpriteRenderer Renderer;

        protected int TimeUntilGownMS;
        GrowthState GrowthState;
        static Rectangle EmptyDrawRec = new Rectangle(0, 0, 64, 68);
        static Rectangle PlantedDrawRec = new Rectangle(0, 69, 64, 68);
        static Rectangle BeansHarvestDrawRec = new Rectangle(65, 53, 64, 84);
        static Rectangle WheatHarvestDrawRec = new Rectangle(130, 49, 64, 88);

        static Farm()
        {
            _CollisionMesh = new RectangleD2D(4, 4);
        }

        public Farm(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            GrowthState = GrowthState.Empty;
            Renderer = new SpriteRenderer("Farms", EmptyDrawRec);
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
                GrowthState = GrowthState.BeansPlanted;
                TimeUntilGownMS = 10000;
                Renderer.SourceRect = PlantedDrawRec;
            }
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            if(GrowthState == GrowthState.BeansPlanted || GrowthState == GrowthState.WheatPlanted)
            {
                TimeUntilGownMS -= timeMS;
                if (TimeUntilGownMS <= 0)
                {
                    if (GrowthState == GrowthState.BeansPlanted)
                    {
                        GrowthState = GrowthState.BeansHarvestable;
                        Renderer.SourceRect = BeansHarvestDrawRec;
                    }
                    else if (GrowthState == GrowthState.WheatPlanted)
                    {
                        GrowthState = GrowthState.WheatHarvestable;
                        Renderer.SourceRect = WheatHarvestDrawRec;
                    }
                }
            }
        }

        public void HarvestFarm()
        {
            GrowthState = GrowthState.Empty;
            Renderer.SourceRect = EmptyDrawRec;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            GrowthState = (GrowthState)message.ReadInt32();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            message.Write((int)GrowthState);

            WriteTasks(message);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            var text = context.Content.Load<Texture2D>("Farms");
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 4, 4, overlay);
        }

    }
}
