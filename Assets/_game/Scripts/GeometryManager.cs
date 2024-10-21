using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryManager : MonoBehaviour
{
    public static GeometryManager Instance;
    private void Awake() => Instance = this;

    [SerializeField] private GeometryObject _prefab;
    [SerializeField] private Transform parent;

    private List<GeometryObject> Objects = new();
    private List<GeometryData> Data = new();

    private float spawnTimer;

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= 1f)
        {
            StartCoroutine(spawnGeometry(Random.Range(1, 6)));
            spawnTimer = 0f;
        }
    }

    IEnumerator spawnGeometry(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Instantiate(_prefab, parent);
            yield return null;
        }
    }

    public void AddObject(GeometryObject obj)
    {
        Objects.Add(obj);
        Data.Add(new GeometryData());
    }

    public void RemoveObject(int index)
    {
        Destroy(Objects[index].gameObject);
        Objects.RemoveAt(index);
        Data.RemoveAt(index);
    }
}

public struct GeometryData
{
    public GeometryData(Vector2 position, Vector2 direction, float size)
    {
        Position = position;
        Direction = direction;
        Size = size;
    }

    public Vector2 Position;
    public Vector2 Direction;
    public float Size;
}