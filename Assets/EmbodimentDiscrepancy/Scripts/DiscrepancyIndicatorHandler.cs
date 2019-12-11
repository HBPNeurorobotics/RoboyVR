using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbodimentDiscrepancy
{
	public class DiscrepancyIndicatorHandler : MonoBehaviour
	{
		enum PointsAt
		{
			trackedPos,
			simulatedPos
		}

		[SerializeField]
		[Tooltip("where the indicators should point to")]
		PointsAt pointsAt = PointsAt.simulatedPos;

		[SerializeField]
		[Tooltip("set to Prefabs/DiscrepancyIndicatorPrefab")]
		DiscrepancyIndicator discrepancyIndicatorPrefab;

		[SerializeField]
		[Tooltip("set to Prefabs/DiscrepancyIndicatorCanvasPrefab")]
		Canvas discrepancyIndicatorCanvasPrefab;

		[SerializeField]
		[Tooltip("set to main Camera (probably Camera (eye)), indicator canvas will be added as child to this")]
		GameObject mainCamera;

		[SerializeField]
		[Tooltip("only show indicators if pointed at position is visible to camera")]
		bool hideIndicatorsWhenTargetInView = true;

		[SerializeField]
		[Tooltip("how far into view target must be to hide indicator, 0 is outer edge, 0.5 is center of view")]
		[Range(0, 0.5f)]
		float inViewportOffset = 0.2f;

		Canvas discrepancyIndicatorCanvas;
		Dictionary<TrackedJoint, DiscrepancyIndicator> discrepancyIndicatorDict = new Dictionary<TrackedJoint, DiscrepancyIndicator>();


		//called every frame when discrepancy should be handled
		public void HandleIndicator(Discrepancy disc)
		{
			if (!discrepancyIndicatorDict.ContainsKey(disc.joint)){
				DiscrepancyIndicator discrepancyIndicator = Instantiate(discrepancyIndicatorPrefab);
				discrepancyIndicator.transform.SetParent(discrepancyIndicatorCanvas.transform, false);
				discrepancyIndicator.cam = mainCamera.transform;		
				discrepancyIndicatorDict[disc.joint] = discrepancyIndicator;
			}

			Transform target = null;
			if (pointsAt == PointsAt.simulatedPos){
				target = disc.simulatedPos;
			}
			else if (pointsAt == PointsAt.trackedPos){
				target = disc.trackedPos;
			}

			bool targetInView = false;
			if (hideIndicatorsWhenTargetInView)
			{
				Vector3 viewportPos = mainCamera.GetComponent<Camera>().WorldToViewportPoint(target.position);
				float upperInViewportBound = 1 - inViewportOffset;
				float lowerInViewportBound = 0 + inViewportOffset;
				if (viewportPos.x >= lowerInViewportBound && viewportPos.x <= upperInViewportBound && viewportPos.y >= lowerInViewportBound && viewportPos.y <= upperInViewportBound && viewportPos.z > 0){
					targetInView = true;
				}
			}
			if (!targetInView || !hideIndicatorsWhenTargetInView){
				discrepancyIndicatorDict[disc.joint].PointAtTarget(disc.simulatedPos);
			}
		}

		// Use this for initialization
		void Start()
		{
			discrepancyIndicatorCanvas = Instantiate(discrepancyIndicatorCanvasPrefab);
			discrepancyIndicatorCanvas.transform.SetParent(mainCamera.transform, false);

		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}