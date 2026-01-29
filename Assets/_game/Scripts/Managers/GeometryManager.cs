using AndrewDowsett.CommonObservers;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GeometryManager : MonoBehaviour, IUpdateObserver
{
    public static GeometryManager Instance;
    private void Awake() => Instance = this;

    [SerializeField] private GeometryObject _prefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private float spawnRate = 1f;
    [SerializeField] private Vector2Int minMaxSpawns = new(1, 3);

    private List<GeometryObject> Objects = new();
    private float spawnTimer;
    private Coroutine currentCoroutine;

    public async void StartSpawning()
    {
        currentCoroutine = StartCoroutine(DespawnGeometry());
        await UniTask.WaitWhile(() => currentCoroutine != null);
        UpdateManager.RegisterObserver(this);
    }

    public void StopSpawning()
    {
        UpdateManager.UnregisterObserver(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        spawnTimer += deltaTime;
        if (spawnTimer >= spawnRate)
        {
            StartCoroutine(SpawnGeometry(Random.Range(minMaxSpawns.x, minMaxSpawns.y)));
            spawnTimer = 0f;
        }
    }

    IEnumerator SpawnGeometry(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Instantiate(_prefab, _parent);
            yield return null;
        }
    }

    IEnumerator DespawnGeometry()
    {
        while (Objects.Count > 0)
        {
            if (Objects[0] != null)
                Objects[0].DestroyImmediate();
            yield return null;
        }
        Objects.Clear();
        currentCoroutine = null;
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