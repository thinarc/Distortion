using System;

namespace _Project.Develop.Input
{
    public class InputController : IService, IDisposable
    {
        public PlayerInput Player { get; } = new();

        public void Initialize()
        {
            Player.Enable();
        }

        public void Dispose()
        {
            Player.Disable();
        }
    }
}