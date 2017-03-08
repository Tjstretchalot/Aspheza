using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    public class IFindTarget
    {
        protected bool LocOrID;
        protected PointD2D Loc;
        protected int ID;

        public IFindTarget(PointD2D loc)
        {
            Loc = loc;
            LocOrID = true;
        }

        public IFindTarget(int id)
        {
            ID = id;
            LocOrID = false;
        }

        public IFindTarget(NetIncomingMessage message)
        {
            LocOrID = message.ReadBoolean();
            if (LocOrID)
            {
                Loc = new PointD2D(message);
            }
            else
                ID = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(LocOrID);
            if (LocOrID)
            {
                Loc.Write(message);
            }
            else
                message.Write(ID);
        }

        public Entity FindTarget(SharedGameState sharedGameState)
        {
            if(LocOrID)
            {
                return FindTarget(sharedGameState, Loc.X, Loc.Y);
            }
            else
            {
                return FindTarget(sharedGameState, ID);
            }
        }

        public Entity FindTarget(SharedGameState sharedGameState, PointD2D loc)
        {
            return FindTarget(sharedGameState, loc.X, loc.Y);
        }

        public Entity FindTarget(SharedGameState sharedGameState, PointI2D loc)
        {
            return FindTarget(sharedGameState, loc.X, loc.Y);
        }

        public Entity FindTarget(SharedGameState sharedGameState, double locX, double locY)
        {
            return sharedGameState.World.GetEntityAtLocation(locX, locY);
        }

        public Entity FindTarget(SharedGameState sharedGameState, int ID)
        {
            var entitiesMo = sharedGameState.World.MobileEntities;
            foreach (var ent in entitiesMo)
            {
                if (ent.ID == ID)
                    return ent;
            }

            var entitiesIm = sharedGameState.World.ImmobileEntities;
            foreach (var ent in entitiesIm)
            {
                if (ent.ID == ID)
                    return ent;
            }

            return null;
        }
    }
}
