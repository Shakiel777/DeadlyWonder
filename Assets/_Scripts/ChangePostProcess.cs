using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Rendering.PostProcessing;

public class ChangePostProcess : MonoBehaviour {

    public PostProcessingProfile Normal, fx;

    // private PostProcessingBehavior camImageFx;
    private PostProcessingBehaviour camImageFx;

	// Use this for initialization
	void Start ()
    {
        camImageFx = FindObjectOfType<PostProcessingBehaviour>();
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            camImageFx.profile = fx;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            camImageFx.profile = Normal;
        }
    }

}
