using AndrewDowsett.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UIBarType
{
    NUMBER,
    PERCENT
}

public class UIBar : MonoBehaviour
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

    public float Value => _value;
    private float _value;

    public float MaxValue => _maxValue;
    private float _maxValue;

    public float Percent => _value / _maxValue;
    public void SetValue(float value) => _value = Mathf.Clamp(value, 0, _maxValue);
    public void SetMaxValue(float value) => _maxValue = value;
    public void RemoveValue(float value) => _value = Mathf.Clamp(_value - value, 0, _maxValue);
    public void AddValue(float value) => _value = Mathf.Clamp(_value + value, 0, _maxValue);

    private void Start()
    {
        if (_nameText != null)
            _nameText.text = _name;
    }

    private void Update()
    {
        if (_fill != null)
            _fill.fillAmount = Percent;
        if (_valueText != null)
            _valueText.text = _type == UIBarType.NUMBER ? $"{_value} / {_maxValue}" : $"{(Percent * 100).ToString("N1")}%";
    }
}
