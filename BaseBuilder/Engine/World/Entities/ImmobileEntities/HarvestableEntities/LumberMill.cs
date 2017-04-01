using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;
using Microsoft.Xna.Framework.Content;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.HarvestableEntities
{
    class LumberMill : HarvestableEntity
    {
        protected static CollisionMeshD2D _CollisionMesh;
        protected static List<HarvestableRecipe> _Recipes;

        protected static Rectangle SourceRec = new Rectangle(0, 0, 164, 204);

        static LumberMill()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(5.125, 5, 0, 1.375),
                new PolygonD2D(new List<PointD2D> { new PointD2D(0.4375, 1.375), new PointD2D(0.4375, 0.9375), new PointD2D(1.4375, 0), new PointD2D(2.4375, 0.9375), new PointD2D(2.4375, 1.375) }) });

            _Recipes = new List<HarvestableRecipe>
            {
                new HarvestableRecipe(new List<Tuple<Material, int>> { Tuple.Create(Material.Wood, 1) }, new List<Tuple<Material, int>> { Tuple.Create(Material.Lumber, 1) }, 5000)
            };
        }

        protected SpriteRenderer Renderer;

        public LumberMill(PointD2D position, int id) : base(position, _CollisionMesh, id, _Recipes, 1)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("LumberMill", SourceRec);

            Inventory = new EntityInventory(1);
            Inventory.SetDefaultStackSize(10);
            OutputInventory = new EntityInventory(1);
            OutputInventory.SetDefaultStackSize(10);
            InitRecipeListeners();
        }

        public LumberMill()
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
            Renderer?.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 164 / 32.0, 204 / 32.0, overlay);
        }
        
        public override SpriteSheetAnimationRenderer GetInprogressRenderable(ContentManager content)
        {
            const int width = 164;
            const int height = 204;
            const string img = "LumberMill";
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
