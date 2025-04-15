using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Nebula : MonoBehaviour
{
    ParticleSystem particles;

    void Awake()
    {
        particles = gameObject.GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!particles.isPlaying && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Starting nebula");
            particles.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (particles.isPlaying && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Stopping nebula");
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
