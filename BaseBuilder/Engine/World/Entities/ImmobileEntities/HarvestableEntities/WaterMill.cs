﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;
using BaseBuilder.Engine.State;
using Lidgren.Network;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.HarvestableEntities
{
    class WaterMill : HarvestableEntity
    {
        protected static CollisionMeshD2D _CollisionMesh;
        protected static List<HarvestableRecipe> _Recipes;
        
        static WaterMill()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(11, 6) });

            _Recipes = new List<HarvestableRecipe>
            {
                new HarvestableRecipe(new List<Tuple<Material, int>> { Tuple.Create(Material.Wheat, 1) }, new List<Tuple<Material, int>> { Tuple.Create(Material.Flour, 1) }, 4000),
                new HarvestableRecipe(new List<Tuple<Material, int>> { Tuple.Create(Material.Sugarcane, 1) }, new List<Tuple<Material, int>> { Tuple.Create(Material.Sugar, 1) }, 4000)
            };
        }

        protected SpriteSheetAnimationRenderer AnimationRenderer;

        public WaterMill(PointD2D position, int id) : base(position, _CollisionMesh, id, _Recipes, 1)
        {
            CollisionMesh = _CollisionMesh;

            Inventory = new EntityInventory(1);
            Inventory.SetDefaultStackSize(10);
            OutputInventory = new EntityInventory(1);
            OutputInventory.SetDefaultStackSize(10);
            InitRecipeListeners();
        }

        public WaterMill()
        {

        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);
            OutputInventory = new EntityInventory(message);
            Recipes = _Recipes;
            CraftSpeed = 1;
            CurrentCraftFromMessage(message);

            InitRecipeListeners();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);
            OutputInventory.Write(message);
            WriteCurrentCraft(message);

            WriteTasks(message);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            if (AnimationRenderer == null)
                InitRenderer(context);

            AnimationRenderer?.Render(context, overlay, screenTopLeft, 2);
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            AnimationRenderer?.Update(timeMS);
        }

        protected void InitRenderer(RenderContext context)
        {
            string sourceFile = "WaterMill";

            AnimationRenderer = new AnimationRendererBuilder(context.Content)

            .BeginAnimation(Direction.Down, AnimationType.Idle, new PointD2D(0, 0), defaultSourceTexture: sourceFile, defualtWidth: 180, defaultHeight: 100)
            .AddFrame()
            .AddFrame(x: 180)
            .AddFrame(x: 360)
            .EndAnimation()

            .Build();
        }
    }
}
