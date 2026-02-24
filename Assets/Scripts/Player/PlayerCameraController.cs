using UI.Settings;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivityX = 2f;
        [SerializeField] private float mouseSensitivityY = 2f;
        [SerializeField] private float verticalLookLimit = 80f;
        [SerializeField] private bool useGlobalSensitivity = true;
        [SerializeField] private PlayerInputHandler inputHandler;
        
        [Header("References")]
        [SerializeField] private Transform orientation;
        [SerializeField] private Camera mainCamera;
        
        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private Transform cameraFollowTarget;
        [SerializeField] private Transform cameraLookAtTarget;
        
        [Header("Auto Look Up")]
        [SerializeField] private bool playIntroTilt = true;
        [SerializeField] private float introDuration = 1.5f;
        [SerializeField] private float targetXRotationAfterIntro = -5f;
        
        private float _xRotation = 10f;
        private float _yRotation;
        private bool _isIntroPlaying;
        private float _introTimer;
        private float _startXRotation;
        
        private void Awake()
        {
            SetupOrientation();
            SetupCameraTargets();
            SetupMainCamera();
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (playIntroTilt)
            {
                _isIntroPlaying = true;
                _introTimer = 0f;
                _startXRotation = _xRotation;
            }
            
            _yRotation = transform.eulerAngles.y;
        }
        
        private void Start()
        {
            if (useGlobalSensitivity && GameSettingsManager.Instance)
            {
                UpdateSensitivity(GameSettingsManager.Instance.MouseSensitivity);
                GameSettingsManager.Instance.OnSensitivityChanged += UpdateSensitivity;
            }
        }
        
        private void OnDestroy()
        {
            if (GameSettingsManager.Instance)
            {
                GameSettingsManager.Instance.OnSensitivityChanged -= UpdateSensitivity;
            }
        }
        
        private void UpdateSensitivity(float sensitivity)
        {
            mouseSensitivityX = sensitivity * 6;
            mouseSensitivityY = sensitivity * 6;
        }
        
        private void SetupOrientation()
        {
            if (!orientation)
            {
                GameObject orientationObj = new GameObject("Orientation");
                orientationObj.transform.SetParent(transform);
                orientationObj.transform.localPosition = Vector3.zero;
                orientation = orientationObj.transform;
            }
        }
        
        private void SetupCameraTargets()
        {
            if (!cameraFollowTarget)
            {
                GameObject followObj = new GameObject("CameraFollowTarget");
                followObj.transform.SetParent(transform);
                followObj.transform.localPosition = Vector3.up * 1.6f;
                cameraFollowTarget = followObj.transform;
            }
            
            if (!cameraLookAtTarget)
            {
                GameObject lookAtObj = new GameObject("CameraLookAtTarget");
                lookAtObj.transform.SetParent(transform);
                lookAtObj.transform.localPosition = Vector3.up * 1.6f;
                cameraLookAtTarget = lookAtObj.transform;
            }
        }
        
        private void SetupMainCamera()
        {
            if (!mainCamera)
            {
                GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
                if (camObj)
                {
                    mainCamera = camObj.GetComponent<Camera>();
                }
                else
                {
                    mainCamera = Camera.main;
                }
            }
        }
        
        private void Update()
        {
            if (!inputHandler) return;
            
            float deltaTime = Mathf.Min(Time.deltaTime, 0.033f);
            
            HandleCameraRotation(deltaTime);
        }
        
        private void LateUpdate()
        {
            ApplyRotations();
        }
        
        private void HandleCameraRotation(float deltaTime)
        {
            Vector2 lookInput = inputHandler.LookInput;
            
            float mouseX = lookInput.x * deltaTime * mouseSensitivityX * 50f;
            float mouseY = lookInput.y * deltaTime * mouseSensitivityY * 50f;
            
            mouseX = lookInput.x * deltaTime * mouseSensitivityX;
            mouseY = lookInput.y * deltaTime * mouseSensitivityY;
            
            _yRotation += mouseX;
            _xRotation -= mouseY;
            
            if (_isIntroPlaying)
            {
                _introTimer += deltaTime;
                float timer = Mathf.Clamp01(_introTimer / introDuration);
                _xRotation = Mathf.Lerp(_startXRotation, targetXRotationAfterIntro, timer);
                
                if (timer >= 1f)
                {
                    _isIntroPlaying = false;
                }
            }
            
            _xRotation = Mathf.Clamp(_xRotation, -verticalLookLimit, verticalLookLimit);
        }
        
        private void ApplyRotations()
        {
            if (orientation)
            {
                orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
            }
            
            transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            
            if (cameraFollowTarget)
            {
                cameraFollowTarget.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            }
            
            if (cameraLookAtTarget)
            {
                cameraLookAtTarget.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            }
            
            if (mainCamera)
            {
                mainCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            }
        }
        
        public Transform GetOrientation() => orientation;
        public Transform GetCameraFollowTarget() => cameraFollowTarget;
    }
}

