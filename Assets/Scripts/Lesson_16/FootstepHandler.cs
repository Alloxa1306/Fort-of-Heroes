using UnityEngine;
using FMODUnity;

public class FootstepHandler : MonoBehaviour
{
    [Header("Звук шага")]
    public EventReference stepSound;


    public void PlayStep()
    {
        if (!stepSound.IsNull)
        {
            RuntimeManager.PlayOneShot(stepSound, transform.position);
        }
    }
}