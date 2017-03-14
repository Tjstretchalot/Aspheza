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
                return $"Harvesting from {HarvestedID} to {HarvesterID}";
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

        protected Container Harvester;
        protected Harvestable Harvested;
        protected int HarvesterID;
        protected int HarvestedID;

        protected int TotalTimeRequiredMS;
        protected int TimeLeftMS;
        protected string ThingToHarvest;

        protected bool FixedDirection;

        public EntityHarvestTask(Container harvester, Harvestable harvested, int timeReqMS)
        {
            HarvesterID = harvester.ID;
            HarvestedID = harvested.ID;

            TotalTimeRequiredMS = timeReqMS;
            TimeLeftMS = TotalTimeRequiredMS;
            ThingToHarvest = harvested.GetHarvestNamePretty();

            FixedDirection = false;
        }

        public EntityHarvestTask()
        {
            HarvesterID = -1;
            HarvestedID = -1;
            TotalTimeRequiredMS = -1;
            TimeLeftMS = -1;
            ThingToHarvest = "Bad state";

            FixedDirection = false;
        }

        public EntityHarvestTask(NetIncomingMessage message)
        {
            HarvesterID = message.ReadInt32();
            HarvestedID = message.ReadInt32();

            TotalTimeRequiredMS = message.ReadInt32();
            TimeLeftMS = message.ReadInt32();
            ThingToHarvest = message.ReadString();

            FixedDirection = message.ReadBoolean();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(HarvesterID);
            message.Write(HarvestedID);

            message.Write(TotalTimeRequiredMS);
            message.Write(TimeLeftMS);
            message.Write(ThingToHarvest);

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

            if (Harvester == null)
                Harvester = gameState.World.MobileEntities.Find((m) => m.ID == HarvesterID) as Container;
            if (Harvested == null)
                Harvested = gameState.World.ImmobileEntities.Find((im) => im.ID == HarvestedID) as Harvestable;

            if (!Harvested.ReadyToHarvest(gameState))
                return EntityTaskStatus.Failure;

            if (!Harvester.CollisionMesh.Intersects(Harvested.CollisionMesh, Harvester.Position, Harvested.Position) && !Harvester.CollisionMesh.MinDistanceShorterThan(Harvested.CollisionMesh, 1, Harvester.Position, Harvested.Position))
                return EntityTaskStatus.Failure;

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
                Harvest(gameState);
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

        protected void Harvest(SharedGameState gameState)
        {
            Harvested.TryHarvest(gameState, Harvester);
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
        }

        public bool IsValid()
        {
            return HarvesterID != -1 && HarvestedID != -1 && TotalTimeRequiredMS > 0;
        }
    }
}
