using SRPGDemo.Battle.Map;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using GridLib.Hex;
using SRPGDemo.Battle.Gameplay.Extensions;

namespace SRPGDemo.Battle.Gameplay
{
    class MoveUnit : GameControllerAnimation
    {
        #region Unit movement

        private MapUnit unit;
        private IEnumerable<HexCoords> path;

        private float timePer = 0.25f;

        public MoveUnit(
            MapUnit unit,
            IEnumerable<HexCoords> path)
        {
            this.unit = unit;
            this.path = path;
        }

        public override IEnumerator AnimationCoroutine()
        {
            game.PlaySound(unit.runSound);

            HexCoords oldLoc = map.WhereIs(unit);

            foreach (HexCoords newLoc in path)
            {
                yield return unit.AnimateLinearMove(
                    oldLoc,
                    newLoc,
                    timePer);

                map.UnplaceUnit(unit);
                map.PlaceUnit(unit, newLoc);
                oldLoc = newLoc;
            }

            unit.ap.Increment(-1);

            yield return null;

            List<MapUnit> reactionList = new List<MapUnit>(
                map[oldLoc].threatSet.Where(x => x.team != unit.team));

            foreach (MapUnit enemyUnit in reactionList)
            {
                unit.TakeDamage(1);
                yield return enemyUnit.AnimateMeleeAttack(unit);
            }

            if (map[oldLoc].threatSet.Any())
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
