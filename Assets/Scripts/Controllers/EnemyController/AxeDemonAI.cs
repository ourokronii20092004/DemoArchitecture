using UnityEngine;
using Attrition.Controllers;

public class AxeDemonAI : MonoBehaviour
{
    private EnemyController controller;
    private Transform targetPlayer;

    [Header("--- AI Settings ---")]
    public float detectRadius = 8f;
    public float chaseSpeed = 3f;
    public float attackInterval = 2f;
    private float nextAttackTime;

    [Header("--- Patrol Settings ---")]
    public float patrolSpeed = 1.5f;
    public float patrolRange = 5f; // Khoảng cách đi sang trái/phải từ điểm xuất phát
    public float waitAtPoint = 2f; // Thời gian đứng đợi ở mỗi đầu điểm tuần tra

    private Vector2 startPosition;
    private Vector2 patrolTarget;
    private bool movingRight = true;
    private float waitTimer;
    private bool isWaiting = false;

    void Start()
    {
        controller = GetComponent<EnemyController>();
        startPosition = transform.position;
        SetNextPatrolTarget();
    }

    void FixedUpdate()
    {
        if (controller == null || controller.IsDead()) return;

        FindClosestPlayer();

        // 1. Logic Tấn công & Đuổi (Nếu thấy người)
        if (targetPlayer != null && Vector2.Distance(transform.position, targetPlayer.position) <= detectRadius)
        {
            isWaiting = false; // Hủy đợi nếu thấy người
            float distance = Vector2.Distance(transform.position, targetPlayer.position);

            if (distance <= controller.attackRange)
            {
                StopAndAttack();
            }
            else
            {
                ChasePlayer();
            }
        }
        // 2. Logic Tuần tra (Nếu KHÔNG thấy người)
        else
        {
            Patrol();
        }
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = p.transform;
            }
        }
        targetPlayer = closest;
    }

    void ChasePlayer()
    {
        controller.anim?.SetFloat("Speed", 1f);
        Vector2 direction = (targetPlayer.position - (Vector3)transform.position).normalized;
        controller.rb.linearVelocity = new Vector2(direction.x * chaseSpeed, controller.rb.linearVelocity.y);
        FlipSprite(direction.x);
    }

    void Patrol()
    {
        if (isWaiting)
        {
            controller.rb.linearVelocity = new Vector2(0, controller.rb.linearVelocity.y);
            controller.anim?.SetFloat("Speed", 0f);
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                SetNextPatrolTarget();
            }
            return;
        }

        controller.anim?.SetFloat("Speed", 0.5f); // Tốc độ đi bộ tuần tra chậm hơn
        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        controller.rb.linearVelocity = new Vector2(direction.x * patrolSpeed, controller.rb.linearVelocity.y);
        FlipSprite(direction.x);

        // Nếu đến gần điểm tuần tra thì dừng lại đợi
        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
        {
            isWaiting = true;
            waitTimer = waitAtPoint;
        }
    }

    void SetNextPatrolTarget()
    {
        if (movingRight)
            patrolTarget = startPosition + Vector2.right * patrolRange;
        else
            patrolTarget = startPosition + Vector2.left * patrolRange;

        movingRight = !movingRight;
    }

    void StopAndAttack()
    {
        controller.rb.linearVelocity = new Vector2(0, controller.rb.linearVelocity.y);
        controller.anim?.SetFloat("Speed", 0f);
        FlipSprite(targetPlayer.position.x - transform.position.x);

        if (Time.time >= nextAttackTime)
        {
            controller.anim?.SetTrigger("Attack");
            nextAttackTime = Time.time + attackInterval;
        }
    }

    void FlipSprite(float dirX)
    {
        if (dirX > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (dirX < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    public void AlertAndFacePlayer()
    {
        if (targetPlayer != null) FlipSprite(targetPlayer.position.x - transform.position.x);
    }

    // Vẽ đường tuần tra trong Editor để dễ căn chỉnh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector2 start = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.DrawLine(start + Vector2.left * patrolRange, start + Vector2.right * patrolRange);
        Gizmos.DrawWireSphere(start + Vector2.left * patrolRange, 0.3f);
        Gizmos.DrawWireSphere(start + Vector2.right * patrolRange, 0.3f);
    }
}