using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grids;
using SRPGDemo.Extensions;

namespace SRPGDemo.Map
{
    public enum PassageType
    {
        open,
        canPass,
        inThreat,
        blocked,
        notWalkable,
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

        public IEnumerable<PathNode> PathBack()
        {
            List<PathNode> result = new List<PathNode>();

            PathNode ancestor = previous;

            while (ancestor != null)
            {
                result.Add(ancestor);
                ancestor = ancestor.previous;
            }

            return result;
        }
    }

    public static class PathingExtensions
    {
        private static bool IsEnemy(this MapMobile mob, MapMobile other)
        {
            return (mob.team != other.team);
        }

        private static bool IsAlly(this MapMobile mob, MapMobile other)
        {
            return (mob.team == other.team);
        }

        public static PassageType EvaluatePassage(this MapMobile mob, PassageNode node)
        {
            if (!node.cell.walkable)
                return PassageType.notWalkable;
            else if (node.blocked || (node.passList.Any(mob.IsEnemy)) || (node.passList.Any() && node.threatList.Any(mob.IsEnemy)))
                return PassageType.blocked;
            else if (node.threatList.Any(mob.IsEnemy))
                return PassageType.inThreat;
            else if (node.passList.Any() && node.passList.All(mob.IsAlly))
                return PassageType.canPass;
            else
                return PassageType.open;
        }

        public static bool CanStayIn(this MapMobile mob, PassageNode node)
        {
            switch(mob.EvaluatePassage(node))
            {
                case PassageType.open:
                case PassageType.inThreat:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanLeaveFrom(this MapMobile mob, PassageNode node)
        {
            switch (mob.EvaluatePassage(node))
            {
                case PassageType.open:
                case PassageType.canPass:
                    return true;
                default:
                    return false;
            }
        }

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
        
        public static IGrid<PassageNode, PointyHexPoint> PassageMap(this MapController map, MapMobile moving)
        {
            IGrid<PassageNode, PointyHexPoint> result = map.Grid.CloneStructure<PassageNode>();

            // Clear map to fully open
            result.ToList().ForEach(x => result[x] = new PassageNode(map.mapGrid[x]));

            // Handle mobiles
            foreach (MapMobile standing in map.Mobiles().Where(x => x != moving))
            {
                PointyHexPoint mapMobLoc = map.WhereIs(standing);

                // Hard block
                if (moving is LargeMobile && standing is LargeMobile)
                {
                    map.Map.GetCircle(mapMobLoc, 0, 1)
                        .Where(map.InBounds).ToList()
                        .ForEach(x => result[x].blocked = true);
                }
                else if (moving is LargeMobile || standing is LargeMobile)
                {
                    result[mapMobLoc].blocked = true;
                }

                // Soft block
                if (moving is SmallMobile && standing is SmallMobile)
                {
                    result[mapMobLoc].passList.Add(standing);
                }
                else if (moving is SmallMobile || standing is SmallMobile)
                {
                    map.Map.GetCircle(mapMobLoc, 1, 1)
                        .Where(map.InBounds).ToList()
                        .ForEach(x => result[x].passList.Add(standing));
                }

                // Threat
                if ((standing.team != moving.team) && (standing.engaged == false))
                {
                    standing.ThreatArea().ToList()
                        .ForEach(x => result[x].threatList.Add(standing));
                }
            }

            // Block anything blocked by terrain
            map.mapGrid.Where(x => map.mapGrid[x].walkable == false).ToList()
                .ForEach(x => result[x].blocked = true);

            return result;
        }

        public static Dictionary<PointyHexPoint, PathNode> PathFlood(this MapMobile mob)
        {
            Dictionary<PointyHexPoint, PathNode> result = new Dictionary<PointyHexPoint, PathNode>();

            stepMap = new Dictionary<PointyHexPoint, int>();
            stepMap[Controllers.map.WhereIs(mob)] = mob.moveSpeed;

            foreach (PathNode node in Dijkstra(
                Controllers.map.PassageMap(mob),
                mob,
                Controllers.map.WhereIs(mob),
                mob.moveSpeed))
            {
                result[node.loc] = node;
            }

            return result;
        }

        private static IEnumerable<PathNode> Dijkstra(
            IGrid<PassageNode, PointyHexPoint> passageMap,
            MapMobile mob,
            PointyHexPoint loc,
            int stepsLeft,
            PathNode parent = null)
        {
            List<PathNode> result = new List<PathNode>();

            // For each viable neighbor
            foreach (PointyHexPoint neighbor in loc.GetNeighbors().Where(Controllers.map.InBounds))
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
                        mob.EvaluatePassage(passageMap[neighbor]));

                    // If we can stand there, add it to the path
                    if (mob.CanStayIn(passageMap[neighbor]))
                    {
                        result.Add(newNode);
                    }

                    // If we can leave there, pathfind from it
                    if (mob.CanLeaveFrom(passageMap[neighbor]) &&
                        (newNode.stepsLeft > 0))
                    {
                        // Collect recursion
                        List<PathNode> subResult = new List<PathNode>(Dijkstra(
                            passageMap,
                            mob,
                            neighbor,
                            stepsLeft - 1,
                            newNode));

                        // Reference once
                        PointyHexPoint mobLoc = Controllers.map.WhereIs(mob);

                        foreach (PathNode subNode in subResult)
                        {
                            result.Add(subNode);
                        }
                    }
                }
            }

            return result;
        }
    }
}
