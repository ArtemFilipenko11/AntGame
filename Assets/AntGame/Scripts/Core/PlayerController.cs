using UnityEngine;
using UnityEngine.InputSystem;

namespace AntGame.Presentation
{
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Controlled Pawn")]
        [SerializeField]
        private CameraPawn cameraPawn;

        private Vector2 previousMousePosition;

        private void Update()
        {
            HandleKeyboardMovement();
            HandleMouseDrag();
            HandleZoom();
        }

        private void HandleKeyboardMovement()
        {
            if (Keyboard.current == null || cameraPawn == null)
                return;

            Vector2 direction = Vector2.zero;

            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
                direction.y += 1f;

            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
                direction.y -= 1f;

            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
                direction.x -= 1f;

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
                direction.x += 1f;

            cameraPawn.Move(direction);
        }

        private void HandleMouseDrag()
        {
            if (Mouse.current == null || cameraPawn == null)
                return;

            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                previousMousePosition =
                    Mouse.current.position.ReadValue();
            }

            if (!Mouse.current.middleButton.isPressed)
                return;

            Vector2 currentMousePosition =
                Mouse.current.position.ReadValue();

            Vector2 delta =
                currentMousePosition - previousMousePosition;

            cameraPawn.Pan(delta);

            previousMousePosition = currentMousePosition;
        }

        private void HandleZoom()
        {
            if (Mouse.current == null || cameraPawn == null)
                return;

            float scroll = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) > 0.01f)
                cameraPawn.Zoom(scroll);
        }
    }
}