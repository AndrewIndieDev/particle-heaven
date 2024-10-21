using AndrewDowsett.CommonObservers;
using AndrewDowsett.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum UIBarType
{
    NUMBER,
    PERCENT
}

public class UIBar : MonoBehaviour, IUpdateObserver
{
    public static List<UIBar> UIBars = new();
    private void Awake()
    {
        if (addToStaticList)
            UIBars.AddUnique(this);
    }
    public static UIBar GetByName(string name) => UIBars.Find(x => x.Name == name);

    [SerializeField] private bool addToStaticList = false;

    public string Name => _name;
    [SerializeField] private string _name;

    public UIBarType Type => _type;
    [SerializeField] private UIBarType _type;

    [Header("References")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _fill;
    [SerializeField] private TMP_Text _valueText;

    [Header("Callback Actions")]
    [SerializeField] private UnityEvent eventTrigger;

    public float Value => _value;
    private float _value;

    public float MaxValue => _maxValue;
    private float _maxValue;

    public float Percent => _value / _maxValue;
    private void SetValue(float value) => _value = Mathf.Clamp(value, 0, _maxValue);
    private void SetMaxValue(float value) => _maxValue = value;
    public void RemoveValue(float value) => _value = Mathf.Clamp(_value - value, 0, _maxValue);
    public void AddValue(float value) => _value = Mathf.Clamp(_value + value, 0, _maxValue);

    public void Init(float value, float maxValue = -1)
    {
        if (_nameText != null)
            _nameText.text = _name;

        SetMaxValue(maxValue < 0 ? value : maxValue);
        SetValue(value);

        UpdateManager.RegisterObserver(this);
    }

    public void Uninit()
    {
        UpdateManager.UnregisterObserver(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        if (_fill != null)
            _fill.fillAmount = Percent;
        if (_valueText != null)
            _valueText.text = _type == UIBarType.NUMBER ? $"{_value.ToString("N0")} / {_maxValue}" : $"{(Percent * 100).ToString("N1")}%";

        switch (Type)
        {
            case UIBarType.NUMBER:
                if (_value <= 0)
                    eventTrigger?.Invoke();
                break;
            case UIBarType.PERCENT:
                if (Percent >= 1)
                    eventTrigger?.Invoke();
                break;
            default:
                Debug.LogError($"<{gameObject.name}> : Unknown type {Type}");
                break;
        }
    }
}
