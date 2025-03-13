using AndrewDowsett.IDisposables;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace AndrewDowsett.ObjectPooling
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject poolPrefab;
        [SerializeField] private int defaultPoolSize;
        [SerializeField] private int maxPoolSize;

        private ObjectPool<IPooledObject> _pool;

        public void Initialize()
        {
            IPooledObject pooledObject = poolPrefab.GetComponent<IPooledObject>();
            if (pooledObject == null)
            {
                Debug.LogError($"{poolPrefab.name} on {gameObject.name} does not implement IPooledObject!");
                return;
            }
        }

        public async UniTask SpawnDefaultPool(DisposableShowEntryScreen loadingSceneDisposable = null, float percentageToUse = 0.01f)
        {
            _pool = new ObjectPool<IPooledObject>(
                () => Instantiate(poolPrefab, transform).GetComponent<IPooledObject>(),
                obj => obj.Spawn(),
                obj => obj.Despawn(),
                null,
                false,
                defaultPoolSize,
                maxPoolSize);

            float initialPercentage = loadingSceneDisposable.GetLoadingBarPercent();

            for (int i = 0; i < defaultPoolSize; i++)
            {
                if (loadingSceneDisposable != null && percentageToUse > 0)
                    loadingSceneDisposable.SetLoadingBarPercent(initialPercentage + (percentageToUse / defaultPoolSize * (i + 1)));

                _pool.Get();

                if (i % 10 == 1)
                await UniTask.Yield();
            }
        }
    }
}