using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.ImmobileEntities;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    public class EntityMineGoldTask : IEntityTask
    {
        public string TaskDescription
        {
            get
            {
                return $"Mining gold ore ({RemainingTimeForNextMS}ms for next)";
            }
        }

        public string TaskName
        {
            get
            {
                return "Mining gold ore";
            }
        }

        public string TaskStatus
        {
            get
            {
                return $"Next gold ore in {RemainingTimeForNextMS}ms";
            }
        }

        public string PrettyDescription
        {
            get
            {
                return $"Mining Gold";
            }
        }

        public double Progress
        {
            get
            {
                return 1.0 - ((double)RemainingTimeForNextMS / TimeToMineMS);
            }
        }

        protected bool FixedDirection;
        protected int TimeToMineMS;
        protected int RemainingTimeForNextMS;

        protected int WorkerID;
        protected int VeinID;
        protected CaveManWorker Worker;
        protected GoldOre Vein;

        public EntityMineGoldTask(CaveManWorker worker, GoldOre vein)
        {
            WorkerID = worker.ID;
            VeinID = vein.ID;
            TimeToMineMS = 3000;
            RemainingTimeForNextMS = TimeToMineMS;
        }

        public EntityMineGoldTask(SharedGameState gameState, NetIncomingMessage message)
        {
            WorkerID = message.ReadInt32();
            VeinID = message.ReadInt32();

            FixedDirection = message.ReadBoolean();
            TimeToMineMS = message.ReadInt32();
            RemainingTimeForNextMS = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(WorkerID);
            message.Write(VeinID);

            message.Write(FixedDirection);
            message.Write(TimeToMineMS);
            message.Write(RemainingTimeForNextMS);
        }


        public void Reset(SharedGameState gameState)
        {
        }

        public void Cancel(SharedGameState gameState)
        {
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (Vein == null)
            {
                Vein = gameState.World.ImmobileEntities.Find((e) => e.ID == VeinID) as GoldOre;
                if (Vein == null)
                    throw new InvalidProgramException("Impossible");
            }

            if(Worker == null)
            {
                Worker = gameState.World.MobileEntities.Find((e) => e.ID == WorkerID) as CaveManWorker;
                if (Worker == null)
                    throw new InvalidProgramException("Impossible");
            }

            if(!FixedDirection)
            {
                DirectionUtils.Face(gameState, Worker, Vein);

                FixedDirection = true;
            }

            if (!Worker.Inventory.HaveRoomFor(Material.GoldOre, 1))
                return EntityTaskStatus.Success;

            RemainingTimeForNextMS -= timeMS;

            if(RemainingTimeForNextMS <= 0)
            {
                Worker.Inventory.AddMaterial(Material.GoldOre, 1);

                RemainingTimeForNextMS = TimeToMineMS;
            }

            return EntityTaskStatus.Running;
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
        }

        public bool IsValid()
        {
            throw new NotImplementedException();
        }
    }
}
