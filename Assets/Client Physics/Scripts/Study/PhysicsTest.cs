using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTest : MonoBehaviour {
	public LatencyHandler latencyHandler;
	public PIDTuning.TestRunner pidTestRunner;

	float startTime;
	int grabTries = 1;
	float timeUntilGrabbed;
	bool countTimeUntilGrabbed = true;
	bool savedPhaseOneTime;
	float timePhaseOne;
	

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (Input.GetKeyDown(KeyCode.S))
		{
			pidTestRunner.StopManualRecord();
			pidTestRunner.SaveTestData();
		}

		if (Input.GetKeyDown(KeyCode.A))
		{
			startTime = Time.time;
			timeUntilGrabbed = 0;
			pidTestRunner.ResetTestRunner();
			pidTestRunner.CurrentTestLabel = ""+latencyHandler.latency_ms;
			pidTestRunner.StartManualRecord();
			countTimeUntilGrabbed = true;
		}
		if (countTimeUntilGrabbed)
		{
			timeUntilGrabbed += Time.deltaTime;
		}
		else
		{
			if (!savedPhaseOneTime)
			{
				timePhaseOne = timeUntilGrabbed - startTime;
				Debug.Log("Item grabbed in " + timePhaseOne + " seconds" + " with " + grabTries + " tries");
				savedPhaseOneTime = true;
			}
		}
	}

	public void SetCountTimeUntilGrabbed(bool hasGrabbed)
	{
		countTimeUntilGrabbed = !hasGrabbed;
	}

	public void IncrementTries()
	{
		grabTries++;
	}
}
