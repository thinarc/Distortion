using System;
using _Project.Develop.Editing.Photo;

namespace _Project.Develop
{
    public class EventBus : IService
    {
        public Action CurtainChanged;

        public Action<EditingSpot> DustEnter;
        public Action<EditingSpot> DirtEnter;

        public Action PhotoEnter;
        public Action PhotoExit;

        public Action PhotoDown;

        public Action ChecklistDown;
    }
}