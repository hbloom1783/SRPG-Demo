using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Map;
using SRPGDemo.Extensions;
using SRPGDemo.UI;

namespace SRPGDemo.Gameplay
{
    class BeginPlayerTurn : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // Display marquee
            ui.marqueeText.gameObject.SetActive(true);
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
            ui.marqueeText.gameObject.SetActive(false);

            // Begin player turn
            game.StateTransition(new PlayerSelectUnit());
        }
    }

    class PlayerSelectUnit : GameControllerState
    {
        #region Map highlighting

        IEnumerable<PointyHexPoint> playerMobileLocs = null;

        public PlayerSelectUnit()
        {
        }

        #endregion

        #region State implementation

        protected override void SubEnterState()
        {
            playerMobileLocs = map.Mobiles()
                .Where(x => (x.team == MobileTeam.player) && (x.actions > 0))
                .Select(x => map.WhereIs(x));

            MapHighlight.ClearMap();
            MapHighlight.ShimmerRange(playerMobileLocs);

            ui.endTurnButton.gameObject.SetActive(true);
        }

        protected override void SubExitState()
        {
            MapHighlight.ClearMap();

            ui.endTurnButton.gameObject.SetActive(false);
        }

        #endregion

        #region Input handling

        PointyHexPoint? selectedLoc = null;

        protected override void HexTouchedHandler(PointyHexPoint? loc)
        {
            MapHighlight.ClearMap();
            MapHighlight.ShimmerRange(playerMobileLocs);

            if (loc.HasValue && playerMobileLocs.Contains(loc.Value))
                selectedLoc = loc;
            else
                selectedLoc = null;

            if (selectedLoc.HasValue)
                map.CellAt(selectedLoc.Value).SetTint(Color.green);
        }

        protected override void LeftMouseHandler(bool state)
        {
            if ((state == true) && (selectedLoc.HasValue))
                game.StateTransition(new PlayerGiveOrder(
                    map.CellAt(selectedLoc.Value).MobilesPresent().First()));
        }

        protected override void RightMouseHandler(bool state)
        {
        }

        protected override void UiSignalHandler(UiSignal signal)
        {
            switch (signal)
            {
                case UiSignal.endTurn:
                    game.StateTransition(new EndPlayerTurn());
                    break;
            }
        }

        #endregion
    }

    class PlayerGiveOrder : GameControllerState
    {
        #region Map highlighting

        MapMobile playerMobile = null;

        public PlayerGiveOrder(MapMobile playerMobile)
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
                map[loc].SetTint(Color.blue);

                lastPath = pathFlood[loc].PathTo()
                    .Select(x => x.loc)
                    .ToList();
                lastPath.ForEach(x => map[x].SetTint(Color.blue.Mix(Color.white)));

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

        protected override void SubEnterState()
        {
            Debug.Log("Selected " + playerMobile.name);

            MapHighlight.ClearMap();
            PaintMap();
            if (game.mouseLoc.HasValue)
                PaintPath(game.mouseLoc.Value);

            //ui.jumpButton.gameObject.SetActive(true);
        }

        protected override void SubExitState()
        {
            MapHighlight.ClearMap();

            ui.jumpButton.gameObject.SetActive(false);
        }

        #endregion

        #region Input handling

        PointyHexPoint? selectedLoc = null;

        protected override void HexTouchedHandler(PointyHexPoint? loc)
        {
            MapHighlight.ClearMap();
            PaintMap();
            if (loc.HasValue && PaintPath(loc.Value))
                selectedLoc = loc;
            else
                selectedLoc = null;
        }

        protected override void LeftMouseHandler(bool state)
        {
            if (selectedLoc.HasValue)
            {
                game.StateTransition(new MoveUnit(playerMobile, lastPath, new PlayerSelectUnit()));
            }
        }

        protected override void RightMouseHandler(bool state)
        {
            game.StateTransition(new PlayerSelectUnit());
        }

        protected override void UiSignalHandler(UiSignal signal)
        {
            switch (signal)
            {
                case UiSignal.jump:
                    Debug.Log("Jump signal received.");
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
            map.Mobiles().Where(x => x.team == MobileTeam.player).ForEach(x => x.actions.Reset());

            // Stall for one frame before changing states
            yield return null;

            // Pass the baton
            game.StateTransition(new BeginEnemyTurn());
        }
    }
}
