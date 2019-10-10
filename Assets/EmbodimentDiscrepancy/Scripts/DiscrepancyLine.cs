using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbodimentDiscrepancy
{
	[RequireComponent(typeof(LineRenderer))]
	public class DiscrepancyLine : MonoBehaviour
	{
		[SerializeField]
		LineRenderer lineRenderer;
		public Transform source, target;
		public float thickness;

		// Use this for initialization
		void Start()
		{
			lineRenderer.positionCount = 2;
			lineRenderer.enabled = false;
		}

		// Update is called once per frame
		void Update()
		{
			DrawLine();
		}

		public void DrawLine(Transform source, Transform target, float thickness)
		{
			lineRenderer.SetPosition(0, source.position);
			lineRenderer.SetPosition(1, target.position);
			lineRenderer.widthMultiplier = thickness; 
			lineRenderer.enabled = true;
		}

		public void DrawLine()
		{
			DrawLine(source, target, thickness);
		}

		public void StopDrawLine()
		{
			lineRenderer.enabled = false;
		}

	}
}