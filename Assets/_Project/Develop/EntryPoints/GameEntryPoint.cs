using _Project.Develop.Switch;
using _Project.Develop.UI.Cursor;
using _Project.Develop.UI.Menus;
using UnityEngine;

namespace _Project.Develop.EntryPoints
{
    public class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private UICursor cursor;
        [SerializeField] private SwitchView switchView;

        private async void Start()
        {
            G.Register(cursor);
            
            // await mainMenu.ShowMenu();

            var switchModel = new SwitchModel(switchView);
            switchModel.Initialize(startLocation: "Workplace", FindObjectsByType<BaseLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            
            var switchController = new SwitchController(switchModel);
        }
    }
}