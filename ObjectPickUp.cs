using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPickUp : MonoBehaviour
{
    public AudioClip SoundToPlay;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = SoundToPlay;
    }

    public void ToggleSound(bool OnOff)
    {
        if (OnOff)
            audioSource.clip = SoundToPlay;
        else
            audioSource.clip = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Play sound
        audioSource.Play();

        // Remove any attached objects
        if (AttachedObject.instance == null)
            new AttachedObject();
        else
            AttachedObject.instance.Detach(true);
        
        // Attach a copy to the PlayerController
        AttachedObject.instance.Attach(gameObject.GetComponent<MeshFilter>().mesh, gameObject.GetComponent<MeshRenderer>().material, other.transform, gameObject);
    }

    public void Detach(bool showCueObject)
    {
        if (AttachedObject.instance != null)
            AttachedObject.instance.Detach(showCueObject); 
    }
    // Update is called once per frame
    private void OnDisable()
    {
        Detach(false);
    }
}
