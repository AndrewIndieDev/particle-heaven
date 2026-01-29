using AndrewDowsett.ObjectPooling;
using UnityEngine;

public class Bullet : MonoBehaviour, IPooledObject
{
    [SerializeField] private float _forwardSpeed;
    [SerializeField] private GameObject _visual;
    [SerializeField] private float _lifeTime = 5f;
    [SerializeField] private Collider2D col;

    private Vector3 direction;
    private bool isEnabled;
    private float lifeTimer;

    public void Spawn(ObjectPool pool)
    {
        
    }

    public void Despawn()
    {
        col.enabled = false;
        _visual.SetActive(false);
        isEnabled = false;
        lifeTimer = 0f;
    }

    public void Fire(Vector3 position)
    {
        direction = (position - transform.position).normalized;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        _visual.SetActive(true);
        isEnabled = true;
        col.enabled = true;
    }

    private void Update()
    {
        if (!isEnabled)
            return;

        transform.position += transform.right * _forwardSpeed * Time.deltaTime;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= _lifeTime)
            ObjectPool.Pools["Bullet"].Release(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Geometry")
        {
            collision.gameObject.GetComponent<GeometryObject>().OnHit(3f);
            ObjectPool.Pools["Bullet"].Release(this);
        }
    }
}
