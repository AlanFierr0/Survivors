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
        destination.z = current.z; // Keep current Z for 2D scenes

        float distance = Vector3.Distance(current, destination);
        if (distance <= stoppingDistance) return;

        transform.position = Vector3.MoveTowards(current, destination, moveSpeed * Time.deltaTime);
    }
}