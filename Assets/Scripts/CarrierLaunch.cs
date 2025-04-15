using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// This class contains code for launching fighters, bombers and landers.
public class CarrierLaunch : MonoBehaviour
{
    // Prefabs for each ship type
    public GameObject fighterPrefab = null;
    public GameObject bomberPrefab = null;
    public GameObject landerPrefab = null;

    // List that holds all launched ships
    private List<GameObject> ships = new List<GameObject>();
    private int maxShips = 24;

    // Location to spawn the launched ships
    // These are child objects of the carrier.
    private GameObject fighterLaunchLocation = null;
    private GameObject bomberLaunchLocation = null;
    private GameObject landerLaunchLocation = null;

    // Resource tracker for using/adding resources
    private ResourceTracker resourceTracker = null;

    // Audio
    public AudioSource audioSource;
    public AudioClip launchClip;
    public float volume = 0.5f;


    void Start()
    {
        resourceTracker = GetComponent<ResourceTracker>();
        fighterLaunchLocation = transform.Find("LaunchBow").gameObject;
        bomberLaunchLocation = transform.Find("LaunchPort").gameObject;
        landerLaunchLocation = transform.Find("LaunchStarboard").gameObject;

        // Clean up any launched ships from the previous level.
        RecallShips();
    }

    private GameObject GetPatrolLocation(GameObject shipPrefab)
    {
        if (shipPrefab == fighterPrefab)
        {
            return fighterLaunchLocation;
        }
        else if (shipPrefab == bomberPrefab)
        {
            return bomberLaunchLocation;
        }
        else if (shipPrefab == landerPrefab)
        {
            return landerLaunchLocation;
        }
        return landerLaunchLocation;
    }

    // Launch (i.e. deploy) a ship next to the carrier.
    public void LaunchShip(GameObject shipPrefab)
    {
        if (shipPrefab == null)
        {
            return;
        }
        // Make room for new ships in case anything was recently destroyed.
        PruneDeadShips();

        if (ships.Count < maxShips)
        {
            float cost = shipPrefab.gameObject.GetComponent<Interceptor>().BuildCost;
            if ((gameObject != null) && (resourceTracker.HasResources(cost)))
            {
                Debug.Log("Launching a " + shipPrefab.name);
                GameObject ship = Instantiate(shipPrefab, transform.position,
                transform.rotation);
                ship.GetComponent<Interceptor>().Carrier = gameObject;
                ship.GetComponent<Interceptor>().patrolLocation = GetPatrolLocation(shipPrefab);
                if (shipPrefab == fighterPrefab)
                {
                    ship.GetComponent<Interceptor>().shouldOrbit = true;
                }
                ships.Add(ship);
                resourceTracker.UseResources(cost);
                if ((audioSource != null) && (launchClip != null))
                {
                    audioSource.PlayOneShot(launchClip, volume);
                }
            }
        }
    }

    // This routine instructs all ships to return to the carrier.
    public void RecallShips()
    {
        for (int i = 0; i < ships.Count; i++)
        {
            GameObject ship = ships[i];
            if (ship != null)
            {
                ship.GetComponent<Interceptor>().Recall();
            }
        }
    }

   public int CountShips()
    {
        int count = 0;
        for (int i = 0; i < ships.Count; i++)
        {
            if (ships[i] != null) {
                count++;
            }
        }
        return count;
    }

    // Remove all destroyed ships from the list of launched ships. 
    void PruneDeadShips()
    {
        List<GameObject> prunedShips = new List<GameObject>();
        for (int i = 0; i < ships.Count; i++)
        {
            if (ships[i] != null)
            {
                prunedShips.Add(ships[i]);
            }
        }
        ships = prunedShips;
    }

    // Helper funtions for handling player input.
    public void ReadLaunchLander(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LaunchShip(landerPrefab);
        }
    }
    public void ReadLaunchFighter(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LaunchShip(fighterPrefab);
        }
    }
    public void ReadLaunchBomber(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LaunchShip(bomberPrefab);
        }
    }
    public void ReadRecallShips(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RecallShips();
        }
    }
}

