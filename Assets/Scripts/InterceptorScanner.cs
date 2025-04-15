using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterceptorScanner : MonoBehaviour
{
    public float scanRange = 8f;
    private GameObject carrier = null;
    public float reloadTime = 0.5f;
    private float lastFireTime = 0;

    void Start()
    {
        carrier = GetCarrier(gameObject);
    }

    private GameObject GetCarrier(GameObject target)
    {
        Interceptor interceptor = target.GetComponent<Interceptor>();
        if (interceptor == null)
        {
            return null;
        }
        return interceptor.Carrier;
    }

    private List<int> FilterEnemies(Collider2D[] enemies)
    {
        List<int> validIndexes = new List<int>();

        for (int i = 0; i < enemies.Length; i++)
        {
            //Debug.LogWarning(gameObject.name + "/" + carrier + ": considering " + enemies[i].gameObject.name + "/" + GetCarrier(enemies[i].gameObject));
            if (carrier != GetCarrier(enemies[i].gameObject))
            {
                //Debug.LogWarning(gameObject.name + "/" + carrier + ": targeting " + enemies[i].gameObject.name + "/" + GetCarrier(enemies[i].gameObject) );
                validIndexes.Add(i);
            }
        }
        return validIndexes;
    }

    public GameObject FindTarget(List<string> scanLayers)
    {
        Collider2D[] enemies;

        if (Time.time > lastFireTime + reloadTime)
        {
            lastFireTime = Time.time;

            // Search in a loop, rather than combine all layers into one mask,
            // since we want to find high-priority targets first.
            foreach (string scanLayer in scanLayers)
            {
                //Debug.LogWarning("Scanning for " + scanLayer + " at range of: " + scanRange);
                enemies = Physics2D.OverlapCircleAll(
                    transform.position, scanRange, LayerMask.GetMask(scanLayer));
                List<int> validIndexes = FilterEnemies(enemies);
                if (validIndexes.Count > 0)
                {
                    // Randomly select amongst the available targets, to prevent
                    // ships from all ganging up on one.
                    int selectedEnemy = validIndexes[Random.Range(0, validIndexes.Count - 1)];
                    return enemies[selectedEnemy].gameObject;
                }
            }
        }
        return null;
    }

    // Helper routine to check if an object is a valid target
    public bool IsTarget(List<string> scanLayers, GameObject target)
    {           
        if (carrier == GetCarrier(target)) {
            return false;
        }
        foreach (string targetType in scanLayers)
        {
            if (target.layer == LayerMask.NameToLayer(targetType))
            {
                return true;
            }
        }
        return false;
    }

}
