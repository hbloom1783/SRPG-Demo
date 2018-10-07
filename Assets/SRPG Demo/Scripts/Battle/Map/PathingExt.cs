using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grids;
using SRPGDemo.Extensions;

namespace SRPGDemo.Battle.Map
{
    public enum PassageType
    {
        open,
        canPassThru,
        inThreat,
        blocked,
    }

    public class PathNode
    {
        public PointyHexPoint loc = PointyHexPoint.Zero;
        public PathNode previous = null;
        public int stepsLeft = 0;
        public PassageType passage = PassageType.open;

        public PathNode(PointyHexPoint loc, PathNode previous, int stepsLeft, PassageType passage)
        {
            this.loc = loc;
            this.previous = previous;
            this.stepsLeft = stepsLeft;
            this.passage = passage;
        }

        public bool LocsMatch(PathNode other)
        {
            return loc.DistanceFrom(other.loc) == 0;
        }

        public bool LocsMatch(PointyHexPoint other)
        {
            return loc.DistanceFrom(other) == 0;
        }

        public IEnumerable<PathNode> PathTo()
        {
            List<PathNode> result = new List<PathNode>();

            // Each ancestor goes before its children
            for (PathNode ancestor = previous; ancestor != null; ancestor = ancestor.previous)
                result.Insert(0, ancestor);

            // This node is last
            result.Add(this);

            return result;
        }
    }

    public static class PathingExt
    {
        #region MapUnit predicates

        private static bool IsEnemy(this MapUnit unit, MapUnit other)
        {
            return (unit.team != other.team);
        }

        private static bool IsAlly(this MapUnit unit, MapUnit other)
        {
            return (unit.team == other.team);
        }

        private static bool IsMe(this MapUnit unit, MapUnit other)
        {
            return (unit == other);
        }

        private static bool IsntMe(this MapUnit unit, MapUnit other)
        {
            return (unit != other);
        }

        public static bool CanStayIn(this MapUnit unit, MapCell cell)
        {
            switch (unit.EvaluatePassage(cell))
            {
                case PassageType.open:
                case PassageType.inThreat:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanStayIn(this MapUnit unit, PointyHexPoint loc)
        {
            return unit.CanStayIn(Controllers.map.cache.CellAt(loc));
        }

        public static bool CanLeaveFrom(this MapUnit unit, MapCell cell)
        {
            switch (unit.EvaluatePassage(cell))
            {
                case PassageType.open:
                case PassageType.canPassThru:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanLeaveFrom(this MapUnit unit, PointyHexPoint loc)
        {
            return unit.CanLeaveFrom(Controllers.map.cache.CellAt(loc));
        }

        #endregion
        
        #region Passage evaluation

        private static PassageType ApplyThreat(this PassageType passage, bool isThreatened)
        {
            if (isThreatened)
            {
                switch (passage)
                {
                    default:
                    case PassageType.inThreat:
                    case PassageType.open:
                        return PassageType.inThreat;

                    case PassageType.canPassThru:
                    case PassageType.blocked:
                        return PassageType.blocked;
                }
            }
            else
            {
                return passage;
            }
        }

        private static PassageType EvaluatePassageSmall(this MapUnit unit, MapCell cell)
        {
            // Check for obvious blockage
            if (!cell.isWalkable)
                return PassageType.blocked;
            else
            {
                // Determine threat status
                bool isThreatened = cell.threatList.Any(unit.IsEnemy);

                // Check for units in target cell
                if (cell.unitPresent != null)
                {
                    // If unit present is small friendly, then we can pass through
                    if ((cell.unitPresent.size == UnitSize.small) &&
                        (cell.unitPresent.team == unit.team))
                        return PassageType.canPassThru.ApplyThreat(isThreatened);
                    // Otherwise, blocked
                    else
                        return PassageType.blocked;
                }
                // No units in target cell
                else
                {
                    // Check for large units in neighbor cells
                    IEnumerable<MapUnit> largeNeighbors = cell.neighborUnitList
                        .Where(unit.IsntMe)
                        .Where(x => x.size == UnitSize.large);
                    if (largeNeighbors.Any())
                    {
                        // If all large neighbors are friendly, then we can pass through
                        if (largeNeighbors.All(unit.IsAlly))
                            return PassageType.canPassThru.ApplyThreat(isThreatened);
                        // Otherwise, blocked
                        else
                            return PassageType.blocked;
                    }
                    // No large units in neighbor cells
                    else
                    {
                        return PassageType.open.ApplyThreat(isThreatened);
                    }
                }
            }
        }

        private static PassageType EvaluatePassageLarge(this MapUnit unit, MapCell cell)
        {
            // Check for obvious blockage
            if (!cell.isWalkable || cell.hasBlockingNeighbors)
                return PassageType.blocked;
            else
            {
                // Determine threat status
                bool isThreatened = cell.threatList.Any(unit.IsEnemy);

                // If any unit is present in target cell, blocked
                if (cell.unitPresent != null)
                    return PassageType.blocked;
                // No units in target cell
                else
                {
                    // Check for units in neighbor cells
                    IEnumerable<MapUnit> neighbors = cell.neighborUnitList
                        .Where(unit.IsntMe);
                    if (neighbors.Any())
                    {
                        // If any neighbors are large, blocked
                        if (neighbors.Any(x => x.size == UnitSize.large))
                            return PassageType.blocked;
                        // If any neighbors are enemies, blocked
                        else if (neighbors.Any(unit.IsEnemy))
                            return PassageType.blocked;
                        // Otherwise, we can pass through
                        else
                            return PassageType.canPassThru.ApplyThreat(isThreatened);
                    }
                    // No units in neighbor cells
                    else
                    {
                        return PassageType.open.ApplyThreat(isThreatened);
                    }
                }
            }
        }

        public static PassageType EvaluatePassage(this MapUnit unit, MapCell cell)
        {
            switch(unit.size)
            {
                case UnitSize.large:
                    return unit.EvaluatePassageLarge(cell);

                default:
                case UnitSize.small:
                    return unit.EvaluatePassageSmall(cell);
            }
        }

        public static PassageType EvaluatePassage(this MapUnit unit, PointyHexPoint loc)
        {
            return unit.EvaluatePassage(Controllers.map.cache.CellAt(loc));
        }

        #endregion

        #region Step Map

        private static Dictionary<PointyHexPoint, int> stepMap = null;

        private static bool CheckStepMap(PointyHexPoint loc, int stepsLeft)
        {
            if (!stepMap.Keys.Contains(loc))
                return true;
            else if (stepMap[loc] < stepsLeft)
                return true;
            else
                return false;
        }

        private static void MarkStepMap(PointyHexPoint loc, int stepsLeft)
        {
            stepMap[loc] = stepsLeft;
        }

        private static void ResetStepMap(MapUnit unit)
        {
            stepMap = new Dictionary<PointyHexPoint, int>();
            stepMap[unit.loc] = unit.moveRange;
        }

        #endregion

        #region Pathfinding algorithm

        private static MapController map { get { return Controllers.map; } }

        public static Dictionary<PointyHexPoint, PathNode> PathFlood(this MapUnit unit)
        {
            Dictionary<PointyHexPoint, PathNode> result = new Dictionary<PointyHexPoint, PathNode>();

            ResetStepMap(unit);

            foreach (PathNode node in Dijkstra(
                unit,
                Controllers.map.cache.WhereIs(unit),
                unit.moveRange))
            {
                result[node.loc] = node;
            }

            return result;
        }

        private static IEnumerable<PathNode> Dijkstra(
            MapUnit unit,
            PointyHexPoint loc,
            int stepsLeft,
            PathNode parent = null)
        {
            List<PathNode> result = new List<PathNode>();

            // For each viable neighbor
            foreach (PointyHexPoint neighbor in loc.GetNeighbors().Where(Controllers.map.cache.InBounds))
            {
                // Only go in if we haven't already been there.
                if (CheckStepMap(neighbor, stepsLeft - 1))
                {
                    // Note that we've been here.
                    MarkStepMap(neighbor, stepsLeft - 1);

                    // Form a node.
                    PathNode newNode = new PathNode(
                        neighbor,
                        parent,
                        stepsLeft - 1,
                        unit.EvaluatePassage(map.CellAt(neighbor)));

                    // If we can stand there, add it to the path
                    if (unit.CanStayIn(map.CellAt(neighbor)))
                    {
                        result.Add(newNode);
                    }

                    // If we can leave there, pathfind from it
                    if (unit.CanLeaveFrom(map.CellAt(neighbor)) &&
                        (newNode.stepsLeft > 0))
                    {
                        // Collect recursion
                        List<PathNode> subResult = new List<PathNode>(Dijkstra(
                            unit,
                            neighbor,
                            stepsLeft - 1,
                            newNode));

                        // Reference once
                        foreach (PathNode subNode in subResult)
                        {
                            result.Add(subNode);
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
