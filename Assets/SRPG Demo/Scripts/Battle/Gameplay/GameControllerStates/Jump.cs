using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Battle.Map;
using SRPGDemo.Battle.GUI;
using SRPGDemo.Extensions;

namespace SRPGDemo.Battle.Gameplay
{
    using Extensions;

    public static class JumpHelpers
    {
        public static bool CanJumpOn(this MapUnit unit, MapUnit other)
        {
            if (other == null)
                return false;
            else if (unit.team == other.team)
                return false;
            else
                return unit.GetLandingArea(other).Where(unit.CanStayIn).Count() > 0;
        }
    }

    class JumpChooseTarget : GameControllerState
    {
        #region State implementation

        public override void EnterState()
        {
            List<PointyHexPoint> jumpArea = map.Map
                .GetCircle(map.WhereIs(unit), 1, unit.jumpRange)
                .Where(map.InBounds)
                .ToList();

            jumpMoveArea = jumpArea
                .Where(x => unit.CanStayIn(map.CellAt(x)))
                .ToList();

            jumpAttackArea = jumpArea
                .Where(x => unit.CanJumpOn(map.UnitAt(x)))
                .ToList();

            MapHighlight.ClearMap();
            MapHighlight.ShimmerRange(jumpMoveArea);
            MapHighlight.ShimmerRange(jumpAttackArea, Color.green.Mix(Color.white));
        }

        public override void LeaveState()
        {
            MapHighlight.ClearMap();
        }

        #endregion

        #region Input handling

        MapUnit unit;

        public JumpChooseTarget(MapUnit unit)
        {
            this.unit = unit;
        }

        private List<PointyHexPoint> jumpMoveArea;
        private List<PointyHexPoint> jumpAttackArea;

        private PointyHexPoint? selectedLoc = null;
        private bool isAttack = false;

        public override void HexTouchedHandler(PointyHexPoint? loc)
        {
            MapHighlight.ClearMap();
            MapHighlight.ShimmerRange(jumpMoveArea);
            MapHighlight.ShimmerRange(jumpAttackArea, Color.green.Mix(Color.white));

            if (loc.HasValue)
            {
                if (jumpMoveArea.Contains(loc.Value))
                {
                    selectedLoc = loc;
                    isAttack = false;
                }
                else if (jumpAttackArea.Contains(loc.Value))
                {
                    selectedLoc = loc;
                    isAttack = true;
                }

                if (selectedLoc.HasValue)
                    map.CellAt(selectedLoc.Value).SetTint(Color.green);
            }
            else
            {
                selectedLoc = null;
            }
        }

        public override void LeftMouseHandler(bool state)
        {
            if (state && selectedLoc.HasValue)
            {
                if (isAttack)
                    game.ChangeState(new JumpChooseLanding(
                        unit,
                        map.UnitAt(selectedLoc.Value)));
                else
                    game.ChangeState(new JumpMove(
                        unit,
                        selectedLoc.Value));
            }
        }

        public override void RightMouseHandler(bool state)
        {
            if (state) game.ChangeState(new PlayerGiveOrder(unit));
        }

        public override void GuiSignalHandler(GuiID signal)
        {
        }

        #endregion
    }

    class JumpChooseLanding : GameControllerState
    {
        #region State implementation

        private MapUnit unit;
        private MapUnit target;
        private IEnumerable<PointyHexPoint> landingArea;

        public JumpChooseLanding(MapUnit unit, MapUnit target)
        {
            this.unit = unit;
            this.target = target;
            landingArea = unit.GetLandingArea(target).Where(unit.CanStayIn);
        }

        public override void EnterState()
        {
            MapHighlight.ClearMap();
            MapHighlight.ShimmerRange(landingArea);
        }

        public override void LeaveState()
        {
            MapHighlight.ClearMap();
        }

        #endregion

        #region Input handling

        PointyHexPoint? selectedLoc = null;

        public override void HexTouchedHandler(PointyHexPoint? loc)
        {
            MapHighlight.ClearMap();
            MapHighlight.ShimmerRange(landingArea);

            if (loc.HasValue && landingArea.Contains(loc.Value))
            {
                selectedLoc = loc;
                map.CellAt(selectedLoc.Value).SetTint(Color.green);
            }
            else
            {
                selectedLoc = null;
            }
        }

        public override void LeftMouseHandler(bool state)
        {
            if (state && selectedLoc.HasValue)
            {
                game.ChangeState(new JumpAttack(
                    unit, target, selectedLoc.Value));
            }
        }

        public override void RightMouseHandler(bool state)
        {
            if (state) game.ChangeState(new JumpChooseTarget(unit));
        }

        public override void GuiSignalHandler(GuiID signal)
        {
        }

        #endregion
    }

    class JumpMove : GameControllerAnimation
    {
        private MapUnit unit;
        private PointyHexPoint newLoc;

        public JumpMove(
            MapUnit unit,
            PointyHexPoint newLoc)
        {
            this.unit = unit;
            this.newLoc = newLoc;
        }

        public override IEnumerator AnimationCoroutine()
        {
            game.PlaySound(unit.jumpSound);

            PointyHexPoint oldLoc = map.WhereIs(unit);

            yield return unit.AnimateParabolicMove(
                oldLoc,
                newLoc);

            map.UnplaceUnit(unit);
            map.PlaceUnit(unit, map.mapGrid[newLoc]);
            unit.ap.Increment(-1);

            yield return null;

            if (unit.team == UnitTeam.player)
                game.ChangeState(new PlayerGiveOrder(unit));
            else if (unit.team == UnitTeam.enemy)
                game.ChangeState(new EnemyTurnAI());
        }
    }

    class JumpAttack : GameControllerAnimation
    {
        private MapUnit unit;
        private MapUnit target;
        private PointyHexPoint landingLoc;

        public JumpAttack(
            MapUnit unit,
            MapUnit target,
            PointyHexPoint landingLoc)
        {
            this.unit = unit;
            this.target = target;
            this.landingLoc = landingLoc;
        }

        public override IEnumerator AnimationCoroutine()
        {
            game.PlaySound(unit.jumpSound);

            PointyHexPoint oldLoc = map.WhereIs(unit);
            PointyHexPoint targetLoc = map.WhereIs(target);
            
            yield return unit.AnimateParabolicMove(
                oldLoc,
                targetLoc);

            game.PlaySound(unit.attackSound);
            target.TakeDamage(1);

            yield return unit.AnimateParabolicMove(
                targetLoc,
                landingLoc,
                1.0f,
                1.0f);

            map.UnplaceUnit(unit);
            map.PlaceUnit(unit, map.mapGrid[landingLoc]);
            unit.ap.Increment(-1);

            yield return null;

            if (unit.team == UnitTeam.player)
                game.ChangeState(new PlayerGiveOrder(unit));
            else if (unit.team == UnitTeam.enemy)
                game.ChangeState(new EnemyTurnAI());
        }
    }
}
