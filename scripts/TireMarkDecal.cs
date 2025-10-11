using UnityEngine;

public class TireMarkDecal : MonoBehaviour
{
    public Material decalMaterial;
    public float decalWidth = 0.3f;
    public float decalLength = 1.0f;
    public float decalLifetime = 10f;
    public float fadeStartTime = 8f;
    public float minDistanceBetweenDecals = 0.1f;
    public LayerMask groundLayer;

    private Vector3 lastDecalPosition;
    private float lastDecalTime;
    private float fadeProgress;

    void Start()
    {
        lastDecalPosition = transform.position;
        lastDecalTime = Time.time;
    }

    public void CreateDecal(Vector3 position, Vector3 normal, Vector3 forward)
    {
        // Check if we're too close to the last decal
        if (Vector3.Distance(position, lastDecalPosition) < minDistanceBetweenDecals)
            return;

        // Create decal mesh
        GameObject decal = new GameObject("TireMarkDecal");
        decal.transform.position = position;
        decal.transform.rotation = Quaternion.LookRotation(forward, normal);

        // Create mesh
        MeshFilter meshFilter = decal.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = decal.AddComponent<MeshRenderer>();
        meshRenderer.material = decalMaterial;

        // Create quad mesh
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        // Set vertices
        float halfWidth = decalWidth * 0.5f;
        float halfLength = decalLength * 0.5f;
        vertices[0] = new Vector3(-halfWidth, 0, -halfLength);
        vertices[1] = new Vector3(halfWidth, 0, -halfLength);
        vertices[2] = new Vector3(halfWidth, 0, halfLength);
        vertices[3] = new Vector3(-halfWidth, 0, halfLength);

        // Set UVs
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(1, 1);
        uv[3] = new Vector2(0, 1);

        // Set triangles
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        // Add decal controller
        DecalController controller = decal.AddComponent<DecalController>();
        controller.Initialize(decalLifetime, fadeStartTime);

        // Update last position
        lastDecalPosition = position;
        lastDecalTime = Time.time;
    }
}

public class DecalController : MonoBehaviour
{
    private float lifetime;
    private float fadeStartTime;
    private float startTime;
    private Material material;
    private Color originalColor;

    public void Initialize(float lifetime, float fadeStartTime)
    {
        this.lifetime = lifetime;
        this.fadeStartTime = fadeStartTime;
        this.startTime = Time.time;
        this.material = GetComponent<MeshRenderer>().material;
        this.originalColor = material.color;
    }

    void Update()
    {
        float currentTime = Time.time - startTime;

        if (currentTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (currentTime >= fadeStartTime)
        {
            float fadeProgress = (currentTime - fadeStartTime) / (lifetime - fadeStartTime);
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(originalColor.a, 0f, fadeProgress);
            material.color = newColor;
        }
    }
} 