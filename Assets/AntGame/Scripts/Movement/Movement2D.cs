using UnityEngine;

namespace AntGame.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Movement2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float maxSpeed = 2f;

        [SerializeField, Min(0.01f)]
        private float acceleration = 5f;

        [SerializeField, Min(0.01f)]
        private float deceleration = 7f;

        [SerializeField, Min(0f)]
        private float stoppingDistance = 0.05f;

        [SerializeField, Min(0f)]
        private float stopSpeedThreshold = 0.01f;

        private Rigidbody2D body;

        private Vector2 destination;
        private bool hasDestination;
        private float currentSpeed;

        public float CurrentSpeed => currentSpeed;
        public float MaxSpeed => maxSpeed;
        public bool HasDestination => hasDestination;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();

            if (body == null)
                return;

            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void MoveTo(Vector2 target)
        {
            destination = target;
            hasDestination = true;
        }

        public void Stop()
        {
            hasDestination = false;
        }

        private void FixedUpdate()
        {
            if (!hasDestination)
            {
                Decelerate();

                if (currentSpeed <= stopSpeedThreshold)
                    currentSpeed = 0f;

                return;
            }

            Vector2 toTarget = destination - body.position;
            float distance = toTarget.magnitude;

            if (distance <= stoppingDistance)
            {
                body.MovePosition(destination);
                currentSpeed = 0f;
                hasDestination = false;

                return;
            }

            Vector2 direction = toTarget / distance;

            float brakingDistance =
                (currentSpeed * currentSpeed) /
                (2f * deceleration);

            bool shouldBrake =
                distance <= stoppingDistance + brakingDistance;

            if (shouldBrake)
            {
                Decelerate();

                if (currentSpeed <= stopSpeedThreshold)
                {
                    currentSpeed = 0f;
                    body.MovePosition(destination);
                    hasDestination = false;

                    return;
                }
            }
            else
            {
                Accelerate();
            }

            float movementDistance =
                currentSpeed * Time.fixedDeltaTime;

            movementDistance =
                Mathf.Min(movementDistance, distance);

            Vector2 movement =
                direction * movementDistance;

            body.MovePosition(body.position + movement);
        }

        private void Accelerate()
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }

        private void Decelerate()
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                deceleration * Time.fixedDeltaTime
            );
        }
    }
}