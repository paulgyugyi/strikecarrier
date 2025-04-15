using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scenario : MonoBehaviour
{
    public GameObject Carrier1 = null;
    public GameObject Carrier2 = null;

    private CarrierLaunch carrier1Launch = null;
    private CarrierMove carrier1Move = null;

    private CarrierLaunch carrier2Launch = null;
    private CarrierMove carrier2Move = null;

    // Start is called before the first frame update
    void Start()
    {
        carrier1Launch = Carrier1.GetComponent<CarrierLaunch>();
        carrier1Move = Carrier1.GetComponent<CarrierMove>();
        carrier2Launch = Carrier2.GetComponent<CarrierLaunch>();
        carrier2Move = Carrier2.GetComponent<CarrierMove>();
        StartCoroutine(RunScenario());
    }

    private IEnumerator RunScenario()
    {
        Vector3 carrier1StartPosition = Carrier1.transform.position;
        Vector3 carrier2StartPosition = Carrier2.transform.position;
        yield return new WaitForSeconds(3);
        for (int trial = 0; trial < 30; trial++)
        {
            Carrier1.transform.position = carrier1StartPosition;
            Carrier2.transform.position = carrier2StartPosition;
            Carrier1.GetComponent<ResourceTracker>().AddResources(1500f);
            Carrier2.GetComponent<ResourceTracker>().AddResources(1500f);
            for (int i = 0; i < 15; i++)
            {
                carrier1Launch.LaunchShip(carrier1Launch.fighterPrefab);
                carrier2Launch.LaunchShip(carrier2Launch.fighterPrefab);
                yield return new WaitForSeconds(0.5f);
            }
            carrier1Move.inputThrust = true;
            carrier2Move.inputThrust = true;
            yield return new WaitForSeconds(2f);
            carrier1Move.inputThrust = false;
            carrier2Move.inputThrust = false;
            yield return new WaitForSeconds(20f);
            if (carrier1Launch.CountShips() > carrier2Launch.CountShips())
            {
                Carrier1.GetComponent<ResourceTracker>().AddEnergy(1f);
            }
            else
            {
                Carrier2.GetComponent<ResourceTracker>().AddEnergy(1f);
            }
            Debug.LogWarning("Trial " + trial + " Remaining ships: Player1: " + carrier1Launch.CountShips() + " Player2: " + carrier2Launch.CountShips());
            carrier1Launch.RecallShips();
            carrier2Launch.RecallShips();
            yield return new WaitForSeconds(10f);
        }
        Debug.LogWarning("Wins: Player1: " + Carrier1.GetComponent<ResourceTracker>().energy + " Player2: " + Carrier2.GetComponent<ResourceTracker>().energy);
    }
}
