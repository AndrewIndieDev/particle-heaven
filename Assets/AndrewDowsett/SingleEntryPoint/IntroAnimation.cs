using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AndrewDowsett.SingleEntryPoint
{
    public class IntroAnimation : MonoBehaviour
    {
        public async UniTask Play()
        {
            // play an animation, or MMFeedbacks.
            await UniTask.Delay(3000);
        }
    }
}