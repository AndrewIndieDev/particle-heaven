using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AndrewDowsett.Utilities
{
    public class UIMouseEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Hover Graphic")]
        public Graphic targetGraphic;
        public Color hoverColor;

        [Header("Events")]
        public UnityEvent onPointerEnter;
        public UnityEvent onPointerExit;
        public UnityEvent onPrimaryPointerDown;
        public UnityEvent onPrimaryPointerUp;
        public UnityEvent onSecondaryPointerDown;
        public UnityEvent onSecondaryPointerUp;

        private Color startingColor;

        private void Start()
        {
            if (targetGraphic != null)
                startingColor = targetGraphic.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (targetGraphic != null)
                targetGraphic.color = hoverColor;
            onPointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (targetGraphic != null)
                targetGraphic.color = startingColor;
            onPointerExit?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onPrimaryPointerDown?.Invoke();
            else
                onSecondaryPointerDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onPrimaryPointerUp?.Invoke();
            else
                onSecondaryPointerUp?.Invoke();
        }
    }
}