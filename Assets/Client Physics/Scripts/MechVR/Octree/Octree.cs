//#define LOG_COLLIDER_ALLOC
#define VIRTUAL_COLLIDER_DEPTH
//#define RAND_SPLIT
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MeshNode = System.Collections.Generic.LinkedListNode<MeshNodeData>;
using QType = System.Byte;

// form stanzen :
// schwerpunkt-stoﬂ verh‰ltnis
// multithread

public sealed class Octree
{
	public readonly int depth;
	public readonly int colliderDepth;

	public Node root = null;
	public readonly MeshData meshData;
	public readonly GameObject gameObject;
	public readonly Transform objectTransform;
	private HashSet<Node> hotNodesChache;
#if LOG_COLLIDER_ALLOC
	public int colliderAlloc = 0;
	public int colliderFree = 0;
#endif

	public Octree(int maxDepth, GameObject gameObject, NodeData rootValue, int colliderDepth = -1)
	{
		meshData = new MeshData();

		depth = maxDepth;
		if (colliderDepth > depth)
			throw new ArgumentOutOfRangeException("colliderDepth");
		this.colliderDepth = colliderDepth < 0 ? depth : colliderDepth;

		this.gameObject = gameObject;
		objectTransform = gameObject.transform;

		hotNodesChache = new HashSet<Node>();
		Init(rootValue);
	}

	public void Init(NodeData value)
	{
		root = new Node(value, null, Pos.Root, this);
		root.ParentManaged = false;
		HotNode(root);
		FinalizeMesh();
	}

	public void FinalizeMesh()
	{
		CalculateHotNodes();

		var meshFiller = gameObject.GetComponent<MeshFilter>();
		var mesh = meshFiller.mesh;

		mesh.Clear();
		meshData.GenerateMesh();
		mesh.vertices = meshData.vertices;
		mesh.triangles = meshData.indices;

		mesh.colors = new Color[0];
		mesh.RecalculateNormals();
	}

	private void CalculateHotNodes()
	{
		foreach (var node in hotNodesChache)
		{
			node.Update();
		}

#if LOG_COLLIDER_ALLOC
		Debug.Log(string.Format("Allocations: {0}, Deletions: {1}, HotNodes: {2}", colliderAlloc, colliderFree, hotNodesChache.Count));
		colliderAlloc = 0;
		colliderFree = 0;
#endif

		hotNodesChache.Clear();
	}

	public Node HitNode(Vector3 pos)
	{
		return NodeFromPos(pos, true, false);
	}

	internal Node NodeFromPos(Vector3 pos, bool expand, bool onlyIfRendered)
	{
		int dmax = (1 << depth);
		var ix = (QType)(Mathf.Clamp(pos.x, 0, .9999999f) * dmax);
		var iy = (QType)(Mathf.Clamp(pos.y, 0, .9999999f) * dmax);
		var iz = (QType)(Mathf.Clamp(pos.z, 0, .9999999f) * dmax);

		return NodeFromPos(new QVec3(ix, iy, iz), expand, onlyIfRendered);
	}

	internal Node NodeFromPos(QVec3 pos, bool expand, bool onlyIfRendered)
	{
		if (root == null)
			return null;

		Node retNode = root;
		for (int i = 0; i < depth; i++)
		{
			if (retNode.IsLeaf)
			{
				if (expand)
					retNode.Expand();
				else
					return retNode;
			}

			int depthFromLeft = depth - i - 1;
			int index = (Util.GetBit(pos.x, depthFromLeft) << 2) | (Util.GetBit(pos.y, depthFromLeft) << 1) | (Util.GetBit(pos.z, depthFromLeft) << 0);
			var child = retNode.Children[index];
			if (child == null)
				return null;
			if (onlyIfRendered && child.ParentManaged)
				return retNode;
			retNode = child;
		}

		return retNode;
	}

	internal void RemoveNode(QVec3 pos)
	{
		var node = NodeFromPos(pos, true, false);
		if (node == null)
			return;
		RemoveNode(node);
	}

	public void RemoveNode(Vector3 pos)
	{
		var node = NodeFromPos(pos, true, false);
		if (node == null)
			return;
		RemoveNode(node);
	}

	private void RemoveNode(Node node)
	{
		node.DestroyRecursive();
	}

	internal void HotNode(Node node)
	{
		//if (node.ParentManagedCollider)
		//	return;
		hotNodesChache.Add(node);
	}

	// actions

	public void Carve(Ray ray)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<Node> Carve(BoundingSphere sphere)
	{
		if (root == null)
			return Enumerable.Empty<Node>();
		return root.Carve(sphere);
	}

	public override string ToString()
	{
		return root == null ? "<empty>" : root.ToString();
	}
}

public sealed class Node
{
	private readonly Octree Owner;
	public NodeData Value;
	public readonly int Level;
	public readonly Pos Position;
	public readonly QVec3 Position3d;
	public readonly Node Parent;
	public Node[] Children;
	private readonly bool[] faces = { true, true, true, true, true, true };

	// runtime static values
	private int depthVal { get { return 1 << Owner.depth; } }
	private int levelVal { get { return 1 << Level; } }
	private int inverseLevelVal { get { return 1 << (Owner.depth - Level); } }
	public bool IsLeaf { get { return Children == null; } }
	public Vector3 Center3df
	{
		get
		{
			float offdim = .5f - (1f / (2 << Level));
			return new Vector3(Position3d.x, Position3d.y, Position3d.z) / depthVal - new Vector3(offdim, offdim, offdim);
		}
	}
	private readonly int cachedHash;

	// State logic
	private bool alive;
	public bool Alive
	{
		get { return alive; }
		private set
		{
			var changed = meshActive ^ value;
			MeshChanged |= changed;
			ColliderChanged |= changed;
			alive = value;
		}
	}
	private bool meshActive;
	private bool MeshActive
	{
		get { return meshActive; }
		set
		{
			MeshChanged |= meshActive ^ value;
			meshActive = value;
		}
	}
	private bool MeshChanged { get; set; }

	private bool parentManaged;
	public bool ParentManaged
	{
		get { return parentManaged; }
		set
		{
			var changed = parentManaged ^ value;
			ColliderChanged |= changed;
			MeshChanged |= changed;
			parentManaged = value;
		}
	}
	private bool ColliderChanged { get; set; }
	private bool colliderActive;
	private bool ColliderActive
	{
		get { return colliderActive; }
		set
		{
			ColliderChanged |= colliderActive ^ value;
			colliderActive = value;
		}
	}


	// Meshdata
	public GameObject ownCollider;
	public GameObject[] optCollider;
	private MeshNode node;

	public Node(NodeData value, Node parent, Pos position, Octree owner)
	{
		Value = value;
		Parent = parent;
		Position = position;
		Owner = owner;

		Alive = true;
		Children = null;
		MeshActive = true;
		ColliderActive = true;
		ParentManaged = true;

		if (parent == null)
		{
			Level = 0;
			Position3d = new QVec3(0, 0, 0);
		}
		else
		{
			Level = parent.Level + 1;
			int remDepthVal = 1 << (Owner.depth - Level);
			Position3d = new QVec3(
				(QType)(Parent.Position3d.x + (((int)position & (1 << 2)) != 0 ? remDepthVal : 0)),
				(QType)(Parent.Position3d.y + (((int)position & (1 << 1)) != 0 ? remDepthVal : 0)),
				(QType)(Parent.Position3d.z + (((int)position & (1 << 0)) != 0 ? remDepthVal : 0)));
		}

		cachedHash = Level << 24 | Position3d.GetHashCode();
	}

	public void Update()
	{
		UpdateColliderOptimized();
		UpdateMeshOptimized();
	}

	// // Collider // //

	private void UpdateColliderOptimized()
	{
#if VIRTUAL_COLLIDER_DEPTH
		if (Level > Owner.colliderDepth)
		{
			ColliderActive = false;
			UpdateCollider();
			return;
		}
		else if (Level == Owner.colliderDepth)
		{
			ColliderActive = true;
			UpdateCollider();
			return;
		}
		// else Level < Owner.colliderDepth
#endif
		if (IsLeaf)
		{
			UpdateCollider();
			return;
		}

		// TODO / still naive
		UpdateCollider();
		foreach (var item in Children)
		{
			if (item != null)
				item.UpdateCollider();
		}
	}

	private void UpdateCollider()
	{
		if (!ColliderChanged)
			return;
		ColliderChanged = false;

		if (!ColliderActive || ParentManaged || !Alive)
		{
			DestroyCollider();
			return;
		}

		if (ownCollider == null)
		{
#if LOG_COLLIDER_ALLOC
			Owner.colliderAlloc++;
#endif

			ownCollider = new GameObject();
			var boxCol = ownCollider.AddComponent<BoxCollider>();

			boxCol.enabled = false;

			var ownTransform = ownCollider.transform;
			var ownerTransform = Owner.objectTransform;

			ownTransform.parent = ownerTransform;
			ownTransform.localScale = Vector3.one / levelVal;
			var tmpPos = ownerTransform.rotation * Center3df;
			var tmpPos2 = Vector3.Scale(tmpPos, ownerTransform.localScale);
			ownTransform.position = tmpPos2 + ownerTransform.position;
			ownTransform.rotation = new Quaternion();

			boxCol.enabled = true;
		}
	}

	private void DestroyCollider()
	{
		if (ownCollider != null)
		{
#if LOG_COLLIDER_ALLOC
			Owner.colliderFree++;
#endif
			GameObject.Destroy(ownCollider);
			ownCollider = null;
		}
		if (optCollider != null)
		{
			Array.ForEach(optCollider, GameObject.Destroy);
			optCollider = null;
		}
	}

	// // Mesh // //

	private void UpdateMeshOptimized()
	{
		if (IsLeaf)
		{
			UpdateMesh();
			return;
		}

		// TODO / still naive
		UpdateMesh();
		foreach (var item in Children)
		{
			if (item != null)
				item.UpdateMesh();
		}
	}

	private void UpdateMesh()
	{
		if (!MeshChanged)
			return;
		MeshChanged = false;

		if (!MeshActive || ParentManaged || !Alive)
		{
			DestroyMesh();
			return;
		}

		int facesCount = 0;
		for (int i = 0; i < Util.OctDirs; i++)
			facesCount += faces[i] ? 1 : 0;

		if (facesCount == 0)
		{
			DestroyMesh();
			return;
		}

		if (node != null)
		{
			if (node.Value.GetFacesCount() != facesCount)
			{
				DestroyMesh();
				node = Owner.meshData.AllocateMeshNode(facesCount);
			}
		}
		else
		{
			node = Owner.meshData.AllocateMeshNode(facesCount);
		}

		DrawSomePlane();
	}

	private void DestroyMesh()
	{
		if (node != null)
		{
			Owner.meshData.FreeMeshNode(node);
			node = null;
		}
	}

	// // Other // //

	public void Destroy(bool remove)
	{
		Owner.HotNode(this);
		MeshActive = false;
		ColliderActive = false;

		if (!remove)
			return;

		Alive = false;

		if (Parent == null)
		{
			Owner.root = null;
			return;
		}

		Parent.Children[(int)Position] = null;
		Parent.DisableParentManaged();

		foreach (var dir in Util.Dirs)
		{
			var move = Util.GetMove(Position, dir);
			if (move.IsOutsideCube)
			{
				var neighPos = Position3d.Move(dir, inverseLevelVal, depthVal);
				if (!neighPos.HasValue)
					continue;
				var neighNode = Owner.NodeFromPos(neighPos.Value, false, true);
				if (neighNode == null)
					continue;
				if (neighNode.Level > Level)
					continue;
				neighNode.SetFace(Util.SwapDirs[(int)dir], true);
			}
			else
			{
				var neighNode = Parent.Children[(int)move.OutPos];
				if (neighNode == null)
					continue;
				neighNode.SetFace(Util.SwapDirs[(int)dir], true);
			}
		}

		if (Util.AllNull(Parent.Children))
			Parent.Destroy(true);
	}

	private void DisableParentManaged()
	{
		foreach (var child in Children)
		{
			if (child != null)
			{
				child.ParentManaged = false;
				Owner.HotNode(child);
			}
		}
		
		Destroy(false);
		var stop = ParentManaged;
		ParentManaged = false;
		if (Parent != null && stop)
			Parent.DisableParentManaged();
	}

	public void DestroyRecursive()
	{
		if (IsLeaf)
		{
			Destroy(true);
			return;
		}
		foreach (var child in Children)
		{
			if (child != null)
				child.DestroyRecursive();
		}
	}

	private void DrawSomePlane()
	{
		float dim = 1f / (levelVal * 2);
		var center = Center3df;

		int faceNum = 0;
		if (faces[(int)Dir.Xp]) AddPlane(faceNum++, center + new Vector3(dim, 0, 0), new Vector3(0, dim, dim), new Vector3(0, -dim, dim), true); // Xp
		if (faces[(int)Dir.Xm]) AddPlane(faceNum++, center + new Vector3(-dim, 0, 0), new Vector3(0, -dim, dim), new Vector3(0, dim, dim), true); // Xm
		if (faces[(int)Dir.Yp]) AddPlane(faceNum++, center + new Vector3(0, dim, 0), new Vector3(-dim, 0, dim), new Vector3(dim, 0, dim), false); // Yp
		if (faces[(int)Dir.Ym]) AddPlane(faceNum++, center + new Vector3(0, -dim, 0), new Vector3(dim, 0, dim), new Vector3(-dim, 0, dim), false); // Ym
		if (faces[(int)Dir.Zp]) AddPlane(faceNum++, center + new Vector3(0, 0, dim), new Vector3(dim, dim, 0), new Vector3(-dim, dim, 0), false); // Zp
		if (faces[(int)Dir.Zm]) AddPlane(faceNum++, center + new Vector3(0, 0, -dim), new Vector3(-dim, dim, 0), new Vector3(dim, dim, 0), false); // Zm
	}

	private void AddPlane(int faceNum, Vector3 center, Vector3 dim1, Vector3 dim2, bool flip)
	{
		//triange and vertices list must be initianlized
		//flip means flip the
		int vx = faceNum * MeshData.verticesPerFace;
		int vi = faceNum * MeshData.indicesPerFace;

		node.Value.vertices[vx + 0] = center + dim1;
		node.Value.vertices[vx + 1] = center + dim2;
		node.Value.vertices[vx + 2] = center - dim1;
		node.Value.vertices[vx + 3] = center - dim2;

		if (flip)
		{
			node.Value.indices[vi + 0] = vx + 0;
			node.Value.indices[vi + 1] = vx + 1;
			node.Value.indices[vi + 2] = vx + 2;
			node.Value.indices[vi + 3] = vx + 0;
			node.Value.indices[vi + 4] = vx + 2;
			node.Value.indices[vi + 5] = vx + 3;
		}
		else
		{
			node.Value.indices[vi + 0] = vx + 0;
			node.Value.indices[vi + 1] = vx + 1;
			node.Value.indices[vi + 2] = vx + 3;
			node.Value.indices[vi + 3] = vx + 1;
			node.Value.indices[vi + 4] = vx + 2;
			node.Value.indices[vi + 5] = vx + 3;
		}
	}

	public void Expand()
	{
		if (Children != null)
			return;

		//Destroy(false);
		Owner.HotNode(this);

		Children = new Node[Util.OctNodes];
		for (int i = 0; i < Util.OctNodes; i++)
		{
			var child = new Node(Value, this, (Pos)i, Owner);
			child.UpdateAllFaces(true);
			// TODO remove with all parent managed
			Owner.HotNode(child);
			Children[i] = child;
		}
	}

	private void UpdateAllFaces(bool assumeNodeNeighboursActive)
	{
		foreach (var dir in Util.Dirs)
		{
			UpdateFace(dir, assumeNodeNeighboursActive);
		}
	}

	private void UpdateFace(Dir dir, bool assumeNodeNeighboursActive)
	{
		if (Parent == null)
		{
			SetFace(dir, true);
			return;
		}

		var move = Util.GetMove(Position, dir);
		if (move.IsOutsideCube)
		{
			var targetQvec = Position3d.Move(dir, inverseLevelVal, depthVal);
			if (targetQvec.HasValue)
			{
				var neighNode = Owner.NodeFromPos(targetQvec.Value, false, true);
				SetFace(dir, neighNode == null || neighNode.Level > Level);
			}
			else
			{
				SetFace(dir, true);
			}
		}
		else
		{
			if (assumeNodeNeighboursActive)
			{
				SetFace(dir, false);
			}
			else
			{
				var neighNode = Parent.Children[(int)move.OutPos];
				SetFace(dir, neighNode == null);
			}
		}
	}

	private void SetFace(Dir dir, bool status)
	{
		if (IsLeaf && faces[(int)dir] == status)
			return;
		faces[(int)dir] = status;
		MeshChanged = true;
		Owner.HotNode(this);

		if (Children != null)
		{
			var affected = Util.AffectedSites[(int)dir];
			for (int i = 0; i < affected.Length; i++)
			{
				var child = Children[(int)affected[i]];
				if (child != null)
					child.SetFace(dir, status);
			}
		}
	}

	public IEnumerable<Node> Carve(BoundingSphere sphere)
	{
		var carveList = new HashSet<Node>();
		Carve(sphere, carveList);
		return carveList;
	}

	public void Carve(BoundingSphere sphere, HashSet<Node> activeList)
	{
		float dim = 1f / (levelVal * 2);
		var dimVec = new Vector3(dim, dim, dim);
		var center = Center3df;

		var vecMin = center - dimVec;
		var vecMax = center + dimVec;

		if (!MathUtil.IsPointInsideCube(sphere.position, vecMin, vecMax) &&
			!MathUtil.DoesCubeIntersectSphere(vecMin, vecMax, sphere.position, sphere.radius)) return;

		if (Level < Owner.depth)
		{
			if (MathUtil.CubeInsideSphere(vecMin, vecMax, sphere.position, sphere.radius))
			{
				activeList.Add(this);
				return;
			}

#if RAND_SPLIT

#endif
			Expand();
			foreach (var child in Children)
			{
				if (child != null)
					child.Carve(sphere, activeList);
			}
		}
		else
		{
			activeList.Add(this);
		}
	}

	public override string ToString()
	{
		return Position3d + " " + Position + " " +
			(IsLeaf
			? "<leaf>"
			: string.Join(" ", Children.Select(x => x == null ? "X" : "O").ToArray()));

	}

	public override bool Equals(object obj)
	{
		var other = obj as Node;
		if (other == null)
			return false;
		return cachedHash == other.cachedHash;
	}

	public override int GetHashCode() { return cachedHash; }
}
