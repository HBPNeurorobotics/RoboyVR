using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignMeshCollider : MonoBehaviour {

    private string lHand = "COL_Alpha_Surface_015_convex_hull(Clone)";

    private string lThumb1 = "COL_Alpha_Surface_016_convex_hull(Clone)";
    private string lThumb2 = "COL_Alpha_Surface_017_convex_hull(Clone)";
    private string lThumb3 = "COL_Alpha_Surface_018_convex_hull(Clone)";

    private string lIndex1 = "COL_Alpha_Surface_019_convex_hull(Clone)";
    private string lIndex2 = "COL_Alpha_Surface_026_convex_hull(Clone)";
    private string lIndex3 = "COL_Alpha_Surface_027_convex_hull(Clone)";

    private string lMiddle1 = "COL_Alpha_Surface_020_convex_hull(Clone)";
    private string lMiddle2 = "COL_Alpha_Surface_025_convex_hull(Clone)";
    private string lMiddle3 = "COL_Alpha_Surface_028_convex_hull(Clone)";

    private string lRing1 = "COL_Alpha_Surface_021_convex_hull(Clone)";
    private string lRing2 = "COL_Alpha_Surface_024_convex_hull(Clone)";
    private string lRing3 = "COL_Alpha_Surface_029_convex_hull(Clone)";

    private string lPinky1 = "COL_Alpha_Surface_022_convex_hull(Clone)";
    private string lPinky2 = "COL_Alpha_Surface_023_convex_hull(Clone)";
    private string lPinky3 = "COL_Alpha_Surface_030_convex_hull(Clone)";

    private string rHand = "COL_Alpha_Surface_033_convex_hull(Clone)";

    private string rThumb1 = "COL_Alpha_Surface_034_convex_hull(Clone)";
    private string rThumb2 = "COL_Alpha_Joints_030_convex_hull(Clone)";
    private string rThumb3 = "COL_Alpha_Surface_036_convex_hull(Clone)";

    private string rIndex1 = "COL_Alpha_Surface_037_convex_hull(Clone)";
    private string rIndex2 = "COL_Alpha_Surface_044_convex_hull(Clone)";
    private string rIndex3 = "COL_Alpha_Surface_048_convex_hull(Clone)";

    private string rMiddle1 = "COL_Alpha_Surface_038_convex_hull(Clone)";
    private string rMiddle2 = "COL_Alpha_Surface_043_convex_hull(Clone)";
    private string rMiddle3 = "COL_Alpha_Surface_047_convex_hull(Clone)";

    private string rRing1 = "COL_Alpha_Surface_039_convex_hull(Clone)";
    private string rRing2 = "COL_Alpha_Surface_042_convex_hull(Clone)";
    private string rRing3 = "COL_Alpha_Surface_046_convex_hull(Clone)";

    private string rPinky1 = "COL_Alpha_Surface_040_convex_hull(Clone)";
    private string rPinky2 = "COL_Alpha_Surface_041_convex_hull(Clone)";
    private string rPinky3 = "COL_Alpha_Surface_045_convex_hull(Clone)";

    private GameObject box1;
    private GameObject box2;
    private GameObject box3;
    private GameObject box4;

    private Transform box1reference;
    private Transform box2reference;
    private Transform box3reference;
    private Transform box4reference;

    private bool initialized = false;

    //[SerializeField] private UserAvatarService userAvatarService;

    private GameObject lHand_vib;

    private GameObject lThumb1_vib;
    private GameObject lThumb2_vib;
    private GameObject lThumb3_vib;

    private GameObject lIndex1_vib;
    private GameObject lIndex2_vib;
    private GameObject lIndex3_vib;

    private GameObject lMiddle1_vib;
    private GameObject lMiddle2_vib;
    private GameObject lMiddle3_vib;

    private GameObject lRing1_vib;
    private GameObject lRing2_vib;
    private GameObject lRing3_vib;

    private GameObject lPinky1_vib;
    private GameObject lPinky2_vib;
    private GameObject lPinky3_vib;

    private GameObject rHand_vib;

    private GameObject rThumb1_vib;
    private GameObject rThumb2_vib;
    private GameObject rThumb3_vib;

    private GameObject rIndex1_vib;
    private GameObject rIndex2_vib;
    private GameObject rIndex3_vib;

    private GameObject rMiddle1_vib;
    private GameObject rMiddle2_vib;
    private GameObject rMiddle3_vib;

    private GameObject rRing1_vib;
    private GameObject rRing2_vib;
    private GameObject rRing3_vib;

    private GameObject rPinky1_vib;
    private GameObject rPinky2_vib;
    private GameObject rPinky3_vib;
    
    private bool test = false;

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		if(!initialized && UserAvatarService.Instance.IsRemoteAvatarPresent) // delete test, then everything sould work
        {
            lHand_vib = GameObject.Find(lHand);
            lHand_vib.AddComponent<MeshCollider>();

            lThumb1_vib = GameObject.Find(lThumb1);
            lThumb1_vib.AddComponent<MeshCollider>();
            lThumb2_vib = GameObject.Find(lThumb2);
            lThumb2_vib.AddComponent<MeshCollider>();
            lThumb3_vib = GameObject.Find(lThumb3);
            lThumb3_vib.AddComponent<MeshCollider>();

            lIndex1_vib = GameObject.Find(lIndex1);
            lIndex1_vib.AddComponent<MeshCollider>();
            lIndex2_vib = GameObject.Find(lIndex2);
            lIndex2_vib.AddComponent<MeshCollider>();
            lIndex3_vib = GameObject.Find(lIndex3);
            lIndex3_vib.AddComponent<MeshCollider>();

            lMiddle1_vib = GameObject.Find(lMiddle1);
            lMiddle1_vib.AddComponent<MeshCollider>();
            lMiddle2_vib = GameObject.Find(lMiddle2);
            lMiddle2_vib.AddComponent<MeshCollider>();
            lMiddle3_vib = GameObject.Find(lMiddle3);
            lMiddle3_vib.AddComponent<MeshCollider>();

            lRing1_vib = GameObject.Find(lRing1);
            lRing1_vib.AddComponent<MeshCollider>();
            lRing2_vib = GameObject.Find(lRing2);
            lRing2_vib.AddComponent<MeshCollider>();
            lRing3_vib = GameObject.Find(lRing3);
            lRing3_vib.AddComponent<MeshCollider>();

            lPinky1_vib = GameObject.Find(lPinky1);
            lPinky1_vib.AddComponent<MeshCollider>();
            lPinky2_vib = GameObject.Find(lPinky2);
            lPinky2_vib.AddComponent<MeshCollider>();
            lPinky3_vib = GameObject.Find(lPinky3);
            lPinky3_vib.AddComponent<MeshCollider>();

            rHand_vib = GameObject.Find(rHand);
            rHand_vib.AddComponent<MeshCollider>();

            rThumb1_vib = GameObject.Find(rThumb1);
            rThumb1_vib.AddComponent<MeshCollider>();
            rThumb2_vib = GameObject.Find(rThumb2);
            rThumb2_vib.AddComponent<MeshCollider>();
            rThumb3_vib = GameObject.Find(rThumb3);
            rThumb3_vib.AddComponent<MeshCollider>();

            rIndex1_vib = GameObject.Find(rIndex1);
            rIndex1_vib.AddComponent<MeshCollider>();
            rIndex2_vib = GameObject.Find(rIndex2);
            rIndex2_vib.AddComponent<MeshCollider>();
            rIndex3_vib = GameObject.Find(rIndex3);
            rIndex3_vib.AddComponent<MeshCollider>();

            rMiddle1_vib = GameObject.Find(rMiddle1);
            rMiddle1_vib.AddComponent<MeshCollider>();
            rMiddle2_vib = GameObject.Find(rMiddle2);
            rMiddle2_vib.AddComponent<MeshCollider>();
            rMiddle3_vib = GameObject.Find(rMiddle3);
            rMiddle3_vib.AddComponent<MeshCollider>();

            rRing1_vib = GameObject.Find(rRing1);
            rRing1_vib.AddComponent<MeshCollider>();
            rRing2_vib = GameObject.Find(rRing2);
            rRing2_vib.AddComponent<MeshCollider>();
            rRing3_vib = GameObject.Find(rRing3);
            rRing3_vib.AddComponent<MeshCollider>();

            rPinky1_vib = GameObject.Find(rPinky1);
            rPinky1_vib.AddComponent<MeshCollider>();
            rPinky2_vib = GameObject.Find(rPinky2);
            rPinky2_vib.AddComponent<MeshCollider>();
            rPinky3_vib = GameObject.Find(rPinky3);
            rPinky3_vib.AddComponent<MeshCollider>();

            box1 = GameObject.Find("box_0_0::link::collision__COLLISION_VISUAL__/Cube");
            box2 = GameObject.Find("box_0_0_0::link::collision__COLLISION_VISUAL__/Cube");
            box3 = GameObject.Find("box_0_0_0_0_0::link::collision__COLLISION_VISUAL__/Cube");
            box4 = GameObject.Find("box_0_0_0_0_0_0::link::collision__COLLISION_VISUAL__/Cube");

            box1reference = GameObject.Find("box_0_0").transform;
            box2reference = GameObject.Find("box_0_0_0").transform;
            box3reference = GameObject.Find("box_0_0_0_0_0").transform;
            box4reference = GameObject.Find("box_0_0_0_0_0_0").transform;

            string ScriptName = "CollisionHandler";
            System.Type MyScriptType = System.Type.GetType(ScriptName + ",Assembly-CSharp");
            box1.AddComponent(MyScriptType);
            box1.AddComponent<BoxCollider>();
            //box1.GetComponent<BoxCollider>().isTrigger = true;
            box1.AddComponent<Rigidbody>();
            box1.GetComponent<Rigidbody>().useGravity = false;
            //box1.GetComponent<Rigidbody>().isKinematic = true;

            box2.AddComponent(MyScriptType);
            box2.AddComponent<BoxCollider>();
            //box2.GetComponent<BoxCollider>().isTrigger = true;
            box2.AddComponent<Rigidbody>();
            box2.GetComponent<Rigidbody>().useGravity = false;
            //box2.GetComponent<Rigidbody>().isKinematic = true;

            box3.AddComponent(MyScriptType);
            box3.AddComponent<BoxCollider>();
            //box3.GetComponent<BoxCollider>().isTrigger = true;
            box3.AddComponent<Rigidbody>();
            box3.GetComponent<Rigidbody>().useGravity = false;
            //box3.GetComponent<Rigidbody>().isKinematic = true;

            box4.AddComponent(MyScriptType);
            box4.AddComponent<BoxCollider>();
            //box4.GetComponent<BoxCollider>().isTrigger = true;
            box4.AddComponent<Rigidbody>();
            box4.GetComponent<Rigidbody>().useGravity = false;
            //box4.GetComponent<Rigidbody>().isKinematic = true;

            initialized = true;
        }
        if (initialized)
        {
            box1.transform.SetPositionAndRotation(box1reference.position, box1reference.rotation);
            box2.transform.SetPositionAndRotation(box2reference.position, box2reference.rotation);
            box3.transform.SetPositionAndRotation(box3reference.position, box3reference.rotation);
            box4.transform.SetPositionAndRotation(box4reference.position, box4reference.rotation);
        }
	}

    public bool GetInitalized()
    {
        return initialized;
    }
}
