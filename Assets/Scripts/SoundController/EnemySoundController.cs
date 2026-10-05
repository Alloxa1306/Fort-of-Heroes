using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(Animator))]
public class EnemySoundController : MonoBehaviour
{
    [Header("Звуки FMOD")]
    public EventReference stepSound;
    public EventReference attackSound;

    public void PlayStep()
    {
        if (!stepSound.IsNull)
        {
            RuntimeManager.PlayOneShot(stepSound, transform.position);
        }
    }

    public void PlayAttackSound()
    {
        if (!attackSound.IsNull)
        {
            RuntimeManager.PlayOneShot(attackSound, transform.position);
        }
    }
}