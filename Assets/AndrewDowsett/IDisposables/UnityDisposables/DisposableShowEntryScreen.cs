using AndrewDowsett.SingleEntryPoint;
using System;

namespace AndrewDowsett.IDisposables
{
    public class DisposableShowEntryScreen : IDisposable
    {
        private readonly EntryScreen _loadingScreen;

        public DisposableShowEntryScreen(EntryScreen loadingScreen)
        {
            _loadingScreen = loadingScreen;
            _loadingScreen.Show();
        }

        public void SetLoadingBarPercent(float percent)
        {
            _loadingScreen.SetBarPercent(percent);
        }

        public void SetLoadingText(string text)
        {
            _loadingScreen.SetBarText(text);
        }

        public void Dispose()
        {
            _loadingScreen.Hide();
        }
    }
}