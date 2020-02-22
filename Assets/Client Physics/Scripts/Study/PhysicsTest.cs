using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;

public class PhysicsTest : MonoBehaviour {
	public bool righthanded = true;
	public int participantID = 0;
	public LatencyHandler latencyHandler;
	public PIDTuning.TestRunner pidTestRunner;
	public float testDurationInS;
	public int noLatency = 0; 
	public int mediumLatency = 35; 
	public int highLatency = 120;
	public int chosenLatency = 0;
	public Pickup pickupObj;
	public Transform phaseOne;
	public Transform phaseTwo;
	int grabTries = 1;
	float timeUntilGrabbed, timePhaseOne, timeUntilFinishLine, timePhaseTwo;
	bool countTimeUntilGrabbed = true;
	bool countTimeUntilFinishLine = false;
	bool savedPhaseOneTime, savedPhaseTwoTime;
	

	// Use this for initialization
	void Start () {
		if (righthanded)
		{
			pickupObj.transform.position = new Vector3(-pickupObj.transform.position.x, pickupObj.transform.position.y, pickupObj.transform.position.z);
			PrepareRightHanded(phaseOne);
			PrepareRightHanded(phaseTwo);
		}
		participantID = Random.Range(0, 100000);
		pidTestRunner.ResetTestRunner();
		pidTestRunner.StartManualRecord();
	}

	void PrepareRightHanded(Transform parent)
	{
		foreach(Transform child in parent)
		{
			child.position = new Vector3(-child.position.x, child.position.y, child.position.z);
		}
	}
	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			SaveResults();
			/*
			string dataPath = Application.dataPath.Replace('/', Path.DirectorySeparatorChar);
			string folder = Path.Combine(dataPath, "PhysicsTest" + Path.DirectorySeparatorChar + participantID + Path.DirectorySeparatorChar + chosenLatency);

			pidTestRunner.StopManualRecord();
			pidTestRunner.SaveTestData(true, folder);
			*/
			Debug.Log("Saved");
		}

		if (Input.GetKeyDown(KeyCode.A))
		{
			timeUntilGrabbed = 0;
			pidTestRunner.ResetTestRunner();
			pidTestRunner.CurrentTestLabel = "" + latencyHandler.latency_ms;
			pidTestRunner.StartManualRecord();
			countTimeUntilGrabbed = true;
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			chosenLatency = noLatency;
			latencyHandler.latency_ms = chosenLatency;
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			chosenLatency = mediumLatency;
			latencyHandler.latency_ms = chosenLatency;
		}

		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			chosenLatency = highLatency;
			latencyHandler.latency_ms = chosenLatency;
		}
	}
	// Update is called once per frame
	void FixedUpdate () {

		if (countTimeUntilGrabbed)
		{
			timeUntilGrabbed += Time.deltaTime;
		}
		else
		{
			if (!savedPhaseOneTime)
			{
				Debug.Log("Item grabbed in " + timeUntilGrabbed + " seconds" + " with " + grabTries + " tries");
				savedPhaseOneTime = true;
			}
		}
		if (countTimeUntilFinishLine)
		{
			timeUntilFinishLine += Time.deltaTime;
		}
		else
		{
			if (!countTimeUntilGrabbed && !savedPhaseTwoTime)
			{
				Debug.Log("Finished Phase 2 in " + timeUntilFinishLine + " seconds");
				savedPhaseTwoTime = true;
				TestWrapUp();
			}
		}
	}

	public void StopCountTimeUntilGrabbed(bool hasGrabbed)
	{
		countTimeUntilGrabbed = !hasGrabbed;
	}

	public void StopCountTimeUntilFinished(bool hasFinished)
	{
		countTimeUntilFinishLine = !hasFinished;
	}

	public void IncrementTries()
	{
		grabTries++;
	}

	public void TestWrapUp()
	{
		SaveResults();
	}

	void SaveResults()
	{
		string id = chosenLatency + "-" + participantID;
		string dataPath = Application.dataPath.Replace('/', Path.DirectorySeparatorChar);
		string folder = Path.Combine(dataPath, "PhysicsTest" + Path.DirectorySeparatorChar + participantID + Path.DirectorySeparatorChar + chosenLatency);

		Directory.CreateDirectory(folder);

		File.WriteAllText(Path.Combine(folder, "physics_test.json"), ToJson().ToString());

		pidTestRunner.StopManualRecord();
		pidTestRunner.SaveTestData(true, folder);
	}

	JObject ToJson()
	{

		JObject json = new JObject();

		json["ID"] = participantID;
		json["latency"] = chosenLatency;
		json["grabTries"] = grabTries;
		json["timeUntilGrabbed"] = timeUntilGrabbed;
		json["timeUntilFinishLine"] = timeUntilFinishLine;


		json["timeA"] = pickupObj.timeA1 + pickupObj.timeA2;
		json["timeA1"] = pickupObj.timeA1;
		json["timeA2"] = pickupObj.timeA2;

		json["timeB"] = pickupObj.timeB1 + pickupObj.timeB2;
		json["timeB1"] = pickupObj.timeB1;
		json["timeB2"] = pickupObj.timeB2;

		json["timeC"] = pickupObj.timeC1 + pickupObj.timeC2;
		json["timeC1"] = pickupObj.timeC1;
		json["timeC2"] = pickupObj.timeC2;

		json["timeD"] = pickupObj.timeD1 + pickupObj.timeD2;
		json["timeD1"] = pickupObj.timeD1;
		json["timeD2"] = pickupObj.timeD2;
		return json;
	}
}
