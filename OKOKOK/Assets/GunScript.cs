using UnityEngine;

public class GunScript : MonoBehaviour
{
    private float maxDamage = 80f;
    private float minDamage = 1f;
    private float maxRange = 50f;
    public Camera fpsCamera;
    public ParticleSystem muzzleFlash;
    public LayerMask enemyLayer;

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Single shot like a pistol
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (muzzleFlash)
        {
            muzzleFlash.Play();
        }

        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, maxRange, enemyLayer))
        {
            float distance = hit.distance;
            float damage = Mathf.Lerp(maxDamage, minDamage, distance / maxRange);
            
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
