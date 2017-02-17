using BaseBuilder.Engine.Math2D.Double;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public class Overseer : MobileEntity
    {
        private static short NetID = 2;
        private static RectangleD2D _CollisionMesh;

        static Overseer()
        {
            EntityIdentifier.Register(typeof(Overseer), NetID);
            _CollisionMesh = new RectangleD2D(1, 0.875);
        }
        
        public Overseer(PointD2D position, int id) : base(position, _CollisionMesh, id, "Overseer")
        {
        }
        
        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public Overseer() : base()
        {
            SpriteName = "Archer";
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
