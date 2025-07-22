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
                List<int> indexesInArc = new List<int>();
                if (validIndexes.Count > 0)
                {
                    int closestShipIdx = -1;
                    float closestShipDistance = float.MaxValue;
                    int closestShipInArcIdx = -1;
                    float closestShipInArcDistance = float.MaxValue;

                    foreach (int vidx in validIndexes)
                    {
                        float targetDistance = Vector3.Distance(transform.position, enemies[vidx].gameObject.transform.position);
                        if (targetDistance < closestShipDistance)
                        {
                            closestShipDistance = targetDistance;
                            closestShipIdx = vidx;
                        }
                        float targetBearing = Vector3.Angle(transform.up, enemies[vidx].gameObject.transform.position - transform.position);
                        if (targetBearing < GetComponent<InterceptorWeapon>().firingArc)
                        {
                            if (targetDistance < closestShipInArcDistance)
                            {
                                closestShipInArcDistance = targetDistance;
                                closestShipInArcIdx = vidx;
                            }
                            indexesInArc.Add(vidx);
                        }
                    }
                    int selectedEnemy;
                    if (indexesInArc.Count > 0)
                    {
                        // randomly select amongst units in firing arc
                        // selectedEnemy = validIndexes[Random.Range(0, validIndexes.Count - 1)];
                        selectedEnemy = closestShipInArcIdx;
                    }
                    else
                    {
                        // Randomly select amongst the available targets, to prevent
                        // ships from all ganging up on one.
                        //selectedEnemy = validIndexes[Random.Range(0, validIndexes.Count - 1)];
                        selectedEnemy = closestShipIdx;
                    }
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
