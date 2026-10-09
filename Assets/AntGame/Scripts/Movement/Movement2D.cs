using System;
using UnityEngine;
using AntGame.Core.Navigation;

namespace AntGame.Movement
{
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
        private float rotationOffset;

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

            float angle =
                Mathf.Atan2(direction.y, direction.x) *
                Mathf.Rad2Deg +
                rotationOffset;

            body.MoveRotation(angle);
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