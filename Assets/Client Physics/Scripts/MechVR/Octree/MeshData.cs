using System;
using System.Collections.Generic;
using UnityEngine;
using MeshNode = System.Collections.Generic.LinkedListNode<MeshNodeData>;

public class MeshData
{
	private LinkedList<MeshNodeData> meshHeap = new LinkedList<MeshNodeData>();
	private int faceCount;

	public Vector3[] vertices;
	public int[] indices;

	public const int indicesPerFace = 6;
	public const int verticesPerFace = 4;

	public MeshNode AllocateMeshNode(int faces)
	{
		faceCount += faces;
		return meshHeap.AddLast(new MeshNodeData
		{
			indices = new int[faces * indicesPerFace],
			vertices = new Vector3[faces * verticesPerFace]
		});
	}

	public void FreeMeshNode(MeshNode node)
	{
		faceCount -= node.Value.GetFacesCount();
		meshHeap.Remove(node);
	}

	public void GenerateMesh()
	{
		indices = new int[faceCount * indicesPerFace];
		vertices = new Vector3[faceCount * verticesPerFace];

		int indexPos = 0;
		int vertexPos = 0;
		foreach (var node in meshHeap)
		{
			for (int i = 0; i < node.indices.Length; i++)
				indices[i + indexPos] = node.indices[i] + vertexPos;
			indexPos += node.indices.Length;

			Array.Copy(node.vertices, 0, vertices, vertexPos, node.vertices.Length);
			vertexPos += node.vertices.Length;
		}
	}
}

public struct MeshNodeData
{
	public Vector3[] vertices;
	public int[] indices;

	public int GetFacesCount() { return vertices.Length / MeshData.verticesPerFace; }
}