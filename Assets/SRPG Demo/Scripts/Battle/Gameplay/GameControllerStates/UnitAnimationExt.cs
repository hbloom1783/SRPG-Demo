using System.Collections;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Battle.Map;

namespace SRPGDemo.Battle.Gameplay.Extensions
{
    public static class UnitAnimationExt
    {
        private static MapController map { get { return Controllers.map; } }
        private static GameController game { get { return Controllers.game; } }

        // Assumes a parabola crossing (0,0) (0.5,1) (1,0)
        // 4x - 4x^2
        private static float ParabolicHeight(float x)
        {
            float x2 = x * x;
            return (4 * x) - (4 * x2);
        }

        public static IEnumerator AnimateLinearMove(
            this MapUnit unit,
            PointyHexPoint oldLoc,
            PointyHexPoint newLoc,
            float moveTime)
        {
            unit.facing = (newLoc - oldLoc).ToFacing();

            for (float timePassed = 0.0f;
                timePassed < moveTime;
                timePassed += Time.deltaTime)
            {
                unit.transform.position = Vector3.Lerp(
                    map.mapGrid[oldLoc].transform.position,
                    map.mapGrid[newLoc].transform.position,
                    timePassed / moveTime);

                yield return null;
            }
        }

        public static IEnumerator AnimateParabolicMove(
            this MapUnit unit,
            PointyHexPoint oldLoc,
            PointyHexPoint newLoc,
            float airTime,
            float peakHeight)
        {
            unit.Face(newLoc);

            for (float timePassed = 0.0f; timePassed < airTime; timePassed += Time.deltaTime)
            {
                Vector3 airPos = Vector3.Lerp(
                        map.mapGrid[oldLoc].transform.position,
                        map.mapGrid[newLoc].transform.position,
                        timePassed / airTime);

                airPos.y += ParabolicHeight(timePassed / airTime) * peakHeight;

                unit.transform.position = airPos;

                yield return null;
            }
        }

        public static IEnumerator AnimateParabolicMove(
            this MapUnit unit,
            PointyHexPoint oldLoc,
            PointyHexPoint newLoc)
        {
            float airTime = (float)oldLoc.DistanceFrom(newLoc) / 3;
            float peakHeight = airTime * 0.66f;
            return AnimateParabolicMove(unit, oldLoc, newLoc, airTime, peakHeight);
        }

        public static IEnumerator AnimateMeleeAttack(
            this MapUnit unit,
            MapUnit target)
        {
            unit.Face(target);
            game.PlaySound(unit.attackSound);
            yield return new WaitWhile(game.IsPlayingSound);
        }
    }
}
