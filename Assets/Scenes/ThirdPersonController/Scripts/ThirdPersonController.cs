using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Rigging")]
        [Tooltip("Rig Builder для управления анимациями при прицеливании")]
        public RigBuilder rigBuilder;

        [Header("Aim Settings")]
        [Tooltip("Минимальный угол между направлением камеры и персонажа для поворота")]
        public float AimRotationThreshold = 30.0f;

        [Tooltip("Скорость сглаживания поворота при прицеливании")]
        public float AimRotationSmoothTime = 0.1f;

        [Header("Sprint Settings")]
        [Tooltip("Максимальное время ускорения (в секундах)")]
        public float maxSprintTime = 10f;

        [Tooltip("Скорость восстановления полоски ускорения при стоянии (в секундах)")]
        public float sprintRechargeRateStanding = 0.5f;

        [Tooltip("Скорость восстановления полоски ускорения при движении (в секундах)")]
        public float sprintRechargeRateMoving = 0.25f;

        [Tooltip("Минимальное значение полоски ускорения для активации ускорения (в процентах)")]
        public float minSprintThreshold = 0.25f;

        private float currentSprintTime;
        private bool isSprinting = false;
        private float baseSpeed;

        [Header("UI")]

        [Tooltip("Обычный Canvas для отображения UI во время игры")]
        public GameObject regularCanvas;

        [Tooltip("UI элемент для отображения полоски ускорения")]
        public Image sprintBar;
        [Tooltip("UI элемент для отображения здоровья (текст)")]
        public TextMeshProUGUI hpText;

        [Tooltip("UI элемент для отображения полоски здоровья")]
        public Image hpBar;

        [Header("HP")]
        [Tooltip("Здоровье")]
        public int HP = 100;



        [Header("Death Settings")]
        [Tooltip("Интерфейс, который отображается после смерти")]
        public GameObject deathUI;

        [Tooltip("Текст для отображения статистики после смерти")]
        public TextMeshProUGUI statsText;

        private bool isDead = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;


        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        public GameObject arrowObject;
        public Transform arrowPoint;
        public GameObject playerFollowCamera;
        public GameObject playerAimCamera;
        public Transform mouseTarget;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            currentSprintTime = maxSprintTime;
            baseSpeed = MoveSpeed;
        }

        private void Update()
        {
            if (isDead) return;

            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();

            Move();
            UpdateSprint();
            UpdateSprintBar();

            AimShoot();
            RayMouse();
            UpdateHPText();

        }

        private void AimShoot()
        {
            if (_input.isAiming && Grounded && !_input.sprint)
            {
                _animator.SetBool("Aiming", _input.isAiming);
                _animator.SetBool("Shooting", _input.isShooting);
                playerFollowCamera.SetActive(false);
                playerAimCamera.SetActive(true);

                // Получаем направление камеры и персонажа
                Vector3 cameraForward = CinemachineCameraTarget.transform.forward;
                Vector3 playerForward = transform.forward;

                // Игнорируем вертикальную составляющую
                cameraForward.y = 0;
                playerForward.y = 0;

                // Вычисляем угол между направлением камеры и персонажа
                float angleBetween = Vector3.Angle(cameraForward, playerForward);

                // Если угол превышает пороговое значение, поворачиваем персонажа
                if (angleBetween > AimRotationThreshold)
                {
                    // Вычисляем целевой угол поворота
                    float targetRotation = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

                    // Плавный поворот с использованием SmoothDampAngle
                    float rotation = Mathf.SmoothDampAngle(
                        transform.eulerAngles.y, // Текущий угол персонажа
                        targetRotation,          // Целевой угол
                        ref _rotationVelocity,   // Ссылка на переменную для хранения скорости
                        AimRotationSmoothTime   // Время сглаживания
                    );

                    // Применяем поворот
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

                if (rigBuilder != null)
                {
                    rigBuilder.enabled = true;
                }
            }
            else
            {
                _animator.SetBool("Aiming", false);
                _animator.SetBool("Shooting", false);
                playerFollowCamera.SetActive(true);
                playerAimCamera.SetActive(false);

                if (rigBuilder != null)
                {
                    rigBuilder.enabled = false;
                }
            }
        }

        public void Shoot()
        {
            // Проверяем, существует ли mouseTarget
            if (mouseTarget != null)
            {
                // Вычисляем направление от ArrowPoint к MouseTarget
                Vector3 direction = (mouseTarget.position - arrowPoint.position).normalized;

                // Создаем стрелу в позиции ArrowPoint
                GameObject arrow = Instantiate(arrowObject, arrowPoint.position, Quaternion.LookRotation(direction));

                // Применяем силу к стреле в направлении MouseTarget
                arrow.GetComponent<Rigidbody>().AddForce(direction * 25f, ForceMode.Impulse);

                // Визуализация луча от ArrowPoint до MouseTarget
                Debug.DrawRay(arrowPoint.position, direction * 100f, Color.green, 2f);
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // Устанавливаем целевую скорость в зависимости от состояния спринта
            float targetSpeed = isSprinting ? SprintSpeed : MoveSpeed;

            // Если нет движения, сбрасываем скорость
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
        private void RayMouse()
        {
            // Проверяем, используется ли мышь
            if (IsCurrentDeviceMouse)
            {
                // Создаем луч из центра экрана
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                RaycastHit hit;

                // Визуализация луча в редакторе
                Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);

                // Если луч пересекается с объектом в мире
                if (Physics.Raycast(ray, out hit))
                {
                    // Если mouseTarget задан, перемещаем его в точку пересечения
                    if (mouseTarget != null)
                    {
                        mouseTarget.position = hit.point;
                    }
                }
                else
                {
                    // Если луч не пересекается с объектом, перемещаем mouseTarget на большое расстояние по направлению луча
                    if (mouseTarget != null)
                    {
                        // Задаем большое расстояние (например, 1000 единиц)
                        float distance = 1000f;
                        mouseTarget.position = ray.origin + ray.direction * distance;
                    }
                }
            }
        }
        private void UpdateSprint()
        {
            
            // Минимальное значение полоски ускорения для активации ускорения (25% от maxSprintTime)
            float minSprintThreshold = maxSprintTime * 0.25f;

            // Проверка нажатия Shift и наличия запаса в полоске ускорения
            if (_input.sprint && currentSprintTime > 0)
            {
                // Ускорение можно использовать только если полоска выше 25% или ускорение уже активно
                if (currentSprintTime >= minSprintThreshold || isSprinting)
                {
                    isSprinting = true;
                    MoveSpeed = SprintSpeed; // Увеличиваем скорость
                    currentSprintTime -= Time.deltaTime; // Уменьшаем запас ускорения
                }
                else
                {
                    // Если полоска меньше 25% и ускорение не активно, ускорение не запускается
                    isSprinting = false;
                    MoveSpeed = baseSpeed; // Возвращаем стандартную скорость
                }
            }
            else
            {
                isSprinting = false;
                MoveSpeed = baseSpeed; // Возвращаем стандартную скорость
            }

            // Восстановление полоски ускорения
            if (!isSprinting)
            {
                float rechargeRate = (_input.move == Vector2.zero) ? sprintRechargeRateStanding : sprintRechargeRateMoving;
                currentSprintTime += rechargeRate * Time.deltaTime;
                currentSprintTime = Mathf.Clamp(currentSprintTime, 0, maxSprintTime); // Ограничиваем значение
            }

            // Обновляем UI полоски ускорения
            UpdateSprintBar();
        }
        private void UpdateSprintBar()
        {
            if (sprintBar != null)
            {
                sprintBar.fillAmount = currentSprintTime / maxSprintTime; // Обновляем заполнение полоски
            }
        }

        public void TakeDamage(int damageAmount)
        {
            HP -= damageAmount;
            if (HP <= 0)
            {
                AudioManager.instance.Play("Player_Death");
                _animator.SetTrigger("Die");
                //GetComponent<Collider>().enabled = false;
                Die();
            }
            else
            {
                string[] damageSounds = { "Player_Damage1", "Player_Damage2" };
                int randomIndex = Random.Range(0, damageSounds.Length);
                AudioManager.instance.Play(damageSounds[randomIndex]);
                _animator.SetTrigger("Damage");
            }

            // Обновляем текст HP
            UpdateHPText();
        }
        private void UpdateHPText()
        {
            if (hpText != null)
            {
                hpText.text = $"HP: {HP}"; // Обновляем текст HP
            }

            if (hpBar != null)
            {
                // Обновляем заполнение полоски HP (от 0 до 1)
                hpBar.fillAmount = (float)HP / 100f; // Предполагаем, что максимальное HP = 100
            }
        }



        public void Die()
        {
            isDead = true; // Устанавливаем состояние смерти

            // Отключаем управление персонажем
            _input.enabled = false;
            _controller.enabled = false;

            // Останавливаем анимации
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, 0f);
                _animator.SetBool(_animIDGrounded, true);
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // Скрываем обычный UI
            HideRegularUI();

            // Разблокируем мышку
            UnlockMouse();

            // Показываем интерфейс после смерти
            ShowDeathUI();
        }


        private void ShowDeathUI()
        {
            if (deathUI != null)
            {
                deathUI.SetActive(true); // Активируем интерфейс после смерти

                //// Пример отображения статистики
                //if (statsText != null)
                //{
                //    statsText.text = $"Пройдено расстояние: {_controller.velocity.magnitude:F2} м\n" +
                //                     $"Время выживания: {Time.timeSinceLevelLoad:F2} сек";
                //}
            }
        }

        private void UnlockMouse()
        {
            Cursor.lockState = CursorLockMode.None; // Разблокируем курсор
            Cursor.visible = true; // Делаем курсор видимым
        }

        private void HideRegularUI()
        {
            if (regularCanvas != null)
            {
                regularCanvas.SetActive(false); // Отключаем обычный Canvas
            }
        }

        public void ReturnToMainMenu()
        {
            // Загружаем главное меню 
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

    }
}