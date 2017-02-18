using BaseBuilder.Engine.Math2D.Double;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public class OverseerMage : MobileEntity
    {
        private static short NetID = 3;
        private static RectangleD2D _CollisionMesh;

        static OverseerMage()
        {
            EntityIdentifier.Register(typeof(OverseerMage), NetID);
            _CollisionMesh = new RectangleD2D(1, 0.875);
        }
        
        public OverseerMage(PointD2D position, int id) : base(position, _CollisionMesh, id, "OverseerMage")
        {
        }
        
        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public OverseerMage() : base()
        {
            SpriteName = "OverseerMage";
            CollisionMesh = _CollisionMesh;
        }

        public override void FromMessage(NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
        }
    }
}
