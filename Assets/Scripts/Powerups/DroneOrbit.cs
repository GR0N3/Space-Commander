using UnityEngine;

[DisallowMultipleComponent]
public class DroneOrbit : MonoBehaviour
{
    [HideInInspector] public Transform target;
    [HideInInspector] public float radius = 5f;
    [HideInInspector] public float speed = 60f;
    public bool useRandomStart = true;
    public float angle = 0f;
    public bool faceOutward = false;

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
        if (target != null)
        {
            if (useRandomStart)
            {
                angle = Random.Range(0f, 360f);
            }
            UpdatePosition();
        }
    }

    void Update()
    {
        if (target == null) return;
        angle += speed * Time.deltaTime;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        transform.position = target.position + offset;
        if (faceOutward)
        {
            transform.forward = (transform.position - target.position).normalized;
        }
        else
        {
            transform.forward = (target.position - transform.position).normalized;
        }
    }
}
