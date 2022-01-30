using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public enum PlatformType {Basic, Pull, Push, RotateZ, RotateY, SpeedUp, SpeedDown};
    public PlatformType type = PlatformType.Pull;
    public float speed = 5;
    public AudioSource audioSource;
    public List<AudioClip> audioClips = new List<AudioClip>();

    void FixedUpdate()
    {
        if (type == PlatformType.RotateZ)
        {
            transform.Rotate(transform.forward * speed * Time.deltaTime);
        } else if (type == PlatformType.RotateY) {
            transform.Rotate(transform.up * speed * Time.deltaTime);
        }
    }

    public void Step()
    {
        if (audioSource != null && audioClips.Count > 0 && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Count)]);
        }
    }
}
