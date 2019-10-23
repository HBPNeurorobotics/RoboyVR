//#define POKE_TEST
//#define RECORD_ACTIONS

using System.Collections.Generic;
using UnityEngine;

public class OctreeHandler : MonoBehaviour
{
	public int meshDepth;
	public int colliderDepth;
	public float blockHp;
	/// <summary>
	/// factor the impact is multiplied with
	/// </summary>
	public float hitFactor;

	private Octree tree;

#if RECORD_ACTIONS
	private int step = 0;
	private bool replaying = false;
	private List<RecordData> record = new List<RecordData>();
#endif

#if POKE_TEST
	private RngVec rng = new RngVec(1);
#endif

	// Use this for initialization
	void Start()
	{
		this.GetComponent<Rigidbody>().mass = transform.localScale.x * transform.localScale.y * transform.localScale.z;

		ResetOct();

#if POKE_TEST
		long hitTime = 0;
		long meshTime = 0;
		System.Diagnostics.Stopwatch sw;

		for (int i = 0; i < 100; i++)
		{
			sw = System.Diagnostics.Stopwatch.StartNew();
			HitLocal(rng.NextVector());
			hitTime += sw.ElapsedTicks;
			sw = System.Diagnostics.Stopwatch.StartNew();
			tree.FinalizeMesh();
			meshTime += sw.ElapsedTicks;
		}
		//sw = System.Diagnostics.Stopwatch.StartNew();
		//tree.FinalizeMesh();
		//meshTime += sw.ElapsedTicks;
		Debug.Log(string.Format("HitTime: {0}, MeshTime {1}", TickToMs(hitTime), TickToMs(meshTime)));
#endif
	}

	private static float TickToMs(long ticks)
	{
		return (float)System.Math.Round(((ticks * 1000) / (float)System.Diagnostics.Stopwatch.Frequency), 1);
	}

	// Update is called once per frame
	void Update()
	{
		//var sw = System.Diagnostics.Stopwatch.StartNew();
		//HitLocal(rng.NextVector());
		//UpdateMesh();
		//time += sw.ElapsedTicks;

		//if (rng.VecCnt % 100 == 0)
		//	Debug.Log("Time: " + (time / System.Diagnostics.Stopwatch.Frequency));
	}

	void OnCollisionEnter(Collision collision)
	{
		//var sw = System.Diagnostics.Stopwatch.StartNew();
		var hitStr = collision.impulse.magnitude * hitFactor;

		// radius-damage, proportional to velocity
		// see: https://www.google.de/search?q=2%2F(x%5E0.3%2B1)+-+1&oq=2%2F(x%5E0.3%2B1)+-+1&aqs=chrome..69i57.691j0j7&sourceid=chrome&ie=UTF-8#q=log2(x+%2B+1)&*
		var radiusFact = Mathf.Log(hitStr + 1, 10);

		foreach (var i in collision.contacts)
		{
			//i.point
			var trans = i.otherCollider.transform;

			//var otherRigid = trans.GetComponent<Rigidbody>();
			//if (otherRigid == null)
			//	continue;
			//var sphereCenter2 = transform.InverseTransformPoint(trans.TransformPoint(otherRigid.centerOfMass));
			var hitPointLocal = transform.InverseTransformPoint(i.point);

			// maximal affected radius
			var sphereCenter = hitPointLocal - collision.impulse.normalized * (Mathf.Min(hitStr, 1000) / 1000);

			var dist = Vector3.Distance(sphereCenter, hitPointLocal);
			var nodes = Carve(new BoundingSphere(sphereCenter, dist * 3f / 2f));
			foreach (var node in nodes)
			{
				if (!node.Alive)
					continue;
				var x = Vector3.Distance(sphereCenter, node.Center3df);

				// impact damage, proportional to distance, 0 = point of impact, 1 = furthest away. x = distance, result(y) = multiplication factor
				// see: https://www.google.de/search?q=2%2F(x%5E0.3%2B1)+-+1&oq=2%2F(x%5E0.3%2B1)+-+1&aqs=chrome..69i57.691j0j7&sourceid=chrome&ie=UTF-8
				var damageFact = 2f / (Mathf.Pow(x, 0.2f) + 1f) - 1f;

				node.Value.Hp -= radiusFact * damageFact;

				if (node.Value.Hp < -0.2f)
					node.DestroyRecursive();
			}
		}
		FinalizeMesh();
		//sw.Stop();
		//var contactsms = TickX(sw);

		//Debug.Log("Ctx: " + collision.contacts.Length + " Ms: " + contactsms + " ms/ctx: " + System.Math.Round(contactsms / collision.contacts.Length, 1) + " Mesh: " + TickX(sw));
	}

	private static Vector3 Log10Vector(Vector3 vec)
	{
		return new Vector3(Mathf.Log10(vec.x + 1), Mathf.Log10(vec.y + 1), Mathf.Log10(vec.z + 1));
	}

	private static Vector3 OneThroughVector(Vector3 vec)
	{
		return new Vector3(1f / vec.x, 1f / vec.y, 1f / vec.z);
	}

	private void FinalizeMesh()
	{
#if RECORD_ACTIONS
		Record(RecordAction.Rebuild, null);
#endif
		tree.FinalizeMesh();
	}

	public void HitLocal(Vector3 localPoint)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		HitInternal(localPoint);
		FinalizeMesh();
		sw.Stop();
		Debug.Log(sw.ElapsedMilliseconds);
	}

	private void HitInternal(Vector3 localPoint)
	{
#if RECORD_ACTIONS
		Record(RecordAction.HitPoint, localPoint);
#endif
		var node = tree.HitNode(localPoint + new Vector3(.5f, .5f, .5f));
		if (node == null) return;
		node.Value.Hp -= 0.6f;
		if (node.Value.Hp <= -1f)
			node.DestroyRecursive();
	}

	public IEnumerable<Node> Carve(BoundingSphere localSphere)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		var items = tree.Carve(localSphere);
		sw.Stop();
		//Debug.Log(sw.ElapsedMilliseconds);
		return items;
	}

	public void ResetOct()
	{
#if RECORD_ACTIONS
		replaying = true;
		if (tree != null)
		{
			tree.root.DestroyRecursive();
			FinalizeMesh();
		}

		tree = new Octree(meshDepth, gameObject, new NodeData(), colliderDepth);
		FinalizeMesh();
		replaying = false;
		step = 0;
#else
		if (tree != null)
		{
			tree.root.DestroyRecursive();
			FinalizeMesh();
		}

		tree = new Octree(meshDepth, gameObject, new NodeData(), colliderDepth);
		FinalizeMesh();
#endif
	}

	public void Replay()
	{
#if RECORD_ACTIONS
		while (step < record.Count)
			Step();
#endif
	}

	public void Step()
	{
#if RECORD_ACTIONS
		if (step < 0 || step >= record.Count)
			return;

		replaying = true;
		var act = record[step++];
		switch (act.Action)
		{
		case RecordAction.HitPoint:
			HitInternal((Vector3)act.Data);
			break;
		case RecordAction.Rebuild:
			FinalizeMesh();
			break;
		default:
			break;
		}
		replaying = false;

		if (act.Action != RecordAction.Rebuild || (step < record.Count && record[step].Action == RecordAction.Rebuild))
			Step();
#endif
	}

	public void Undo()
	{
#if RECORD_ACTIONS
		if (step >= record.Count)
			step = record.Count - 1;
		while (step > 0 && record[step].Action != RecordAction.Rebuild)
			step--;
		while (step > 0 && record[step].Action == RecordAction.Rebuild)
			step--;
		var reStep = step;
		ResetOct();
		while (step < reStep)
			Step();
#endif
	}

#if RECORD_ACTIONS
	private void Record(RecordAction action, object data)
	{
		if (!replaying)
		{
			Replay();
			record.Add(new RecordData { Action = action, Data = data });
			step = record.Count;
		}
	}
#endif
}

public struct NodeData
{
	public float Hp;
}

class RngVec
{
	System.Random rng;
	public int VecCnt { get; private set; }

	public RngVec(int seed)
	{
		rng = new System.Random(1);
	}

	public Vector3 NextVector()
	{
		VecCnt++;
		var x = (float)rng.NextDouble();
		var y = (float)rng.NextDouble();
		var z = (float)rng.NextDouble();
		return new Vector3(x, y, z) - new Vector3(.5f, .5f, .5f);
	}
}

#if RECORD_ACTIONS
class RecordData
{
	public RecordAction Action;
	public object Data;
	public override string ToString() { return Action + " " + Data; }
}

enum RecordAction
{
	HitPoint,
	Rebuild,
}
#endif