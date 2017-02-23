using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.WorldObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Pathfinders
{
    /// <summary>
    /// A* Pathfinding (modified for any-size collision meshes)
    /// </summary>
    public class EnhancedAStarPathfinder
    {
        TileWorld World;
        MobileEntity Entity;
        PointI2D Start;
        PointI2D End;

        List<WorkingAStarNode> Open;
        HashSet<PointI2D> Closed;

        protected void Init(TileWorld world, MobileEntity entity, PointI2D start, PointI2D end)
        {
            World = world;
            Entity = entity;
            Start = start;
            End = end;

            Open = new List<WorkingAStarNode>();
            Closed = new HashSet<PointI2D>();
        }

        protected void Cleanup()
        {
            World = null;
            Entity = null;
            Start = null;
            End = null;
            Open = null;
            Closed = null;
        }

        /// <summary>
        /// Calculates a path from start to end for the specified entity in the specified
        /// world. The path will match the grid, it is up to the move order to interpolate 
        /// between-grid locations.
        /// </summary>
        /// <param name="world">The world to find a path in</param>
        /// <param name="entity">The entity who will follow the path</param>
        /// <param name="start">The start of the path</param>
        /// <param name="end">The end of the path</param>
        /// <returns>The path to travel if one can be found, null if no path can be found</returns>
        public UnitPath CalculatePath(TileWorld world, MobileEntity entity, PointI2D start, PointI2D end)
        {
            Init(world, entity, start, end);
            var path = CalculatePathImpl();
            Cleanup();

            return path;
        }

        protected UnitPath CalculatePathImpl()
        {
            if (!IsTilePassable(End))
                return null;

            AddToOpen(new WorkingAStarNode(null, Start, Heuristic(Start), 0));

            while(Open.Count > 0)
            {
                var expanding = GetNextFromOpen();
                AddToClosed(expanding.Location);

                foreach(var offset in AdjacentOffsets(expanding.Location))
                {
                    var x = expanding.Location.X + offset.X;
                    var y = expanding.Location.Y + offset.Y;
                    var loc = new PointI2D(x, y);
                    if (ClosedContains(loc))
                        continue;

                    int index;
                    if(OpenContains(loc, out index))
                    {
                        var fromOpen = Open[index];
                        if (expanding.ScoreFromStartToHere >= fromOpen.ScoreFromStartToHere)
                            continue;
                    }

                    if (!CanPerformMove(expanding.Location, offset))
                        continue;
                    
                    var node = new WorkingAStarNode(expanding, loc, Heuristic(loc), expanding.ScoreFromStartToHere + Score(expanding.Location, loc, offset));
                    
                    if(loc == End)
                    {
                        return RewindAndReturnPath(node);
                    }

                    if(index != -1)
                    {
                        Open.RemoveAt(index);
                    }

                    AddToOpen(node);
                }
            }

            return null;
        }

        UnitPath RewindAndReturnPath(WorkingAStarNode last)
        {
            AStarNode current = new AStarNode();
            current.Location = last.Location;
            
            while(last.Parent != null)
            {
                last = last.Parent;

                var tmp = new AStarNode();
                tmp.Next = current;
                tmp.Location = last.Location;
                current = tmp;
            }

            return new UnitPath(current);
        }

        bool CanPerformMove(PointI2D from, PointI2D moveDir)
        {
            var intrOffsets = Entity.CollisionMesh.TilesIntersectedWhenMovingFromOriginTo(moveDir);

            foreach(var offset in intrOffsets)
            {
                if (!IsTilePassable(offset.X + from.X, offset.Y + from.Y))
                    return false;
            }

            return true;
        }

        bool IsTilePassable(PointI2D pos)
        {
            return World.IsPassable(pos, Entity);
        }

        bool IsTilePassable(int x, int y)
        {
            return World.IsPassable(x, y, Entity);
        }

        bool OpenContains(PointI2D loc, out int index)
        {
            for(int i = 0; i < Open.Count; i++)
            {
                if(Open[i].Location == loc)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        void AddToOpen(WorkingAStarNode node)
        {
            // sorted by score in descending order
            LogicUtils.BinaryInsert(Open, node, (n1, n2) => Math.Sign(n2.CombinedScore - n1.CombinedScore));
        }


        WorkingAStarNode GetNextFromOpen()
        {
            var result = Open[Open.Count - 1];
            Open.RemoveAt(Open.Count - 1);
            return result;
        }

        void AddToClosed(PointI2D point)
        {
            Closed.Add(point);
        }

        bool ClosedContains(PointI2D point)
        {
            return Closed.Contains(point);
        }

        double Heuristic(PointI2D pos)
        {
            return (End - pos).DistanceToSquared(PointI2D.Origin);
        }

        double Score(PointI2D from, PointI2D to, PointI2D offset)
        {
            if (offset.X != 0 && offset.Y != 0)
                return 2;
            return 1;
        }

        PointI2D[] ReusedPoints;
        IEnumerable<PointI2D> AdjacentOffsets(PointI2D pos)
        {
            if(ReusedPoints == null)
            {
                ReusedPoints = new PointI2D[]
                {
                    new PointI2D(-1, -1),
                    new PointI2D(-1,  0),
                    new PointI2D(-1,  1),
                    new PointI2D(0,  -1),
                    new PointI2D(0,   1),
                    new PointI2D(1,  -1),
                    new PointI2D(1,   0),
                    new PointI2D(1,   1),
                };
            }
            if(pos.X > 0)
            {
                if (pos.Y > 0)
                    yield return ReusedPoints[0];

                yield return ReusedPoints[1];

                if (pos.Y < World.TileHeight - 1)
                    yield return ReusedPoints[2];
            }

            if (pos.Y > 0)
                yield return ReusedPoints[3];

            if (pos.Y < World.TileHeight - 1)
                yield return ReusedPoints[4];

            if(pos.X < World.TileWidth - 1)
            {
                if (pos.Y > 0)
                    yield return ReusedPoints[5];

                yield return ReusedPoints[6];

                if (pos.Y < World.TileHeight - 1)
                    yield return ReusedPoints[7];
            }
        }
    }
}
