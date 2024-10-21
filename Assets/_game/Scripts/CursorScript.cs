using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    [SerializeField] private Transform bulletParent;
    [SerializeField] private MMF_Player shootFeedbacks;

    private void Start()
    {
        Cursor.visible = false;

        StartCoroutine(ShootCooldown());
    }

    private IEnumerator ShootCooldown()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            shootFeedbacks.PlayFeedbacks();
            bulletParent.position = transform.position;
            for (int i = 0; i < bulletParent.childCount; i++)
            {
                bulletParent.GetChild(i).GetComponent<Bullet>().Shoot();
            }
        }
    }
    
    void Update()
    {
        // raycast to find an object to hit to set the position of this object
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            transform.position = hit.point;
        }
    }
}
