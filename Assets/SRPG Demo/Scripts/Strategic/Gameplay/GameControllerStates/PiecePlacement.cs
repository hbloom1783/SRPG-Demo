using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Strategic.GUI;
using SRPGDemo.Extensions;
using System;
using System.Collections;
using SRPGDemo.Strategic.Map;

namespace SRPGDemo.Strategic.Gameplay
{
    class Initial : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            yield return null;
            game.ChangeState(new PiecePlacement());
        }
    }

    class PiecePlacement : GameControllerState
    {
        #region State implementation

        MapPiece mapPiece = null;

        public override void EnterState()
        {
            // Spawn map piece
            mapPiece = UnityEngine.Object.Instantiate(game.mapPiecePrefab);

            base.EnterState();
        }

        public override void LeaveState()
        {
            base.LeaveState();

            // Delete map piece
            UnityEngine.Object.Destroy(mapPiece);
        }

        #endregion

        #region Input handling

        public override void HexTouchedHandler(FlatHexPoint? loc)
        {
        }

        public override void LeftMouseHandler(bool state)
        {
        }

        public override void RightMouseHandler(bool state)
        {
        }

        public override void GuiSignalHandler(GuiID signal)
        {
        }

        #endregion
    }
}
