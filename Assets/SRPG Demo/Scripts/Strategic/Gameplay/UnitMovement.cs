using System;
using System.Collections;
using SRPGDemo.Strategic.Map;
using UnityEngine.EventSystems;
using UnityEngine;
using SRPGDemo.Strategic.Map.Pathing;
using GridLib.Hex;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using SRPGDemo.Gameplay;

namespace SRPGDemo.Strategic.Gameplay
{
    static class AnimationExtensions
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
    }

    class TransferToBattle : StrategicGameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // Nothing for now!
            Debug.Log("Battle!");
            yield return null;
            game.ChangeState(new Idle());
        }
    }

    class UnitMoveAnimation : StrategicGameControllerAnimation
    {
        public MapUnit unit;
        public IEnumerable<HexCoords> path;

        private float timePer = 0.25f;

        public UnitMoveAnimation(MapUnit unit, IEnumerable<HexCoords> path)
        {
            this.unit = unit;
            this.path = path;
        }

        public override IEnumerator AnimationCoroutine()
        {
            yield return null;

            HexCoords oldLoc = map.WhereIs(unit);

            foreach (HexCoords newLoc in path)
            {
                yield return unit.AnimateLinearMove(
                    oldLoc,
                    newLoc,
                    timePer);

                // Move unit to be in the new cell
                map.UnplaceUnit(unit);
                map.PlaceUnit(unit, newLoc);

                // Update FoW
                map[newLoc].fog = CellFogOfWar.clear;

                // Check for random battles
                if (Random.Range(0.0f, 1.0f) > 0.75f)
                {
                    // If random battle, move to battle
                    game.ChangeState(new TransferToBattle());
                }

                // If no random battle, go to next step.
                oldLoc = newLoc;
            }

            // If we got to the end of our move, go back to idle.
            game.ChangeState(new Idle());
        }
    }

    class UnitMovement : StrategicGameControllerState
    {
        MapUnit selectedUnit;
        public UnitMovement(MapUnit selectedUnit)
        {
            this.selectedUnit = selectedUnit;
        }

        public override void EnterState()
        {
            map.events.pointerClick += OnClick;

            game.mouseDown += MouseDown;
        }

        private void OnClick(PointerEventData eventData, GameObject child)
        {
            IEnumerable<HexCoords> path = selectedUnit.AStar(child.GetComponent<MapCell>().loc);

            // on left-click, move unit
            game.ChangeState(new UnitMoveAnimation(selectedUnit, path));
        }

        private void MouseDown(PointerEventData.InputButton button)
        {
            // on right-click, back out
            if (button == PointerEventData.InputButton.Right)
                game.ChangeState(new Idle());
        }

        // draw path to cursor - particles?
    }

    class RandomBattleAnimation : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            yield return null;
        }
    }
}
