using UnityEngine;

public class ChaseMovement : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.1f;

    private void Awake()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 current = transform.position;
        Vector3 destination = target.position;

        Vector3 toTarget = destination - current;
        if (toTarget.sqrMagnitude <= stoppingDistance * stoppingDistance) return;

        Vector3 direction = toTarget.normalized;
        transform.position = current + direction * (moveSpeed * Time.deltaTime);
    }
}