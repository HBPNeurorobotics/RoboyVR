﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ROSBridgeLib.geometry_msgs;

public class GzLinkLinearVelocityPID : MonoBehaviour {

    public float p = 4f, i = 0.2f, d = 0.5f;

    private Vector3 integral = new Vector3(), position_diff_previous = new Vector3();

    private bool active = false;

    private string topic, gz_link_name;
    private Transform rig_transform, gz_target_transform, gz_link_transform;
    private float publish_rate = 0.05f;
    private float t_last_publish;

    private Quaternion rotation_ybot_unity2gazebo;
    
	void Start () {
        Matrix4x4 matrix_ybot_unity2gazebo = new Matrix4x4();
        matrix_ybot_unity2gazebo.SetRow(0, new Vector4(-1, 0, 0, 0));
        matrix_ybot_unity2gazebo.SetRow(1, new Vector4(0, 0, 1, 0));
        matrix_ybot_unity2gazebo.SetRow(2, new Vector4(0, 1, 0, 0));
        matrix_ybot_unity2gazebo.SetRow(3, new Vector4(0, 0, 0, 1));

        this.rotation_ybot_unity2gazebo = matrix_ybot_unity2gazebo.rotation;
    }
	
	void FixedUpdate ()
    {
        if (!this.active) return;

        float t = Time.time;
        if (t - this.t_last_publish > this.publish_rate)
        {
            this.t_last_publish = t;

            this.UpdateTargetFromRig();

            Vector3 position_diff = this.gz_target_transform.position - this.gz_link_transform.position;

            this.integral = this.integral + position_diff * Time.fixedDeltaTime;
            Vector3 derivative = (position_diff - this.position_diff_previous) / Time.fixedDeltaTime;
            Vector3 pidOutput = p * position_diff + i * this.integral + d * derivative;

            Vector3 velocity = GazeboSceneManager.Unity2GzVec3(pidOutput);
            Vector3Msg velocity_msg = new Vector3Msg(velocity.x, velocity.y, velocity.z);
            ROSBridgeService.Instance.websocket.Publish(topic, velocity_msg);
        }
    }

    public void Initialize(string avatar_name, string gz_link_name, Transform rig_transform, Transform gz_target_transform, Transform gz_link_transform)
    {
        this.gz_link_name = gz_link_name;
        this.topic = "/" + avatar_name + "/avatar_ybot/" + gz_link_name + "/linear_vel";

        this.rig_transform = rig_transform;
        this.gz_target_transform = gz_target_transform;
        this.gz_link_transform = gz_link_transform;

        this.active = true;
    }

    private void UpdateTargetFromRig()
    {
        this.gz_target_transform.position = this.rig_transform.position;
        this.gz_target_transform.rotation = this.rig_transform.rotation * this.rotation_ybot_unity2gazebo;
    }
}
