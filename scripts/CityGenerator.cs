using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CityGridGenerator : MonoBehaviour
{
    [Header("Kontenery prefabów (dzieci = dostępne prefaby)")]
    public Transform roadsSource;          // np. "Roads" z różnymi prefabami dróg prostych
    public Transform intersectionsSource;  // np. "Intersections" z prefabami skrzyżowań
    public Transform buildingsSource;      // np. "Buildings"
    public Transform propsSource;          // np. "Props" (lampy, ławki itd.)

    [Header("Rodzice dla wygenerowanych obiektów")]
    public Transform roadsParent;
    public Transform buildingsParent;
    public Transform propsParent;

    [Header("Parametry miasta")]
    [Range(2, 100)] public int cityBlocks = 10;     // liczba bloków (miasto N x N)
    public float roadLength = 50f;                  // długość odcinka drogi
    [Range(0f, 1f)] public float buildingDensity = 0.8f;
    [Range(0f, 1f)] public float propDensity = 0.4f;

    public void GenerateCity()
    {
        ClearOldCity();

        // Tworzymy siatkę NxN skrzyżowań i dróg między nimi
        for (int x = 0; x <= cityBlocks; x++)
        {
            for (int z = 0; z <= cityBlocks; z++)
            {
                Vector3 position = new Vector3(x * roadLength, 0, z * roadLength);

                // 1️⃣ Skrzyżowania na przecięciach
                if (x < cityBlocks && z < cityBlocks)
                {
                    SpawnPrefabFromSource(intersectionsSource, position, Quaternion.identity, roadsParent);
                }

                // 2️⃣ Drogi poziome (między skrzyżowaniami)
                if (x < cityBlocks)
                {
                    Vector3 roadPos = new Vector3(x * roadLength + roadLength / 2, 0, z * roadLength);
                    SpawnPrefabFromSource(roadsSource, roadPos, Quaternion.Euler(0, 0, 0), roadsParent);
                }

                // 3️⃣ Drogi pionowe
                if (z < cityBlocks)
                {
                    Vector3 roadPos = new Vector3(x * roadLength, 0, z * roadLength + roadLength / 2);
                    SpawnPrefabFromSource(roadsSource, roadPos, Quaternion.Euler(0, 90, 0), roadsParent);
                }
            }
        }

        // 4️⃣ Budynki między drogami (na blokach)
        GenerateBuildings();

        Debug.Log("✅ Siatkowe miasto wygenerowane poprawnie!");
    }

    private void GenerateBuildings()
    {
        if (buildingsSource == null || buildingsSource.childCount == 0) return;

        float half = roadLength / 2f;
        for (int x = 0; x < cityBlocks; x++)
        {
            for (int z = 0; z < cityBlocks; z++)
            {
                // pozycja środka bloku miejskiego
                Vector3 center = new Vector3(x * roadLength + half, 0, z * roadLength + half);

                // pozycje 4 budynków wokół bloku
                Vector3[] positions = new Vector3[]
                {
                    center + new Vector3(half - 5f, 0, half - 5f),
                    center + new Vector3(-half + 5f, 0, half - 5f),
                    center + new Vector3(half - 5f, 0, -half + 5f),
                    center + new Vector3(-half + 5f, 0, -half + 5f),
                };

                foreach (var pos in positions)
                {
                    if (Random.value < buildingDensity)
                    {
                        Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        SpawnPrefabFromSource(buildingsSource, pos, rot, buildingsParent);
                    }

                    // ewentualne propy obok budynków
                    if (propsSource != null && Random.value < propDensity)
                    {
                        Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        Vector3 propPos = pos + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                        SpawnPrefabFromSource(propsSource, propPos, rot, propsParent);
                    }
                }
            }
        }
    }

    private void SpawnPrefabFromSource(Transform sourceParent, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (sourceParent == null || sourceParent.childCount == 0) return;

        Transform randomChild = sourceParent.GetChild(Random.Range(0, sourceParent.childCount));
        GameObject prefab = randomChild.gameObject;

        Instantiate(prefab, position, rotation, parent);
    }

    private void ClearOldCity()
    {
        if (roadsParent != null)
            foreach (Transform child in roadsParent) DestroyImmediate(child.gameObject);
        if (buildingsParent != null)
            foreach (Transform child in buildingsParent) DestroyImmediate(child.gameObject);
        if (propsParent != null)
            foreach (Transform child in propsParent) DestroyImmediate(child.gameObject);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CityGridGenerator))]
public class CityGridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityGridGenerator generator = (CityGridGenerator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("🧱 Generate City (Grid)"))
        {
            generator.GenerateCity();
        }

        if (GUILayout.Button("🗑️ Clear City"))
        {
            generator.SendMessage("ClearOldCity");
        }
    }
}
#endif
