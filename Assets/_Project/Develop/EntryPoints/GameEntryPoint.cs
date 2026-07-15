using System;
using System.Collections.Generic;
using _Project.Develop.Editing;
using _Project.Develop.Editing.Tools;
using _Project.Develop.Switch;
using _Project.Develop.Switch.Scenes;
using _Project.Develop.UI.Cursor;
using _Project.Develop.UI.Curtain;
using _Project.Develop.UI.Menus;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace _Project.Develop.EntryPoints
{
    public class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private UICursor cursor;
        [SerializeField] private CurtainSide[] sides;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private SwitchView switchView;
        [SerializeField] private EditView editView;
        [SerializeField] private SprayTool sprayTool;
        
        private readonly List<IDisposable> _disposables = new();

        private async void Start()
        {
            G.Register(cursor);
            
            // await mainMenu.ShowMenu();
            
            var curtain = new UICurtain(sides, globalLight);
            curtain.Initialize();
            G.Register(curtain);

            var switchModel = new SwitchModel(switchView);
            await switchModel.Initialize(startLocation: "Workplace", FindObjectsByType<BaseLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            var switchController = new SwitchController(switchModel);
            switchView.BindButtons(switchController);

            var editController = new EditController(editView);
            editView.Initialize();
            editController.Initialize(sprayTool);
            G.Register(editController);
            
            AddToDisposables(editController);
        }

        private void AddToDisposables(params IDisposable[] disposables)
        {
            _disposables.Add(switchView);
            _disposables.AddRange(disposables);
        }

        private void OnDestroy()
        {
            foreach(var disposable in _disposables) disposable.Dispose();
        }
    }
}