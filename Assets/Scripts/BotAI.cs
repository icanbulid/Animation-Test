using UnityEngine;

public class BotAI : MonoBehaviour
{
    public float fieldOfViewAngle = 110f; // Угол обзора бота
    public float sightRange = 10f; // Дальность зрения бота
    public float escapeRange = 15f; // Дистанция, на которую бот убегает от врага
    public float minEscapeRange = 5f; // Дистанция, на которую бот убегает от врага в любом случае
    public float moveSpeed = 3f; // Скорость движения бота
    public float alertDuration = 30f; // Время в режиме настороже

    private Transform enemy; // Трансформ врага
    private bool enemyInSight; // Флаг, указывающий, виден ли враг
    private float alertTimer; // Таймер для режима настороже
    private enum BotState { Calm, Alert, Panic } // Режимы бота
    private BotState currentState = BotState.Calm; // Текущий режим бота

    private Animator animator;
    private Vector3 lastPosition; // Последняя позиция бота для определения движения

    public float maxHealth = 100f; // Максимальное здоровье
    private float currentHealth; // Текущее здоровье

    void Start()
    {
        currentHealth = maxHealth; // Устанавливаем текущее здоровье на максимум
        // Получаем компонент Animator
        animator = GetComponent<Animator>();
        // Инициализируем последнюю позицию
        lastPosition = transform.position;
    }

    void Update()
    {


        // Поиск врага в поле зрения
        FindEnemy();

// Логика в зависимости от режима
        switch (currentState)
        {
            case BotState.Calm:
                // Если враг в зоне побега, переключаемся в режим паники
                if ((enemyInSight && IsEnemyInEscapeRange()) || IsEnemyInMinEscapeRange())
                {
                    SwitchState(BotState.Panic);
                }
                break;

            case BotState.Alert:
                // Уменьшаем таймер настороже
                alertTimer -= Time.deltaTime;

                // Если враг в зоне побега, переключаемся в режим паники
                if ((enemyInSight && IsEnemyInEscapeRange()) || IsEnemyInMinEscapeRange())
                {
                    SwitchState(BotState.Panic);
                }
                // Если таймер истек, переключаемся в режим спокойствия
                else if (alertTimer <= 0)
                {
                    SwitchState(BotState.Calm);
                }
                break;

            case BotState.Panic:
                // Убегаем от врага
                RunAwayFromEnemy();

                // Если враг вышел из зоны побега, переключаемся в режим настороже
                if (!IsEnemyInEscapeRange())
                {
                    SwitchState(BotState.Alert);
                    alertTimer = alertDuration; // Сбрасываем таймер настороже
                }
                break;
        }
        // Управление анимацией
        UpdateAnimation();
    }

    void FindEnemy()
    {
        // Сбрасываем флаг
        enemyInSight = false;

        // Получаем все объекты с тегом "Enemy" в радиусе sightRange
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sightRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                // Проверяем, находится ли враг в поле зрения
                Vector3 directionToEnemy = hitCollider.transform.position - transform.position;
                float angle = Vector3.Angle(directionToEnemy, transform.forward);

                if (angle < fieldOfViewAngle * 0.5f)
                {
                    // Проверяем, нет ли препятствий между ботом и врагом
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, directionToEnemy.normalized, out hit, sightRange))
                    {
                        if (hit.collider.CompareTag("Enemy"))
                        {
                            enemy = hitCollider.transform;
                            enemyInSight = true;
                        }
                    }
                }
            }
        }
    }

    void RunAwayFromEnemy()
    {
        if (enemy != null)
        {
            // Направление от врага к боту
            Vector3 directionAwayFromEnemy = transform.position - enemy.position;
            directionAwayFromEnemy.Normalize();

            // Двигаем бота в противоположную сторону от врага
            transform.position += directionAwayFromEnemy * moveSpeed * Time.deltaTime;

            // Поворачиваем бота в направлении движения
            Quaternion targetRotation = Quaternion.LookRotation(directionAwayFromEnemy);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    bool IsEnemyInEscapeRange()
    {
        // Проверяем, находится ли враг в зоне побега
        if (enemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.position);
            return distanceToEnemy <= escapeRange;
        }
        return false;
    }

    bool IsEnemyInMinEscapeRange()
    {
        // Проверяем, находится ли враг в зоне побега
        if (enemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.position);
            return distanceToEnemy <= minEscapeRange;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        // Рисуем поле зрения в редакторе
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Рисуем угол обзора
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward * sightRange;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward * sightRange;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Рисуем область побега
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, escapeRange);


        // Рисуем минимальную область побега
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minEscapeRange);
    }

    void UpdateAnimation()
    {
        // Определяем, движется ли бот
        bool isMoving = (transform.position != lastPosition);

        // Обновляем анимацию
        animator.SetBool("IsRunning", isMoving);

        // Обновляем последнюю позицию
        lastPosition = transform.position;
    }

    void SwitchState(BotState newState)
    {
        // Логируем смену состояния
        Debug.Log("Бот переключился из режима " + currentState + " в режим " + newState);

        // Устанавливаем новое состояние
        currentState = newState;
    }

    void ProjectileCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Debug.Log("ZZZ");
        }
    }

    void OnGUI()
    {
        // Рисуем полоску здоровья
        Vector2 pos = Camera.main.WorldToScreenPoint(transform.position);
        GUI.color = Color.red;
        GUI.HorizontalSlider(new Rect(pos.x - 50, Screen.height - pos.y - 50, 100, 20), currentHealth, 0, maxHealth);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            TakeDamage(50f); // Пример: бот теряет 50 единиц здоровья при попадании
            Destroy(collision.gameObject); // Уничтожаем снаряд после попадания
        }
    }

    void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Бот умер");
        // Здесь можно добавить логику для уничтожения бота или других действий при смерти
        Destroy(gameObject);
    }
}
