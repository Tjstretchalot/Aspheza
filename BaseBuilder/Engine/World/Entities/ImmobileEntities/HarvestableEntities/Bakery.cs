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

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.HarvestableEntities
{
    /// <summary>
    /// Bakes bread
    /// </summary>
    public class Bakery : HarvestableEntity
    {
        protected static CollisionMeshD2D _CollisionMesh;
        protected static List<HarvestableRecipe> _Recipes;

        protected static Rectangle SourceRec = new Rectangle(0, 0, 158, 114);

        static Bakery()
        {
            //_CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(5, 3.5) });

            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> {
                new PolygonD2D(new List<PointD2D> { new PointD2D(0, 0), new PointD2D(2.5, 0), new PointD2D(2.5, 3.5), new PointD2D(0, 3.5), }),
                new PolygonD2D(new List<PointD2D> { new PointD2D(2.5, 0), new PointD2D(5, 0), new PointD2D(5, 4.5), new PointD2D(2.5, 4.5) })
            });

            _Recipes = new List<HarvestableRecipe>
            {
                new HarvestableRecipe(new List<Tuple<Material, int>> { Tuple.Create(Material.Sugar, 1), Tuple.Create(Material.Egg, 1), Tuple.Create(Material.Wheat, 1) }, new List<Tuple<Material, int>> { Tuple.Create(Material.Bread, 1) }, 5000)
            };
        }

        protected SpriteRenderer Renderer;

        public Bakery(PointD2D position, int id) : base(position, _CollisionMesh, id, _Recipes, 1)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Bakery", SourceRec);

            Inventory = new EntityInventory(3);
            Inventory.SetDefaultStackSize(10);
            OutputInventory = new EntityInventory(1);
            OutputInventory.SetDefaultStackSize(10);
            InitRecipeListeners();
        }

        public Bakery()
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
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 156 / 32.0, 114 / 32.0, overlay);
        }
    }
}
