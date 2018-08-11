using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Gamelogic.Grids;
using SRPGDemo.Map;
using SRPGDemo.UI;

namespace SRPGDemo.Gameplay
{
    public delegate void LocationEvent(PointyHexPoint? loc);

    public delegate void ButtonEvent(bool state);
    public delegate void AxisEvent(int x, int y);

    public delegate void UiSignalEvent(UiSignal signal);

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

        protected UiController ui
        {
            get { return Controllers.ui; }
        }

        #endregion

        #region Input handlers

        protected abstract void HexTouchedHandler(PointyHexPoint? loc);

        protected abstract void LeftMouseHandler(bool state);
        protected abstract void RightMouseHandler(bool state);

        protected abstract void UiSignalHandler(UiSignal signal);

        #endregion

        #region State implementation

        protected abstract void SubEnterState();
        protected abstract void SubExitState();

        public virtual void EnterState()
        {
            game.hexTouchedEvent += HexTouchedHandler;

            game.leftMouseEvent += LeftMouseHandler;
            game.rightMouseEvent += RightMouseHandler;

            game.uiSignalEvent += UiSignalHandler;

            SubEnterState();

            if (game.mouseLoc.HasValue)
                HexTouchedHandler(game.mouseLoc.Value);
        }

        public void ExitState()
        {
            SubExitState();

            game.hexTouchedEvent -= HexTouchedHandler;

            game.leftMouseEvent -= LeftMouseHandler;
            game.rightMouseEvent -= RightMouseHandler;

            game.uiSignalEvent -= UiSignalHandler;

            game.StopAllCoroutines();
        }

        #endregion
    }

    abstract class GameControllerAnimation : GameControllerState
    {
        public abstract IEnumerator AnimationCoroutine();

        #region State implementation

        protected override void SubEnterState()
        {
            game.StartCoroutine(AnimationCoroutine());
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

    public class GameController : MonoBehaviour
    {
        #region State machine

        GameControllerState currentState = null;

        private bool stateLocked;

        public void StateTransition(GameControllerState newState)
        {
            if (stateLocked)
            {
                throw new System.Exception("Trying to change state while in a state transition!");
            }
            else if (newState == currentState)
            {
                Debug.Log("Neutral state change to " + newState.GetType().Name + ".");
            }
            else
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
                {
                    Debug.Log("State machine entering " + currentState.GetType().Name + ".");
                    currentState.EnterState();
                }
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

        private bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        private void UpdateInput()
        {
            // Check for hexTouched
            PointyHexPoint? newMouseLoc = Controllers.map.MousePosition;
            if (!Controllers.map.InBounds(newMouseLoc.Value))
                newMouseLoc = null;

            if (newMouseLoc != mouseLoc)
            {
                if (hexTouchedEvent != null)
                    hexTouchedEvent(newMouseLoc);

                mouseLoc = newMouseLoc;
            }

            if (!IsPointerOverUIObject())
            {
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
        }

        public event UiSignalEvent uiSignalEvent;

        public void ReceiveUiSignal(UiSignal signal)
        {
            if (uiSignalEvent != null)
                uiSignalEvent(signal);
        }
    
        #endregion

        #region Monobehaviour

            void Start()
        {
            // Enter initial state
            StateTransition(new BeginPlayerTurn());
        }

        void Update()
        {
            UpdateInput();
        }

        #endregion
    }
}
