using UnityEngine.Localization.Settings;

namespace _Project.Develop.UI
{
    // Reference copy: Sprites/_Examples/Tutorial/!tutorialText.docx.
    // The editing lines follow the current game order rather than the old mockups.
    public static class TutorialDialogueText
    {
        private const string Important = "<color=#081661>";
        private const string End = "</color>";

        public static string Get(int number)
        {
            var locale = LocalizationSettings.SelectedLocale;
            return locale == null || locale.Identifier.Code.StartsWith("ru") ? Russian(number) : English(number);
        }

        private static string Russian(int number) => number switch
        {
            1 => "Вот чёрт. Мне нужно срочно собрать фотоархив семьи Куркиных до утра, в редакции уже ругаются...",
            2 => "Лампочка перегорела. Ну конечно. Самое время. Ладно, уличный фонарь, сегодня ты спасёшь меня от увольнения.",
            3 => "Пока я работаю, " + Important + "следует держать окно открытым." + End + " Ужасные обстоятельства.",
            4 => "Мне следует " + Important + "иногда отвлекаться от работы и проверять окно." + End + " Если увижу " +
                 Important + "что-то странное," + End + " тут же " + Important + "закрою штору" + End + " на пару секунд.",
            5 => "Ну, пора за работу! Нужно закончить до 6 утра.",
            6 => "Всё просто. Мне нужно пройтись по фото " + Important + "щёткой, потом клячкой, а затем нанести защитный слой." + End,
            7 => "Отлично. Теперь надо " + Important + "перевернуть фото" + End + " и " + Important + "сверить дату" + End +
                 " с моим списком из редакции. Она должна совпадать.",
            8 => "Фото уже в архиве. " + Important + "Зачеркну нужный пункт," + End + " чтобы не запутаться.",
            9 => "Сверю дату с моим списком. Если всё совпало, можно " + Important + "положить фото в архив." + End,
            10 => "Если дата не совпадает, я " + Important + "уберу фото в ящик." + End + " Наверное, попало случайно.",
            11 => "Моя комната пугает меня. Она похожа на гроб.",
            12 => "И меня беспокоит постоянное дурное предчувствие... Всё-таки следует иногда " + Important +
                  "отвлекаться от работы и смотреть в окно." + End + " Мало ли что.",
            13 => "Ну, за работу! Нужно успеть выполнить норму до 6 утра и не сойти с ума без света. Делов-то...",
            _ => throw new System.ArgumentOutOfRangeException(nameof(number))
        };

        private static string English(int number) => number switch
        {
            1 => "Damn. I need to put together the Kurkin family photo archive by morning. The editors are already complaining...",
            2 => "The bulb has burned out. Of course. Perfect timing. All right, streetlight, tonight you're saving my job.",
            3 => "While I'm working, " + Important + "I need to keep the window open." + End + " What awful circumstances.",
            4 => "I should " + Important + "take breaks from work and check the window." + End + " If I see " + Important +
                 "anything strange," + End + " I'll " + Important + "close the curtains" + End + " for a couple of seconds.",
            5 => "Well, time to get to work! I need to finish by 6 a.m.",
            6 => "It's simple. I need to clean the photo with " + Important + "the brush, then the kneaded eraser, and finally apply a protective coat." + End,
            7 => "Great. Now I need to " + Important + "turn the photo over" + End + " and " + Important + "check the date" + End +
                 " against the list from the editors. It has to match.",
            8 => "The photo is in the archive. I'll " + Important + "cross out the right entry" + End + " so I don't get confused.",
            9 => "I'll check the date against my list. If it matches, I can " + Important + "add the photo to the archive." + End,
            10 => "If the date doesn't match, I'll " + Important + "put the photo in the drawer." + End + " It probably got mixed in by accident.",
            11 => "My room scares me. It looks like a coffin.",
            12 => "And I can't shake this feeling of dread... I really should " + Important + "take breaks from work and look out the window." + End + " Just in case.",
            13 => "Right, back to work! I need to finish my quota by 6 a.m. without losing my mind in the dark. Easy enough...",
            _ => throw new System.ArgumentOutOfRangeException(nameof(number))
        };
    }
}
