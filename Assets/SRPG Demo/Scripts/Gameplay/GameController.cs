using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Map;

namespace SRPGDemo.Gameplay
{
    public delegate void LocationEvent(PointyHexPoint loc);
    public delegate void ButtonEvent(bool state);
    public delegate void AxisEvent(int x, int y);

    public abstract class GameControllerState
    {
        #region Controller passthru

        protected MapController map
        {
            get { return Controllers.map; }
        }

        protected GameController game
        {
            get { return Controllers.game; }
        }

        #endregion

        #region Input handlers

        protected abstract void HexTouchedHandler(PointyHexPoint loc);
        protected abstract void LeftMouseHandler(bool state);
        protected abstract void RightMouseHandler(bool state);

        #endregion

        #region State implementation

        protected abstract void SubEnterState();
        protected abstract void SubExitState();

        public virtual void EnterState()
        {
            turn.hexTouchedEvent += HexTouchedHandler;
            turn.leftMouseEvent += LeftMouseHandler;
            turn.rightMouseEvent += RightMouseHandler;

            SubEnterState();

            if (turn.mouseLoc.HasValue)
                HexTouchedHandler(turn.mouseLoc.Value);
        }

        public void ExitState()
        {
            SubExitState();

            turn.hexTouchedEvent -= HexTouchedHandler;
            turn.leftMouseEvent -= LeftMouseHandler;
            turn.rightMouseEvent -= RightMouseHandler;

            turn.StopAllCoroutines();
        }

        #endregion
    }

    public class GameController : MonoBehaviour
    {
        #region State machine

        GameControllerState currentState = null;

        private bool stateLocked;

        public void StateTransition(GameControllerState newState)
        {
            if ((newState != currentState) && !stateLocked)
            {
                stateLocked = true;

                if (currentState != null)
                {
                    Debug.Log("State machine exiting " + currentState.GetType().Name + ".");
                    currentState.ExitState();
                }
                else
                    Debug.Log("State machine exiting null.");

                currentState = newState;

                if (currentState != null)
                    currentState.EnterState();
                else
                    Debug.Log("State machine entering null.");

                stateLocked = false;
            }
        }

        #endregion

        #region Input handling

        public event LocationEvent hexTouchedEvent;
        public PointyHexPoint? mouseLoc = null;

        public event ButtonEvent leftMouseEvent;
        public bool leftMouseState = false;

        public event ButtonEvent rightMouseEvent;
        public bool rightMouseState = false;

        private void UpdateInput()
        {
            // Check for hexTouched
            PointyHexPoint newMouseLoc = Controllers.map.MousePosition;
            if (Controllers.map.InBounds(newMouseLoc) &&
                (newMouseLoc != mouseLoc))
            {
                if (hexTouchedEvent != null)
                    hexTouchedEvent(newMouseLoc);

                mouseLoc = newMouseLoc;
            }

            // Check on LMB
            bool newLeftMouseState = Input.GetMouseButton(0);
            if (newLeftMouseState != leftMouseState)
            {
                if (leftMouseEvent != null)
                    leftMouseEvent(newLeftMouseState);
                leftMouseState = newLeftMouseState;
            }

            // Check on RMB
            bool newRightMouseState = Input.GetMouseButton(1);
            if (newRightMouseState != rightMouseState)
            {
                if (rightMouseEvent != null)
                    rightMouseEvent(newRightMouseState);
                rightMouseState = newRightMouseState;
            }
        }
    
        #endregion

        #region Monobehaviour

            void Start()
        {
            // Enter initial state
            StateTransition(new PlayerSelectUnit());
        }

        void Update()
        {
            UpdateInput();
        }

        #endregion
    }
}
