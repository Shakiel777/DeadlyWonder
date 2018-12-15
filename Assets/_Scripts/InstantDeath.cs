using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InstantDeath : MonoBehaviour {

    private GameObject player;

    [SerializeField] private GameObject explosion;

    private void OnTriggerEnter(Collider player)
    {
        player.GetComponent<HealthManager>().ApplyDamage(200f);
        Explode();
    }

    void Explode()
    {
        //var exp = GetComponent<ParticleSystem>();
        //exp.Play();

        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void KillPlayer()
    {
        player.GetComponent<HealthManager>().ApplyDamage(200f);
    }
}
