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

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    public class EntityTransferItemTask : IEntityTask
    {
        Transferer transfer = new Transferer();
        EntityTaskStatus? LastReturn;
        TargetFinder _From;
        int FromID;
        Container From;
        TargetFinder _To;
        int ToID;
        Entity To;
        List<TransferRestrictors.ITransferRestrictor> Restrictors;
        TransferResultDecider Decider;

        public EntityTransferItemTask(Container from, PointD2D toLoc, List<TransferRestrictors.ITransferRestrictor> restrictors, TransferResultDecider decider)
        {
            LastReturn = null;
            FromID = from.ID;
            _From = new TargetFinder(FromID);
            _To = new TargetFinder(toLoc);
            Restrictors = restrictors;
            Decider = decider;
        }

        public EntityTransferItemTask(Container from, int toID, List<ITransferRestrictor> restrictors, TransferResultDecider decider)
        {
            LastReturn = null;
            FromID = from.ID;
            _From = new TargetFinder(FromID);
            ToID = toID;
            _To = new TargetFinder(toID);
            Restrictors = restrictors;
            Decider = decider;
        }

        public EntityTransferItemTask(Container from, Entity to, List<ITransferRestrictor> restrictors, TransferResultDecider decider)
        {
            LastReturn = null;
            FromID = from.ID;
            _From = new TargetFinder(FromID);
            ToID = to.ID;
            _To = new TargetFinder(to.ID);
            Restrictors = restrictors;
            Decider = decider;
        }

        public EntityTransferItemTask()
        {
            FromID = -1;
            ToID = -1;
            _From = null;
            _To = null;
            Restrictors = new List<ITransferRestrictor>();
            Decider = null;
        }

        public EntityTransferItemTask(NetIncomingMessage message)
        {
            FromID = message.ReadInt32();

            if (message.ReadBoolean())
                _To = new TargetFinder(message);

            if (message.ReadBoolean())
                Decider = new TransferResultDecider(message);

            if(message.ReadBoolean())
            {
                int numRestrictors = message.ReadInt32();

                for(int i = 0; i < numRestrictors; i++)
                {
                    var id = message.ReadInt16();
                    Restrictors.Add(TransferRestrictorIdentifier.InitTransferRestrictor(TransferRestrictorIdentifier.GetTypeOfID(id), message));
                }
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(FromID);
            if (_To == null)
            {
                message.Write(false);
            }
            else
            {
                message.Write(true);
                _To.Write(message);
            }

            if (Decider != null)
            {
                message.Write(true);
                Decider.Write(message);
            }else
            {
                message.Write(false);
            }

            if (Restrictors == null)
            {
                message.Write(false);
            }
            else
            {
                message.Write(true);
                message.Write(Restrictors.Count);
                foreach (var restrictor in Restrictors)
                {
                    message.Write(TransferRestrictorIdentifier.GetIDOfTransferRestrictor(restrictor.GetType()));
                    restrictor.Write(message);
                }
            }
        }

        public string PrettyDescription
        {
            get
            {
                return "Transfering Item";
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
                throw new NotImplementedException();
            }
        }

        public string TaskName
        {
            get
            {
                return "Transfer";
            }
        }

        public string TaskStatus
        {
            get
            {
                return "No status available";
            }
        }

        public void Cancel(SharedGameState gameState)
        {
        }

        public void Reset(SharedGameState gameState)
        {
            LastReturn = null;
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (!IsValid())
                return EntityTaskStatus.Failure;

            if (LastReturn.HasValue)
                return LastReturn.Value;

            if (To == null)
            {
                To = _To.FindTarget(gameState);
                ToID = To.ID;
            }
            if (From == null)
            {
                From = _From.FindTarget(gameState) as Container;
            }

            Dictionary<Material, int> transfers = From.Inventory.GetSimplifiedInvitory();

            foreach (var restrict in Restrictors)
            {
                restrict.Restrict(gameState, From, To, transfers);
                restrict.Reset();
            }

            Dictionary<Material, int> transfered = transfer.Transfer(gameState, From, To, transfers);

            LastReturn = Decider.Decide(transfered);
            if (LastReturn == EntityTaskStatus.Success)
                Decider.Reset();
            return LastReturn.Value;
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
        }

        public bool IsValid()
        {
            return FromID != -1 && _To != null && Restrictors != null && Decider == null;
        }
    }
}
