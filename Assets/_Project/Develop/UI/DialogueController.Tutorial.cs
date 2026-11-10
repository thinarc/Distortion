using System.Collections.Generic;
using _Project.Develop.UI.Curtain;
using _Project.Develop.UI.Menus;

namespace _Project.Develop.UI
{
    public partial class DialogueController
    {
        private sealed class TutorialCue
        {
            public string Location;
            public string[] Lines;
            public bool CompletesTutorial;
            public int LastLine;
        }

        private readonly List<TutorialCue> _tutorialCues = new();
        private readonly HashSet<int> _tutorialLines = new();
        private string _tutorialLocation = "Workplace";
        private string _activeTutorialLocation;
        private bool _tutorialDialogueEnabled;
        private bool _restartTutorialDialogue;
        private bool _firstPhotoCompleted;
        private int _editingDialoguePhase;
        private bool _activeCueCompletesTutorial;
        private int _activeTutorialLastLine;
        private readonly HashSet<int> _readTutorialCues = new();
        private bool _visitedStreet;
        private bool _returnedFromStreet;
        private bool _reachedEditing;

        public bool TutorialDialogueCompleted { get; private set; }

        public bool CanNavigateTo(string location)
        {
            if (!_tutorialDialogueEnabled || TutorialDialogueCompleted || !TutorialSettings.ShouldShow) return true;
            if (_reachedEditing)
                return _tutorialLocation == "Editing"
                    ? _firstPhotoCompleted && location == "Workplace"
                    : HasReadTutorialCue(13);

            return _tutorialLocation switch
            {
                "Workplace" => _returnedFromStreet
                    ? location == "Editing" && HasReadTutorialCue(5)
                    : location == "Window" && HasReadTutorialCue(2),
                "Window" => _visitedStreet
                    ? location == "Workplace" && HasReadTutorialCue(5)
                    : location == "Street" && G.Has<UICurtain>() && G.Get<UICurtain>().BothOpen && HasReadTutorialCue(3),
                "Street" => location == "Window" && HasReadTutorialCue(4),
                _ => false
            };
        }

        private bool HasReadTutorialCue(int lastLine) => _readTutorialCues.Contains(lastLine) ||
            IsActive && _activeTutorialLastLine == lastLine && _index == _lines.Length - 1 &&
            _prepareLine == null && !_writer.IsWriting;

        private void MarkTutorialCueRead()
        {
            if (_activeTutorialLastLine != 0) _readTutorialCues.Add(_activeTutorialLastLine);
        }

        private void StartTutorialDialogue()
        {
            _restartTutorialDialogue = false;
            if (!TutorialSettings.ShouldShow || _tutorialDialogueEnabled || _tutorialLines.Count > 0) return;
            _tutorialDialogueEnabled = true;
            QueueTutorialCue("Workplace", 1, 2);
        }

        private void Update()
        {
            if (!_tutorialDialogueEnabled) return;
            if (!TutorialSettings.ShouldShow)
            {
                StopTutorialDialogue();
                return;
            }

            // Observe the existing game. These messages never wait for or gate an action.
            switch (_tutorialLocation)
            {
                case "Window":
                    if (G.Has<UICurtain>() && G.Get<UICurtain>().BothOpen) QueueTutorialCue("Window", 3);
                    if (_tutorialLines.Contains(4)) QueueTutorialCue("Window", 5);
                    break;
                case "Street":
                    QueueTutorialCue("Street", 4);
                    break;
                case "Editing":
                    if (_editingDialoguePhase == 0) QueueTutorialCue("Editing", 6);
                    break;
                case "Workplace":
                    if (_firstPhotoCompleted) QueueTutorialCue("Workplace", 11, 12, 13);
                    break;
            }

            if (IsActive || !_locationVisible || _paused || _effect) return;
            var index = _tutorialCues.FindIndex(cue => cue.Location == _tutorialLocation);
            if (index < 0) return;
            var cue = _tutorialCues[index];
            _tutorialCues.RemoveAt(index);
            Show(cue.Lines);
            _activeTutorialLocation = cue.Location;
            _activeCueCompletesTutorial = cue.CompletesTutorial;
            _activeTutorialLastLine = cue.LastLine;
        }

        private void QueueTutorialCue(string location, params int[] numbers)
        {
            if (!_tutorialDialogueEnabled || !TutorialSettings.ShouldShow || _tutorialLines.Contains(numbers[0])) return;
            var lines = new string[numbers.Length];
            for (var i = 0; i < numbers.Length; i++)
            {
                _tutorialLines.Add(numbers[i]);
                lines[i] = TutorialDialogueText.Get(numbers[i]);
            }
            _tutorialCues.Add(new TutorialCue
            {
                Location = location, Lines = lines, CompletesTutorial = numbers[^1] == 13, LastLine = numbers[^1]
            });
        }

        private void ChangeTutorialLocation(string location)
        {
            if (_activeTutorialLocation != null && _activeTutorialLocation != location && IsActive)
            {
                // An allowed departure completes this step even if its dialogue is still open.
                MarkTutorialCueRead();
                if (_activeCueCompletesTutorial) TutorialDialogueCompleted = true;
                Close();
            }
            _tutorialCues.RemoveAll(cue => cue.Location == _tutorialLocation);
            _activeTutorialLocation = null;
            _tutorialLocation = location;
            if (_tutorialDialogueEnabled)
            {
                if (location == "Street") _visitedStreet = true;
                if (location == "Window" && _visitedStreet) _returnedFromStreet = true;
                if (location == "Editing") _reachedEditing = true;
            }
        }

        private void QueueEditingDialogue(int phase, params int[] numbers)
        {
            if (!_tutorialDialogueEnabled || phase <= _editingDialoguePhase) return;
            _editingDialoguePhase = phase;
            // If the player has already performed an action, don't show its old instructions later.
            _tutorialCues.RemoveAll(cue => cue.Location == "Editing");
            if (_activeTutorialLocation == "Editing" && IsActive) Close();
            _activeTutorialLocation = null;
            QueueTutorialCue("Editing", numbers);
        }

        private void OnPhotoCleaned() => QueueEditingDialogue(1, 7);
        private void OnPhotoFlipped() => QueueEditingDialogue(2, 9, 10);
        public void ShowChecklistDialogue() => QueueEditingDialogue(3, 8);

        private void OnPhotoCompleted()
        {
            if (!_tutorialDialogueEnabled || _firstPhotoCompleted) return;
            _firstPhotoCompleted = true;
            _editingDialoguePhase = 4;
            _tutorialCues.RemoveAll(cue => cue.Location == "Editing");
            if (_activeTutorialLocation == "Editing" && IsActive) Close();
            _activeTutorialLocation = null;
        }

        private void StopTutorialDialogue()
        {
            _tutorialDialogueEnabled = false;
            _tutorialCues.Clear();
            _activeTutorialLocation = null;
            Close();
        }

        private void ResetTutorialDialogue()
        {
            StopTutorialDialogue();
            TutorialDialogueCompleted = false;
            _readTutorialCues.Clear();
            _visitedStreet = false;
            _returnedFromStreet = false;
            _reachedEditing = false;
            _tutorialLines.Clear();
            _editingDialoguePhase = 0;
            _firstPhotoCompleted = false;
            _restartTutorialDialogue = true;
        }

        private void RestartTutorialDialogue()
        {
            if (_restartTutorialDialogue) StartTutorialDialogue();
        }
    }
}
