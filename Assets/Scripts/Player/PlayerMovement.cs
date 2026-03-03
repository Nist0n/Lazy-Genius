using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private AudioSource step;
        
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float airControlMultiplier = 0.3f;
        
        [Header("Camera Reference")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform orientation;
        
        private Vector2 _moveInput;
        private bool _sprintInput;

        private bool _isDead = false;

        public Vector3 Velocity => rb.linearVelocity;
        
        private void Awake()
        {
            if (!rb) rb = GetComponent<Rigidbody>();
            
            SetupRigidbody();
            
            if (!cameraTransform)
            {
                Camera mainCam = Camera.main;
                if (mainCam) cameraTransform = mainCam.transform;
            }

            step.enabled = false;
        }
        
        private void SetupRigidbody()
        {
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        private void Update()
        {
            if (!cameraTransform)
            {
                Camera mainCam = Camera.main;
                if (mainCam) cameraTransform = mainCam.transform;
            }
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
        }
        
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }
        
        public void SetSprintInput(bool input)
        {
            _sprintInput = input;
        }
        
        public void SetCameraTransform(Transform camTransform)
        {
            cameraTransform = camTransform;
        }
        
        public void SetOrientation(Transform orient)
        {
            orientation = orient;
        }
        
        private void HandleMovement()
        {
            if (_isDead) return;
            
            if (_moveInput.magnitude < 0.1f)
            {
                step.enabled = false;
                Vector3 velocity = rb.linearVelocity;
                velocity.x = 0f;
                velocity.z = 0f;
                rb.linearVelocity = velocity;
                return;
            }
            
            float targetSpeed = moveSpeed;
            if (_sprintInput)
            {
                targetSpeed = sprintSpeed;
            }
            
            Vector3 moveDirection;
            
            if (orientation)
            {
                moveDirection = orientation.forward * _moveInput.y + orientation.right * _moveInput.x;
            }
            else if (cameraTransform)
            {
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                
                forward.y = 0f;
                right.y = 0f;
                
                forward.Normalize();
                right.Normalize();
                
                moveDirection = forward * _moveInput.y + right * _moveInput.x;
            }
            else
            {
                moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;
            }
            
            moveDirection.Normalize();
            
            
            Vector3 targetVelocity = moveDirection * (targetSpeed);
            targetVelocity.y = rb.linearVelocity.y;
            
            rb.linearVelocity = targetVelocity;

            step.enabled = true;
        }
        
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }
        
        public void SetDead(bool dead)
        {
            _isDead = dead;
            if (dead)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}

