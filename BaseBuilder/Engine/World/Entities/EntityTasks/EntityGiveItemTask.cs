using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.Entities.MobileEntities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// The task of giving an item to something else. This is required so that the order happens
    /// as expected
    /// </summary>
    public class EntityGiveItemTask : IEntityTask
    {
        public string PrettyDescription
        {
            get
            {
                return "Giving item";
            }
        }

        public double Progress
        {
            get
            {
                return 0;
            }
        }

        public string TaskDescription
        {
            get
            {
                return "Give item";
            }
        }

        public string TaskName
        {
            get
            {
                return "Give";
            }
        }

        public string TaskStatus
        {
            get
            {
                return "No status available";
            }
        }
        
        protected int FromID;
        protected bool FromMobile;
        protected int ToID;
        protected bool ToMobile;
        
        protected int Index;

        public EntityGiveItemTask(Container from, Container to, int index)
        {
            FromID = from.ID;
            ToID = to.ID;

            FromMobile = typeof(MobileEntity).IsAssignableFrom(from.GetType());
            ToMobile = typeof(MobileEntity).IsAssignableFrom(to.GetType());

            Index = index;
        }

        public EntityGiveItemTask(NetIncomingMessage message)
        {
            FromID = message.ReadInt32();
            ToID = message.ReadInt32();

            FromMobile = message.ReadBoolean();
            ToMobile = message.ReadBoolean();

            Index = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(FromID);
            message.Write(ToID);

            message.Write(FromMobile);
            message.Write(ToMobile);

            message.Write(Index);
        }

        public void Cancel(SharedGameState gameState)
        {
        }

        public void Reset(SharedGameState gameState)
        {
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            var from = (FromMobile ? gameState.World.MobileEntities.Find((m) => m.ID == FromID) as Container : gameState.World.ImmobileEntities.Find((im) => im.ID == FromID) as Container);
            var to = (ToMobile ? gameState.World.MobileEntities.Find((m) => m.ID == ToID) as Container : gameState.World.ImmobileEntities.Find((im) => im.ID == ToID) as Container);
            
            var mat = from.Inventory.MaterialAt(Index);

            if (!to.Inventory.HaveRoomFor(mat.Item1, mat.Item2))
                return EntityTaskStatus.Failure;

            from.Inventory.RemoveMaterialAt(Index);
            to.Inventory.AddMaterial(mat.Item1, mat.Item2);

            if (FromMobile)
                DirectionUtils.Face(gameState, (MobileEntity)from, to);

            return EntityTaskStatus.Success;
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
        }

    }
}
