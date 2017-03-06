using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
     public class SpriteSheetBuilding : ImmobileEntity
    {
        protected List<Tuple<Rectangle, PointD2D>> SourceRectsToOffsetLocations;
        private string _SheetName;
        protected string SheetName
        {
            get
            {
                return _SheetName;
            }

            set
            {
                Texture = null;
                _SheetName = value;
            }
        }

        protected Texture2D Texture;


        public SpriteSheetBuilding(PointD2D position, CollisionMeshD2D collisionMesh, int id, string sheetName, List<Tuple<Rectangle, PointD2D>> sourceRectsToOffsetLocations) : base(position, collisionMesh, id)
        {
            SheetName = sheetName;
            SourceRectsToOffsetLocations = sourceRectsToOffsetLocations;
        }
        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public SpriteSheetBuilding() : base()
        {
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            if(Texture == null)
                Texture = context.Content.Load<Texture2D>(SheetName);

            foreach (var tuple in SourceRectsToOffsetLocations)
            {
                var sourceRect = tuple.Item1;
                var offset = tuple.Item2;
                var destRect = new Rectangle(
                        (int)(offset.X * context.Camera.Zoom + screenTopLeft.X), (int)(offset.Y * context.Camera.Zoom + screenTopLeft.Y), (int)context.Camera.Zoom, (int)context.Camera.Zoom
                    );
                context.SpriteBatch.Draw(Texture, sourceRectangle: sourceRect, destinationRectangle: destRect, color: overlay);
            }
        }
    }
}
