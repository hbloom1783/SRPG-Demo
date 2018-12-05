using System.Collections;
using UnityEngine;
using SRPGDemo.Battle.Map;
using GridLib.Hex;

namespace SRPGDemo.Battle.Gameplay.Extensions
{
    public static class UnitAnimationExt
    {
        private static MapController map { get { return MapController.instance; } }
        private static GameController game { get { return GameController.instance; } }

        // Assumes a parabola crossing (0,0) (0.5,1) (1,0)
        // 4x - 4x^2
        private static float ParabolicHeight(float x)
        {
            return (4 * x) - (4 * x * x);
        }

        public static IEnumerator AnimateLinearMove(
            this MapUnit unit,
            HexCoords oldLoc,
            HexCoords newLoc,
            float moveTime)
        {
            unit.facing = oldLoc.FacingTo(newLoc);

            for (float timePassed = 0.0f;
                timePassed < moveTime;
                timePassed += Time.deltaTime)
            {
                unit.transform.position = Vector3.Lerp(
                    map[oldLoc].transform.position,
                    map[newLoc].transform.position,
                    timePassed / moveTime);

                yield return null;
            }
        }

        public static IEnumerator AnimateParabolicMove(
            this MapUnit unit,
            HexCoords oldLoc,
            HexCoords newLoc,
            float airTime,
            float peakHeight)
        {
            unit.facing = oldLoc.FacingTo(newLoc);

            for (float timePassed = 0.0f; timePassed < airTime; timePassed += Time.deltaTime)
            {
                Vector3 airPos = Vector3.Lerp(
                        map[oldLoc].transform.position,
                        map[newLoc].transform.position,
                        timePassed / airTime);

                airPos.y += ParabolicHeight(timePassed / airTime) * peakHeight;

                unit.transform.position = airPos;

                yield return null;
            }
        }

        public static IEnumerator AnimateParabolicMove(
            this MapUnit unit,
            HexCoords oldLoc,
            HexCoords newLoc)
        {
            float airTime = (float)oldLoc.DistanceTo(newLoc) / 3;
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
