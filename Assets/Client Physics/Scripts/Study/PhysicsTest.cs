using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

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
	public Transform phaseHand;
	public Transform phaseFoot;

    public Text countdownDisplay;
    public Text remainingTimeDisplay;

	CheckFinish.PHASE phase = CheckFinish.PHASE.HAND;
	float timeUntilCompletion, phaseRunTime, countdownTime;
	bool run, saved;

	List<SuccessfullTask> handCompletions = new List<SuccessfullTask>();
	List<SuccessfullTask> footCompletions = new List<SuccessfullTask>();

	public CheckBound[] handBounds;
	public CheckBound[] footBounds;

	public GameObject finishHand;

	// Use this for initialization
	void Start() {

	}

	void OnEnable()
	{
		SetActiveBounds(handBounds, false);
		if (righthanded)
		{
			PrepareRightHanded(phaseHand);
			PrepareRightHanded(phaseFoot);
		}
		participantID = Random.Range(0, 100000);
		StartCoroutine(CountDown());
	}

	void SetActiveBounds(CheckBound[] bounds, bool active)
	{
		foreach(CheckBound testBound in bounds)
		{
			testBound.measure = active;
		}
	}

	void PrepareRightHanded(Transform parent)
	{
		foreach (Transform child in parent)
		{
			child.position = new Vector3(-child.position.x, child.position.y, child.position.z);
		}
	}

	void Update()
	{
		
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

	void ResetTask()
	{
		Debug.Log(handCompletions.Count);
		timeUntilCompletion = 0;
		finishHand.transform.position = new Vector3(-finishHand.transform.position.x, finishHand.transform.position.y, finishHand.transform.position.z);
		finishHand.GetComponent<CheckFinish>().trigger = true;
	}

	IEnumerator CountDown()
	{
		run = false;
		yield return new WaitForSeconds(10);
		pidTestRunner.gameObject.SetActive(true);
		pidTestRunner.ResetTestRunner();
		pidTestRunner.StartManualRecord();
		run = true;
		SetActiveBounds(phase.Equals(CheckFinish.PHASE.HAND) ? handBounds : footBounds, true);
		finishHand.GetComponent<CheckFinish>().trigger = true;
		Debug.Log("Begin");
	}

    IEnumerator CountDownVisual()
    {
        countdownDisplay.text = "" + countdownTime;
        countdownTime--;

        yield return new WaitForSeconds(1);
    }

    public void OnFinishLineReached(CheckFinish.PHASE phase)
	{
		Dictionary<string, float> boundsTimes = new Dictionary<string, float>();
		CheckBound[] testBounds;
		List<SuccessfullTask> completions;
		if (phase.Equals(CheckFinish.PHASE.HAND))
		{
			testBounds = handBounds;
			completions = handCompletions;
		}
		else
		{
			testBounds = footBounds;
			completions = footCompletions;
		}
		foreach(CheckBound bound in testBounds)
		{
			boundsTimes.Add(bound.name, bound.timeSpent);
		}

		SuccessfullTask task = new SuccessfullTask(boundsTimes, timeUntilCompletion);
		completions.Add(task);

		ResetTask();
	}

	void ResetTest()
	{

	}

	// Update is called once per frame
	void FixedUpdate()
	{

		if (run)
		{
			phaseRunTime += Time.deltaTime;
			if (phaseRunTime > testDurationInS)
			{
				timeUntilCompletion += Time.deltaTime;
				if (phase.Equals(CheckFinish.PHASE.HAND))
				{
					//Start foot test
					phaseHand.gameObject.SetActive(false);
					phaseFoot.gameObject.SetActive(true);
					phase = CheckFinish.PHASE.FOOT;
					StartCoroutine(CountDown());
				}
				else
				{
					if (!saved)
					{
						//End Test & Save
						//SaveResults();
						ResetTest();
						saved = true;
					}
				}
			}
		}
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

		File.WriteAllText(Path.Combine(folder, "physics_test" + "_" + chosenLatency + ".json"), ToJson().ToString());

		pidTestRunner.StopManualRecord();
		pidTestRunner.SaveTestData(true, folder);
	}
	JObject ToJson()
	{

		JObject json = new JObject();

		json["ID"] = participantID;
		json["latency"] = chosenLatency;
		json["timeUntilCompletion"] = timeUntilCompletion;

		if(handCompletions.Count > 0)
		{
			Dictionary<string, float> averageTimeInSections = GetAverageTimeInSections(handCompletions);
			Dictionary<string, float>.KeyCollection keys = averageTimeInSections.Keys;
			foreach (string section in keys)
			{
				json[section] = averageTimeInSections[section];
			}
		}

		if (footCompletions.Count > 0)
		{
			Dictionary<string, float> averageTimeInSections = GetAverageTimeInSections(footCompletions);
			Dictionary<string, float>.KeyCollection keys = averageTimeInSections.Keys;
			foreach (string section in keys)
			{
				json[section] = averageTimeInSections[section];
			}
		}

		for (int i = 0; i < handCompletions.Count; i++)
		{
			json["hand_" + i] = handCompletions[i].ToJson();
		}

		for (int i = 0; i < footCompletions.Count; i++)
		{
			json["foot_" + i] = footCompletions[i].ToJson();
		}

		/*
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
		*/
		return json;
	}
	class SuccessfullTask
	{
		float completionTime;
		Dictionary<string, float> bounds;
		public SuccessfullTask(Dictionary<string, float> bounds, float completionTime)
		{

			this.bounds = bounds;
			this.completionTime = completionTime;
		}

		public JObject ToJson()
		{
			JObject json = new JObject();
			json["timeUntilCompletion"] = completionTime;

			foreach (string checkBound in bounds.Keys)
			{
				json[checkBound] = bounds[checkBound];
			}

			return json;
		}

		public Dictionary<string, float> GetBounds()
		{
			return bounds;
		}


    }
    Dictionary<string, float> GetAverageTimeInSections(List<SuccessfullTask> tasks)
	{
		Dictionary<string, float> averageTimeInSections = new Dictionary<string, float>();
		List<string> keys = averageTimeInSections.Keys.ToList<string>();
		foreach(string sectionName in tasks[0].GetBounds().Keys)
		{
			keys.Add(sectionName);
		}
		
		foreach (string section in keys) {
			float totalTimeInSection = 0;
			foreach (SuccessfullTask success in tasks)
			{
				totalTimeInSection += success.GetBounds()[section];
			}
			averageTimeInSections.Add(section, totalTimeInSection / tasks.Count);
		}

		return averageTimeInSections;
	}
}
