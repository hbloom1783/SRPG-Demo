using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Battle.Map;
using SRPGDemo.Battle.GUI;
using SRPGDemo.Extensions;
using System;

namespace SRPGDemo.Battle.Gameplay
{
    static class PlayerExt
    {
        public static IEnumerator PlayerLoses()
        {
            yield return null;
            Controllers.game.cache.ChangeState(new EnemyWins());
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
                GuiText marqueeText = gui.GetText(GuiID.marqueeText);

                // Display marquee
                marqueeText.Activate();
                marqueeText.text = "Player Turn";
                marqueeText.color = Color.white.Alpha(0.0f);

                for (float timePassed = 0.0f; timePassed < 1.0f; timePassed += Time.deltaTime)
                {
                    marqueeText.color = Color.Lerp(Color.white.Alpha(0.0f), Color.blue, timePassed / 1.0f);
                    yield return null;
                }

                for (float timePassed = 0.0f; timePassed < 1.0f; timePassed += Time.deltaTime)
                {
                    marqueeText.color = Color.Lerp(Color.blue, Color.blue.Alpha(0.0f), timePassed / 1.0f);
                    yield return null;
                }

                // Clear marquee
                marqueeText.Deactivate();

                // Begin player turn
                game.ChangeState(new PlayerSelectUnit());
            }
        }
    }

    class PlayerSelectUnit : GameControllerState
    {
        #region Map highlighting

        IEnumerable<PointyHexPoint> playerUnitLocs = null;

        #endregion

        #region State implementation

        GuiButton endTurnButton = null;

        public override void EnterState()
        {
            // If the player is out of units, defeat them
            if (map.Units(UnitTeam.player).Count() <= 0)
            {
                game.StartCoroutine(PlayerExt.PlayerLoses());
            }
            else
            {
                playerUnitLocs = map.Units()
                    .Where(x => (x.team == UnitTeam.player) && (x.ap > 0))
                    .Select(x => map.WhereIs(x));

                MapHighlight.ClearMap();
                MapHighlight.ShimmerRange(playerUnitLocs);

                endTurnButton = gui.GetButton(GuiID.endTurnButton);
                endTurnButton.Activate();
            }

            base.EnterState();
        }

        public override void LeaveState()
        {
            MapHighlight.ClearMap();

            if (endTurnButton != null)
                endTurnButton.Deactivate();

            base.LeaveState();
        }

        #endregion

        #region Input handling

        PointyHexPoint? selectedLoc = null;

        public override void HexTouchedHandler(PointyHexPoint? loc)
        {
            MapHighlight.ClearMap();
            MapHighlight.ShimmerRange(playerUnitLocs);

            if (loc.HasValue && playerUnitLocs.Contains(loc.Value))
                selectedLoc = loc;
            else
                selectedLoc = null;

            if (selectedLoc.HasValue)
                map.CellAt(selectedLoc.Value).SetTint(Color.green);
        }

        public override void LeftMouseHandler(bool state)
        {
            if (state && selectedLoc.HasValue)
                game.ChangeState(new PlayerGiveOrder(
                    map.CellAt(selectedLoc.Value).unitPresent));
        }

        public override void RightMouseHandler(bool state)
        {
        }

        public override void GuiSignalHandler(GuiID signal)
        {
            switch (signal)
            {
                case GuiID.endTurnButton:
                    game.ChangeState(new EndPlayerTurn());
                    break;
            }
        }

        #endregion
    }

    class PlayerGiveOrder : GameControllerState
    {
        #region Map highlighting

        MapUnit playerMobile = null;

        public PlayerGiveOrder(MapUnit playerMobile)
        {
            this.playerMobile = playerMobile;
        }

        private void PaintMap()
        {
            if (playerMobile != null)
            {
                playerMobile.PathFlood().ForEach(node =>
                {
                    switch (node.Value.passage)
                    {
                        case PassageType.open:
                            MapHighlight.Shimmer(node.Key);
                            break;
                        case PassageType.inThreat:
                            MapHighlight.Shimmer(node.Key, Color.red.Mix(Color.white));
                            break;
                    }
                });
            }
        }

        List<PointyHexPoint> lastPath = null;

        private bool PaintPath(PointyHexPoint loc)
        {
            var pathFlood = playerMobile.PathFlood();

            if (pathFlood.ContainsKey(loc))
            {
                map.CellAt(loc).SetTint(Color.blue);

                lastPath = pathFlood[loc].PathTo()
                    .Select(x => x.loc)
                    .ToList();
                lastPath.ForEach(x => map.CellAt(x).SetTint(Color.blue.Mix(Color.white)));

                if (pathFlood[loc].passage == PassageType.inThreat)
                    map.CellAt(loc).SetTint(Color.red.Mix(Color.white, 0.25f));

                return true;
            }
            else
            {
                lastPath = null;
                return false;
            }
        }

        #endregion

        #region State implementation

        private GuiButton jumpButton = null;

        public override void EnterState()
        {
            // If the player is out of units, defeat them
            if (map.Units(UnitTeam.player).Count() <= 0)
            {
                game.StartCoroutine(PlayerExt.PlayerLoses());
            }
            else if (playerMobile.ap > 0)
            {
                Debug.Log("Selected " + playerMobile.name);

                MapHighlight.ClearMap();
                PaintMap();
                if (game.mouseLoc.HasValue)
                    PaintPath(game.mouseLoc.Value);

                jumpButton = gui.GetButton(GuiID.jumpButton);
                jumpButton.Activate();
            }
            else
            {
                Debug.Log("Tried to select " + playerMobile.name + ", no actions remaining.");
                game.StartCoroutine(ExitToSelectUnit());
            }
        }

        IEnumerator ExitToSelectUnit()
        {
            // Stall one frame
            yield return null;

            // Fall back to unit selection
            game.ChangeState(new PlayerSelectUnit());
        }

        public override void LeaveState()
        {
            MapHighlight.ClearMap();

            if (jumpButton != null)
                jumpButton.Deactivate();
        }

        #endregion

        #region Input handling

        PointyHexPoint? selectedLoc = null;

        public override void HexTouchedHandler(PointyHexPoint? loc)
        {
            MapHighlight.ClearMap();
            PaintMap();
            if (loc.HasValue && PaintPath(loc.Value))
                selectedLoc = loc;
            else
                selectedLoc = null;
        }

        public override void LeftMouseHandler(bool state)
        {
            if (state && selectedLoc.HasValue)
            {
                game.ChangeState(new MoveUnit(playerMobile, lastPath));
            }
        }

        public override void RightMouseHandler(bool state)
        {
            if (state) game.ChangeState(new PlayerSelectUnit());
        }

        public override void GuiSignalHandler(GuiID signal)
        {
            switch (signal)
            {
                case GuiID.jumpButton:
                    game.ChangeState(new JumpChooseTarget(playerMobile));
                    break;
            }
        }

        #endregion
    }

    class EndPlayerTurn : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // Reset player APs for next turn
            map.Units(UnitTeam.player).ForEach(x => x.ap.Reset());

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
            game.ChangeState(new EndBattle());
        }
    }
}
