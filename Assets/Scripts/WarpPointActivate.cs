using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpPointActivate : MonoBehaviour
{
    // The amount of energy needed to activate the warp point.
    // Typically set to the number of colonies in the level.
    public float activationCost = 0;

    // The scene to load after entering the warp point.
    public int scene = 0;

    // The effect to start when the warp point is activated.
    private ParticleSystem myParticleSystem = null;
    bool warpActive = false;

    // Start is called before the first frame update
    void Start()
    {
        myParticleSystem = GetComponent<ParticleSystem>();
        myParticleSystem.Stop();

    }

    // Some  script will call this every time the carrier gains energy.
    // Args: energy: total amount of carrier's energy
    public void CheckActivate(float energy)
    {
        Debug.Log("Checking energy: " + energy);
        if (energy >= activationCost) {
            if (myParticleSystem != null)
            {
                myParticleSystem.Play();
            }
            warpActive = true;
        }
        else
        {
            if (myParticleSystem != null)
            {
                myParticleSystem.Stop();
            }
            warpActive = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (warpActive)
        {
            Debug.Log("Warp! Loading scene " + scene);
            SceneManager.LoadScene(scene);
        }
    }

}
