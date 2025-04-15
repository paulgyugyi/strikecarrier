using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
//using UnityEngine.Perception.Randomization.Utilities;

public class SystemGenerator : MonoBehaviour
{
    public bool createSystem = false;
    public float systemSizeX = 440.0f;
    public float systemSizeY = 440.0f;
    public float systemSizeR = 220.0f;
    public float planetSpacing = 24.0f;
    public GameObject RedPlanetPrefab;
    public GameObject BluePlanetPrefab;
    public GameObject BrownPlanetPrefab;
    public GameObject PurplePlanetPrefab;
    public GameObject InterceptorPrefab;
    public GameObject ColonyPrefab;
    public GameObject OrbitalLaserPrefab;
    public GameObject NebulaPrefab;

    public struct PlanetDescription
    {
        public string planetName;
        public float planetRadius;
        public int numColonies;
        public float colonyRadius;
        public float colonySpeed;
        public int numOrbitalLasers;
        public float orbitalLaserRadius;
        public float orbitalLaserSpeed;
        public int numInterceptors;
        public float interceptorRadius;
        public float interceptorSpeed;
    };

    private List<GameObject> planetList = new List<GameObject>();
    private List<GameObject> interceptors = new List<GameObject>();
    private List<GameObject> orbitalLasers = new List<GameObject>();
    private List<GameObject> colonies = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> blackoutPositions = new List<Vector3>();
        Vector3 playerPosition = GameObject.Find("Player").transform.position;
        blackoutPositions.Add(playerPosition);
        Vector3 warpPointPosition = GameObject.Find("WarpPoint").transform.position;
        blackoutPositions.Add(warpPointPosition);
        // Create System
        if (createSystem)
        {
           CreateSystem(blackoutPositions);
            transform.Find("WarpPoint").gameObject.GetComponent<WarpPointActivate>().activationCost = colonies.Count;
        }
    }

    public void CreateSystem(List<Vector3> blackoutPositions)
    {
        NativeList<float2> planetPositions = GenerateSamples(systemSizeX, systemSizeY, planetSpacing);
        foreach (float2 position in planetPositions)
        {
            float x = position.x - systemSizeX / 2.0f;
            float y = position.y - systemSizeY / 2.0f;
            Vector3 newPosition = new Vector3(x, y, 0);
            bool validPosition = true;
            foreach (Vector3 blackoutPosition in blackoutPositions)
            {
                if (Vector3.Distance(blackoutPosition, newPosition) < 20.0f)
                {
                    validPosition = false;
                    break;
                }
            }
            if (Vector3.Distance(Vector3.zero, newPosition) > systemSizeR)
            {
                validPosition = false;
            }
            if (UnityEngine.Random.Range(0,3) == 0)
            {
                // randomly prune some planets for a better layout.
                validPosition = false;
            }
            // Avoid creating planet near start position or warp point
            if (validPosition)
            {
                if (UnityEngine.Random.Range(0, 5) == 0)
                {
                    GameObject nebula = CreateRandomNebula(x, y);
                    if (nebula == null)
                    {
                        Debug.LogWarning("Nebula not created.");
                    }
                    else
                    {
                        // Debug.Log("Nebula created at (" + x + ", " + y + ")");
                        planetList.Add(nebula);
                    }
                }
                else
                {
                    GameObject planet = CreateRandomPlanet(x, y);
                    if (planet == null)
                    {
                        Debug.LogWarning("Planet not created.");
                    }
                    else
                    {
                        // Debug.Log("Planet created at (" + x + ", " + y + ")");
                        planetList.Add(planet);
                    }
                }
            }
        }
        planetPositions.Dispose();
        Debug.Log("Generated " + planetList.Count + " Planets.");
    }
 
    public GameObject CreateRandomPlanet(float x_pos, float y_pos)
    {

        int[] interceptor_dist = { 0, 0, 1, 2, 3, 4, 4, 5, 6, 8, 10, 12 };
        int[] orbital_dist = { 0, 0, 0, 1, 1, 2, 2, 3 };
        PlanetDescription planetDescription = new PlanetDescription();

        planetDescription.planetName = pickPlanetName();
        planetDescription.numColonies = UnityEngine.Random.Range(1, 3);
        planetDescription.colonyRadius = UnityEngine.Random.Range(1f, 4f);
        planetDescription.colonySpeed = UnityEngine.Random.Range(0.15f, 0.3f);
        planetDescription.numOrbitalLasers = 1 *
            orbital_dist[UnityEngine.Random.Range(0, orbital_dist.Length)];
        planetDescription.orbitalLaserRadius = UnityEngine.Random.Range(1f, 5f);
        planetDescription.orbitalLaserSpeed = UnityEngine.Random.Range(0.1f, 0.3f);
        planetDescription.numInterceptors = 1 *
            interceptor_dist[UnityEngine.Random.Range(0, interceptor_dist.Length)];
        planetDescription.interceptorRadius = UnityEngine.Random.Range(1f, 4f);
        planetDescription.interceptorSpeed = 0f;

        GameObject planet = CreatePlanet(planetDescription);
        if (planet != null)
        {
            Vector3 planetPosition = Vector3.zero;
            planetPosition.x = x_pos;
            planetPosition.y = y_pos;
            planet.transform.position = planetPosition;
            planet.transform.parent = transform;
        }
        return planet;
    }

    private GameObject findPrefabPlanet(string planetName)
    {
        if (planetName == "RedPlanet")
        {
            return RedPlanetPrefab;
        }
        else if (planetName == "BluePlanet")
        {
            return BluePlanetPrefab;
        }
        else if (planetName == "BrownPlanet")
        {
            return BrownPlanetPrefab;
        }
        else if (planetName == "PurplePlanet")
        {
            return PurplePlanetPrefab;
        }
        return RedPlanetPrefab;
    }

    private string pickPlanetName()
    {
        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0:
                return "RedPlanet";
            case 1:
                return "BluePlanet";
            case 2:
                return "BrownPlanet";
            case 3:
                return "PurplePlanet";
        }
        return "RedPlanet";
    }

    private GameObject CreatePlanet(PlanetDescription planetDescription)
    {
        GameObject planet = new GameObject(planetDescription.planetName);
        if (planet == null)
        {
            Debug.LogWarning("Failed to create planet.");
            return null;
        }
        planet.transform.localPosition = Vector3.zero;
        planet.transform.localRotation = Quaternion.identity;

        GameObject planetPrefab = Instantiate(
            findPrefabPlanet(planetDescription.planetName),
            planet.transform, false);

        Vector3 unitPosition = Vector3.zero;
        for (int i = 0; i < planetDescription.numInterceptors; i++)
        {
            float rad = planetDescription.interceptorRadius;
            float theta = i * 2 * Mathf.PI / planetDescription.numInterceptors;
            unitPosition.x = rad * Mathf.Cos(theta);
            unitPosition.y = rad * Mathf.Sin(theta);
            GameObject interceptor = Instantiate(InterceptorPrefab,
                planet.transform, false);
            interceptor.transform.localPosition = unitPosition;
            interceptor.transform.localRotation = Quaternion.identity;
            interceptors.Add(interceptor);
        }
        for (int i = 0; i < planetDescription.numOrbitalLasers; i++)
        {
            float rad = planetDescription.orbitalLaserRadius;
            float theta = i * 2 * Mathf.PI / planetDescription.numOrbitalLasers;
            unitPosition.x = rad * Mathf.Cos(theta);
            unitPosition.y = rad * Mathf.Sin(theta);
            GameObject orbitalLaser = Instantiate(OrbitalLaserPrefab,
                planet.transform, false);
            orbitalLaser.transform.localPosition = unitPosition;
            orbitalLaser.GetComponent<Orbit>().OrbitCenter = planet.transform;
            orbitalLaser.GetComponent<Orbit>().OrbitSpeed =
                planetDescription.orbitalLaserSpeed;
            orbitalLasers.Add(orbitalLaser);
        }
        for (int i = 0; i < planetDescription.numColonies; i++)
        {
            float rad = planetDescription.colonyRadius;
            float theta = i * 2 * Mathf.PI / planetDescription.numColonies;
            unitPosition.x = rad * Mathf.Cos(theta);
            unitPosition.y = rad * Mathf.Sin(theta);
            GameObject colony = Instantiate(ColonyPrefab,
                planet.transform, false);
            colony.transform.localPosition = unitPosition;
            colony.GetComponent<Orbit>().OrbitCenter = planet.transform;
            colony.GetComponent<Orbit>().OrbitSpeed =
                planetDescription.colonySpeed;
            colonies.Add(colony);
        }
        return planet;
    }

    public GameObject CreateRandomNebula(float x_pos, float y_pos)
    {
        GameObject nebula = CreateNebula();
        if (nebula != null)
        {
            Vector3 nebulaPosition = Vector3.zero;
            nebulaPosition.x = x_pos;
            nebulaPosition.y = y_pos;
            nebula.transform.position = nebulaPosition;
            nebula.transform.parent = transform;
        }
        return nebula;
    }

    private GameObject CreateNebula()
    {
        GameObject nebula = new GameObject("Nebula");
        if (nebula == null)
        {
            Debug.LogWarning("Failed to create nebula.");
            return null;
        }
        nebula.transform.localPosition = Vector3.zero;
        nebula.transform.localRotation = Quaternion.identity;

        GameObject planetPrefab = Instantiate(
            NebulaPrefab,
            nebula.transform, false);
        return nebula;
    }

    // Following code is in the UnityEngine.Perception.Randomization.Utilities package.
    // The full package does not compile to mobile targets due to dependencies 
    // on the editor envonment, so just this routine is in-lined here.

    /// <summary>
    /// Returns a list of poisson disc sampled points for a given area and density
    /// </summary>
    /// <param name="width">Width of the sampling area</param>
    /// <param name="height">Height of the sampling area</param>
    /// <param name="minimumRadius">The minimum distance required between each sampled point</param>
    /// <param name="seed">The random seed used to initialize the algorithm state</param>
    /// <param name="samplingResolution">The number of potential points sampled around every valid point</param>
    /// <param name="allocator">The allocator to use for the samples container</param>
    /// <returns>The list of generated poisson points</returns>
    public static NativeList<float2> GenerateSamples(
            float width,
            float height,
            float minimumRadius,
            uint seed = 12345,
            int samplingResolution = 30,
            Allocator allocator = Allocator.TempJob)
    {
        if (width < 0)
            throw new ArgumentException($"Width {width} cannot be negative");
        if (height < 0)
            throw new ArgumentException($"Height {height} cannot be negative");
        if (minimumRadius < 0)
            throw new ArgumentException($"MinimumRadius {minimumRadius} cannot be negative");
        if (seed == 0)
            throw new ArgumentException("Random seed cannot be 0");
        if (samplingResolution <= 0)
            throw new ArgumentException($"SamplingAttempts {samplingResolution} cannot be <= 0");

        var superSampledPoints = new NativeList<float2>(allocator);
        var sampleJob = new SampleJob
        {
            width = width + minimumRadius * 2,
            height = height + minimumRadius * 2,
            minimumRadius = minimumRadius,
            seed = seed,
            samplingResolution = samplingResolution,
            samples = superSampledPoints
        }.Schedule();

        var croppedSamples = new NativeList<float2>(allocator);
        new CropJob
        {
            width = width,
            height = height,
            minimumRadius = minimumRadius,
            superSampledPoints = superSampledPoints,
            croppedSamples = croppedSamples
        }.Schedule(sampleJob).Complete();
        superSampledPoints.Dispose();

        return croppedSamples;
    }

    [BurstCompile]
    struct SampleJob : IJob
    {
        public float width;
        public float height;
        public float minimumRadius;
        public uint seed;
        public int samplingResolution;
        public NativeList<float2> samples;

        public void Execute()
        {
            var newSamples = Sample(width, height, minimumRadius, seed, samplingResolution, Allocator.Temp);
            samples.AddRange(newSamples);
            newSamples.Dispose();
        }
    }

    /// <summary>
    /// This job is for filtering out all super sampled Poisson points that are found outside of the originally
    /// specified 2D region. This job will also shift the cropped points back to their original region.
    /// </summary>
    [BurstCompile]
    struct CropJob : IJob
    {
        public float width;
        public float height;
        public float minimumRadius;
        [ReadOnly] public NativeList<float2> superSampledPoints;
        public NativeList<float2> croppedSamples;

        public void Execute()
        {
            var results = new NativeArray<bool>(
                superSampledPoints.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            // The comparisons operations made in this loop are done separately from the list-building loop
            // so that burst can automatically generate vectorized assembly code for this portion of the job.
            for (var i = 0; i < superSampledPoints.Length; i++)
            {
                var point = superSampledPoints[i];
                results[i] = point.x >= minimumRadius && point.x <= width + minimumRadius
                    && point.y >= minimumRadius && point.y <= height + minimumRadius;
            }

            // This list-building code is done separately from the filtering loop
            // because it cannot be vectorized by burst.
            for (var i = 0; i < superSampledPoints.Length; i++)
            {
                if (results[i])
                    croppedSamples.Add(superSampledPoints[i]);
            }

            // Remove the positional offset from the filtered-but-still-super-sampled points
            var offset = new float2(minimumRadius, minimumRadius);
            for (var i = 0; i < croppedSamples.Length; i++)
                croppedSamples[i] -= offset;

            results.Dispose();
        }
    }

    // Algorithm sourced from Robert Bridson's paper "Fast Poisson Disk Sampling in Arbitrary Dimensions"
    // https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
    /// <summary>
    /// Returns a list of poisson disc sampled points for a given area and density
    /// </summary>
    /// <param name="width">Width of the sampling area</param>
    /// <param name="height">Height of the sampling area</param>
    /// <param name="minimumRadius">The minimum distance required between each sampled point</param>
    /// <param name="seed">The random seed used to initialize the algorithm state</param>
    /// <param name="samplingResolution">The number of potential points sampled around every valid point</param>
    /// <param name="allocator">The allocator type of the generated native container</param>
    /// <returns>The list of generated poisson points</returns>
    static NativeList<float2> Sample(
        float width,
        float height,
        float minimumRadius,
        uint seed,
        int samplingResolution,
        Allocator allocator)
    {
        var samples = new NativeList<float2>(allocator);

        // Calculate occupancy grid dimensions
        var random = new Unity.Mathematics.Random(seed);
        var cellSize = minimumRadius / math.sqrt(2f);
        var rows = Mathf.CeilToInt(height / cellSize);
        var cols = Mathf.CeilToInt(width / cellSize);
        var gridSize = rows * cols;
        if (gridSize == 0)
            return samples;

        // Initialize a few constants
        var rSqr = minimumRadius * minimumRadius;
        var samplingArc = math.PI * 2 / samplingResolution;
        var halfSamplingArc = samplingArc / 2;

        // Initialize a hash array that maps a sample's grid position to it's index
        var gridToSampleIndex = new NativeArray<int>(gridSize, Allocator.Temp);
        for (var i = 0; i < gridSize; i++)
            gridToSampleIndex[i] = -1;

        // This list will track all points that may still have space around them for generating new points
        var activePoints = new NativeList<float2>(Allocator.Temp);

        // Randomly place a seed point to kick off the algorithm
        var firstPoint = new float2(random.NextFloat(0f, width), random.NextFloat(0f, height));
        samples.Add(firstPoint);
        var firstPointCol = Mathf.FloorToInt(firstPoint.x / cellSize);
        var firstPointRow = Mathf.FloorToInt(firstPoint.y / cellSize);
        gridToSampleIndex[firstPointCol + firstPointRow * cols] = 0;
        activePoints.Add(firstPoint);

        while (activePoints.Length > 0)
        {
            var randomIndex = random.NextInt(0, activePoints.Length);
            var activePoint = activePoints[randomIndex];

            var nextPointFound = false;
            for (var i = 0; i < samplingResolution; i++)
            {
                var length = random.NextFloat(minimumRadius, minimumRadius * 2);
                var angle = samplingArc * i + random.NextFloat(-halfSamplingArc, halfSamplingArc);

                // Generate a new point within the circular placement region around the active point
                var newPoint = activePoint + new float2(
                    math.cos(angle) * length,
                    math.sin(angle) * length);

                var col = Mathf.FloorToInt(newPoint.x / cellSize);
                var row = Mathf.FloorToInt(newPoint.y / cellSize);

                if (row < 0 || row >= rows || col < 0 || col >= cols)
                    continue;

                // Iterate over the 8 surrounding grid locations to check if the newly generated point is too close
                // to an existing point
                var tooCloseToAnotherPoint = false;
                for (var x = -2; x <= 2; x++)
                {
                    if ((col + x) < 0 || (col + x) >= cols)
                        continue;

                    for (var y = -2; y <= 2; y++)
                    {
                        if ((row + y) < 0 || (row + y) >= rows)
                            continue;

                        var gridIndex = (col + x) + (row + y) * cols;
                        if (gridToSampleIndex[gridIndex] < 0)
                            continue;

                        var distanceSqr = math.distancesq(samples[gridToSampleIndex[gridIndex]], newPoint);

                        if (distanceSqr >= rSqr)
                            continue;
                        tooCloseToAnotherPoint = true;
                        break;
                    }
                }

                if (tooCloseToAnotherPoint)
                    continue;

                // If the new point is accepted, add it to the occupancy grid and the list of generated samples
                nextPointFound = true;
                activePoints.Add(newPoint);
                samples.Add(newPoint);
                gridToSampleIndex[col + row * cols] = samples.Length - 1;
            }

            if (!nextPointFound)
                activePoints.RemoveAtSwapBack(randomIndex);
        }
        gridToSampleIndex.Dispose();
        activePoints.Dispose();

        return samples;

    }
}

