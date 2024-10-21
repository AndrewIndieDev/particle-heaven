using AndrewDowsett.CommonObservers;
using AndrewDowsett.IDisposables;
using AndrewDowsett.SceneLoading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AndrewDowsett.SingleEntryPoint
{
    public class GameInitiator : MonoBehaviour
    {
        [Header("Scene Post Initiation")]
        public string SceneToLoad;

        [Header("Prefabs to Instantiate")]
        public EntryScreen _entryScreen;
        public Camera _camera;
        public UpdateManager _updateManager;
        public FixedUpdateManager _fixedUpdateManager;
        public LateUpdateManager _lateUpdateManager;
        public SceneLoader _sceneLoader;
        public IntroAnimation _introAnimation;

        private async void Start()
        {
            BindObjects();

            // we can use an IDisposable here to simplify showing and hiding the loading screen:
            using (var loadingSceneDisposable = new DisposableShowEntryScreen(_entryScreen))
            {
                loadingSceneDisposable.SetLoadingText("Initializing...");
                loadingSceneDisposable.SetLoadingBarPercent(0.01f);
                await InitializeObjects();
                loadingSceneDisposable.SetLoadingText("Creating Objects...");
                loadingSceneDisposable.SetLoadingBarPercent(0.2f);
                await CreateObjects();
                loadingSceneDisposable.SetLoadingText("Preparing Game...");
                loadingSceneDisposable.SetLoadingBarPercent(0.7f);
                await PrepareGame();
                loadingSceneDisposable.SetLoadingText("Done...");
                loadingSceneDisposable.SetLoadingBarPercent(1.00f);
            }

            await BeginGame();
        }

        private void BindObjects()
        {
            // Bind all objects
            _entryScreen = Instantiate(_entryScreen);
            _entryScreen.name = "EntryScreen";

            _camera = Instantiate(_camera);
            _camera.name = "MainCamera";

            _updateManager = Instantiate(_updateManager);
            _updateManager.name = "UpdateManager";

            _fixedUpdateManager = Instantiate(_fixedUpdateManager);
            _fixedUpdateManager.name = "FixedUpdateManager";

            _lateUpdateManager = Instantiate(_lateUpdateManager);
            _lateUpdateManager.name = "LateUpdateManager";

            _sceneLoader = Instantiate(_sceneLoader);
            _sceneLoader.name = "SceneLoader";

            _introAnimation = Instantiate(_introAnimation);
            _introAnimation.name = "IntroAnimation";
        }

        private async UniTask InitializeObjects()
        {
            // Perform initialization for analytics/steam etc.
            await UniTask.Delay(1000);
        }

        private async UniTask CreateObjects()
        {
            // Instantiate all objects into the scene
            await UniTask.Delay(1000);
        }

        private async UniTask PrepareGame()
        {
            // Prepare objects in the scene, if they need methods called before the game starts
            await UniTask.Delay(1000);
        }

        private async UniTask BeginGame()
        {
            // Load the main menu scene
            _sceneLoader.LoadScene(SceneToLoad);
            // Unload the entry scene
            _sceneLoader.UnloadScene(SceneManager.GetActiveScene().name);
            // Wait for the intro animation
            await _introAnimation.Play();
            // Wait for the scenes to finish loading if not done before the animation finishes
            await UniTask.WaitUntil(() => !_sceneLoader.CurrentlyLoadingScene);
            Debug.Log("Game has been loaded...");
        }
    }
}