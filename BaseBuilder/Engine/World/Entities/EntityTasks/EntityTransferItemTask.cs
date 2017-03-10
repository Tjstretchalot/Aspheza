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

        public EntityTransferItemTask(Container from, int toID, List<TransferRestrictors.ITransferRestrictor> restrictors, TransferResultDecider decider)
        {
            LastReturn = null;
            FromID = from.ID;
            _From = new TargetFinder(FromID);
            ToID = toID;
            _To = new TargetFinder(toID);
            Restrictors = restrictors;
            Decider = decider;
        }

        public EntityTransferItemTask(Container from, Entity to, List<TransferRestrictors.ITransferRestrictor> restrictors, TransferResultDecider decider)
        {
            LastReturn = null;
            FromID = from.ID;
            _From = new TargetFinder(FromID);
            ToID = to.ID;
            _To = new TargetFinder(to.ID);
            Restrictors = restrictors;
            Decider = decider;
        }

        public EntityTransferItemTask(NetIncomingMessage message)
        {
            FromID = message.ReadInt32();
            _To = new TargetFinder(message);
            Decider = new TransferResultDecider(message);
            
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(FromID);
            _To.Write(message);
            Decider.Write(message);
            foreach (var restrictor in Restrictors)
                restrictor.Write(message);
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
            throw new NotImplementedException();
        }
    }
}
