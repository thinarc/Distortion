using System;

namespace _Project.Develop.Input
{
    public class InputController : IService, IDisposable
    {
        public PlayerInput Player { get; } = new();

        public void Init()
        {
            Player.Enable();
        }

        public void Dispose()
        {
            Player.Disable();
        }
    }
}