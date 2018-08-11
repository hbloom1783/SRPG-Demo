using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamelogic.Grids;

namespace SRPGDemo.Gameplay
{
    abstract class GameControllerAnimation : GameControllerState
    {
        public abstract IEnumerator AnimationCoroutine();

        #region State implementation

        protected override void SubEnterState()
        {
            turn.StartCoroutine(AnimationCoroutine());
        }

        protected override void SubExitState()
        {
        }

        #endregion

        #region Input handling

        protected override void HexTouchedHandler(PointyHexPoint loc)
        {
        }

        protected override void LeftMouseHandler(bool state)
        {
        }

        protected override void RightMouseHandler(bool state)
        {
        }

        #endregion
    }
}
