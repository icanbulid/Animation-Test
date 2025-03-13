using UnityEngine;
using UnityEngine.UI; // Для работы с UI
using Unity.Cinemachine; // Для работы с Cinemachine

public class PlayerMovement : MonoBehaviour
{
    public LayerMask collisionMask; // исключить слой игрока

    public float moveSpeed;
    public float baseSpeed = 5f;
    public float sprintSpeed = 5f;

    public float mouseSensitivity = 100f;

    public float orbitDistance = 5f; // Расстояние камеры от персонажа
    public float orbitHeight = 2f; // Высота камеры относительно персонажа
    public float minCameraDistance = 1f; // Минимальное расстояние камеры до персонажа
    private bool isAiming = false; // Флаг для режима прицеливания персонажа

    private float xRotation = 0f;
    private float yRotation = 0f;
    public float rotationSmoothness = 10f; // Плавность вращения персонажа
    private Animator animator;

    public GameObject projectilePrefab; // Префаб объекта, который будет выстреливаться
    public Transform firePoint; // Точка, откуда будут вылетать объекты
    public float projectileForce = 30f; // Сила выстрела
    public float fireRate = 0.5f; // Скорострельность (выстрелов в секунду)
    private float nextFireTime = 0f; // Время следующего выстрела

    private CharacterController characterController;

    // Гравитация
    private Vector3 velocity; // Вектор скорости (включая вертикальную скорость)
    public float gravity = -9.81f; // Ускорение свободного падения
    public float groundCheckDistance = 0.1f; // Расстояние для проверки нахождения на земле
    public LayerMask groundMask; // Слой, который считается землей
    private bool isGrounded; // Флаг, указывающий, находится ли персонаж на земле

    public float jumpHeight = 1.5f; // Высота прыжка
    public Transform groundCheck; // Пустой объект для проверки земли

    // Ускорение
    public float maxSprintTime = 10f; // Максимальное время ускорения (10 секунд)
    private float currentSprintTime; // Текущее время ускорения
    public float sprintRechargeRateStanding = 0.5f; // Скорость восстановления полоски ускорения при стоянии (20 секунд)
    public float sprintRechargeRateMoving = 0.25f; // Скорость восстановления полоски ускорения при движении (10 секунд)
    private bool isSprinting = false; // Флаг, указывающий, что персонаж ускоряется

    // UI для полоски ускорения
    public Image sprintBar; // Полоска ускорения (UI Image)

    public Image crosshair; // Ссылка на прицел (UI Image)
    public Transform shoulderCameraPosition; // Позиция камеры за плечом
    public float aimTransitionSpeed = 5f; // Скорость перехода камеры в режим прицеливания

    public int HP = 100;

    // Cinemachine
    [SerializeField] public CinemachineCamera freeLookCamera; // Обычная камера
    [SerializeField] public CinemachineVirtualCamera aimCamera; // Камера для прицеливания

    void Start()
    {
        Application.targetFrameRate = 24;
        // Заблокировать и скрыть курсор
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>(); // Получаем компонент CharacterController

        // Инициализация полоски ускорения
        currentSprintTime = maxSprintTime;
        UpdateSprintBar();

        // Отключаем прицел по умолчанию
        if (crosshair != null)
        {
            crosshair.enabled = false;
        }

        // Инициализация камер
        freeLookCamera.Priority = 20; // Высокий приоритет для обычной камеры
        aimCamera.Priority = 10; // Низкий приоритет для камеры прицеливания
    }

    void Update()
    {
        // Проверка, находится ли персонаж на земле
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);

        // Если персонаж на земле и вертикальная скорость меньше 0, сбрасываем её
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Небольшое отрицательное значение, чтобы персонаж "прилип" к земле
        }

        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Debug.Log("Jump pressed and grounded");
            Jump();
        }

        // Движение персонажа
        float moveX = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
        float moveZ = Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime;

        // Ускорение
        HandleSprint(moveX, moveZ, baseSpeed, sprintSpeed);

        Vector3 move;

        if (isAiming)
        {
            // В режиме прицеливания движение относительно персонажа
            move = transform.right * moveX + transform.forward * moveZ;
        }
        else
        {
            // В обычном режиме движение относительно камеры
            Vector3 cameraForward = freeLookCamera.transform.forward;
            Vector3 cameraRight = freeLookCamera.transform.right;
            cameraForward.y = 0; // Игнорируем наклон камеры вверх/вниз
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            move = cameraRight * moveX + cameraForward * moveZ;
            // Плавное вращение персонажа в сторону движения
            if (move.magnitude > 0.1f) // Если есть движение
            {
                Quaternion targetRotation = Quaternion.LookRotation(move.normalized); // Целевое вращение
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime); // Плавное вращение
            }
        }

        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;

        // Перемещаем персонажа с учетом гравитации
        characterController.Move(move + velocity * Time.deltaTime);

        // Управление анимацией
        if (moveX != 0 || moveZ != 0)
        {
            animator.SetBool("IsRunning", true);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }

        // Переключение режима прицеливания
        if (Input.GetMouseButtonDown(1)) // 1 - это правая кнопка мыши
        {
            isAiming = true;
            Debug.Log("isAiming");

            // Включаем прицел
            if (crosshair != null)
            {
                crosshair.enabled = true;
            }
            // Переключаем камеру на режим прицеливания
            freeLookCamera.Priority = 10; // Низкий приоритет
            aimCamera.Priority = 20; // Высокий приоритет
        }
        if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            Debug.Log("Not Aiming");

            // Отключаем прицел
            if (crosshair != null)
            {
                crosshair.enabled = false;
            }
            // Возвращаем обычную камеру
            freeLookCamera.Priority = 20; // Высокий приоритет
            aimCamera.Priority = 10; // Низкий приоритет
        }

        // Вращение камеры
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        if (isAiming)
        {
            // Плавное вращение персонажа по оси Y (влево-вправо)
            float targetYRotation = yRotation + mouseX * mouseSensitivity * Time.deltaTime;
            yRotation = Mathf.Lerp(yRotation, targetYRotation, rotationSmoothness * Time.deltaTime);

            // Вращение камеры по оси X (вверх-вниз)
            xRotation -= mouseY * mouseSensitivity * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Ограничиваем угол наклона камеры

            // Плавное вращение персонажа
            Quaternion targetRotation = Quaternion.Euler(0f, yRotation, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);

            // Стрельба в режиме прицеливания
            if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime) // 0 - это левая кнопка мыши
            {
                Attack();
                nextFireTime = Time.time + fireRate; // Устанавливаем время следующего выстрела
            }
        }
        else
        {
            // Режим полного вращения камеры вокруг персонажа
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Ограничиваем угол наклона камеры

            yRotation += mouseX; // Вращение по оси Y

            // Вращение камеры вокруг персонажа
            Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);
            Vector3 orbitPosition = rotation * new Vector3(0f, orbitHeight, -orbitDistance);

            freeLookCamera.transform.position = transform.position + orbitPosition;
            freeLookCamera.transform.rotation = rotation;
        }
    }

    void HandleSprint(float moveX, float moveZ, float baseSpeed, float sprintSpeed)
    {
        // Минимальное значение полоски ускорения для активации ускорения (25% от maxSprintTime)
        float minSprintThreshold = maxSprintTime * 0.25f;

        // Проверка нажатия Shift и наличия запаса в полоске ускорения
        if (Input.GetKey(KeyCode.LeftShift) && currentSprintTime > 0)
        {
            // Ускорение можно использовать только если полоска выше 25% или ускорение уже активно
            if (currentSprintTime >= minSprintThreshold || isSprinting)
            {
                isSprinting = true;
                moveSpeed = sprintSpeed; // Увеличиваем скорость (5 * 1.5)
                currentSprintTime -= Time.deltaTime; // Уменьшаем запас ускорения
            }
            else
            {
                // Если полоска меньше 25% и ускорение не активно, ускорение не запускается
                isSprinting = false;
                moveSpeed = baseSpeed; // Возвращаем стандартную скорость
            }
        }
        else
        {
            isSprinting = false;
            moveSpeed = baseSpeed; // Возвращаем стандартную скорость
        }

        // Восстановление полоски ускорения
        if (!isSprinting)
        {
            float rechargeRate = (moveX == 0 && moveZ == 0) ? sprintRechargeRateStanding : sprintRechargeRateMoving;
            currentSprintTime += rechargeRate * Time.deltaTime;
            currentSprintTime = Mathf.Clamp(currentSprintTime, 0, maxSprintTime); // Ограничиваем значение
        }

        // Обновляем UI полоски ускорения
        UpdateSprintBar();
    }

    void UpdateSprintBar()
    {
        if (sprintBar != null)
        {
            sprintBar.fillAmount = currentSprintTime / maxSprintTime;
        }
    }

    void Jump()
    {
        // Применяем вертикальную скорость для прыжка
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void Attack()
    {
        // Логика атаки (например, выстрел)
        Shoot();
    }

    void Shoot()
    {
        // Получаем направление взгляда игрока
        Vector3 shootDirection = aimCamera.transform.forward;

        // Создаем объект (пулю) и выстреливаем его
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * projectileForce, ForceMode.Impulse);
            // Включаем гравитацию для снаряда
            rb.useGravity = true;
        }

        // Уничтожаем объект через некоторое время, чтобы избежать накопления объектов в сцене
        Destroy(projectile, 5f);
    }



}