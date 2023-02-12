using System.Linq;
using UnityEngine;

public class PlayerController : BasePawnController
{
    public static PlayerController Instance { get; protected set; }


    [Header("Control Settings")]
    bool m_IsPaused = false;
    CharacterController _characterController;



    #region Cameras
    // camera 1p
    [HideInInspector]
    public Camera Camera1P_Main;

    [Header("Camera")]
    public float Camera1P_Main_FOV = 60;

    public Transform Camera1P_Position;
    //[HideInInspector]
    //public Camera Camera1P_Weapon;

    float m_HorizontalAngle;
    float m_VerticalAngle;


    // camera 3p
    [HideInInspector]
    public Camera Camera3P;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    // cinemachine
    public GameObject CinemachineCameraTarget;
    private float _cinemachineTargetPitch;
    private float _cinemachineTargetYaw;


    // camera3P aim position
    public Transform aimPos;
    [SerializeField] LayerMask aimMask;
    [SerializeField] float aimSmoothSpeed = 20;
    #endregion

    //bool wasGrounded;

    public bool LockControl { get; set; }
    public bool CanPause { get; set; } = true;

    public void DisplayCursor(bool display)
    {
        m_IsPaused = display;
        Cursor.lockState = display ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = display;
    }

    protected override void Awake()
    {
        Instance = this;
        
        _characterController = GetComponent<CharacterController>();
        
        InitCamera();
        weapon1PLayer = EditorLayer.Weapon1P;
        weapon3PLayer = EditorLayer.Weapon3P;

        base.Awake();
    }

    protected override void Start()
    {
        m_IsPaused = false;
        SetCameraLayer();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _mouseSensitivityUse = MouseSensitivity;

        m_VerticalAngle = 0.0f;
        m_HorizontalAngle = transform.localEulerAngles.y;

        base.Start();
    }

    protected override void Update()
    {
        UpdateControl();

        

        //FullscreenMap.Instance.gameObject.SetActive(Input.GetButton("Map"));
        UpdateAimPosition();



        base.Update();
    }

    private void UpdateControl()
    {
        // menu
        if (CanPause && Input.GetKeyDown(KeyCode.Escape))
        {
            // todo menu
            //PauseMenu.Instance.Display();
        }

        if (!m_IsPaused && !LockControl)
        {
            hasControl = true;
        }

        if (hasControl)
        {
            // switch camera
            if (Input.GetKeyDown(KeyCode.C))
            {
                SwitchCamera(!isFirstPerson);
            }

            // Switch weapon
            #region Inventory Key
            for (int i = 0; i < 10; ++i)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    // start form 0
                    //int bagId = 0;
                    //if (i == 0)
                    //    bagId = 10;
                    //else
                    //    bagId = i - 1;

                    // id = key
                    int bagId = i;
                    if (bagId == 0)
                    {
                        bagId = 10;
                    }

                    // 0-2
                    if (bagId <= GlobalConstants.BagSize)
                    {
                        if (bagId != (int)_currentBagPos)
                        {
                            ChangeWeapon(bagId);
                        }
                    }
                }
            }
            #endregion
        }
    }

    private void LateUpdate()
    {
        UpdateCamera();
    }

    /// <summary>
    /// Move
    /// Jump
    /// </summary>
    protected override void UpdateMovement()
    {
        bool wasGrounded = m_Grounded;
        bool loosedGrounding = false;

        #region Grounded
        //we define our own grounded and not use the Character controller one as the character controller can flicker
        //between grounded/not grounded on small step and the like. So we actually make the controller falling/"not grounded" only
        //if the character controller reported not being grounded for at least .5 second;

        // isGrounded 这个的调用必须在m_CharacterController.Move(move)
        // 执行之后，避免isGrounded一直为false
        // https://blog.csdn.net/weixin_42430021/article/details/123452679
        if (!_characterController.isGrounded)
        {
            if (m_Grounded)
            {
                m_GroundedTimer += Time.deltaTime;
                if (m_GroundedTimer >= 0.5f)
                {
                    loosedGrounding = true;
                    m_Grounded = false;
                }
            }
        }
        else
        {
            m_GroundedTimer = 0.0f;
            m_Grounded = true;
        }
        #endregion

        #region Movement
        _speed = 0;
        Vector3 move = Vector3.zero;
        // jump
        if (hasControl)
        {
            if (m_Grounded && Input.GetKeyDown(KeyCode.Space))
            {
                // v^2 = 2gh
                float JumpSpeed = Mathf.Sqrt(2f * Gravity * JumpHeight);

                m_VerticalSpeed = JumpSpeed;
                m_Grounded = false;
                loosedGrounding = true;
                if (FootstepPlayer != null)
                {
                    FootstepPlayer.PlayClip(JumpingAudioCLip, 0.8f, 1.1f);
                }

                animController.TriggerJump();
            }
        }

        // move
        if (hasControl)
        {
            // walk
            bool walking = Input.GetKeyDown(KeyCode.LeftShift);//m_Weapons[m_CurrentWeapon].CurrentState == Weapon.WeaponState.Idle &&
            float actualSpeed = walking ? WalkSpeed : RunSpeed;

            if (loosedGrounding)
            {
                m_SpeedAtJump = actualSpeed;
            }

            // wasd
            _hzInput = Input.GetAxis("Horizontal");
            _vInput = Input.GetAxisRaw("Vertical");

            move = new Vector3(_hzInput,
                0,
                _vInput);
            if (move.sqrMagnitude > 1.0f)
                move.Normalize();

            float usedSpeed = m_Grounded ? actualSpeed : m_SpeedAtJump;
            move = move * usedSpeed * Time.deltaTime;

            move = transform.TransformDirection(move);
            _characterController.Move(move);

            _speed = move.magnitude / (RunSpeed * Time.deltaTime);
        }

        #endregion

        // Fall down / gravity
        m_VerticalSpeed = m_VerticalSpeed - Gravity * Time.deltaTime;
        if (m_VerticalSpeed < -MaxFallSpeed)
            m_VerticalSpeed = -MaxFallSpeed; // max fall speed

        var verticalMove = new Vector3(0, m_VerticalSpeed * Time.deltaTime, 0);
        var flag = _characterController.Move(verticalMove);
        if ((flag & CollisionFlags.Below) != 0)
            m_VerticalSpeed = 0;

        if (!wasGrounded && m_Grounded)
        {
            if (FootstepPlayer != null)
            {
                FootstepPlayer.PlayClip(LandingAudioClip, 0.8f, 1.1f);
            }
        }
    }

    #region Cameras
    private void InitCamera()
    {
        var cameras = GetComponentsInChildren<Camera>();

        // 1p
        Camera1P_Main = cameras
            .Where(it => it.name == nameof(Camera1P_Main)).FirstOrDefault();
        Camera1P_Main.transform.SetParent(Camera1P_Position, false);
        Camera1P_Main.transform.localPosition = Vector3.zero;
        Camera1P_Main.transform.localRotation = Quaternion.identity;

        // not in use
        //Camera1P_Weapon = cameras
        //    .Where(it => it.name == nameof(Camera1P_Weapon)).FirstOrDefault();

        // 3p
        Camera3P = cameras
            .Where(it => it.name == nameof(Camera3P)).FirstOrDefault();

        SwitchCamera(isFirstView: true);
    }

    /// <summary>
    /// call in start
    /// </summary>
    private void SetCameraLayer()
    {
        Camera1P_Main.cullingMask = GameSystem.Instance
            ._camera1PLayer;

        Camera3P.cullingMask = GameSystem.Instance
            ._camera3PLayer;
    }

    protected void SwitchCamera(bool isFirstView)
    {
        Camera1P_Main.enabled = isFirstView;
        //Camera1P_Weapon.enabled = isFirstView;

        Camera3P.enabled = !isFirstView;

        isFirstPerson = isFirstView;
    }

    private void UpdateAimPosition()
    {
        if (aimPos != null)
        {
            Ray ray = Camera1P_Main
                .ViewportPointToRay(Vector3.one * 0.5f);

            // aim down 
            //ray.direction += Vector3.down * 0.3f;
            Vector3 hitPosition = ray.origin + ray.direction * 100.0f;
            aimPos.position = Vector3.Lerp(aimPos.position, hitPosition, aimSmoothSpeed * Time.deltaTime);
        }
    }

    private void UpdateCamera()
    {
        if (hasControl)
        {
            var mouseX = Input.GetAxis("Mouse X");
            var mouseY = -Input.GetAxis("Mouse Y");

            if (mouseX.HasValue()
                || mouseY.HasValue())
            {
                #region 1P
                // mouse x
                float turnPlayer = mouseX * _mouseSensitivityUse;
                m_HorizontalAngle = m_HorizontalAngle + turnPlayer;

                if (m_HorizontalAngle > 360)
                {
                    m_HorizontalAngle -= 360.0f;
                }
                if (m_HorizontalAngle < 0)
                {
                    m_HorizontalAngle += 360.0f;
                }

                Vector3 currentAngles = transform.localEulerAngles;
                currentAngles.y = m_HorizontalAngle;
                transform.localEulerAngles = currentAngles;

                // mouse y
                var turnCam = mouseY;
                turnCam = turnCam * _mouseSensitivityUse;
                m_VerticalAngle = Mathf.Clamp(turnCam + m_VerticalAngle,
                    MinCameraAngle,
                    MaxCameraAngle);
                currentAngles = Camera1P_Position.transform.localEulerAngles;
                currentAngles.x = m_VerticalAngle;
                Camera1P_Position.transform.localEulerAngles = currentAngles;
                #endregion

                #region 3P
                var LockCameraPosition = false;
                var IsCurrentDeviceMouse = true;

                if (!LockCameraPosition)
                {
                    float deltaTimeMultiplier = IsCurrentDeviceMouse ?
                        1.0f :
                        Time.deltaTime;

                    deltaTimeMultiplier *= _mouseSensitivityUse;

                    _cinemachineTargetYaw += mouseX * deltaTimeMultiplier;
                    _cinemachineTargetPitch += mouseY * deltaTimeMultiplier;
                }

                // clamp our rotations so our values are limited 360 degrees
                _cinemachineTargetYaw = CameraHelper. ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
                _cinemachineTargetPitch = CameraHelper. ClampAngle(_cinemachineTargetPitch, MinCameraAngle, MaxCameraAngle);

                // Cinemachine will follow this target
                CinemachineCameraTarget.transform.rotation = Quaternion
                    .Euler(_cinemachineTargetPitch + CameraAngleOverride,
                    _cinemachineTargetYaw, 0.0f);
                #endregion
            }
        }
    }

    #endregion

    public override Ray GetShotRay()
    {
        // spread
        float spreadRatio = GetCurrentWeapon.weaponRecoil.spreadAngle
            / Camera1P_Main.fieldOfView;
        Vector2 spread = spreadRatio * UnityEngine.Random.insideUnitCircle;

        // ray from camera1P
        Ray ray = Camera1P_Main
            .ViewportPointToRay(Vector3.one * 0.5f
            + (Vector3)spread);

        return ray;
    }
}
