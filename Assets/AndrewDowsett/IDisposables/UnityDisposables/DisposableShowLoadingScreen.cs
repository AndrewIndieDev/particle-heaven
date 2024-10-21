using AndrewDowsett.SingleEntryPoint;
using System;

namespace AndrewDowsett.IDisposables
{
    public class DisposableShowLoadingScreen : IDisposable
    {
        private readonly LoadingScreen _loadingScreen;

        public DisposableShowLoadingScreen(LoadingScreen loadingScreen)
        {
            _loadingScreen = loadingScreen;
            _loadingScreen.Show();
        }

        public void SetLoadingBarPercent(float percent)
        {
            _loadingScreen.SetBarPercent(percent);
        }

        public void Dispose()
        {
            _loadingScreen.Hide();
        }
    }
}