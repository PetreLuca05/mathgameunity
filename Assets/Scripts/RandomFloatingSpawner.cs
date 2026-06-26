using UnityEngine;

// Structure to hold prefab and size
[System.Serializable]
public struct SpawnableObject
{
    public GameObject prefab;
    public float size;
}

public class RandomFloatingSpawner : MonoBehaviour
{
    [Header("World Avoidance Settings")]
    public SphereCollider[] worldAvoidanceColliders;

    [Header("Spawning Settings")]
    public SpawnableObject[] spawnableObjects;
    public int spawnCount = 10;
    public float spawnRadius = 20f;
    public float spawnHeight = 10f;

    [Header("Floating Settings")]
    public float minDriftSpeed = 0.5f;
    public float maxDriftSpeed = 2f;
    public float minSpinSpeed = 30f;
    public float maxSpinSpeed = 120f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        // Draw cylinder base
        Vector3 center = transform.position;
        Vector3 top = center + Vector3.up * (spawnHeight / 2f);
        Vector3 bottom = center - Vector3.up * (spawnHeight / 2f);
        // Draw top and bottom circles
        DrawCircle(top, spawnRadius);
        DrawCircle(bottom, spawnRadius);
        // Draw lines between top and bottom
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2f / 12;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Gizmos.DrawLine(top + dir * spawnRadius, bottom + dir * spawnRadius);
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments = 32)
    {
        float angleStep = 2 * Mathf.PI / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    private Transform _spawnParent;

    private void Start()
    {
        _spawnParent = new GameObject("FloatingObjects").transform;
        _spawnParent.SetParent(transform);

        // Ensure at least one of each spawnable object
        int count = 0;
        if (spawnableObjects != null)
        {
            for (int i = 0; i < spawnableObjects.Length && count < spawnCount; i++, count++)
            {
                SpawnSpecificObject(i);
            }
        }
        // Fill the rest randomly
        for (; count < spawnCount; count++)
        {
            SpawnRandomObject();
        }
        // Spawns a specific object by index in spawnableObjects
        void SpawnSpecificObject(int index)
        {
            if (spawnableObjects == null || spawnableObjects.Length == 0) return;
            if (index < 0 || index >= spawnableObjects.Length) return;
            var spawnData = spawnableObjects[index];
            if (spawnData.prefab == null) return;
            Vector3 randomPos = GetRandomPointInCylinder();
            GameObject obj = Instantiate(spawnData.prefab, randomPos, Random.rotation, _spawnParent);
            obj.transform.localScale = Vector3.one * spawnData.size;
            DisableShadows(obj);
            var floater = obj.AddComponent<FloatingDrifter>();
            floater.driftSpeed = Random.Range(minDriftSpeed, maxDriftSpeed);
            floater.spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
            floater.spinAxis = Random.onUnitSphere;
            floater.cylinderCenter = transform.position;
            floater.cylinderRadius = spawnRadius;
            floater.cylinderHeight = spawnHeight;
            floater.avoidanceColliders = worldAvoidanceColliders;
        }
    }

    void SpawnRandomObject()
    {
        if (spawnableObjects == null || spawnableObjects.Length == 0) return;
        int idx = Random.Range(0, spawnableObjects.Length);
        var spawnData = spawnableObjects[idx];
        if (spawnData.prefab == null) return;
        Vector3 randomPos = GetRandomPointInCylinder();
        GameObject obj = Instantiate(spawnData.prefab, randomPos, Random.rotation, _spawnParent);
        obj.transform.localScale = Vector3.one * spawnData.size;
        DisableShadows(obj);
        var floater = obj.AddComponent<FloatingDrifter>();
        floater.driftSpeed = Random.Range(minDriftSpeed, maxDriftSpeed);
        floater.spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
        floater.spinAxis = Random.onUnitSphere;
        floater.cylinderCenter = transform.position;
        floater.cylinderRadius = spawnRadius;
        floater.cylinderHeight = spawnHeight;
        floater.avoidanceColliders = worldAvoidanceColliders;
    }

    void DisableShadows(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    Vector3 GetRandomPointInCylinder()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt(Random.Range(0f, 1f)) * spawnRadius;
        float y = Random.Range(-spawnHeight / 2f, spawnHeight / 2f);
        Vector3 localPos = new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
        return transform.position + localPos;
    }
}

public class FloatingDrifter : MonoBehaviour
{
        [HideInInspector] public SphereCollider[] avoidanceColliders;
    [HideInInspector] public float driftSpeed;
    [HideInInspector] public float spinSpeed;
    [HideInInspector] public Vector3 spinAxis;

    private Vector3 driftDirection;
    [HideInInspector] public Vector3 cylinderCenter;
    [HideInInspector] public float cylinderRadius;
    [HideInInspector] public float cylinderHeight;

    void Start()
    {
        driftDirection = Random.onUnitSphere;
    }

    void Update()
    {
        // Drift movement
        Vector3 nextPos = transform.position + driftDirection * driftSpeed * Time.deltaTime;

        // Avoidance logic
        if (avoidanceColliders != null)
        {
            foreach (var col in avoidanceColliders)
            {
                if (col == null) continue;
                Vector3 closest = col.ClosestPoint(nextPos);
                float dist = Vector3.Distance(nextPos, col.transform.position);
                float avoidRadius = col.radius * Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.y, col.transform.lossyScale.z);
                if (dist < avoidRadius + 0.5f) // 0.5f is a buffer, adjust as needed
                {
                    // Steer away from collider
                    Vector3 away = (nextPos - col.transform.position).normalized;
                    driftDirection = Vector3.Lerp(driftDirection, away, 0.2f).normalized;
                    nextPos = col.transform.position + away * (avoidRadius + 0.5f);
                }
            }
        }

        transform.position = nextPos;

        // Spin
        transform.Rotate(spinAxis, spinSpeed * Time.deltaTime);

        // Keep inside cylinder
        Vector3 local = transform.position - cylinderCenter;
        float y = Mathf.Clamp(local.y, -cylinderHeight / 2f, cylinderHeight / 2f);
        Vector2 xz = new Vector2(local.x, local.z);
        if (xz.magnitude > cylinderRadius)
        {
            xz = xz.normalized * cylinderRadius;
            // Optionally, bounce or reflect drift direction here
            driftDirection = Vector3.Reflect(driftDirection, new Vector3(local.x, 0, local.z).normalized);
        }
        transform.position = cylinderCenter + new Vector3(xz.x, y, xz.y);
    }
}
