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
    public class CaveManWorker : MobileEntity
    {
        private const double SpeedConst = 0.005;
        private static RectangleD2D _CollisionMesh;

        public EntityInventory Inventory { get; protected set; }

        static CaveManWorker()
        {
            _CollisionMesh = new RectangleD2D(1, 1);
        }

        public CaveManWorker(PointD2D position, int id) : base(position, _CollisionMesh, id, "CaveManWorker", SpeedConst)
        {
            Inventory = new EntityInventory(6);
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public CaveManWorker() : base()
        {
            SpriteName = "CaveManWorker";
            CollisionMesh = _CollisionMesh;
            SpeedUnitsPerMS = SpeedConst;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);

            WriteTasks(message);
        }
    }
}
