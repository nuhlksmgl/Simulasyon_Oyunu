using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] private float walkRadius = 5f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private Transform plane; // Plane nesnesini Inspector'dan atamak için

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 targetPosition;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private Vector3 lastDirection = Vector3.zero;
    private Vector2 planeSize;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null || animator == null)
        {
            Debug.LogError("NavMeshAgent veya Animator bileþeni eksik!");
            return;
        }

        if (plane != null)
        {
            Renderer planeRenderer = plane.GetComponent<Renderer>();
            if (planeRenderer != null)
            {
                planeSize = new Vector2(planeRenderer.bounds.size.x / 2, planeRenderer.bounds.size.z / 2);
            }
            else
            {
                Debug.LogError("Plane nesnesinde Renderer bileþeni bulunamadý!");
            }
        }
        else
        {
            Debug.LogError("Plane nesnesi atanmadý!");
        }

        SetNewDestination();
    }

    void Update()
    {
        if (animator == null) return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        // NPC hareket ediyorsa yüzünü hareket yönüne çevir
        if (speed > 0.1f)
        {
            RotateTowards(agent.destination);
        }

        // Bekleme süresi kontrolü
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                SetNewDestination();
            }
        }
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Y ekseninde dönmemesi için

        if (direction != Vector3.zero)
        {
            if (direction.z > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0); // Z ekseni artýyorsa Y rotasyonu 0
            }
            else if (direction.z < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0); // Z ekseni azalýyorsa Y rotasyonu 180
            }
            lastDirection = direction;
        }
    }

    void SetNewDestination()
    {
        if (plane == null) return;

        Vector3 planeCenter = plane.position;
        float randomX = Random.Range(planeCenter.x - planeSize.x, planeCenter.x + planeSize.x);
        float randomZ = Random.Range(planeCenter.z - planeSize.y, planeCenter.z + planeSize.y);
        Vector3 randomPoint = new Vector3(randomX, transform.position.y, randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, walkRadius, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
            agent.SetDestination(targetPosition);
        }
    }
}
