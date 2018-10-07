using Gamelogic.Grids;
using SRPGDemo.Battle.Map;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SRPGDemo.Battle.Gameplay
{
    using Extensions;

    class MoveUnit : GameControllerAnimation
    {
        #region Unit movement

        private MapUnit unit;
        private IEnumerable<PointyHexPoint> path;

        private float timePer = 0.25f;

        public MoveUnit(
            MapUnit unit,
            IEnumerable<PointyHexPoint> path)
        {
            this.unit = unit;
            this.path = path;
        }

        public override IEnumerator AnimationCoroutine()
        {
            game.PlaySound(unit.runSound);

            PointyHexPoint oldLoc = map.WhereIs(unit);

            foreach (PointyHexPoint newLoc in path)
            {
                yield return unit.AnimateLinearMove(
                    oldLoc,
                    newLoc,
                    timePer);

                oldLoc = newLoc;
            }

            map.UnplaceUnit(unit);
            map.PlaceUnit(unit, map.mapGrid[oldLoc]);
            unit.ap.Increment(-1);

            yield return null;

            List<MapUnit> reactionList = new List<MapUnit>(
                map.CellAt(oldLoc).threatList.Where(x => x.team != unit.team));

            foreach (MapUnit enemyUnit in reactionList)
            {
                unit.TakeDamage(1);
                yield return enemyUnit.AnimateMeleeAttack(unit);
            }

            if (map.CellAt(oldLoc).threatList.Any())
            {
                yield return null;
            }

            if (unit.team == UnitTeam.player)
                game.ChangeState(new PlayerGiveOrder(unit));
            else if (unit.team == UnitTeam.enemy)
                game.ChangeState(new EnemyTurnAI());
        }

        #endregion
    }
}
