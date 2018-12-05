using System.Collections;
using SRPGDemo.Battle.Map;
using SRPGDemo.Battle.Gameplay.Extensions;

namespace SRPGDemo.Battle.Gameplay
{
    class MeleeAttack : GameControllerAnimation
    {
        private MapUnit unit;
        private MapUnit target;

        public MeleeAttack(MapUnit unit, MapUnit target)
        {
            this.unit = unit;
            this.target = target;
        }

        public override IEnumerator AnimationCoroutine()
        {
            yield return unit.AnimateMeleeAttack(target);

            unit.ap.Increment(-1);
            target.TakeDamage(1);

            if (unit.team == UnitTeam.player)
                game.ChangeState(new PlayerGiveOrder(unit));
            else
                game.ChangeState(new EnemyTurnAI());
        }
    }
}
