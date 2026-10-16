using System;
using System.Collections.Generic;
using _Project.Develop.Anomaly;
using _Project.Develop.Anomaly.Anxiety;
using _Project.Develop.Editing;
using _Project.Develop.Editing.Tools;
using _Project.Develop.Sound;
using _Project.Develop.Switch;
using _Project.Develop.Switch.Scenes;
using _Project.Develop.UI;
using _Project.Develop.UI.Cursor;
using _Project.Develop.UI.Curtain;
using _Project.Develop.UI.HUD;
using _Project.Develop.UI.Menus;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace _Project.Develop.EntryPoints
{
    public class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private UICursor cursor;
        [SerializeField] private UIBar bar;
        [SerializeField] private CurtainSide[] sides;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private SwitchView switchView;
        [SerializeField] private EditView editView;
        [SerializeField] private SprayTool sprayTool;
        [SerializeField] private AnomalyView anomalyView;
        [SerializeField] private DarknessEffect darknessEffect;
        [SerializeField] private RestDialog restDialog;
        [SerializeField] private UIStats stats;
        [SerializeField] private EndingMenu endingMenuPrefab;

        [SerializeField, Space(5)] private Canvas streetCanvas;
        [SerializeField] private RectTransform streetMask;
        
        private AnxietyController _anxietyController;
        
        private readonly List<IDisposable> _disposables = new();

        private void Start()
        {
            StartAsync().Forget();
        }

        private async UniTaskVoid StartAsync()
        {
            G.Register(cursor);
            G.Register(bar);
            
            G.Register(darknessEffect);

            EndingMenu endingMenu;
            
            if (!G.Has<EndingMenu>())
            {
                endingMenu = Instantiate(endingMenuPrefab);
                endingMenu.Init();
                G.Register(endingMenu);
                
                await endingMenu.SelectLanguage();
            }
            else endingMenu = G.Get<EndingMenu>();
            
            await mainMenu.ShowMenu();
            
            _anxietyController = new AnxietyController();
            _anxietyController.Init();
            G.Register(_anxietyController);
            
            var curtain = new UICurtain(sides, globalLight, streetMask, streetCanvas);
            curtain.Init();
            G.Register(curtain);

            var switchModel = new SwitchModel(switchView);
            await switchModel.Init(startLocation: "Workplace", FindObjectsByType<BaseLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            var switchController = new SwitchController(switchModel);
            switchView.Init(switchController);
            
            restDialog.Init(switchModel, stats);
            G.Register(restDialog);
            
            endingMenu.Init2(switchModel);

            var editController = new EditController(editView);
            editView.Init();
            editController.Init(sprayTool);
            G.Register(editController);

            var anomalyController = new AnomalyController(anomalyView);
            anomalyController.Init();
            
            AddToDisposables(editController, anomalyController, curtain, _anxietyController, G.Get<SoundController>());
        }

        private void Update()
        {
            _anxietyController?.Tick();
        }

        private void AddToDisposables(params IDisposable[] disposables)
        {
            _disposables.AddRange(disposables);
        }

        private void OnDestroy()
        {
            G.Unregister<UICursor>();
            G.Unregister<UIBar>();
            G.Unregister<DarknessEffect>();
            if (G.Has<AnxietyController>()) G.Unregister<AnxietyController>();
            if (G.Has<UICurtain>()) G.Unregister<UICurtain>();
            if (G.Has<RestDialog>()) G.Unregister<RestDialog>();
            if (G.Has<EditController>()) G.Unregister<EditController>();
            
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }
    }
}