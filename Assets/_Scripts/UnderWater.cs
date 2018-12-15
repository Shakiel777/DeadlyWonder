using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderWater : MonoBehaviour {

    public float underwaterDens = 0.15F;
    public Color underwaterColor = new Color(0.1F, 0.3F, 0.4F, 1.0F);

    private bool oldFog;
    private float oldDens;
    private Color oldColor;
    private FogMode oldMode;
    private GameObject curWater;
    private bool underwater = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        { // if entering a waterplane
            if (transform.position.y < other.transform.position.y)
            {
                // set reference to the current waterplane
                curWater = other.gameObject;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == curWater)
        { // if exiting the waterplane...
            if (transform.position.y + 0.6F > curWater.transform.position.y)
            {
                //  null the current waterplane reference
                curWater = null;
            }
        }
    }

    void Update()
    {
        // if it's underwater...
        if (curWater && Camera.main.transform.position.y < curWater.transform.position.y + 0.2F)
        {
            if (!underwater)
            { // turn on underwater effect only once
                oldFog = RenderSettings.fog;
                oldMode = RenderSettings.fogMode;
                oldDens = RenderSettings.fogDensity;
                oldColor = RenderSettings.fogColor;
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = underwaterDens;
                RenderSettings.fogColor = underwaterColor;
                underwater = true;
            }

        }
        else // but if it's not underwater...
            if (underwater)
        { // turn off underwater effect, if any
            RenderSettings.fog = oldFog;
            RenderSettings.fogMode = oldMode;
            RenderSettings.fogDensity = oldDens;
            RenderSettings.fogColor = oldColor;
            underwater = false;
        }
    }
}
