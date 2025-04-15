using UnityEngine;

public class HealthTracker : MonoBehaviour
{
    // Hitpoints to track.
    public float maxHealth = 100;
    public float health = 0;

    // Whether to capture object at 0 health, or simply destroy.
    public enum ActionOnDefeat { Capture, Destroy };
    public ActionOnDefeat actionOnDefeat = ActionOnDefeat.Destroy;

    // Explosion effect.
    public GameObject explosionPrefab = null;

    public float shieldStrength = 0f;
    public float shieldArc = 90f;
    private float lastFireTime = 0;

    private GameObject shield = null;
    // Audio
    public AudioClip clip;
    public float volume = 0.5f;

    public Material capturedMaterial = null;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        Transform t = gameObject.transform.Find("sprite/shield");
        if (t != null)
        {
            // Debug.Log(gameObject.name + ": found shield");
            shield = t.gameObject;
        }
    }
    void Update()
    {
        if ((shield != null) && shield.activeSelf)
        {
            if (Time.time > lastFireTime + 1f)
            {
                shield.SetActive(false);
            }
        }
    }

    // Apply damage (usually will reduce health).
    // Returns true if target destroyed or captured.
    public bool TakeDamage(float damage)
    {
        return TakeDamage(damage, null);
    }

    public bool TakeDamage(float damage, GameObject source)
    {
        if (health == 0f)
        {
            // Ignore damage to defeated objects.
            return false;
        }
        Debug.Log(name + " hit for " + damage + " damage");
        if (shieldStrength > 0)
        {
            float effectiveShieldStrength = shieldStrength;
            float sourceBearing = 180f;
            if (source != null)
            {
                sourceBearing = Vector3.Angle(transform.up, source.transform.position - transform.position);
                Debug.Log(gameObject.name + ": Damage from bearing " + sourceBearing);
            }
            if (Mathf.Abs(sourceBearing) > shieldArc)
            {
                effectiveShieldStrength = 0;
            }
            if (shield != null)
            {
                lastFireTime = Time.time;
                shield.SetActive(true);
            }
            if (damage > effectiveShieldStrength)
            {
                if (effectiveShieldStrength > 0)
                {
                    Debug.Log("Shields blocked " + effectiveShieldStrength + " damage");
                    damage -= effectiveShieldStrength;
                }
            }
            else
            {
                Debug.Log("Shields blocked all damage");
                damage = 0f;
            }
        }
        health -= damage;
        if (health > 0f)
        {
            return false;
        }
        health = 0f;
        if (actionOnDefeat == ActionOnDefeat.Capture)
        {
            Debug.Log(name + " has been captured.");
            gameObject.transform.Find("sprite").gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
            // Switch layer so friendly ships will no longer target captured objects.
            gameObject.layer = LayerMask.NameToLayer("Captured");
            if (capturedMaterial != null)
            {
                GameObject planetMarker = gameObject.transform.Find("Sphere").gameObject;
                planetMarker.GetComponent<Renderer>().material = capturedMaterial;
            }
        }
        else
        {
            Debug.Log(name + " has been destroyed.");
            if (explosionPrefab != null)
            {
                // Display a could of smoke/debris for 1 second after destruction.
                // Needs to persist after this object is destroyed.
                GameObject explosion = Instantiate(explosionPrefab, transform.position,
                    transform.rotation);
                Destroy(explosion, 1.0f);
            }
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
            Destroy(gameObject);
        }
        return true;

    }
}
