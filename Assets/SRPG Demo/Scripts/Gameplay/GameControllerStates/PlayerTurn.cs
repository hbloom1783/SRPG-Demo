using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Map;
using SRPGDemo.Extensions;
using System;
using System.Collections;

namespace SRPGDemo.Gameplay
{
    class BeginPlayerTurn : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // Stall for one frame
            yield return null;

            // Display marquee

            // Clear marquee

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
            playerMobileLocs = map.Mobiles()
                .Where(x => (x.team == MobileTeam.player) && (x.actionsLeft > 0))
                .Select(x => map.WhereIs(x));
        }

        private void ClearMap()
        {
            foreach (PointyHexPoint point in Controllers.map.mapGrid)
            {
                map.CellAt(point).ClearTint();
            }
        }

        private void PaintMap()
        {
            playerMobileLocs.ForEach(
                x => map.CellAt(x).SetTint(
                    Color.white,
                    ColorExt.Grayscale(0.5f),
                    0.25f));
        }

        #endregion

        #region State implementation

        protected override void SubEnterState()
        {
            ClearMap();
            PaintMap();
        }

        protected override void SubExitState()
        {
            ClearMap();
        }

        #endregion

        #region Input handling

        PointyHexPoint? selectedLoc = null;

        protected override void HexTouchedHandler(PointyHexPoint loc)
        {
            ClearMap();
            PaintMap();
            if (playerMobileLocs.Contains(loc))
                selectedLoc = loc;
            else
                selectedLoc = null;

            if (selectedLoc.HasValue)
                map.CellAt(selectedLoc.Value).SetTint(Color.green);
        }

        protected override void LeftMouseHandler(bool state)
        {
            if ((state == true) && (selectedLoc.HasValue))
                turn.StateTransition(new PlayerUnitSelected(
                    map.CellAt(selectedLoc.Value).MobilesPresent().First()));
        }

        protected override void RightMouseHandler(bool state)
        {
        }

        #endregion
    }

    class PlayerUnitSelected : GameControllerState
    {
        #region Map highlighting

        MapMobile playerMobile = null;

        public PlayerUnitSelected(MapMobile playerMobile)
        {
            this.playerMobile = playerMobile;
        }

        private void ClearMap()
        {
            foreach (PointyHexPoint point in Controllers.map.mapGrid)
            {
                map.CellAt(point).ClearTint();
            }
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
                            map.mapGrid[node.Key].SetTint(
                                Color.white,
                                ColorExt.Grayscale(0.5f),
                                0.25f);
                            break;
                        case PassageType.canPass:
                            map.mapGrid[node.Key].SetTint(Color.green);
                            break;
                        case PassageType.inThreat:
                            map.mapGrid[node.Key].SetTint(Color.yellow);
                            break;
                        case PassageType.blocked:
                            map.mapGrid[node.Key].SetTint(Color.red);
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

                lastPath = new List<PointyHexPoint>();

                pathFlood[loc].PathBack().ForEach(x =>
                {
                    map[x.loc].SetTint(Color.blue);
                    lastPath.Insert(0, x.loc);
                });

                lastPath.Add(loc);

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
            Debug.Log(playerMobile);

            ClearMap();
            PaintMap();
            if (turn.mouseLoc.HasValue)
                PaintPath(turn.mouseLoc.Value);
        }

        protected override void SubExitState()
        {
            ClearMap();
        }

        #endregion

        #region Input handling

        PointyHexPoint? selectedLoc = null;

        protected override void HexTouchedHandler(PointyHexPoint loc)
        {
            ClearMap();
            PaintMap();
            if (PaintPath(loc))
                selectedLoc = loc;
            else
                selectedLoc = null;
        }

        protected override void LeftMouseHandler(bool state)
        {
            if (selectedLoc.HasValue)
            {
                turn.StateTransition(new MoveUnit(playerMobile, lastPath));
                playerMobile.actionsLeft -= 1;
            }
        }

        protected override void RightMouseHandler(bool state)
        {
            turn.StateTransition(new PlayerSelectUnit());
        }

        #endregion
    }
}
