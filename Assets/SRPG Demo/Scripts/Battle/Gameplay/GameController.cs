using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Gamelogic.Grids;
using SRPGDemo.Battle.Map;
using SRPGDemo.Battle.GUI;
using SRPGDemo.Utility;

namespace SRPGDemo.Battle.Gameplay
{
    public interface IInputHandler
    {
        void HexTouchedHandler(PointyHexPoint? loc);

        void LeftMouseHandler(bool state);
        void RightMouseHandler(bool state);

        void GuiSignalHandler(GuiID signal);
    }

    public abstract class GameControllerState : IStateMachineState, IInputHandler
    {
        #region Shorthands

        protected MapController map { get { return Controllers.map; } }
        protected GameController game { get { return Controllers.game; } }
        protected GuiController gui { get { return Controllers.gui; } }

        #endregion

        #region State implementation

        public virtual void EnterState()
        {
            if (game.mouseLoc.HasValue)
                HexTouchedHandler(game.mouseLoc.Value);
        }

        public virtual void LeaveState()
        {
            game.StopAllCoroutines();
            game.StopAllSounds();
        }

        #endregion

        #region Input handler

        public abstract void HexTouchedHandler(PointyHexPoint? loc);

        public abstract void LeftMouseHandler(bool state);
        public abstract void RightMouseHandler(bool state);

        public abstract void GuiSignalHandler(GuiID signal);

        #endregion
    }

    abstract class GameControllerAnimation : GameControllerState
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

        #region Input handler

        public override void HexTouchedHandler(PointyHexPoint? loc)
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

    class EndBattle : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            yield return null;
            game.LoadSceneByName("Main Menu");
        }
    }

    [AddComponentMenu("SRPG Demo/Battle/Gameplay Controller")]
    [RequireComponent(typeof(AudioSource))]
    public class GameController : StateMachineMonoBehaviour<GameControllerState>
    {
        #region Shorthands

        private MapController map { get { return Controllers.map; } }
        private GuiController gui { get { return Controllers.gui; } }

        private CachedReference<AudioSource> audioSource;

        public GameController()
        {
            audioSource = new CachedReference<AudioSource>(GetComponent<AudioSource>);
        }

        #endregion
        
        #region Input handling
        
        public PointyHexPoint? mouseLoc = null;
        
        public bool leftMouseState = false;
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
            // Check if the player is trying to quit (maybe make this access a menu later)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LoadSceneByName("Main Menu");
            }

            // Check for hexTouched
            PointyHexPoint? newMouseLoc = map.MousePosition;
            if (!map.InBounds(newMouseLoc.Value))
                newMouseLoc = null;

            if (newMouseLoc != mouseLoc)
            {
                if (currentState != null)
                    currentState.HexTouchedHandler(newMouseLoc);
                mouseLoc = newMouseLoc;
            }

            if (!IsPointerOverUIObject())
            {
                // Check on LMB
                bool newLeftMouseState = Input.GetMouseButton(0);
                if (newLeftMouseState != leftMouseState)
                {
                    if (currentState != null)
                        currentState.LeftMouseHandler(newLeftMouseState);
                    leftMouseState = newLeftMouseState;
                }

                // Check on RMB
                bool newRightMouseState = Input.GetMouseButton(1);
                if (newRightMouseState != rightMouseState)
                {
                    if (currentState != null)
                        currentState.RightMouseHandler(newRightMouseState);
                    rightMouseState = newRightMouseState;
                }
            }
        }

        public void ReceiveUiSignal(GuiID signal)
        {
            if (currentState != null)
                currentState.GuiSignalHandler(signal);
        }

        #endregion

        #region Navigation

        public void LoadSceneByIndex(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }

        public void LoadSceneByName(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        #endregion

        #region Sound

        public void PlaySound(AudioClip sound, float vol = 1.0f)
        {
            audioSource.cache.Stop();
            if (sound != null) audioSource.cache.PlayOneShot(sound, vol);
        }

        public void StopAllSounds()
        {
            audioSource.cache.Stop();
        }

        public bool IsPlayingSound()
        {
            return audioSource.cache.isPlaying;
        }

        #endregion

        #region Monobehaviour

        void Start()
        {
            // Enter initial state
            ChangeState(new BeginPlayerTurn());
        }

        void Update()
        {
            UpdateInput();
        }

        #endregion
    }
}
