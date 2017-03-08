﻿using System;
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

            if(TimeLeftMS <= 0)
            {
                Harvest(gameState);
                return EntityTaskStatus.Success;
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
    }
}
