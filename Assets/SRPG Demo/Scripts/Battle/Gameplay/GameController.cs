using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using SRPGDemo.Battle.Map;
using SRPGDemo.Battle.UI;
using SRPGDemo.Utility;

namespace SRPGDemo.Battle.Gameplay
{
    public abstract class GameControllerState : IStateMachineState
    {
        #region Shorthands

        protected MapController map { get { return MapController.instance; } }
        protected GameController game { get { return GameController.instance; } }
        protected GuiController ui { get { return GuiController.instance; } }

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

    public abstract class GameControllerAnimation : GameControllerState
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

    class Initial : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            yield return null;

            game.ChangeState(new BeginPlayerTurn());
        }
    }

    [AddComponentMenu("SRPG Demo/Battle/Gameplay Controller")]
    [RequireComponent(typeof(AudioSource))]
    public class GameController : StateMachineMonoBehaviour<GameControllerState>
    {
        #region Singleton

        private static GameController _instance = null;
        public static GameController instance
        {
            get { return _instance; }
            private set { _instance = value; }
        }

        void Awake()
        {
            if (instance != null)
                Destroy(gameObject);
            else
                instance = this;
        }

        void OnDestroy()
        {
            instance = null;
        }

        #endregion

        #region State machine

        protected override void EnterCurrentState()
        {
            base.EnterCurrentState();
        }

        protected override void LeaveCurrentState()
        {
            StopAllCoroutines();
            StopAllSounds();
            ClearEvents();
            if (map.events != null) map.events.ClearEvents();
            ui.ClearEvents();

            base.LeaveCurrentState();
        }

        #endregion

        #region Shorthands

        private MapController map { get { return MapController.instance; } }
        private GuiController ui { get { return GuiController.instance; } }

        private CachedReference<AudioSource> audioSource;

        public GameController()
        {
            audioSource = new CachedReference<AudioSource>(GetComponent<AudioSource>);
        }

        #endregion

        #region Navigation

        public void LoadSceneByIndex(int sceneIndex)
        {
            ChangeState(null);
            SceneManager.LoadScene(sceneIndex);
        }

        public void LoadSceneByName(string sceneName)
        {
            ChangeState(null);
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

        #region Input handling

        public delegate void KeyCodeEvent(KeyCode kc);
        public delegate void MouseButtonEvent(PointerEventData.InputButton button);

        public event KeyCodeEvent keyDown = null;
        public event KeyCodeEvent keyUp = null;

        public event MouseButtonEvent mouseDown = null;
        public event MouseButtonEvent mouseUp = null;

        public void ClearEvents()
        {
            keyDown = null;
            keyUp = null;

            mouseDown = null;
            mouseUp = null;
        }

        #endregion

        #region Monobehaviour

        void Start()
        {
            // Enter initial state
            ChangeState(new Initial());
        }

        void OnGUI()
        {
            Event e = Event.current;

            if (e.isKey)
            {
                if (e.keyCode != KeyCode.None)
                {
                    if ((e.type == EventType.keyDown) && (keyDown != null))
                        keyDown(e.keyCode);
                    else if ((e.type == EventType.keyUp) && (keyUp != null))
                        keyUp(e.keyCode);
                }
            }
            else if (e.isMouse)
            {
                if ((e.type == EventType.mouseDown) && (mouseDown != null))
                    mouseDown(IdentifyButton(e.button));
                if ((e.type == EventType.mouseUp) && (mouseUp != null))
                    mouseUp(IdentifyButton(e.button));
            }
        }

        private PointerEventData.InputButton IdentifyButton(int mb)
        {
            switch (mb)
            {
                default:
                case 0: return PointerEventData.InputButton.Left;
                case 1: return PointerEventData.InputButton.Right;
                case 2: return PointerEventData.InputButton.Middle; // I think
            }
        }

        #endregion
    }
}
