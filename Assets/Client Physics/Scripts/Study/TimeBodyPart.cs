using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class TimeBodyPart : MonoBehaviour
    {
    public string name;
        public float time = 0f;

        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            time += Time.fixedDeltaTime;
        }
    }

