using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private AudioSource step;
        
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float airControlMultiplier = 0.3f;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private float capsuleHeight = 2f;
        
        [Header("Camera Reference")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform orientation;
        
        private Vector2 _moveInput;
        private bool _jumpInput;
        private bool _sprintInput;
        
        private bool _isGrounded;
        private bool _isJumping;
        private float _jumpHoldTimer;
        private bool _isDead = false;
        
        public bool IsGrounded => _isGrounded;
        public Vector3 Velocity => rb.linearVelocity;
        
        private void Awake()
        {
            if (!rb) rb = GetComponent<Rigidbody>();
            
            SetupRigidbody();
            
            if (!groundCheck)
            {
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0, -capsuleHeight + groundCheckRadius, 0);
                groundCheck = groundCheckObj.transform;
            }
            
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
            CheckGrounded();
            HandleJump();
            
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
        
        public void SetJumpInput(bool input)
        {
            _jumpInput = input;
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
            if (_sprintInput && _isGrounded)
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
            
            float controlMultiplier;
            if (_isGrounded) controlMultiplier = 1f;
            else controlMultiplier = airControlMultiplier;
            
            Vector3 targetVelocity = moveDirection * (targetSpeed * controlMultiplier);
            targetVelocity.y = rb.linearVelocity.y;
            
            rb.linearVelocity = targetVelocity;

            step.enabled = true;
        }
        
        private void CheckGrounded()
        {
            Vector3 checkPosition = groundCheck.position;
            _isGrounded = Physics.CheckSphere(checkPosition, groundCheckRadius, groundLayer);
            
            if (_isGrounded && rb.linearVelocity.y <= 0.1f)
            {
                _isJumping = false;
            }
        }
        
        private void HandleJump()
        {
            if (_jumpInput && _isGrounded && !_isJumping)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _isJumping = true;
            }
        }
        
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }
        
        public void SetJumpForce(float force)
        {
            jumpForce = force;
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

