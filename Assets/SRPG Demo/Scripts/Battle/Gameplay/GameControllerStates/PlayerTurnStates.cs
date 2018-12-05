using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SRPGDemo.Battle.Map;
using SRPGDemo.Battle.Map.Pathing;
using SRPGDemo.Battle.UI;
using SRPGDemo.Extensions;
using GridLib.Hex;
using UnityEngine.EventSystems;

namespace SRPGDemo.Battle.Gameplay
{
    static class PlayerExt
    {
        public static IEnumerator PlayerLoses()
        {
            yield return null;
            GameController.instance.ChangeState(new EnemyWins());
        }
    }

    class BeginPlayerTurn : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // If the player is out of units, defeat them
            if (map.Units(UnitTeam.player).Count() <= 0)
            {
                game.StartCoroutine(PlayerExt.PlayerLoses());
            }
            else
            {
                // Display marquee
                ui.marqueeText.Activate();
                ui.marqueeText.text = "Player Turn";
                ui.marqueeText.color = Color.white.Alpha(0.0f);

                for (float timePassed = 0.0f; timePassed < 1.0f; timePassed += Time.deltaTime)
                {
                    ui.marqueeText.color = Color.Lerp(Color.white.Alpha(0.0f), Color.blue, timePassed / 1.0f);
                    yield return null;
                }

                for (float timePassed = 0.0f; timePassed < 1.0f; timePassed += Time.deltaTime)
                {
                    ui.marqueeText.color = Color.Lerp(Color.blue, Color.blue.Alpha(0.0f), timePassed / 1.0f);
                    yield return null;
                }

                // Clear marquee
                ui.marqueeText.Deactivate();

                // Remove player threat
                foreach (MapUnit unit in map.Units(UnitTeam.player))
                    foreach (MapCell cell in unit.GetThreatArea().Select(map.CellAt))
                        cell.RemoveThreat(unit);

                // Begin player turn
                game.ChangeState(new PlayerSelectUnit());
            }
        }
    }

    class PlayerSelectUnit : ShimmerState
    {
        #region Map highlighting

        IEnumerable<MapCell> playerUnitCells = null;

        #endregion

        #region State implementation

        public override void EnterState()
        {
            // If the player is out of units, defeat them
            if (map.Units(UnitTeam.player).Count() <= 0)
            {
                game.StartCoroutine(PlayerExt.PlayerLoses());
            }
            else
            {
                playerUnitCells = map.Units()
                    .Where(x => (x.team == UnitTeam.player) && (x.ap > 0))
                    .Select(map.UnitCell);

                AddShimmer(playerUnitCells);

                map.events.pointerEnter += PointerEnter;
                map.events.pointerExit += PointerExit;
                map.events.pointerClick += PointerClick;

                ui.signal += GuiSignal;

                ui.endTurnButton.Activate();
            }

            base.EnterState();
        }

        public override void LeaveState()
        {
            ui.endTurnButton.Deactivate();

            foreach (MapCell cell in playerUnitCells)
                cell.tint = Color.white;

            base.LeaveState();
        }

        #endregion

        #region Input handling

        void PointerEnter(PointerEventData eventData, GameObject child)
        {
            MapCell cell = child.GetComponent<MapCell>();

            if (playerUnitCells.Contains(cell))
            {
                RemoveShimmer(cell);
                cell.tint = Color.green;
            }
        }

        void PointerExit(PointerEventData eventData, GameObject child)
        {
            MapCell cell = child.GetComponent<MapCell>();

            if (playerUnitCells.Contains(cell))
            {
                AddShimmer(cell);
                cell.tint = Color.white;
            }
        }

        void PointerClick(PointerEventData eventData, GameObject child)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                MapCell cell = child.GetComponent<MapCell>();

                if (playerUnitCells.Contains(cell))
                    game.ChangeState(new PlayerGiveOrder(
                        cell.unitPresent));
            }
        }

        void GuiSignal(GameObject child)
        {
            if (child.GetComponent<GuiButton>() == ui.endTurnButton)
            {
                game.ChangeState(new EndPlayerTurn());
            }
        }

        #endregion
    }

    class PlayerGiveOrder : ShimmerState
    {
        #region Map highlighting

        MapUnit playerUnit = null;

        IDictionary<HexCoords, PathNode> pathFlood = null;

        public PlayerGiveOrder(MapUnit playerUnit)
        {
            this.playerUnit = playerUnit;
        }

        #endregion

        #region State implementation

        public override void EnterState()
        {
            // If the player is out of units, defeat them
            if (map.Units(UnitTeam.player).Count() <= 0)
            {
                game.StartCoroutine(PlayerExt.PlayerLoses());
            }
            else if (playerUnit.ap > 0)
            {
                Debug.Log("Selected " + playerUnit.name);

                pathFlood = playerUnit.Dijkstra();

                IEnumerable<MapCell> floodCells = pathFlood.Values
                    .Select(x => x.loc)
                    .Select(map.CellAt);
                IEnumerable<MapCell> threatCells = floodCells
                    .Where(x => playerUnit.InThreat(x));

                AddShimmer(floodCells);
                foreach (MapCell cell in threatCells)
                    cell.tint = Color.Lerp(Color.red, Color.white, 0.25f);

                map.events.pointerEnter += PointerEnter;
                map.events.pointerExit += PointerExit;
                map.events.pointerClick += PointerClick;

                ui.signal += GuiSignal;

                game.mouseDown += MouseDown;

                ui.jumpButton.Activate();
            }
            else
            {
                Debug.Log("Tried to select " + playerUnit.name + ", no actions remaining.");
                game.StartCoroutine(ExitToSelectUnit());
            }

            base.EnterState();
        }

        public override void LeaveState()
        {
            base.LeaveState();

            if (pathFlood != null)
            {
                IEnumerable<MapCell> floodCells = pathFlood.Values
                    .Select(x => x.loc)
                    .Select(map.CellAt);
                foreach (MapCell cell in floodCells)
                    cell.tint = Color.white;
            }

            ui.jumpButton.Deactivate();
        }

        IEnumerator ExitToSelectUnit()
        {
            // Stall one frame
            yield return null;

            // Fall back to unit selection
            game.ChangeState(new PlayerSelectUnit());
        }

        #endregion

        #region Input handling

        void PointerEnter(PointerEventData eventData, GameObject child)
        {
            MapCell cell = child.GetComponent<MapCell>();

            if (pathFlood.ContainsKey(cell.loc))
            {
                pathFlood[cell.loc].pathBack.Reverse();
                IEnumerable<MapCell> pathCells = pathFlood[cell.loc].pathTo
                    .Select(map.CellAt);
                RemoveShimmer(pathCells);
                foreach (MapCell pathCell in pathCells)
                {
                    if (playerUnit.InThreat(pathCell))
                        pathCell.tint = Color.Lerp(Color.red, Color.white, 0.0f);
                    else
                        pathCell.tint = Color.Lerp(Color.blue, Color.white, 0.25f);
                }
            }
        }

        void PointerExit(PointerEventData eventData, GameObject child)
        {
            MapCell cell = child.GetComponent<MapCell>();
            if (pathFlood.ContainsKey(cell.loc))
            {
                IEnumerable<MapCell> pathCells = pathFlood[cell.loc].pathTo
                    .Select(map.CellAt);
                AddShimmer(pathCells);
                foreach (MapCell pathCell in pathCells)
                {
                    if (playerUnit.InThreat(pathCell))
                        pathCell.tint = Color.Lerp(Color.red, Color.white, 0.25f);
                    else
                        pathCell.tint = Color.white;
                }
            }
        }

        void PointerClick(PointerEventData eventData, GameObject child)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                MapCell cell = child.GetComponent<MapCell>();

                if (pathFlood.ContainsKey(cell.loc))
                {
                    List<HexCoords> path = pathFlood[cell.loc].pathTo.Skip(1).ToList();
                    game.ChangeState(new MoveUnit(playerUnit, path));
                }
            }
        }

        void MouseDown(PointerEventData.InputButton mb)
        {
            if (mb == PointerEventData.InputButton.Right)
            {
                game.ChangeState(new PlayerSelectUnit());
            }
        }

        void GuiSignal(GameObject child)
        {
            if (child.GetComponent<GuiButton>() == ui.jumpButton)
                game.ChangeState(new JumpChooseTarget(playerUnit));
        }

        #endregion
    }

    class EndPlayerTurn : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // Reset player APs for next turn
            map.Units(UnitTeam.player).ForEach(x => x.ap.Reset());

            // Re-establish player threat
            foreach (MapUnit unit in map.Units(UnitTeam.player))
                foreach (MapCell cell in unit.GetThreatArea().Select(map.CellAt))
                    cell.AddThreat(unit);

            // Stall for one frame before changing states
            yield return null;

            // Pass the baton
            game.ChangeState(new BeginEnemyTurn());
        }
    }

    class PlayerWins : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            yield return null;
            game.LoadSceneByName("Main Menu");
        }
    }
}
