using AndrewDowsett.Utility;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float _distance;

    public void Shoot()
    {
        target.localPosition = Vector3.zero.IgnoreAxis(EAxis.Y, 1.5f);
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        target.gameObject.SetActive(true);

        while (target.localPosition.y < _distance)
        {
            yield return null;
            target.localPosition += Vector3.up * 40f * Time.deltaTime;
        }

        target.gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Geometry")
        {
            collision.gameObject.GetComponent<GeometryObject>().OnHit(400f);
            UIBar.GetByName("health")?.RemoveValue(1f);
        }
    }
}
