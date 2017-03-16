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
using BaseBuilder.Engine.World.Entities.ImmobileEntities.Tree;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    /// <summary>
    /// The task of harvesting a harvestable as a worker
    /// </summary>
    public class EntityHarvestTask : IEntityTask
    {
        public string PrettyDescription
        {
            get
            {
                return $"Harvesting {ThingToHarvest}";
            }
        }

        public double Progress
        {
            get
            {
                return ((double)TotalTimeRequiredMS - TimeLeftMS) / TotalTimeRequiredMS;
            }
        }

        public string TaskDescription
        {
            get
            {
                return $"Harvesting";
            }
        }

        public string TaskName
        {
            get
            {
                return "Harvest";
            }
        }

        public string TaskStatus
        {
            get
            {
                return $"{TimeLeftMS}ms remaining";
            }
        }
        
        public int HarvesterID;
        public ITransferTargeter HarvestedTargeter;

        protected int TotalTimeRequiredMS;
        protected int TimeLeftMS;
        protected string ThingToHarvest;

        protected bool FixedDirection;

        public EntityHarvestTask(Container harvester, Harvestable harvested, int timeReqMS)
        {
            HarvesterID = harvester.ID;
            HarvestedTargeter = new TransferTargetByID(harvested.ID);

            TotalTimeRequiredMS = timeReqMS;
            TimeLeftMS = TotalTimeRequiredMS;
            ThingToHarvest = harvested.GetHarvestNamePretty();

            FixedDirection = false;
        }

        public EntityHarvestTask(int sourceID, ITransferTargeter targeter, int timeReqMS = 3000)
        {
            HarvesterID = sourceID;
            HarvestedTargeter = targeter;

            TotalTimeRequiredMS = timeReqMS;
            TimeLeftMS = TotalTimeRequiredMS;
            ThingToHarvest = null;

            FixedDirection = false;
        }

        public EntityHarvestTask()
        {
            HarvesterID = -1;
            HarvestedTargeter = null;
            TotalTimeRequiredMS = -1;
            TimeLeftMS = -1;
            ThingToHarvest = "Bad state";

            FixedDirection = false;
        }

        public EntityHarvestTask(NetIncomingMessage message)
        {
            HarvesterID = message.ReadInt32();
            if(message.ReadBoolean())
            {
                var typeID = message.ReadInt16();
                HarvestedTargeter = TransferTargeterIdentifier.Init(TransferTargeterIdentifier.GetTypeOfID(typeID), message);
            }

            TotalTimeRequiredMS = message.ReadInt32();
            TimeLeftMS = message.ReadInt32();
            if(message.ReadBoolean())
                ThingToHarvest = message.ReadString();

            FixedDirection = message.ReadBoolean();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(HarvesterID);
            if (HarvestedTargeter != null)
            {
                message.Write(true);
                message.Write(TransferTargeterIdentifier.GetIDOfType(HarvestedTargeter.GetType()));
                HarvestedTargeter.Write(message);
            }else
            {
                message.Write(false);
            }

            message.Write(TotalTimeRequiredMS);
            message.Write(TimeLeftMS);
            if (ThingToHarvest != null)
            {
                message.Write(true);
                message.Write(ThingToHarvest);
            }else {
                message.Write(false);
            }

            message.Write(FixedDirection);
        }

        public void Cancel(SharedGameState gameState)
        {
            
        }

        public void Reset(SharedGameState gameState)
        {
            TimeLeftMS = TotalTimeRequiredMS;
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (!IsValid())
                return EntityTaskStatus.Failure;
            
            Container Harvester = gameState.World.MobileEntities.Find((m) => m.ID == HarvesterID) as Container;
            Harvestable Harvested = HarvestedTargeter.FindTarget(gameState, Harvester as MobileEntity) as Harvestable;

            if (!Harvested.ReadyToHarvest(gameState))
                return EntityTaskStatus.Failure;

            if (!Harvester.CollisionMesh.Intersects(Harvested.CollisionMesh, Harvester.Position, Harvested.Position) && !Harvester.CollisionMesh.MinDistanceShorterThan(Harvested.CollisionMesh, 1, Harvester.Position, Harvested.Position))
                return EntityTaskStatus.Failure;

            if (ThingToHarvest == null)
                ThingToHarvest = Harvested.GetHarvestNamePretty();

            if(!FixedDirection)
            {
                FixedDirection = true;

                var mobileHarv = Harvester as MobileEntity;
                if(mobileHarv != null)
                    DirectionUtils.Face(gameState, mobileHarv, Harvested);
            }

            TimeLeftMS -= timeMS;

            var mobileHarvCave = Harvester as CaveManWorker;
            if (TimeLeftMS <= 0)
            {
                Harvested.TryHarvest(gameState, Harvester);
                if (mobileHarvCave != null)
                    mobileHarvCave.Reset();
                return EntityTaskStatus.Success;
            }
            else
            {
                var harvested = Harvested as Tree;
                if (mobileHarvCave != null && harvested != null)
                {
                    mobileHarvCave.OnChopping(gameState, timeMS, "ChopTree");
                }
            }
            
            
            
            return EntityTaskStatus.Running;
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
        }

        public bool IsValid()
        {
            return HarvesterID != -1 && HarvestedTargeter != null && TotalTimeRequiredMS > 0;
        }
    }
}
