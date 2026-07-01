using _Project.Develop.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Develop.G
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        private void Start()
        {
            var soundController = new SoundController();
            soundController.Init();
            
            G.Reset();
            
            G.Register(soundController);

            DontDestroyOnLoad(this);
            SceneManager.LoadScene(1);
        }
    }
}