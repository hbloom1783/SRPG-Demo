using SRPGDemo.Gameplay;
using SRPGDemo.Strategic.Map;
using SRPGDemo.Strategic.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SRPGDemo.Strategic.Gameplay
{
    public abstract class StrategicGameControllerState : GameControllerState
    {
        #region Shorthands

        protected MapController map { get { return MapController.instance; } }
        protected UiController ui { get { return UiController.instance; } }

        #endregion

        #region State implementation

        public virtual void EnterState()
        {
        }

        public virtual void LeaveState()
        {
        }

        #endregion
    }

    public abstract class StrategicGameControllerAnimation : StrategicGameControllerState
    {
        public abstract IEnumerator AnimationCoroutine();

        #region State implementation

        public override void EnterState()
        {
            base.EnterState();

            game.StartCoroutine(AnimationCoroutine());
        }

        public override void LeaveState()
        {
            base.LeaveState();
        }

        #endregion
    }
}
