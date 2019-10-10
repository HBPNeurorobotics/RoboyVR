using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbodimentDiscrepancy 
{
	public class DiscrepancyIndicator : MonoBehaviour
	{

		[SerializeField]
		[Tooltip("pivot transorm on a worldspace canvas close to the camera. if not set, will use attached gameObject's transform")]
		Transform pivot;

		[SerializeField]
		[Tooltip("just to monitor what it's pointing at")]
		Transform target;

		[SerializeField]
		[Tooltip("public, set this to the main camera in scene")]
		public Transform cam;

		//called every frame
		public void PointAtTarget(Transform WorldSpaceTarget)
		{
			target = WorldSpaceTarget;
			pivot.gameObject.SetActive(true);

			//adapted from: https://youtu.be/b_dDgnfedIo
			//Indicator Sprite should be pointing down at rotation.z==0
			//work in camera's local space
			Vector3 dir = cam.transform.worldToLocalMatrix * target.position;
			Vector3 rotation = new Vector3(0,0,0);
			rotation.z = Mathf.Atan2((pivot.transform.position.y - dir.y), (pivot.transform.position.x - dir.x)) * Mathf.Rad2Deg - 90;
			//canvas should be very close to camera, as angle is correct in camera's local space
			pivot.transform.localRotation = Quaternion.Euler(rotation);

			StartCoroutine(DisableAfterFrame());
		}

		IEnumerator DisableAfterFrame()
		{
			yield return new WaitForEndOfFrame();
			pivot.gameObject.SetActive(false);
		}

		// Use this for initialization
		void Start()
		{
			if (pivot == null){
				pivot = this.transform;
			}
			if (cam == null){
				Debug.LogError("cam transform not set");
			}
		}

		// Update is called once per frame
		void Update()
		{
		}
	}
}