using UnityEngine;
using UnityEngine.AI;

public class AddObstaclesRecursively : MonoBehaviour
{
    void Start()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.GetComponent<NavMeshObstacle>() == null)
            {
                var obs = child.gameObject.AddComponent<NavMeshObstacle>();
                obs.carving = true;
            }
        }
    }
}
