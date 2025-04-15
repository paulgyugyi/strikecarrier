using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ResourceTracker : MonoBehaviour
{
    public float maxResources = 10000f;
    public float startingResources = 5000f;
    private float resources = 0f;
    public float maxEnergy = 512f;
    public float startingEnergy = 0f;
    public float energy = 0f;

    private float regenTime = 0;
    private float regenInterval = 4;
    public int regenAmount = 100;

    // Scene to load if game is lost due to resource exhaustion.
    public int sceneYouLose = 4;

    // UI display
    public UIDocument hudDisplay = null;
    private ProgressBar energyBar = null;
    private ProgressBar resourceBar = null;

    private GameObject warpPoint = null;

    // Start is called before the first frame update
    void Start()
    {
        if (hudDisplay != null)
        {
            energyBar = hudDisplay.rootVisualElement.Q("EnergyBar") as ProgressBar;
            var innerEnergyBar = energyBar.Q(className: "unity-progress-bar__progress");
            innerEnergyBar.style.backgroundColor = Color.yellow;

            resourceBar = hudDisplay.rootVisualElement.Q("ResourceBar") as ProgressBar;
            var innerResourceBar = resourceBar.Q(className: "unity-progress-bar__progress");
            innerResourceBar.style.backgroundColor = new StyleColor(new Color32(255, 127, 0, 255)); 
        }
        else
        {
            Debug.Log("No HUD!");
        }
        warpPoint = GameObject.Find("WarpPoint");
        if (warpPoint == null)
        {
            Debug.Log("No WarpPoint!");
        } else
        {
        }

        ResetResources();
        ResetEnergy();
        UpdateHud();
    }

    private void FixedUpdate()
    {
        if (Time.time > regenTime + regenInterval)
        {
            regenTime = Time.time;
            // Slowly add resources so player never gets stuck.
            AddResources(regenAmount);
        }
    }

    // Update UI with energy and resource levels
    private void UpdateHud()
    {
        if (energyBar != null)
        {
            energyBar.highValue = maxEnergy;
            energyBar.lowValue = 0f;
            energyBar.value = energy;
            energyBar.title = energy.ToString();
        }
        if (resourceBar != null)
        {
            resourceBar.highValue =  maxResources;
            resourceBar.lowValue = 0f;
            resourceBar.value = resources;
            resourceBar.title = Mathf.RoundToInt(resources).ToString();
        }
    }

    // Warp point will activate once enough energy has been collected.
    private void UpdateWarpPoint()
    {
        if (warpPoint != null)
        {
            maxEnergy = warpPoint.GetComponent<WarpPointActivate>().activationCost;
            warpPoint.GetComponent<WarpPointActivate>().CheckActivate(energy);
        }
    }

    public void ResetResources()
    {
        resources = startingResources;
        Debug.Log("Resetting to " + resources + " resources.");
    }

    public void AddResources(float newResources)
    {
        resources += newResources;
        if (resources > maxResources)
        {
            resources = maxResources;
        }
        UpdateHud();
        Debug.Log(name + " acquired " + newResources + " resources, now have " + resources + " resources.");
    }

    public bool HasResources(float cost)
    {
        return (resources >= cost);
    }

    // Might trigger end of game.
    public void UseResources(float cost)
    {
        resources -= cost;
        if (resources < 0)
        {
            resources = 0;
            // You Lose!
            SceneManager.LoadScene(sceneYouLose);
        }
        UpdateHud();
    }

    public void AddEnergy(float newEnergy)
    {
        Debug.Log(name + " acquired " + newEnergy + "energy");
        SetEnergy(energy + newEnergy);
    }

    public void ResetEnergy()
    {
        Debug.Log("Resetting energy.");
        SetEnergy(startingEnergy);
    }

    private void SetEnergy(float newEnergy)
    {
        energy = newEnergy;
        if (energy > maxEnergy)
        {
            energy = maxEnergy;
        }
        else if (energy < 0)
        {
            energy = 0;
        }
        Debug.Log(name + " energy is now " + energy);
        UpdateHud();
        UpdateWarpPoint();
    }
}
