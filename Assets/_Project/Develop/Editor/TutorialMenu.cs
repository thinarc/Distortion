using _Project.Develop.UI.Menus;
using UnityEditor;

namespace EditorSpace
{
    public static class TutorialMenu
    {
        private const string MenuPath = "Tools/Tutorials/Enable Tutorials";

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            TutorialSettings.Enabled = !TutorialSettings.ShouldShow;
        }

        [MenuItem(MenuPath, true)]
        private static bool Validate()
        {
            Menu.SetChecked(MenuPath, TutorialSettings.ShouldShow);
            return true;
        }
    }
}
