using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootPhaseResetPlate : MonoBehaviour {

    public PhysicsTest test;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer >= 10 && other.gameObject.layer <= 20)
        {
            test.OnFootReset();
            gameObject.SetActive(false);
        }
    }
}
