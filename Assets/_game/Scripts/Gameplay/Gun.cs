using AndrewDowsett.ObjectPooling;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform _defaultTarget;

    [Header("Shooting")]
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private Transform _firePoint;

    [Header("Audio")]
    [SerializeField] private AudioClip shootClip;

    private Transform target;
    private List<Transform> _targets = new();

    private void Update()
    {
        FindClosestTarget();
        RotateTowardsTarget();
    }

    private void FindClosestTarget()
    {
        target = null;
        float closestDistance = float.MaxValue;
        for (var i = _targets.Count - 1; i >= 0; i--)
        {
            var tar = _targets[i];
            if (tar == null)
            {
                _targets.RemoveAt(i);
                continue;
            }

            var distance = Vector2.Distance(transform.position, tar.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = tar;
            }
        }
    }

    void RotateTowardsTarget()
    {
        if (target == null)
            return;

        var direction = target.position - transform.position;
        var targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        var currentAngle = transform.eulerAngles.z;
        var newAngle = Mathf.MoveTowardsAngle(
            currentAngle,
            targetAngle,
            rotationSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    public void Fire()
    {
        if (!gameObject.activeInHierarchy)
            return;

        AudioManager.PlaySound(shootClip, EMixerGroup.SFX, 0.8f, 0.3f);

        var bullet = ObjectPool.Pools["Bullet"].Get() as Bullet;
        if (bullet == null) return;

        //var tar = target ?? _defaultTarget;
        bullet.transform.position = _firePoint.position;
        bullet.Fire(_defaultTarget.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Geometry"))
            return;

        _targets.Add(other.transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Geometry"))
            return;

        _targets.Remove(other.transform);
    }
}
