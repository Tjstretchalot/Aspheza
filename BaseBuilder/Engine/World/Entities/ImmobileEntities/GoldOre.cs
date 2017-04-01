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
using Microsoft.Xna.Framework.Content;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class GoldOre : ImmobileEntity, Harvestable
    {
        protected static CollisionMeshD2D _CollisionMesh;

        static GoldOre()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(4, 4) });
        }

        public GoldOre(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
        }

        public GoldOre() : base()
        {
            CollisionMesh = _CollisionMesh;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();

            TasksFromMessage(gameState, message);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            var text = context.Content.Load<Texture2D>("gold_ore");
            var destRect = new Rectangle((int)screenTopLeft.X, (int)screenTopLeft.Y, (int)(4 * context.Camera.Zoom), (int)(4 * context.Camera.Zoom));

            context.SpriteBatch.Draw(text, destRect, overlay);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);

            WriteTasks(message);
        }

        public bool ReadyToHarvest(SharedGameState sharedGameState)
        {
            return true;
        }

        public string GetHarvestNamePretty()
        {
            return "Gold";
        }

        public void TryHarvest(SharedGameState sharedGameState, Container reciever)
        {
            reciever.Inventory.AddMaterial(Material.GoldOre, 1);
        }

        public override SpriteSheetAnimationRenderer GetInprogressRenderable(ContentManager content)
        {
            throw new NotImplementedException();
        }
    }
}
