using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using SRPGDemo.MainMenu.UI;
using SRPGDemo.Utility;

namespace SRPGDemo.Gameplay
{
    public abstract class GameControllerState : IStateMachineState
    {
        #region Shorthands
        
        protected GameController game { get { return GameController.instance; } }

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

    class MainMenu : GameControllerState
    {
        protected UiController ui { get { return UiController.instance; } }
        public override void EnterState()
        {
            ui.signal += UiSignal;
        }

        private void UiSignal(GameObject child)
        {
            UiElement element = child.GetComponent<UiElement>();

            if (element == ui.newGameButton)
            {
                game.NewGame();
            }
            else if (element == ui.loadGameButton)
            {
                Debug.Log("Can't handle loading games yet.");
            }
            else if (element == ui.quitButton)
            {
                game.Quit();
            }
            else
            {
                Debug.Log("Unhandled signal from element " + element.name + "!");
            }
        }
    }

    [AddComponentMenu("SRPG Demo/Gameplay Controller")]
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
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
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
            ui.ClearEvents();

            base.LeaveCurrentState();
        }

        #endregion

        #region Shorthands
        
        private UiController ui { get { return UiController.instance; } }

        private CachedReference<AudioSource> audioSource;

        public GameController()
        {
            audioSource = new CachedReference<AudioSource>(GetComponent<AudioSource>);
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

        public void NewGame()
        {
            LoadSceneByName("Strategic");
            ChangeState(new Strategic.Gameplay.Idle());
        }

        public void Quit()
        {
            ChangeState(null);
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
            if (SceneManager.GetActiveScene().name == "Main Menu")
                ChangeState(new MainMenu());
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
