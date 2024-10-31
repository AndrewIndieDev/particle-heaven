using MoreMountains.Feedbacks;
using System;
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

    [Header("References")]
    [SerializeField] private UIBar healthBar;
    [SerializeField] private UIBar xpBar;

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player onGameStart;
    [SerializeField] private MMF_Player onGameStop;

    public void StartGame()
    {
        healthBar.Init(1000f);
        xpBar.Init(0f, 1000f);

        CursorScript.Instance.Enable();
        GeometryManager.Instance.StartSpawning();

        onGameStart.PlayFeedbacks();
    }

    public void StopGame()
    {
        healthBar.Uninit();
        xpBar.Uninit();

        CursorScript.Instance.Disable();
        GeometryManager.Instance.StopSpawning();

        onGameStop.PlayFeedbacks();
    }

    public bool BuyUpgrade(double amount)
    {
        if (amount <= Currency)
        {
            Currency -= amount;
            return true;
        }
        return false;
    }
}
