using UnityEngine;
using AntGame.Systems;

namespace AntGame.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraPawn : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameplayContext gameplayContext;

        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float moveSpeed = 8f;

        [Header("Zoom")]
        [SerializeField, Min(0.001f)]
        private float zoomSpeed = 0.01f;

        [SerializeField, Min(0.1f)]
        private float minZoom = 2f;

        [SerializeField, Min(0.1f)]
        private float maxZoom = 12f;

        private Camera targetCamera;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
        }

        public void Move(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0f)
                return;

            Vector3 movement =
                (Vector3)(direction.normalized *
                          moveSpeed * Time.unscaledDeltaTime);

            SetPosition(transform.position + movement);
        }

        public void Pan(Vector2 screenDelta)
        {
            if (targetCamera == null ||
                !targetCamera.orthographic)
            {
                return;
            }

            float unitsPerPixel =
                targetCamera.orthographicSize * 2f /
                Mathf.Max(1, Screen.height);

            Vector3 movement = new Vector3(
                -screenDelta.x,
                -screenDelta.y,
                0f
            ) * unitsPerPixel;

            SetPosition(transform.position + movement);
        }

        public void Zoom(float scroll)
        {
            if (targetCamera == null ||
                !targetCamera.orthographic)
            {
                return;
            }

            float newSize =
                targetCamera.orthographicSize -
                scroll * zoomSpeed;

            targetCamera.orthographicSize = Mathf.Clamp(
                newSize,
                minZoom,
                maxZoom
            );

            SetPosition(transform.position);
        }

        private void SetPosition(Vector3 position)
        {
            if (gameplayContext != null &&
                gameplayContext.NavigationSystem != null &&
                targetCamera != null &&
                targetCamera.orthographic)
            {
                Rect bounds =
                    gameplayContext.NavigationSystem.WorldBounds;

                float halfHeight = targetCamera.orthographicSize;
                float halfWidth =
                    halfHeight * targetCamera.aspect;

                position.x = ClampAxis(
                    position.x,
                    bounds.xMin + halfWidth,
                    bounds.xMax - halfWidth,
                    bounds.center.x
                );

                position.y = ClampAxis(
                    position.y,
                    bounds.yMin + halfHeight,
                    bounds.yMax - halfHeight,
                    bounds.center.y
                );
            }

            transform.position = position;
        }

        private float ClampAxis(
            float value,
            float min,
            float max,
            float center)
        {
            if (min > max)
                return center;

            return Mathf.Clamp(value, min, max);
        }
    }
}