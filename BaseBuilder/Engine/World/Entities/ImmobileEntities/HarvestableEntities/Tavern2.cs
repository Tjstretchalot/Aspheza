using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.HarvestableEntities
{
    public class Tavern2 : HarvestableEntity
    {
        protected static CollisionMeshD2D _CollisionMesh;
        protected static List<HarvestableRecipe> _Recipes;

        protected static Rectangle SourceRec = new Rectangle(0, 0, 218, 212);

        static Tavern2()
        {
            double scale = 1 / 32.0;
            var pixelPolys = new List<List<PointD2D>>
            {
                new List<PointD2D> { new PointD2D(0, 90), new PointD2D(30, 60), new PointD2D(33, 60), new PointD2D(52, 78), new PointD2D(52, 168), new PointD2D(0, 168) },
                new List<PointD2D> { new PointD2D(52, 80), new PointD2D(102, 80), new PointD2D(102, 168), new PointD2D(52, 168) },
                new List<PointD2D> { new PointD2D(102, 80), new PointD2D(120, 60), new PointD2D(154, 90), new PointD2D(154, 168), new PointD2D(102, 168) },
                new List<PointD2D> { new PointD2D(154, 0), new PointD2D(218, 0), new PointD2D(218, 212), new PointD2D(154, 212) },

                new List<PointD2D> { new PointD2D(60, 168), new PointD2D(60, 174), new PointD2D(70, 186), new PointD2D(102, 181), new PointD2D(102, 168) }
            };

            var worldPolys = new List<PolygonD2D>();
            foreach (var poly in pixelPolys)
            {
                var worldList = new List<PointD2D>();
                foreach (var pixelP in poly)
                {
                    worldList.Add(new PointD2D(pixelP.X * scale, pixelP.Y * scale));
                }

                worldPolys.Add(new PolygonD2D(worldList));
            }

            _CollisionMesh = new CollisionMeshD2D(worldPolys);

            _Recipes = new List<HarvestableRecipe>
            {
                new HarvestableRecipe(new List<Tuple<Material, int>> { Tuple.Create(Material.Sugarcane, 3) }, new List<Tuple<Material, int>> { Tuple.Create(Material.Rum, 1) }, 10000)
            };
        }

        protected SpriteRenderer Renderer;

        public Tavern2(PointD2D position, int id) : base(position, _CollisionMesh, id, _Recipes, 1)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Tavern", SourceRec);

            Inventory = new EntityInventory(1);
            Inventory.SetDefaultStackSize(10);
            OutputInventory = new EntityInventory(1);
            OutputInventory.SetDefaultStackSize(10);
            InitRecipeListeners();
        }

        public Tavern2()
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
            Renderer?.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 218 / 32.0, 212 / 32.0, overlay);
        }

        public override SpriteSheetAnimationRenderer GetInprogressRenderable(ContentManager content)
        {
            const int width = 218;
            const int height = 212;
            const string img = "Tavern";
            return new AnimationRendererBuilder(content)
                .BeginAnimation(null, AnimationType.Idle, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: height - (int)(height * 0.1), height: (int)(height * 0.1), topLeftDif: new PointD2D(0, (int)(height * 0.1) - height))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.Unbuilt, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: height - (int)(height * 0.1), height: (int)(height * 0.1), topLeftDif: new PointD2D(0, (int)(height * 0.1) - height))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltThirty, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: height - (int)(height * 0.3), height: (int)(height * 0.3), topLeftDif: new PointD2D(0, (int)(height * 0.3) - height))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltSixty, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: height - (int)(height * 0.6), height: (int)(height * 0.6), topLeftDif: new PointD2D(0, (int)(height * 0.6) - height))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltNinety, defaultWidth: width, defaultSourceTexture: img)
                    .AddFrame(y: 0, height: height, topLeftDif: new PointD2D(0, 0))
                .EndAnimation()
                .Build();
        }
    }
}
