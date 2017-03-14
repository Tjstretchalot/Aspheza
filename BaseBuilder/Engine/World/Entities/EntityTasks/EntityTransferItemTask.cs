using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferResultDeciders;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferTargeters;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    public class EntityTransferItemTask : IEntityTask
    {
        public string PrettyDescription
        {
            get
            {
                return "Transfering items";
            }
        }

        public double Progress
        {
            get
            {
                return ((double)TotalTimeToTransferMS - TimeRemainingToTransferMS) / TotalTimeToTransferMS;
            }
        }

        public string TaskDescription
        {
            get
            {
                return "Transfering items";
            }
        }

        public string TaskName
        {
            get
            {
                return "Transfer items";
            }
        }

        public string TaskStatus
        {
            get
            {
                return "In progress";
            }
        }

        public int SourceID;
        public bool Pickup;
        public ITransferTargeter Targeter;
        public List<ITransferRestrictor> Restrictors;
        public ITransferResultDecider ResultDecider;

        protected bool RunSinceLastReset;
        protected EntityTaskStatus? CachedResult;
        protected int TotalTimeToTransferMS;
        protected int TimeRemainingToTransferMS;

        public EntityTransferItemTask()
        {
            SourceID = -1;
            Pickup = false;
            Targeter = null;
            Restrictors = null;
            ResultDecider = null;

            RunSinceLastReset = false;
            CachedResult = null;
            TotalTimeToTransferMS = 1500;
            TimeRemainingToTransferMS = TotalTimeToTransferMS;
        }

        public EntityTransferItemTask(int sourceID, bool pickup, ITransferTargeter targeter, List<ITransferRestrictor> restrictors, ITransferResultDecider resultDecider) : this()
        {
            SourceID = sourceID;
            Pickup = pickup;
            Targeter = targeter;
            Restrictors = restrictors;
            ResultDecider = resultDecider;
        }

        public EntityTransferItemTask(NetIncomingMessage message)
        {
            SourceID = message.ReadInt32();
            Pickup = message.ReadBoolean();

            if (message.ReadBoolean()) {
                var typeID = message.ReadInt16();
                Targeter = TransferTargeterIdentifier.Init(TransferTargeterIdentifier.GetTypeOfID(typeID), message);
            }

            if(message.ReadBoolean())
            {
                int amt = message.ReadInt32();

                Restrictors = new List<ITransferRestrictor>(amt);

                for(int i = 0; i < amt; i++)
                {
                    var typeID = message.ReadInt16();
                    Restrictors.Add(TransferRestrictorIdentifier.Init(TransferRestrictorIdentifier.GetTypeOfID(typeID), message));
                }
            }

            if(message.ReadBoolean())
            {
                var typeID = message.ReadInt16();
                ResultDecider = TransferResultDeciderIdentifier.Init(TransferResultDeciderIdentifier.GetTypeOfID(typeID), message);
            }

            RunSinceLastReset = message.ReadBoolean();
            if(message.ReadBoolean())
            {
                CachedResult = (EntityTaskStatus) message.ReadInt32();
            }
            TotalTimeToTransferMS = message.ReadInt32();
            TimeRemainingToTransferMS = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(SourceID);
            message.Write(Pickup);

            if (Targeter == null)
            {
                message.Write(false);
            }
            else
            {
                message.Write(true);
                message.Write(TransferTargeterIdentifier.GetIDOfType(Targeter.GetType()));
                Targeter.Write(message);
            }

            if(Restrictors == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write(Restrictors.Count);

                foreach(var restr in Restrictors)
                {
                    message.Write(TransferRestrictorIdentifier.GetIDOfType(restr.GetType()));
                    restr.Write(message);
                }
            }

            if(ResultDecider == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write(TransferResultDeciderIdentifier.GetIDOfType(ResultDecider.GetType()));
                ResultDecider.Write(message);
            }

            message.Write(RunSinceLastReset);
            if(!CachedResult.HasValue)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write((int)CachedResult.Value);
            }
            message.Write(TotalTimeToTransferMS);
            message.Write(TimeRemainingToTransferMS);
        }

        public bool IsValid()
        {
            if (SourceID < 0)
                return false;

            if (Targeter == null)
                return false;

            if (!Targeter.IsValid())
                return false;

            if (Restrictors == null)
                return false;

            foreach(var restr in Restrictors)
            {
                if (!restr.IsValid())
                    return false;
            }

            if (ResultDecider == null)
                return false;

            if (!ResultDecider.IsValid())
                return false;

            return true;
        }

        public void Reset(SharedGameState gameState)
        {
            TimeRemainingToTransferMS = TotalTimeToTransferMS;

            if (RunSinceLastReset)
            {
                CachedResult = null;

                foreach(var restr in Restrictors)
                {
                    restr.Reset();
                }

                ResultDecider.Reset();
            }
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (CachedResult.HasValue)
                return CachedResult.Value;

            RunSinceLastReset = true;
            TimeRemainingToTransferMS -= timeMS;

            if(TimeRemainingToTransferMS <= 0)
            {
                TimeRemainingToTransferMS = TotalTimeToTransferMS;

                var source = gameState.World.MobileEntities.Find((me) => me.ID == SourceID);
                var sourceContainer = (Container)source;

                var target = Targeter.FindTarget(gameState, source);

                if (target == null)
                    return EntityTaskStatus.Failure;

                if (!target.CollisionMesh.Intersects(source.CollisionMesh, target.Position, source.Position) && !target.CollisionMesh.MinDistanceShorterThan(source.CollisionMesh, 1, target.Position, source.Position))
                    return EntityTaskStatus.Failure;
                
                var from = Pickup ? target : sourceContainer;
                var to = Pickup ? sourceContainer : target;

                var stuffToSend = from.Inventory.GetSimplifiedInventory();

                foreach(var restr in Restrictors)
                {
                    restr.Restrict(gameState, from, to, stuffToSend);
                }

                var stuffSent = new Dictionary<Material, int>();
                foreach(var kvp in stuffToSend)
                {
                    int amountSent = to.Inventory.AddMaterial(kvp.Key, kvp.Value);
                    if (amountSent > 0)
                    {
                        from.Inventory.RemoveMaterial(kvp.Key, amountSent);
                        stuffSent.Add(kvp.Key, amountSent);
                    }
                }

                var result = ResultDecider.GetResult(from, to, stuffSent);

                if(result != EntityTaskStatus.Running)
                {
                    CachedResult = result;
                }

                RunSinceLastReset = true;
                return result;
            }

            return EntityTaskStatus.Running;
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
        }

        public void Cancel(SharedGameState gameState)
        {
        }
    }
}
