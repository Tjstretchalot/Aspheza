using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
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
        private const double SpeedConst = 0.005;
        private static RectangleD2D _CollisionMesh;

        static Overseer()
        {
            _CollisionMesh = new RectangleD2D(1, 1);
        }
        
        public Overseer(PointD2D position, int id) : base(position, _CollisionMesh, id, "Overseer", SpeedConst)
        {
        }
        
        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public Overseer() : base()
        {
            SpriteName = "Overseer";
            CollisionMesh = _CollisionMesh;
            SpeedUnitsPerMS = SpeedConst;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);

            WriteTasks(message);
        }
    }
}
