using System;
using _Project.Develop.Editing.Photo;

namespace _Project.Develop
{
    public class EventBus : IService
    {
        public Action AnySidesChanged;
        public Action BothSidesChanged;

        public Action SideDragging;

        public Action StartEditing;
        public Action EditingAno;
        
        public Action<EditingSpot> DustEnter;
        public Action<EditingSpot> DirtEnter;
        
        public Action PhotoEnter;
        public Action PhotoExit;

        public Action SpotsFinished;
        public Action PhotoDown;

        public Action EndEditing;

        public Action ChecklistDown;

        public Action Pause;
        public Action Resume;

        public Action PreSceneReset;
        public Action SceneReset;
        public Action SceneReady;
        
        public Action<int, int> AnxietyLevelChanged;

        public Action EffectApply;
        public Action EffectRemove;
        
        public Action<int> PassNight;
        public Action GoodEnd;
        public Action AverageEnd;
        public Action BadEnd;
    }
}