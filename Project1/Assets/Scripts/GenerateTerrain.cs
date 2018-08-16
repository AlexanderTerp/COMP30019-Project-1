﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateTerrain : MonoBehaviour {
    // The number of nodes in the equation 2^n + 1 where that equation
    // is the terrain's width and height. Does NOT affect its actual width or
    // height. That's reflected by 'sideLength'. Cannot be > 15 due to the 
    // ~65000 vertice limit on a single mesh. 2^15 + 1 = 32769.
    public int n;

    // The Unity side length of the terrain.
    // High 'n', and low 'sideLength' -> High resolution, but small terrain.
    // Low 'n' and high 'sideLength' -> Low resolution, but large terrain.
    public float sideLength;
    public int seed;
    public float maxCornerHeight;
    public float minCornerHeight;

    // Diamond-square tweakable values.

    public float maxHeightAddition;
    public float minHeightAddition;

    // The % amount that the heightAddition fields should be multiplied
    // by every step to reduce the random value added to each node.
    public float heightAdditionFactor;

    // Constants.

    private int numNodesPerSide;
    private float distBetweenNodes;
    private int numNodes;
    private MeshFilter meshFilter;

    // Variables.

    // This structure will contain the nodes/vertices for the terrain.
    // It is a 2D structure which can be queried as e.g. nodes[x, z].
    // z is used as the second index instead of y to reflect the fact
    // that the organization of these nodes are in order along the 
    // x-z axes. i.e. nodes[0, 0] is a node on the origin, and 
    // nodes[x, z] is the (x+1)th node over on the x axis, and (z+1)th
    // node over on the z axis.
    private Node[,] nodes;

    // Variables used as part of the actual diamond-step algorithm

    // Nodes to initialize on the next step of the algorithm.
    private Node[] verticesToInitialize;
    // A bool used to keep track of whether we've just performed a 
    // diamond or square step.
    private bool onDiamondStep;
    // A variable used to keep track of the number of nodes over to 
    // go when looking for neighbors in each step.
    private int matrixJumpSize;

    // Use this for initialization
    void Start() {
        // Grab references to components.
        meshFilter = GetComponent<MeshFilter>();

        // Calculate and set important values.

        // Comes from each side being 2^n + 1 nodes.
        numNodesPerSide = (int)Mathf.Pow(2, n) + 1;
        numNodes = numNodesPerSide * numNodesPerSide;

        // The actual in-Unity distance between nodes.
        distBetweenNodes = (float)sideLength / numNodesPerSide;
        Debug.Log(string.Format("numNodesPerSide: {0}, distBetweenNodes: {1}", numNodesPerSide, distBetweenNodes));

        // Calculate the terrain.
        float[,] heights = DiamondSquare.GetHeights(seed, n, minCornerHeight, maxCornerHeight, minHeightAddition,
            maxHeightAddition,
            heightAdditionFactor);
        CreateVertices(heights); // Create the nodes/vertices for the terrain and place them.

        // Define the mesh.
        SetMeshVertices(meshFilter.mesh); // Define the vertices for the mesh.
        SetMeshTriangles(meshFilter.mesh); // Define the triangles for the mesh.

        // Calculate the mesh's normals and tangents, to allow for proper lighting
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateTangents();
    }

    /**
     * This creates the 2D data structure to contain the nodes for the terrain.
     * It does *not* initialize the corners.
     */
    private void CreateVertices(float[,] heights) {
        nodes = new Node[numNodesPerSide, numNodesPerSide];
        for (int z = 0; z < numNodesPerSide; z++) {
            for (int x = 0; x < numNodesPerSide; x++) {
                nodes[x, z] = new Node(new Vector3(x * distBetweenNodes, heights[x, z], z * distBetweenNodes));
            }
        }
    }

    /**
     * Pass the 2D 'nodes' structure as a 1D array into mesh.vertices, to define
     * the mesh's vertices. This is done in a simple, successive order (see the
     * loop), so the method that sets the triangles will be the complex one,
     * to properly pick the vertices in the right order.
     */
    private void SetMeshVertices(Mesh mesh) {
        Vector3[] flatVertices = new Vector3[numNodes];

        for (int z = 0, v = 0; z < numNodesPerSide; z++) {
            for (int x = 0; x < numNodesPerSide; x++, v++) {
                flatVertices[v] = nodes[x, z].pos;
            }
        }

        mesh.vertices = flatVertices;
    }

    /**
     * Defines the triangles in the mesh according to the vertices.
     * This method takes care of referencing each vertex in the correct order
     * so as to create the correct triangles in the correct direction i.e. clockwise.
     */
    private void SetMeshTriangles(Mesh mesh) {
        int[] triangles = new int[6 * (numNodesPerSide - 1) * (numNodesPerSide - 1)];
        for (int triangleIndex = 0, vertexIndex = 0, z = 0; z < numNodesPerSide - 1; z++, vertexIndex++) {
            for (int x = 0; x < numNodesPerSide - 1; x++, triangleIndex += 6, vertexIndex++) {
                // Each iteration inside this inner loop defines a full square i.e. two triangles.
                triangles[triangleIndex] = vertexIndex;

                // The following two lines define the triangle vertices that are shared.
                triangles[triangleIndex + 3] = triangles[triangleIndex + 2] = vertexIndex + 1;
                triangles[triangleIndex + 4] = triangles[triangleIndex + 1] = vertexIndex + numNodesPerSide;

                triangles[triangleIndex + 5] = vertexIndex + numNodesPerSide + 1;
            }
        }

        mesh.triangles = triangles;
    }
}
