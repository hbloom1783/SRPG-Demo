using UnityEngine;
using UnityEngine.SceneManagement;

namespace SRPGDemo.MainMenu
{
    public class Navigator : MonoBehaviour
    {
        public void LoadSceneByIndex(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }

        public void LoadSceneByName(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void LoadBattleScene()
        {
            Battle.Controllers.Reset();
            LoadSceneByName("Battle");
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
