using UnityEngine;

namespace _Project.Develop.UI.Menus
{
    public static class TutorialSettings
    {
        private const string EnabledKey = "tutorial.enabled";
        private const string CompletedKey = "tutorial.good-ending-completed";

        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(EnabledKey, 1) != 0;
            set
            {
                PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
                // Explicitly enabling tutorials also allows replaying them for debugging.
                if (value) PlayerPrefs.DeleteKey(CompletedKey);
                Refresh();
            }
        }

        public static bool ShouldShow => Enabled && PlayerPrefs.GetInt(CompletedKey, 0) == 0;

        public static void RecordEnding(bool goodEnding)
        {
            PlayerPrefs.SetInt(CompletedKey, goodEnding ? 1 : 0);
            Refresh();
        }

        private static void Refresh()
        {
            PlayerPrefs.Save();
            ResetTutorials();
        }

        public static void ResetTutorials()
        {
            if (!Application.isPlaying) return;

            foreach (var dialog in Object.FindObjectsByType<TutorialDialog>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                dialog.ResetTip();
            foreach (var hint in Object.FindObjectsByType<HintPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                hint.ResetTutorial();
        }
    }
}
