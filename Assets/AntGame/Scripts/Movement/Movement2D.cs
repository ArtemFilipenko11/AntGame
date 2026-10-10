using System;
using UnityEngine;
using AntGame.Core.Navigation;

namespace AntGame.Movement
{

    public enum MovementForwardAxis
        {
            Right,
            Up,
            Left,
            Down
        }

    [RequireComponent(typeof(Rigidbody2D))]
    public class Movement2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float maxSpeed = 1f;

        [SerializeField, Min(0.01f)]
        private float acceleration = 5f;

        [SerializeField, Min(0.01f)]
        private float deceleration = 7f;

        [SerializeField, Min(0f)]
        private float stoppingDistance = 0.05f;

        [SerializeField, Min(0.01f)]
        private float waypointReachDistance = 0.08f;

        [SerializeField]
        private MovementForwardAxis localForwardAxis =
            MovementForwardAxis.Right;

        private Rigidbody2D body;
        private INavigationSystem navigation;
        private Vector2 destination;
        private Path path;
        private int waypointIndex;
        private float currentSpeed;
        private Vector2 movementDirection;
        private Vector2 facingDirection = Vector2.up;
        private bool hasDestination;
        private bool initialized;

        public event Action MovementStarted;
        public event Action<MovementEndReason> MovementEnded;


        public float CurrentSpeed => currentSpeed;
        public float MaxSpeed => maxSpeed;
        public bool HasDestination => hasDestination;
        public Vector2 MovementDirection => movementDirection;
        public Vector2 FacingDirection => facingDirection;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (initialized && navigation != null)
                navigation.NavigationChanged += OnNavigationChanged;
        }

        private void OnDisable()
        {
            if (navigation != null)
                navigation.NavigationChanged -= OnNavigationChanged;

            hasDestination = false;
            path = null;
            currentSpeed = 0f;
            movementDirection = Vector2.zero;
        }

        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();

            if (body == null)
                return;

            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            body.constraints = RigidbodyConstraints2D.None;
        }

        public bool Initialize(INavigationSystem navigationSystem)
        {
            if (initialized || navigationSystem == null)
                return false;

            if (body == null)
                body = GetComponent<Rigidbody2D>();

            if (body == null)
                return false;

            navigation = navigationSystem;
            navigation.NavigationChanged += OnNavigationChanged;
            initialized = true;

            return true;
        }

        public bool MoveTo(Vector2 target)
        {
            if (!initialized || !isActiveAndEnabled)
                return false;

            Path newPath = BuildPath(target);

            if (newPath == null || newPath.Count == 0)
                return false;

            destination = target;
            path = newPath;
            waypointIndex = 0;
            hasDestination = true;

            SkipReachedWaypoints();
            MovementStarted?.Invoke();
            return true;
        }

        public bool MoveTo(
            Transform target,
            float approachDistance = 0f)
        {
            if (!initialized ||
                !isActiveAndEnabled ||
                target == null)
            {
                return false;
            }

            approachDistance = Mathf.Max(0f, approachDistance);

            Vector2 actorPosition = body.position;

            Collider2D targetCollider =
                target.GetComponent<Collider2D>();

            if (targetCollider == null)
            {
                targetCollider =
                    target.GetComponentInChildren<Collider2D>(true);
            }

            Vector2 approachPoint;

            if (targetCollider != null &&
                targetCollider.enabled &&
                targetCollider.gameObject.activeInHierarchy)
            {
                approachPoint = GetApproachPoint(
                    targetCollider,
                    actorPosition,
                    approachDistance
                );
            }
            else
            {
                approachPoint = GetPointAroundTarget(
                    target.position,
                    actorPosition,
                    approachDistance
                );
            }

            return MoveTo(approachPoint);
        }

        private Vector2 GetApproachPoint(
            Collider2D targetCollider,
            Vector2 actorPosition,
            float approachDistance)
        {
            Vector2 boundaryPoint =
                targetCollider.ClosestPoint(actorPosition);

            Vector2 direction = actorPosition - boundaryPoint;

            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();

                return boundaryPoint +
                    direction * approachDistance;
            }

            Bounds bounds = targetCollider.bounds;

            Vector2 center = bounds.center;

            direction = actorPosition - center;

            if (direction.sqrMagnitude <= 0.0001f)
                direction = Vector2.up;

            direction.Normalize();

            float distanceX =
                Mathf.Abs(direction.x) > 0.0001f
                    ? bounds.extents.x / Mathf.Abs(direction.x)
                    : float.PositiveInfinity;

            float distanceY =
                Mathf.Abs(direction.y) > 0.0001f
                    ? bounds.extents.y / Mathf.Abs(direction.y)
                    : float.PositiveInfinity;

            float distanceToBoundary =
                Mathf.Min(distanceX, distanceY);

            boundaryPoint =
                center + direction * distanceToBoundary;

            return boundaryPoint +
                direction * approachDistance;
        }

        private Vector2 GetPointAroundTarget(
            Vector2 targetPosition,
            Vector2 actorPosition,
            float approachDistance)
        {
            Vector2 direction = actorPosition - targetPosition;

            if (direction.sqrMagnitude <= 0.0001f)
                direction = Vector2.up;

            return targetPosition +
                direction.normalized * approachDistance;
        }

        public void Stop()
        {
            bool wasMoving = hasDestination;

            hasDestination = false;
            path = null;
            currentSpeed = 0f;
            movementDirection = Vector2.zero;

            if (wasMoving)
                MovementEnded?.Invoke(MovementEndReason.Cancelled);
        }

        private void FixedUpdate()
        {
            if (!initialized || !hasDestination || path == null)
                return;

            SkipReachedWaypoints();

            if (waypointIndex >= path.Count)
            {
                UpdateMovementDirection(destination - body.position);

                if (Vector2.Distance(body.position, destination)
                    <= stoppingDistance)
                {
                    FinishMovement(MovementEndReason.Reached);
                    return;
                }

                MoveToward(destination, true);
                return;
            }

            Vector2 target = path[waypointIndex];
            bool isFinalWaypoint = waypointIndex == path.Count - 1;

            UpdateMovementDirection(target - body.position);
            MoveToward(target, isFinalWaypoint);
        }

        private void MoveToward(Vector2 target, bool isFinalWaypoint)
        {
            Vector2 offset = target - body.position;
            float distance = offset.magnitude;

            if (distance <= 0.001f)
            {
                if (isFinalWaypoint)
                    FinishMovement(MovementEndReason.Reached);

                return;
            }

            float brakingDistance =
                currentSpeed * currentSpeed / (2f * deceleration);

            if (isFinalWaypoint &&
                distance <= stoppingDistance + brakingDistance)
            {
                Decelerate();
            }
            else
            {
                Accelerate();
            }

            float step = currentSpeed * Time.fixedDeltaTime;
            step = Mathf.Min(step, distance);

            body.MovePosition(
                body.position + offset / distance * step);

            RotateToward(movementDirection);

            if (isFinalWaypoint &&
                distance <= stoppingDistance &&
                currentSpeed <= deceleration * Time.fixedDeltaTime)
            {
                FinishMovement(MovementEndReason.Reached);
            }
        }

        private Path BuildPath(Vector2 target)
        {
            return navigation?.FindPath(body.position, target);
        }

        private void SkipReachedWaypoints()
        {
            if (path == null)
                return;

            while (waypointIndex < path.Count)
            {
                float distance = Vector2.Distance(
                    body.position,
                    path[waypointIndex]);

                if (distance > waypointReachDistance)
                    break;

                waypointIndex++;
            }
        }

        private void UpdateMovementDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            movementDirection = direction.normalized;
        }

        private void RotateToward(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            facingDirection = direction.normalized;

            float angle = Vector2.SignedAngle(
                GetLocalForwardDirection(),
                facingDirection
            );

            body.MoveRotation(angle);
        }

        private Vector2 GetLocalForwardDirection()
        {
            switch (localForwardAxis)
            {
                case MovementForwardAxis.Up:
                    return Vector2.up;

                case MovementForwardAxis.Left:
                    return Vector2.left;

                case MovementForwardAxis.Down:
                    return Vector2.down;

                default:
                    return Vector2.right;
            }
        }

        private void Accelerate()
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed,
                acceleration * Time.fixedDeltaTime);
        }

        private void Decelerate()
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                deceleration * Time.fixedDeltaTime);
        }

        private void FinishMovement(MovementEndReason reason)
        {
            hasDestination = false;
            path = null;
            currentSpeed = 0f;
            movementDirection = Vector2.zero;

            MovementEnded?.Invoke(reason);
        }

        private void OnNavigationChanged()
        {
            if (!hasDestination)
                return;

            Path newPath = BuildPath(destination);

            if (newPath == null || newPath.Count == 0)
            {
                FinishMovement(MovementEndReason.PathBlocked);
                return;
            }

            path = newPath;
            waypointIndex = 0;

            SkipReachedWaypoints();
        }
    }
}