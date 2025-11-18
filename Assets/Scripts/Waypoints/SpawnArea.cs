using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    public enum Shape { Rectangle, Circle }
    public Shape shape = Shape.Rectangle;
    public Vector2 size = new Vector2(20f, 20f);
    public float radius = 10f;
    public bool showGizmos = true;
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.25f);

    public Vector3 GetRandomPoint()
    {
        if (shape == Shape.Circle)
        {
            Vector2 r = Random.insideUnitCircle * radius;
            return new Vector3(transform.position.x + r.x, transform.position.y, transform.position.z + r.y);
        }
        float x = Random.Range(-size.x * 0.5f, size.x * 0.5f);
        float z = Random.Range(-size.y * 0.5f, size.y * 0.5f);
        return new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        Gizmos.color = gizmoColor;
        if (shape == Shape.Circle)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.1f, size.y));
        }
    }
}