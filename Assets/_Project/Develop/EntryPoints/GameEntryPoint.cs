using _Project.Develop.Menus;
using _Project.Develop.UI.Cursor;
using UnityEngine;

namespace _Project.Develop.EntryPoints
{
    public class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private UICursor cursor;

        private async void Start()
        {
            G.Register(cursor);
            
            // await mainMenu.ShowMenu();
        }
    }
}