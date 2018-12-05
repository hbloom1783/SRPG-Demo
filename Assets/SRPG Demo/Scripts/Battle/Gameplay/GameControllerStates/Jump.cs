using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SRPGDemo.Battle.Map;
using SRPGDemo.Battle.Map.Pathing;
using GridLib.Hex;
using UnityEngine.EventSystems;
using SRPGDemo.Battle.Gameplay.Extensions;

namespace SRPGDemo.Battle.Gameplay
{
    public static class JumpHelpers
    {
        private static MapController map { get { return MapController.instance; } }

        public static bool CanJumpOn(this MapUnit unit, MapUnit other)
        {
            if (other == null)
                return false;
            else if (unit.team == other.team)
                return false;
            else
                return unit.GetLandingArea(other)
                    .Select(map.CellAt)
                    .Where(unit.CanStay)
                    .Count() > 0;
        }
    }

    class JumpChooseTarget : ShimmerState
    {
        #region State implementation

        public override void EnterState()
        {
            List<HexCoords> jumpArea = map.WhereIs(unit)
                .CompoundRing(1, unit.jumpRange)
                .Where(map.InBounds)
                .ToList();

            jumpMoveArea = jumpArea
                .Where(unit.CanEnter)
                .Where(unit.CanStay)
                .Select(map.CellAt)
                .ToList();

            jumpAttackArea = jumpArea
                .Where(x => unit.CanJumpOn(map.UnitAt(x)))
                .Select(map.CellAt)
                .ToList();

            map.events.pointerEnter += PointerEnter;
            map.events.pointerExit += PointerExit;
            map.events.pointerClick += PointerClick;

            game.mouseDown += MouseDown;

            AddShimmer(jumpMoveArea);
            AddShimmer(jumpAttackArea);
            foreach (MapCell cell in jumpAttackArea)
                cell.tint = Color.Lerp(Color.green, Color.white, 0.5f);

            base.EnterState();
        }

        public override void LeaveState()
        {
            base.LeaveState();

            foreach (MapCell cell in jumpAttackArea)
                cell.tint = Color.white;
        }

        #endregion

        #region Input handling

        MapUnit unit;

        public JumpChooseTarget(MapUnit unit)
        {
            this.unit = unit;
        }

        private List<MapCell> jumpMoveArea;
        private List<MapCell> jumpAttackArea;

        public void PointerEnter(PointerEventData eventData, GameObject child)
        {
            MapCell cell = child.GetComponent<MapCell>();
            if (jumpMoveArea.Contains(cell) || jumpAttackArea.Contains(cell))
            {
                RemoveShimmer(cell);
                cell.tint = Color.green;
            }

        }

        public void PointerExit(PointerEventData eventData, GameObject child)
        {
            MapCell cell = child.GetComponent<MapCell>();
            if (jumpMoveArea.Contains(cell) || jumpAttackArea.Contains(cell))
            {
                AddShimmer(cell);
                if (jumpMoveArea.Contains(cell))
                    cell.tint = Color.white;
                else if (jumpAttackArea.Contains(cell))
                    cell.tint = Color.Lerp(Color.green, Color.white, 0.5f);
            }
        }

        public void PointerClick(PointerEventData eventData, GameObject child)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                MapCell cell = child.GetComponent<MapCell>();

                if (jumpMoveArea.Contains(cell))
                {
                    cell.tint = Color.white;
                    game.ChangeState(new JumpMove(
                        unit,
                        cell.loc));
                }
                else if (jumpAttackArea.Contains(cell))
                {
                    cell.tint = Color.white;
                    game.ChangeState(new JumpChooseLanding(
                        unit,
                        map.UnitAt(cell.loc)));
                }
            }
        }

        void MouseDown(PointerEventData.InputButton mb)
        {
            if (mb == PointerEventData.InputButton.Right)
            {
                game.ChangeState(new PlayerGiveOrder(unit));
            }
        }

        #endregion
    }

    class JumpChooseLanding : ShimmerState
    {
        #region State implementation

        private MapUnit unit;
        private MapUnit target;
        private IEnumerable<MapCell> landingArea;

        public JumpChooseLanding(MapUnit unit, MapUnit target)
        {
            this.unit = unit;
            this.target = target;
            landingArea = unit.GetLandingArea(target)
                .Where(unit.CanStay)
                .Select(map.CellAt);
        }

        public override void EnterState()
        {
            AddShimmer(landingArea);

            map.events.pointerEnter += PointerEnter;
            map.events.pointerExit += PointerExit;
            map.events.pointerClick += PointerClick;

            game.mouseDown += MouseDown;

            base.EnterState();
        }

        public override void LeaveState()
        {
            base.LeaveState();
        }

        #endregion

        #region Input handling

        public void PointerEnter(PointerEventData eventData, GameObject child)
        {
            MapCell cell = child.GetComponent<MapCell>();
            if (landingArea.Contains(cell))
            {
                RemoveShimmer(cell);
                cell.tint = Color.green;
            }
        }

        public void PointerExit(PointerEventData eventData, GameObject child)
        {
            MapCell cell = child.GetComponent<MapCell>();
            if (landingArea.Contains(cell))
            {
                cell.tint = Color.white;
                AddShimmer(cell);
            }
        }

        public void PointerClick(PointerEventData eventData, GameObject child)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                MapCell cell = child.GetComponent<MapCell>();

                if (landingArea.Contains(cell))
                {
                    cell.tint = Color.white;
                    game.ChangeState(new JumpAttack(
                        unit, target, cell.loc));
                }
            }
        }

        void MouseDown(PointerEventData.InputButton mb)
        {
            if (mb == PointerEventData.InputButton.Right)
            {
                game.ChangeState(new JumpChooseTarget(unit));
            }
        }

        #endregion
    }

    class JumpMove : GameControllerAnimation
    {
        private MapUnit unit;
        private HexCoords newLoc;

        public JumpMove(
            MapUnit unit,
            HexCoords newLoc)
        {
            this.unit = unit;
            this.newLoc = newLoc;
        }

        public override IEnumerator AnimationCoroutine()
        {
            game.PlaySound(unit.jumpSound);

            HexCoords oldLoc = map.WhereIs(unit);

            yield return unit.AnimateParabolicMove(
                oldLoc,
                newLoc);

            map.UnplaceUnit(unit);
            map.PlaceUnit(unit, newLoc);
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
        private HexCoords landingLoc;

        public JumpAttack(
            MapUnit unit,
            MapUnit target,
            HexCoords landingLoc)
        {
            this.unit = unit;
            this.target = target;
            this.landingLoc = landingLoc;
        }

        public override IEnumerator AnimationCoroutine()
        {
            game.PlaySound(unit.jumpSound);

            HexCoords oldLoc = map.WhereIs(unit);
            HexCoords targetLoc = map.WhereIs(target);
            
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
            map.PlaceUnit(unit, map[landingLoc]);
            unit.ap.Increment(-1);

            yield return null;

            if (unit.team == UnitTeam.player)
                game.ChangeState(new PlayerGiveOrder(unit));
            else if (unit.team == UnitTeam.enemy)
                game.ChangeState(new EnemyTurnAI());
        }
    }
}
