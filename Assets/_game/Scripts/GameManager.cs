using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private UIBar healthBar;
    [SerializeField] private UIBar xpBar;

    private void Start()
    {
        healthBar.SetMaxValue(1000f);
        healthBar.SetValue(1000f);

        xpBar.SetMaxValue(1000f);
        xpBar.SetValue(0f);
    }
}
