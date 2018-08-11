using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamelogic.Grids;
using SRPGDemo.UI;

namespace SRPGDemo.Gameplay
{
    class JumpInput : GameControllerState
    {
        #region State implementation

        protected override void SubEnterState()
        {
        }

        protected override void SubExitState()
        {
        }

        #endregion

        #region Input handling

        protected override void HexTouchedHandler(PointyHexPoint? loc)
        {
        }

        protected override void LeftMouseHandler(bool state)
        {
        }

        protected override void RightMouseHandler(bool state)
        {
        }

        protected override void UiSignalHandler(UiSignal signal)
        {
        }

        #endregion
    }

    class JumpMove : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            yield return null;
        }
    }

    class JumpAttack : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            yield return null;
        }
    }
}
