using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [Header("Currency System")]
    public double Currency;

    [Header("Ore Spawning")]
    [SerializeField] Box2DShapeSpawner _oreSpawner;
    [SerializeField] private double _ore;
    [SerializeField] private float _orePerSecond;

    [Header("References")]
    [SerializeField] private UIBar fuelBar;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera _mainMenuCamera;
    [SerializeField] private CinemachineCamera _upgradesCamera;
    [SerializeField] private CinemachineCamera _gameCamera;

    private Coroutine fuelCoroutine;
    private Coroutine oreCoroutine;

    public void StartGame()
    {
        fuelBar.Init(10f);

        CursorScript.Instance.Enable();
        GeometryManager.Instance.StartSpawning();
        PlayerObject.Instance.Enable();

        GotoCamera(_gameCamera);

        if (fuelCoroutine != null)
            StopCoroutine(fuelCoroutine);
        fuelCoroutine = StartCoroutine(DecreaseFuel());
    }

    [ContextMenu("Stop Game")]
    public void StopGame()
    {
        if (fuelCoroutine != null)
            StopCoroutine(fuelCoroutine);

        fuelBar.Uninit();

        CursorScript.Instance.Disable();
        GeometryManager.Instance.StopSpawning();
        PlayerObject.Instance.Disable();

        GotoCamera(_upgradesCamera);

        if (oreCoroutine == null)
            oreCoroutine = StartCoroutine(SpawnOre());
    }

    public void AddOre(double amount = 1)
    {
        _ore += amount;
    }

    private IEnumerator DecreaseFuel()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            fuelBar.AddValue(-0.1f);
            if (fuelBar.Value <= 0f)
            {
                PlayerObject.Instance.Disable();
                yield return new WaitForSeconds(2f);
                fuelCoroutine = null;
                StopGame();
                break;
            }
        }
    }

    private IEnumerator SpawnOre()
    {
        while (_ore > 0)
        {
            yield return new WaitForSeconds(1f / _orePerSecond);
            _ore--;
            _oreSpawner.SpawnShapes();
        }
        oreCoroutine = null;
    }

    private void GotoCamera(CinemachineCamera cam)
    {
        _mainMenuCamera.Priority = cam == _mainMenuCamera ? 1 : 0;
        _upgradesCamera.Priority = cam == _upgradesCamera ? 1 : 0;
        _gameCamera.Priority = cam == _gameCamera ? 1 : 0;
    }
}
