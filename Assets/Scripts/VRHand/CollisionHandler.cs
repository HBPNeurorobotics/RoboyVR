using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour {

    private bool vibrate = false;

    private string side;

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

    private string[] collisionNamesLeft;
    private string[] collisionNamesRight;
    private string[] collisionNames;

    private string activeCollision;

    // Use this for initialization
    void Start () {
        collisionNames = new string[] {lHand, lThumb1, lThumb2, lThumb3, lIndex1, lIndex2, lIndex3, lMiddle1,
                                        lMiddle2, lMiddle3, lRing1, lRing2, lRing3, lPinky1, lPinky2, lPinky3,
                                        rHand, rThumb1, rThumb2, rThumb3, rIndex1, rIndex2, rIndex3, rMiddle1,
                                        rMiddle2, rMiddle3, rRing1, rRing2, rRing3, rPinky1, rPinky2, rPinky3};
        collisionNamesLeft = new string[] {lHand, lThumb1, lThumb2, lThumb3, lIndex1, lIndex2, lIndex3, lMiddle1,
                                        lMiddle2, lMiddle3, lRing1, lRing2, lRing3, lPinky1, lPinky2, lPinky3,};
        collisionNamesRight = new string [] {rHand, rThumb1, rThumb2, rThumb3, rIndex1, rIndex2, rIndex3, rMiddle1,
                                        rMiddle2, rMiddle3, rRing1, rRing2, rRing3, rPinky1, rPinky2, rPinky3};
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collisionNames.Length; i++)
        {
            if (collision.gameObject.name == collisionNames[i])
            {
                vibrate = true;
                activeCollision = collision.gameObject.name;
                for (int j = 0; j < collisionNamesLeft.Length; j++)
                {
                    if (activeCollision == collisionNamesLeft[j])
                    {
                        side = "LeftHand";
                    }
                }
                for (int j = 0; j < collisionNamesRight.Length; j++)
                {
                    if (activeCollision == collisionNamesRight[j])
                    {
                        side = "RightHand";
                    }
                }
            }
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == activeCollision)
        {
            vibrate = false;
        }
    }
    

    public bool GetVibrate()
    {
        return vibrate;
    }

    public string GetSide()
    {
        return side;
    }
}
