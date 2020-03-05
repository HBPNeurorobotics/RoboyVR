using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class PhysicsTest : MonoBehaviour {
    public string testFolder;
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
    public Text completionsDisplay;

	PHASE phase = PHASE.HAND;
	float timeUntilCompletion, phaseRunTime;
	bool run, saved;

	List<SuccessfullTask> handCompletions = new List<SuccessfullTask>();
	List<SuccessfullTask> footCompletions = new List<SuccessfullTask>();

	public CheckBound[] handBounds;
	public CheckBound[] footBounds;

	public GameObject finishHand;
	public GameObject finishFoot;

    public enum PHASE
    {
        HAND,
        FOOT
    }

    // Use this for initialization
    void Start() {

	}

    void ClearMeasuredTime()
    {

        foreach (CheckBound bound in handBounds)
        {
            bound.timeSpent = 0f;
        }
        foreach (CheckBound bound in footBounds)
        {
            bound.timeSpent = 0f;
        }

    }

    void StartTest(int latency)
    {
        ClearMeasuredTime();
        chosenLatency = latency;
        latencyHandler.latency_ms = chosenLatency;
        phase = PHASE.HAND;
        saved = false;
        StartCoroutine(StartPhaseInS(15f));
    }


	void OnEnable()
	{
		//SetActiveBounds(handBounds, false);
		//SetActiveBounds(footBounds, false);
		if (righthanded)
		{
			PrepareRightHanded(phaseHand);
			//PrepareRightHanded(phaseFoot);
		}
		participantID = Random.Range(0, 100000);
	}

	void SetActiveBounds(CheckBound[] bounds, bool active)
	{
		foreach(CheckBound testBound in bounds)
		{
			testBound.measure = active;
            testBound.contacts = new List<Collider>();
            foreach(Transform target in testBound.transform)
            {
                if (target.name.Equals("Target"))
                {
                    target.GetComponent<CheckFinish>().trigger = active;
                    break;
                }
            }
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
            StartTest(noLatency);
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
            StartTest(mediumLatency);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
		{
            StartTest(highLatency);
        }

    }

	void ResetTask()
	{
        ClearMeasuredTime();
        timeUntilCompletion = 0;
        if (phase.Equals(PHASE.HAND))
        {
            finishHand.transform.position = new Vector3(-finishHand.transform.position.x, finishHand.transform.position.y, finishHand.transform.position.z);
            finishHand.GetComponent<CheckFinish>().trigger = true;
        }
        else
        {
            //TODO pause test until player at start position
        }
	}

    IEnumerator StartPhaseInS(float timeLeft)
    {
        run = false;
        countdownDisplay.enabled = true;
        completionsDisplay.text = "" + 0;

        while (timeLeft != 0)
        {
            Debug.Log("countdown");

            countdownDisplay.text = "" + timeLeft;
            yield return new WaitForSeconds(1.0f);
            timeLeft--;
        }

        countdownDisplay.enabled = false;

        pidTestRunner.gameObject.SetActive(true);
        pidTestRunner.ResetTestRunner();
        pidTestRunner.StartManualRecord();
        run = true;
        if (phase.Equals(PHASE.HAND))
        {
            ClearMeasuredTime();

            finishFoot.GetComponent<CheckFinish>().trigger = false;
            SetActiveBounds(footBounds, false);
            phaseFoot.gameObject.SetActive(false);

            phaseHand.gameObject.SetActive(true);
            SetActiveBounds(handBounds, true);
            finishHand.GetComponent<CheckFinish>().trigger = true;
        }
        else
        {
            ClearMeasuredTime();

            finishHand.GetComponent<CheckFinish>().trigger = false;
            SetActiveBounds(handBounds, false);
            phaseHand.gameObject.SetActive(false);

            phaseFoot.gameObject.SetActive(true);
            SetActiveBounds(footBounds, true);
            finishFoot.GetComponent<CheckFinish>().trigger = true;
        }

        StartCoroutine(DisplayRemainingTime());

    }

    public void OnFinishLineReached(PHASE phase)
	{
		Dictionary<string, float> boundsTimes = new Dictionary<string, float>();
		CheckBound[] testBounds;
		List<SuccessfullTask> completions;
		if (phase.Equals(PHASE.HAND))
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
        completionsDisplay.text = "" + completions.Count();

        ResetTask();
	}


    IEnumerator DisplayRemainingTime()
    {
        float time = testDurationInS;
        remainingTimeDisplay.enabled = true;
        remainingTimeDisplay.text = "" + testDurationInS;
        while (time != 0)
        {
            remainingTimeDisplay.text = "" + time;
            yield return new WaitForSeconds(1.0f);
            time--;
        }
        remainingTimeDisplay.enabled = false;
    }

	// Update is called once per frame
	void FixedUpdate()
	{
		if (run)
		{
			phaseRunTime += Time.deltaTime;
			if (phaseRunTime > testDurationInS)
			{
                StopCoroutine("DisplayRemainingTime");
                StopCoroutine("StartPhaseInS");
				timeUntilCompletion += Time.deltaTime;
				if (phase.Equals(PHASE.HAND))
				{
                    //Start foot test
                    timeUntilCompletion = 0;
                    phaseRunTime = 0;
					phaseHand.gameObject.SetActive(false);
					phaseFoot.gameObject.SetActive(true);
					phase = PHASE.FOOT;
                    Debug.Log("hand phase done");
					StartCoroutine(StartPhaseInS(15));
				}
				else
                {
                    if (!saved)
                    {
                        Debug.Log("foot phase done");
                        saved = true;
                        run = false;
                        //End Test & Save
                        TestWrapUp();
                    }
                }
                remainingTimeDisplay.enabled = false;
            }
		}
	}

	public void TestWrapUp()
	{
        phaseRunTime = 0;
        finishFoot.GetComponent<CheckFinish>().trigger = false;
        SetActiveBounds(footBounds, false);
        phaseFoot.gameObject.SetActive(false);

        SaveResults();
        footCompletions = new List<SuccessfullTask>();
        handCompletions = new List<SuccessfullTask>();
        ClearMeasuredTime();
        timeUntilCompletion = 0;
	}

	void SaveResults()
	{
		string id = chosenLatency + "-" + participantID;
        char directorySeparator = Path.DirectorySeparatorChar;


        string dataPath = testFolder.Replace('/', directorySeparator);
		string folder = Path.Combine(dataPath, "PhysicsTest" + directorySeparator + participantID + directorySeparator + chosenLatency);

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
