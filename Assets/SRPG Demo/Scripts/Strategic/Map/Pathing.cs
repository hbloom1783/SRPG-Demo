using System.Collections.Generic;
using System.Linq;
using GridLib.Hex;
using SRPGDemo.Utility;
using System;
using UnityEngine;

namespace SRPGDemo.Strategic.Map.Pathing
{
    public class PathNode : IComparable<PathNode>
    {
        public int costToNode;
        public int estToGoal;
        public PathNode previousNode;
        public HexCoords loc;

        public PathNode(int costToNode, PathNode previousNode, HexCoords loc)
        {
            this.costToNode = costToNode;
            this.previousNode = previousNode;
            this.loc = loc;
        }

        public PathNode(int costToNode, int estToGoal, PathNode previousNode, HexCoords loc)
        {
            this.costToNode = costToNode;
            this.estToGoal = estToGoal;
            this.previousNode = previousNode;
            this.loc = loc;
        }

        public IEnumerable<HexCoords> pathBack
        {
            get
            {
                for (PathNode cursor = this; cursor != null; cursor = cursor.previousNode)
                    yield return cursor.loc;
            }
        }

        public IEnumerable<HexCoords> pathTo { get { return pathBack.Reverse(); } }

        public int heuristic { get { return (estToGoal < int.MaxValue - costToNode) ? (costToNode + estToGoal) : (int.MaxValue); } }

        public int CompareTo(PathNode other)
        {
            return other.heuristic - heuristic;
        }

        public override string ToString()
        {
            return "PathNode: " + loc + " (c" + costToNode + ", e" + estToGoal + ")";
        }
    }

    internal static class PathingExt
    {
        private static MapController map { get { return MapController.instance; } }

        #region CanEnter

        public static bool CanEnter(
            this MapUnit unit,
            MapCell cell)
        {
            return cell.isFilled;
        }

        public static bool CanEnter(
            this MapUnit unit,
            HexCoords loc)
        {
            if (!map.InBounds(loc)) return false;
            else return unit.CanEnter(map[loc]);
        }

        #endregion

        #region CanStay

        public static bool CanStay(
            this MapUnit unit,
            MapCell cell)
        {
            return cell.isFilled;
        }

        public static bool CanStay(
            this MapUnit unit,
            HexCoords loc)
        {
            if (!map.InBounds(loc)) return false;
            else return unit.CanStay(map[loc]);
        }

        #endregion

        #region CanLeave

        public static bool CanLeave(
            this MapUnit unit,
            MapCell cell)
        {
            return cell.isFilled;
        }

        public static bool CanLeave(
            this MapUnit unit,
            HexCoords loc)
        {
            if (!map.InBounds(loc)) return false;
            else return unit.CanLeave(map[loc]);
        }

        #endregion

        #region Cost/Heuristic functions

        public static int CostToEnter(
            this MapUnit unit,
            MapCell cell)
        {
            if (unit.CanEnter(cell)) return 1;
            else return 10000;
        }

        public static int CostToEnter(
            this MapUnit unit,
            HexCoords loc)
        {
            if (!map.InBounds(loc)) return 10000;
            else return unit.CostToEnter(map[loc]);
        }

        public static int Heuristic(
            this MapUnit unit,
            MapCell src,
            MapCell dst)
        {
            return unit.Heuristic(src.loc, dst.loc);
        }

        public static int Heuristic(
            this MapUnit unit,
            HexCoords src,
            HexCoords dst)
        {
            try
            {
                return src.LineTo(dst)
                    .Skip(1)
                    .Sum(unit.CostToEnter);
            }
            catch (OverflowException)
            {
                return int.MaxValue;
            }
        }

        public static bool IsRedundant(
            this IDictionary<HexCoords, PathNode> dict,
            PathNode candidate)
        {
            if (!dict.Keys.Contains(candidate.loc)) return false;
            else if (dict[candidate.loc].costToNode <= candidate.costToNode) return true;
            else return false;
        }

        #endregion

        #region Algorithms

        public static IDictionary<HexCoords, PathNode> Dijkstra(this MapUnit unit, int maxCost = int.MaxValue)
        {
            Dictionary<HexCoords, PathNode> result = new Dictionary<HexCoords, PathNode>();

            Queue<PathNode> frontier = new Queue<PathNode>();
            frontier.Enqueue(new PathNode(
                0,
                null,
                unit.loc));

            while (frontier.Count > 0)
            {
                PathNode current = frontier.Dequeue();

                // If we already have a path to this location that's at least this cheap,
                // don't repeat it
                if (!result.IsRedundant(current))
                {
                    // If we can stay in the current location, mark it down as a destination
                    result[current.loc] = current;

                    // If we can leave the current location, try entering neighboring locations
                    int movesRemaining = maxCost - current.costToNode;
                    if ((movesRemaining > 0) && unit.CanLeave(current.loc))
                    {
                        // If we can enter any neighboring locations, try to do so later.
                        IEnumerable<MapCell> neighborCells = current.loc.neighbors
                            .Where(map.InBounds)
                            .Select(map.CellAt)
                            .Where(unit.CanEnter);
                        var q = neighborCells.ToList();
                        foreach (MapCell neighbor in neighborCells)
                            frontier.Enqueue(new PathNode(
                                current.costToNode + 1,
                                current,
                                neighbor.loc));
                    }
                }
            }

            return result;
        }

        public static IEnumerable<HexCoords> AStar(this MapUnit unit, HexCoords dst, int maxCost = int.MaxValue)
        {
            Dictionary<HexCoords, PathNode> result = new Dictionary<HexCoords, PathNode>();

            PriorityQueue<PathNode> frontier = new PriorityQueue<PathNode>();
            frontier.Enqueue(new PathNode(
                0,
                unit.Heuristic(unit.loc, dst),
                null,
                unit.loc));

            while (frontier.Count > 0)
            {
                PathNode current = frontier.Dequeue();

                // If we've arrived
                if (current.loc == dst) return current.pathTo;

                // If we already have a path to this location that's at least this cheap,
                // don't repeat it
                if (!result.IsRedundant(current))
                {
                    // If we can stay in the current location, mark it down as a destination
                    result[current.loc] = current;

                    // If we can leave the current location, try entering neighboring locations
                    int movesRemaining = maxCost - current.costToNode;
                    if ((movesRemaining > 0) && unit.CanLeave(current.loc))
                    {
                        // If we can enter any neighboring locations, try to do so later.
                        IEnumerable<MapCell> neighborCells = current.loc.neighbors
                            .Where(map.InBounds)
                            .Select(map.CellAt)
                            .Where(unit.CanEnter);
                        foreach (MapCell neighbor in neighborCells)
                            frontier.Enqueue(new PathNode(
                                current.costToNode + 1,
                                current,
                                neighbor.loc));
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
