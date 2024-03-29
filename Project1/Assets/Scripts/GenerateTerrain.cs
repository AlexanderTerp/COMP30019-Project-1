﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateTerrain : MonoBehaviour
{
    // Public input fields.

    // The number of nodes in the equation 2^n + 1 where that equation
    // is the terrain's width and height. Does NOT affect its actual width or
    // height. That's reflected by 'sideLength'. Cannot be > 15 due to the 
    // ~65000 vertice limit on a single mesh. 2^15 + 1 = 32769.
    public int n;

    // The Unity side length of the terrain.
    // High 'n', and low 'sideLength' -> High resolution, but small terrain.
    // Low 'n' and high 'sideLength' -> Low resolution, but large terrain.
    public float sideLength;
    public float maxCornerHeight;
    public float minCornerHeight;

    // Diamond-square tweakable values.

    public float minNoiseAddition;
    public float maxNoiseAddition;

    // The % amount that the heightAddition fields should be multiplied
    // by every step to reduce the random value added to each node.
    public float noiseReductionFactor;

    public GameObject waterPlane;

    // Min and max heights of the current terrain.
    [HideInInspector]
    public float highestHeight;
    [HideInInspector]
    public float lowestHeight;
    [HideInInspector]
    public int seed;
    
    // Constants.

    private int numNodesPerSide;
    private float distBetweenNodes;
    private int numNodes;
    private MeshFilter meshFilter;
    private TerrainColor terrainColorizer;

    // Variables.

    // Heights of our terrain.
    private float[,] heights;

    // Use this for initialization
    void Start() {
        Initialize();
        Generate(seed);
    }

    void Update() {
        // Update the terrain's colors for live editing.
        terrainColorizer.SetColorVertices(meshFilter.mesh);
    }

    /**
     * Must be called after Initialize() has been called at least one.
     * Utilizes the values set in Initialize() to generate a height map and then
     * creating a terrain mesh for it. Also properly positions the water.
     */
    public void Generate(int _seed)
    {
        // Calculate and set important values.

        // Comes from each side being 2^n + 1 nodes.
        numNodesPerSide = (int)Mathf.Pow(2, n) + 1;
        numNodes = numNodesPerSide * numNodesPerSide;

        // The actual in-Unity distance between nodes.
        distBetweenNodes = (float)sideLength / (numNodesPerSide - 1);
        Debug.Log(string.Format("numNodesPerSide: {0}, distBetweenNodes: {1}", numNodesPerSide, distBetweenNodes));

        // Calculate the terrain.
        heights = DiamondSquare.GetHeights(_seed, n, minCornerHeight, maxCornerHeight, minNoiseAddition,
            maxNoiseAddition,
            noiseReductionFactor);

        // Define the mesh.
        meshFilter.mesh.Clear(); // Clear any existing data.
        CreateMeshVertices(meshFilter.mesh); // Define the vertices for the mesh.
        MeshGenerator.SetMeshTriangles(meshFilter.mesh, numNodesPerSide); // Define the triangles for the mesh.

        // Calculate the mesh's normals and tangents, to allow for proper lighting
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateTangents();
        meshFilter.mesh.RecalculateBounds();

        // Set the height of the water plane (in the middle of the highest and lowest point of the terrain)
        // Also, get our highest/lowest points of the terrain for later shading
        float[] minMaxHeights = Utilities.GetMinMaxNodes(heights);
        lowestHeight = minMaxHeights[0];
        highestHeight = minMaxHeights[1];
        waterPlane.GetComponent<WaterMesh>().Initialize(sideLength, lowestHeight, highestHeight);

        // Generate a collision mesh, first destroying any existing one.
        Destroy(gameObject.GetComponent<MeshCollider>());
        gameObject.AddComponent<MeshCollider>();

        // Initialize terrain colors.
        terrainColorizer.Initialize(waterPlane.transform, minMaxHeights);
    }

    /**
     * Initializes all the important values that will be used to generate the terrain.
     */
    private void Initialize()
    {
        Debug.Log("Initializing terrain.");
        seed = Utilities.GetTimeBasedSeed(8);
        // Grab references to components.
        meshFilter = GetComponent<MeshFilter>();
        terrainColorizer = GetComponent<TerrainColor>();
    }

    /**
     * Define the vertices of the mesh. This is done in a simple, successive order (see the
     * loop), so the method that sets the triangles will be the complex one,
     * to properly pick the vertices in the right order.
     */
    private void CreateMeshVertices(Mesh mesh) {
        Vector3[] flatVertices = new Vector3[numNodes];

        for (int z = 0, v = 0; z < numNodesPerSide; z++) {
            for (int x = 0; x < numNodesPerSide; x++, v++) {
                flatVertices[v] = new Vector3(x * distBetweenNodes, heights[x, z], z * distBetweenNodes);
            }
        }

        mesh.vertices = flatVertices;
    }
}
