using AndrewDowsett.CommonObservers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AndrewDowsett.SingleEntryPoint
{
    public class EntryScreen : MonoBehaviour, IUpdateObserver
    {
        public Image progressBar;
        public TMP_Text progressText;

        private float _progress = 0;

        public void Show()
        {
            UpdateManager.RegisterObserver(this);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            UpdateManager.UnregisterObserver(this);
            gameObject.SetActive(false);
            progressBar.fillAmount = 0;
            progressText.text = string.Empty;
        }

        public void SetBarPercent(float percent)
        {
            _progress = percent;
        }

        public void SetBarText(string text)
        {
            progressText.text = text;
        }

        public void ObservedUpdate(float deltaTime)
        {
            if (_progress > progressBar.fillAmount)
            {
                progressBar.fillAmount += 10f * deltaTime;
            }

            if (_progress < progressBar.fillAmount)
            {
                progressBar.fillAmount = _progress;
            }
        }
    }
}