using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteriorRenderingHandler : MonoBehaviour {

	[SerializeField]
	[Tooltip("If not set, will be searched in scene")]
	GazeboSceneManager gazeboSceneManager;
	
	[SerializeField]
	[Tooltip("if gazeboSceneManager's models_parent object changes, this needs to be updated by setting ModelsParent property")]
	private Transform modelsParent;
	public Transform ModelsParent
	{
		get{
			return modelsParent;
		}

		set
		{
			modelsParent = value;
			initializedGazeboObjects = new List<GameObject>();
		}
	}

	[SerializeField]
	[Tooltip("substrings of names of objects to be ignored")]
	public List<string> ignoredModelSubstrings = new List<string>();

	[SerializeField]
	[Tooltip("material applied to render inside of objects")]
	Material interiorMaterial;

	List<GameObject> initializedGazeboObjects = new List<GameObject>();


	public void AddInteriorMaterialToGazeboObjects()
	{
		if (ModelsParent != null)
		{
			foreach (Transform child in ModelsParent)
			{
				bool ignoreChild = false;
				foreach (string substring in ignoredModelSubstrings)
				{
					if (child.name.Contains(substring))
					{
						ignoreChild = true;
					}
				}
				if (!ignoreChild)
				{
					foreach (MeshRenderer childRenderer in child.GetComponentsInChildren<MeshRenderer>())
					{
						if (!initializedGazeboObjects.Contains(childRenderer.gameObject))
						{
							initializedGazeboObjects.Add(childRenderer.gameObject);
							Material newMat = Instantiate(interiorMaterial);
							newMat.mainTexture = childRenderer.material.mainTexture;
							var materials = new List<Material>(childRenderer.materials);
							materials.Add(newMat);
							childRenderer.materials = materials.ToArray();
						}
					}
				}
			}
		}
	}

	public void Start()
	{
		if (gazeboSceneManager == null)
		{
			gazeboSceneManager = GameObject.FindObjectOfType<GazeboSceneManager>();
			if (gazeboSceneManager == null){
				Debug.LogError("no GazeboSceneManager found");
			}
		}
	}

	public void Update()
	{
		if (gazeboSceneManager.Models_parent != null)
		{
			if (ModelsParent != gazeboSceneManager.Models_parent.transform)
			{
				ModelsParent = gazeboSceneManager.Models_parent.transform;
			}
			AddInteriorMaterialToGazeboObjects();
		}
	}
}
