using System;
using _Project.Develop.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Develop.UI.Cursor
{
    public class CursorInput : IDisposable
    {
        public Vector2 MousePosition { get; private set; }

        private UICursor _cursor;
        
        public CursorInput(UICursor cursor)
        {
            _cursor = cursor;
        }

        public void Initialize()
        {
            var input = G.Get<InputController>();
            input.Player.Player.MousePosition.performed += OnMousePosition;
            input.Player.Player.Click.started += OnMouseDown;
            input.Player.Player.Click.canceled += OnMouseUp;
        }

        private void OnMousePosition(InputAction.CallbackContext ctx)
        {
            MousePosition = ctx.ReadValue<Vector2>();
        }

        private void OnMouseDown(InputAction.CallbackContext ctx)
        {
            _cursor.SetState(UICursor.CursorState.Click);
        }

        private void OnMouseUp(InputAction.CallbackContext ctx)
        {
            _cursor.SetState(UICursor.CursorState.Idle);
        }

        public void Dispose()
        {
            var input = G.Get<InputController>();
            input.Player.Player.MousePosition.performed -= OnMousePosition;
            input.Player.Player.MousePosition.started -= OnMouseDown;
            input.Player.Player.MousePosition.canceled -= OnMouseUp;
        }
    }
}