using AndrewDowsett.CommonObservers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryManager : MonoBehaviour, IUpdateObserver
{
    public static GeometryManager Instance;
    private void Awake() => Instance = this;

    [SerializeField] private GeometryObject _prefab;
    [SerializeField] private Transform parent;

    private List<GeometryObject> Objects = new();

    private float spawnTimer;

    public void StartSpawning()
    {
        UpdateManager.RegisterObserver(this);
        StartCoroutine(DespawnGeometry());
    }

    public void StopSpawning()
    {
        UpdateManager.UnregisterObserver(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        spawnTimer += deltaTime;
        if (spawnTimer >= 1f)
        {
            StartCoroutine(SpawnGeometry(Random.Range(1, 6)));
            spawnTimer = 0f;
        }
    }

    IEnumerator SpawnGeometry(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Instantiate(_prefab, parent);
            yield return null;
        }
    }

    IEnumerator DespawnGeometry()
    {
        while (Objects.Count > 0)
        {
            if (Objects[0] != null)
                Objects[0].DestroyGO();
            yield return null;
        }
        Objects.Clear();
    }

    public void AddObject(GeometryObject obj)
    {
        Objects.Add(obj);
    }

    public void RemoveObject(GeometryObject geoObject)
    {
        Objects.Remove(geoObject);
    }
}