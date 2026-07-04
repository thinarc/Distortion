using _Project.Develop.Input;
using _Project.Develop.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Develop.EntryPoints
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        private InputController _inputController;
        
        private void Start()
        {
            _inputController = new InputController();
            _inputController.Init();

            var soundController = new SoundController();
            soundController.Init();
            
            G.Reset();
            
            G.Register(_inputController);
            G.Register(soundController);

            DontDestroyOnLoad(this);
            SceneManager.LoadScene(1);
        }

        private void OnDisable()
        {
            _inputController.Dispose();
        }
    }
}