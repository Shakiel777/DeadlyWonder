using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlowDeath : MonoBehaviour {

    public GameObject player;

    bool InjurePlayer = true;
    bool WaterLevel = false;
    public int tDelay = 2;

    //[SerializeField] private GameObject explosion;

    private void OnTriggerEnter(Collider player)
    {
        player.GetComponent<HealthManager>().ApplyDamage(10f);
        WaterLevel = true;
        StartCoroutine(myDelay());
        // Explode();
    }


    void SlowDamage()
    {
        if (WaterLevel == true)
        {
            player.GetComponent<HealthManager>().ApplyDamage(10f);
        }   
    }

    void Explode()
    {
        //var exp = GetComponent<ParticleSystem>();
        //exp.Play();

        //Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    IEnumerator myDelay()
    {
        InjurePlayer = false;
        Debug.Log("injure player = false");

        yield return new WaitForSeconds(tDelay);

        InjurePlayer = true;
        SlowDamage();
        Debug.Log("injure player = true");
        StartCoroutine(myDelay2());
    }
    IEnumerator myDelay2()
    {
        InjurePlayer = false;
        Debug.Log("injure player = false");

        yield return new WaitForSeconds(tDelay);

        InjurePlayer = true;
        SlowDamage();
        Debug.Log("injure player = true");
        StartCoroutine(myDelay());
    }
}
