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
            G.Reset();
            
            G.Register(new EventBus());
            
            _inputController = new InputController();
            _inputController.Initialize();
            G.Register(_inputController);

            var soundController = new SoundController();
            soundController.Initialize();
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