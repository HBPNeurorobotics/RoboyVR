using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPosition : MonoBehaviour
{

    [SerializeField]
    public GameObject tumb1;
    public GameObject tumb2;
    public GameObject tumb3;
    public GameObject tumb4;

    public GameObject index1;
    public GameObject index2;
    public GameObject index3;
    public GameObject index4;

    public GameObject middle1;
    public GameObject middle2;
    public GameObject middle3;
    public GameObject middle4;

    public GameObject ring1;
    public GameObject ring2;
    public GameObject ring3;
    public GameObject ring4;

    public GameObject pinky1;
    public GameObject pinky2;
    public GameObject pinky3;
    public GameObject pinky4;

    //public GameObject hand;

    public GameObject vtumb1;
    public GameObject vtumb2;
    public GameObject vtumb3;
    public GameObject vtumb4;

    public GameObject vindex1;
    public GameObject vindex2;
    public GameObject vindex3;
    public GameObject vindex4;

    public GameObject vmiddle1;
    public GameObject vmiddle2;
    public GameObject vmiddle3;
    public GameObject vmiddle4;

    public GameObject vring1;
    public GameObject vring2;
    public GameObject vring3;
    public GameObject vring4;

    public GameObject vpinky1;
    public GameObject vpinky2;
    public GameObject vpinky3;
    public GameObject vpinky4;

    //public GameObject vhand;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        vtumb1.transform.SetPositionAndRotation(tumb1.transform.position, tumb1.transform.rotation);
        vtumb2.transform.SetPositionAndRotation(tumb2.transform.position, tumb2.transform.rotation);
        vtumb3.transform.SetPositionAndRotation(tumb3.transform.position, tumb3.transform.rotation);
        vtumb4.transform.SetPositionAndRotation(tumb4.transform.position, tumb4.transform.rotation);

        vindex1.transform.SetPositionAndRotation(index1.transform.position, index1.transform.rotation);
        vindex2.transform.SetPositionAndRotation(index2.transform.position, index2.transform.rotation);
        vindex3.transform.SetPositionAndRotation(index3.transform.position, index3.transform.rotation);
        vindex4.transform.SetPositionAndRotation(index4.transform.position, index4.transform.rotation);

        vmiddle1.transform.SetPositionAndRotation(middle1.transform.position, middle1.transform.rotation);
        vmiddle2.transform.SetPositionAndRotation(middle2.transform.position, middle2.transform.rotation);
        vmiddle3.transform.SetPositionAndRotation(middle3.transform.position, middle3.transform.rotation);
        vmiddle4.transform.SetPositionAndRotation(middle4.transform.position, middle4.transform.rotation);

        vring1.transform.SetPositionAndRotation(ring1.transform.position, ring1.transform.rotation);
        vring2.transform.SetPositionAndRotation(ring2.transform.position, ring2.transform.rotation);
        vring3.transform.SetPositionAndRotation(ring3.transform.position, ring3.transform.rotation);
        vring4.transform.SetPositionAndRotation(ring4.transform.position, ring4.transform.rotation);

        vpinky1.transform.SetPositionAndRotation(pinky1.transform.position, pinky1.transform.rotation);
        vpinky2.transform.SetPositionAndRotation(pinky2.transform.position, pinky2.transform.rotation);
        vpinky3.transform.SetPositionAndRotation(pinky3.transform.position, pinky3.transform.rotation);
        vpinky4.transform.SetPositionAndRotation(pinky4.transform.position, pinky4.transform.rotation);

        //vhand.transform.SetPositionAndRotation(hand.transform.position, hand.transform.rotation);

    }
}
