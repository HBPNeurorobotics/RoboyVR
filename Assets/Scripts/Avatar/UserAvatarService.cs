﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.gazebo_msgs;

public class UserAvatarService : Singleton<UserAvatarService>
{
    public GameObject avatar
    {
        get { return this.user_avatar; }
    }

    public GameObject avatar_rig = null;

    public List<GameObject> published_links = null;
    public bool publish_all_links = false;

    public bool scripted_publishing = false;

    private string avatar_name = null;

    private GameObject user_avatar = null;
    private GameObject avatar_clone = null;

    private bool spawning_avatar = false;
    private bool avatar_ready = false;

    private float publish_linear_velocity_frequency = 0.01f;
    private float publish_linear_velocity_tlast;
    private float publish_linear_velocity_const = 5f;
    private float publish_linear_velocity_factor = 5f;
    private float publish_linear_velocity_max = 20f;

    private Vector3 gazebo_model_pos_offset = new Vector3();

    void Awake()
    {
        GzBridgeService.Instance.AddCallbackModelInfoMsg(this.OnModelInfoMsg);
        GzBridgeService.Instance.AddCallbackOnCloseConnection(this.DespawnAvatar);
    }
    
    void Start()
    {
        if (this.avatar_rig)
        {
            SkinnedMeshRenderer rig_mesh_renderer = this.avatar_rig.GetComponentInChildren<SkinnedMeshRenderer>();
            //Debug.Log("rig center = " + rig_mesh_renderer.bounds.center);
            this.gazebo_model_pos_offset = new Vector3(0f, -rig_mesh_renderer.bounds.extents.y, 0f);
            this.gazebo_model_pos_offset.y -= 0.25f;  // center of mesh is not the center of the model ?
        }
    }
    
    void Update()
    {
        if (Input.GetKey("s"))
        {
            StartCoroutine(SpawnAvatar("user_avatar_ybot"));
        }

        if (this.scripted_publishing)
        {
            this.PublishScriptedSequence();
            return;
        }

        if (this.avatar_ready)
        {
            //this.PublishModelRotationTarget();

            this.PublishModelPose();
            //this.PublishJointSetPosition();
            this.PublishJointPIDPositionTarget();
        }
    }

    private void FixedUpdate()
    {
    }

    public void DespawnAvatar()
    {
        if (this.user_avatar == null)
        {
            return;
        }

        Debug.Log("Despawning: " + this.avatar_name);
        GzEntityDeleteMsg msg = new GzEntityDeleteMsg(this.avatar_name);

        GzBridgeService.Instance.gzbridge.Publish(GzEntityDeletePublisher.GetMessageTopic(), msg);

        this.avatar_ready = false;
    }

    public void OnModelInfoMsg(GzModelInfoMsg model_info_msg)
    {
        JSONNode json_model_info = model_info_msg.MsgJSON;
        string model_name = json_model_info["name"];
        if (model_name.Contains(this.avatar_name))
        {
            StartCoroutine(this.WaitForAvatarCreation());
        }
    }

    public void SpawnYBot()
    {
        StartCoroutine(SpawnAvatar("user_avatar_ybot"));
    }

    private IEnumerator SpawnAvatar(string avatar_model_name)
    {
        if (this.spawning_avatar)
        {
            yield break;
        }
        this.spawning_avatar = true;

        // wait until authentication token available
        yield return new WaitUntil(() => !string.IsNullOrEmpty(AuthenticationService.Instance.token));

        this.avatar_name = "user_avatar_" + AuthenticationService.Instance.token.Replace("-", "_");
        Debug.Log("SpawnAvatar - auth token: " + AuthenticationService.Instance.token);
        Debug.Log("SpawnAvatar - avatar_name: " + this.avatar_name);
        Vector3 spawn_pos = GazeboSceneManager.Unity2GzVec3(new Vector3(avatar_rig.transform.position.x, avatar_rig.transform.position.y - 1.0f, avatar_rig.transform.position.z));
        Quaternion spawn_rot = new Quaternion();

        GzFactoryMsg msg = new GzFactoryMsg(this.avatar_name, avatar_model_name, new PointMsg(spawn_pos.x, spawn_pos.y, spawn_pos.z), new QuaternionMsg(spawn_rot.x, spawn_rot.y, spawn_rot.z, spawn_rot.w));

        GzBridgeService.Instance.gzbridge.Publish(GzFactoryPublisher.GetMessageTopic(), msg);
    }

    private IEnumerator WaitForAvatarCreation()
    {
        yield return new WaitUntil(() => {
            this.user_avatar = GameObject.Find(this.avatar_name);
            return this.user_avatar != null;
            }
        );
        Debug.Log("Found avatar model: " + this.user_avatar);

        this.PublishJointPIDParams();

        this.avatar_ready = true;

        //this.avatar_clone = Object.Instantiate(this.user_avatar);
        //this.avatar_clone.transform.SetParent(this.transform);  // make sure this is not different from the parent of user_avatar (the "Gazebo Scene" object)
        //this.avatar_clone.name = "avatar_clone";
        //this.avatar_clone.AddComponent<UserAvatarVisuals>().SetOpacity(0.2f);

        //this.InitializeLinkLinearVelocityControllers();
    }

    private void TestPublishPose()
    {
        if (this.user_avatar == null)
        {
            return;
        }

        foreach(GameObject link in this.published_links)
        {
            if (link != null)
            {
                string topic = this.avatar_name + "/avatar_ybot/" + link.name + "/cmd_pose";

                Vector3 position_target = GazeboSceneManager.Unity2GzVec3(link.transform.position);
                PointMsg position_msg = new PointMsg(position_target.x, position_target.y, position_target.z);

                Quaternion rotation_target = GazeboSceneManager.Unity2GzQuaternion(link.transform.rotation);
                QuaternionMsg rotation_msg = new QuaternionMsg(rotation_target.x, rotation_target.y, rotation_target.z, rotation_target.w);
                PoseMsg pose_msg = new PoseMsg(position_msg, rotation_msg);
                ROSBridgeService.Instance.websocket.Publish(topic, pose_msg);
            }
        }
    }

    private void TestPublishAngularVelocity()
    {
        if (this.user_avatar == null)
        {
            return;
        }

        foreach (GameObject target_link in this.published_links)
        {
            if (target_link != null)
            {
                string topic = "/" + this.avatar_name + "/avatar_ybot/" + target_link.name + "/angular_vel";

                string server_link_name = this.avatar_name + "::avatar_ybot::" + target_link.name;
                GameObject gazebo_link = GameObject.Find(server_link_name);
                if (!gazebo_link)
                {
                    Debug.Log("could not find child link " + server_link_name + " of server avatar model");
                    return;
                }

                // debug target rotation object
                Transform debug_target_rotation = target_link.transform.Find("debug_target_rotation");
                if (!debug_target_rotation)
                {
                    GameObject debug_target_rotation_obj = new GameObject();
                    debug_target_rotation_obj.name = "debug_target_rotation";
                    debug_target_rotation_obj.transform.parent = target_link.transform;
                    debug_target_rotation_obj.transform.localPosition = new Vector3();
                }

                /** find rotation difference **/
                // multiplication order?
                // ybot model .dae import in unity and .sdf import in gazebo have differences in how they set up coordinate systems inside the model
                // so this becomes necessary to compare between the same model in different systems
                // ex.: "mixamorig_LeftHand" coordinate axes as inspected within unity
                // unity import of model:   thumb = +z, fingers = -x, back of hand = +y
                // gazebo import of model:  thumb = +y, fingers = +x, back of hand = +z

                // find rotation difference - METHOD 1
                /*Quaternion ybot_model_rotation_diff_unity2gazebo = Quaternion.AngleAxis(180, target_link.transform.up) * Quaternion.AngleAxis(-90, target_link.transform.right);
                Quaternion target_link_rot_in_gazebo_coords = target_link.transform.rotation * ybot_model_rotation_diff_unity2gazebo;
                Quaternion rotation_diff = Quaternion.Inverse(gazebo_link.transform.rotation) * target_link_rot_in_gazebo_coords;

                //Quaternion rotation_diff = target_link.transform.rotation * Quaternion.Inverse(server_link.transform.rotation);
                //rotation_diff = GazeboSceneManager.Unity2GzQuaternion(rotation_diff);

                //Vector3 angular_velocity = GazeboSceneManager.Unity2GzVec3(rotation_diff.eulerAngles);
                Vector3 angular_velocity = rotation_diff.eulerAngles;*/

                //Vector3 rotation = rotation_diff.eulerAngles;

                // find rotation difference - METHOD 2
                /*Vector3 rotation_diff = new Vector3(
                    Vector3.Angle(target_link.transform.right, -gazebo_link.transform.right),
                    Vector3.Angle(target_link.transform.up, gazebo_link.transform.forward),
                    Vector3.Angle(target_link.transform.forward, gazebo_link.transform.up)
                );
                rotation_diff = GazeboSceneManager.Unity2GzVec3(rotation_diff);*/

                // find rotation difference - METHOD 3
                /*Quaternion rotation_diff = Quaternion.FromToRotation(target_link.transform.forward, -gazebo_link.transform.up) *
                    Quaternion.FromToRotation(target_link.transform.up, -gazebo_link.transform.forward) * 
                    Quaternion.FromToRotation(target_link.transform.right, -gazebo_link.transform.right);
                Vector3 angular_velocity = GazeboSceneManager.Unity2GzVec3(rotation_diff.eulerAngles);*/

                // find rotation difference - METHOD 4
                Matrix4x4 matrix_ybot_unity2gazebo = new Matrix4x4();
                matrix_ybot_unity2gazebo.SetRow(0, new Vector4(-1, 0, 0, 0));
                matrix_ybot_unity2gazebo.SetRow(1, new Vector4(0, 0, 1, 0));
                matrix_ybot_unity2gazebo.SetRow(2, new Vector4(0, 1, 0, 0));
                matrix_ybot_unity2gazebo.SetRow(3, new Vector4(0, 0, 0, 1));

                Quaternion target_rot = target_link.transform.rotation * matrix_ybot_unity2gazebo.rotation;
                debug_target_rotation.transform.localRotation = matrix_ybot_unity2gazebo.rotation;  // debug the calculation of the target rotation
                Quaternion rotation_diff = Quaternion.Inverse(gazebo_link.transform.rotation) * debug_target_rotation.transform.rotation;

                Vector3 angular_velocity = rotation_diff.eulerAngles * 0.1f;
                //Vector3 angular_velocity = GazeboSceneManager.Unity2GzVec3(rotation_diff.eulerAngles);

                Debug.Log("rotation difference (" + target_link.name + ") : " + angular_velocity);

                Vector3Msg angular_velocity_msg = new Vector3Msg(angular_velocity.x, angular_velocity.y, angular_velocity.z);
                ROSBridgeService.Instance.websocket.Publish(topic, angular_velocity_msg);
            }
        }
    }

    /*private void TestPublishWrench()
    {
        if (this.user_avatar == null)
        {
            return;
        }

        foreach (GameObject link in this.published_links)
        {
            if (link != null)
            {
                string topic = "/gazebo/default/" + this.avatar_name + "/avatar_ybot/" + link.name + "/wrench";

                string server_link_name = this.avatar_name + "::avatar_ybot::" + link.name;
                GameObject server_link = GameObject.Find(server_link_name);
                if (!server_link)
                {
                    Debug.Log("could not find child link " + server_link_name + " of server avatar model");
                    return;
                }

                Vector3 position_diff = server_link.transform.position - link.transform.position;

                Vector3 force = GazeboSceneManager.Unity2GzVec3(position_diff) * 1000f;
                Vector3Msg force_msg = new Vector3Msg(force.x, force.y, force.z);

                Vector3 torque = new Vector3();  //GazeboSceneManager.Unity2GzQuaternion(link.transform.rotation);
                Vector3Msg torque_msg = new Vector3Msg(torque.x, torque.y, torque.z);
                GzWrenchMsg wrench_msg = new GzWrenchMsg(force_msg, torque_msg);
                GzBridgeService.Instance.gzbridge.Publish(topic, wrench_msg);
            }
        }
    }*/

    /*private void CommandPublishWrench()
    {
        if (this.user_avatar == null) return;

        GameObject visual_link = this.published_links[0];
        if (!visual_link) return;

        string topic = "~/" + this.avatar_name + "/avatar_ybot/" + visual_link.name + "/wrench";
        
        Vector3Msg force_msg = new Vector3Msg(1000, 0, 0);

        Vector3 torque = new Vector3();  //GazeboSceneManager.Unity2GzQuaternion(link.transform.rotation);
        Vector3Msg torque_msg = new Vector3Msg(torque.x, torque.y, torque.z);
        GzWrenchMsg wrench_msg = new GzWrenchMsg(force_msg, torque_msg);
        Debug.Log("CommandPublishWrench() - " + wrench_msg.ToYAMLString());
        GzBridgeService.Instance.gzbridge.Publish(topic, wrench_msg);
    }*/

    private void PublishScriptedSequence()
    {
        string topic = "/user_avatar_ybot_0/avatar_ybot/mixamorig_Spine2/cmd_pose";

        Vector3 position_target = GazeboSceneManager.Unity2GzVec3(new Vector3(Mathf.Sin(Time.time), 0.5f, Mathf.Cos(Time.time)));
        PointMsg position_msg = new PointMsg(position_target.x, position_target.y, position_target.z);

        Quaternion rotation_target = GazeboSceneManager.Unity2GzQuaternion(Quaternion.Euler(0, (Time.time) % 360, 0));
        QuaternionMsg rotation_msg = new QuaternionMsg(rotation_target.x, rotation_target.y, rotation_target.z, rotation_target.w);
        PoseMsg pose = new PoseMsg(position_msg, rotation_msg);
        ROSBridgeService.Instance.websocket.Publish(topic, pose);
    }

    private void PublishModelRotationTarget()
    {
        if (this.user_avatar == null)
        {
            return;
        }

        string topic = this.avatar_name + "/cmd_rot";
        Quaternion rotation_target = GazeboSceneManager.Unity2GzQuaternion(this.avatar_rig.transform.rotation);
        QuaternionMsg rotation_msg = new QuaternionMsg(rotation_target.x, rotation_target.y, rotation_target.z, rotation_target.w);
        ROSBridgeService.Instance.websocket.Publish(topic, rotation_msg);
    }

    private void InitializeLinkLinearVelocityControllers()
    {
        if (!this.user_avatar || !this.avatar_clone) return;

        Transform target_links = this.avatar_clone.transform.Find("links");
        Transform gazebo_links = this.user_avatar.transform.Find("links");
        if (!target_links || !gazebo_links) return;

        List<GameObject> links_to_publish;
        if (this.publish_all_links)
        {
            links_to_publish = new List<GameObject>();
            foreach (Transform rig_transform in this.avatar_rig.transform.Find("mixamorig_Hips").GetComponentsInChildren<Transform>())
            {
                if (rig_transform != null)
                {
                    links_to_publish.Add(rig_transform.gameObject);
                }
            }
        }
        else
        {
            links_to_publish = this.published_links;
        }

        foreach (GameObject rig_link in links_to_publish)
        {
            if (rig_link != null)
            {
                string link_name = rig_link.name;
                string link_full_name = this.avatar_name + "::avatar_ybot::" + link_name;
                GzLinkLinearVelocityPID controller = rig_link.AddComponent<GzLinkLinearVelocityPID>();
                controller.Initialize(this.avatar_name, link_name, rig_link.transform, target_links.Find(link_full_name), gazebo_links.Find(link_full_name));
            }
        }
    }

    private void PublishJointSetPosition()
    {
        Transform joints_parent = this.transform.Find("avatar_rig").Find("mixamorig_Hips");
        Transform[] children = joints_parent.GetComponentsInChildren<Transform>();
        foreach(Transform child in children)
        {
            if (child == joints_parent) continue;

            //Quaternion rot_diff = Quaternion.FromToRotation(child.parent.forward, child.forward);
            Quaternion rot_diff = Quaternion.Inverse(child.parent.rotation) * child.rotation;

            Vector3 euler_angles = rot_diff.eulerAngles;
            euler_angles.x = euler_angles.x % 360;
            euler_angles.y = euler_angles.y % 360;
            euler_angles.z = euler_angles.z % 360;

            euler_angles.x = euler_angles.x > 180 ? euler_angles.x - 360 : euler_angles.x;
            euler_angles.y = euler_angles.y > 180 ? euler_angles.y - 360 : euler_angles.y;
            euler_angles.z = euler_angles.z > 180 ? euler_angles.z - 360 : euler_angles.z;

            //Vector3 euler_angles_rad = euler_angles * Mathf.Deg2Rad;

            string topic = "/" + this.avatar_name + "/avatar_ybot/" + child.name + "/set_position";
            /* 
             * in case the naming seems confusing, the ...Arm joints are actually placed at the shoulder and the ...Shoulder joints are closer to the spine for the ybot type of rigged model
             * the name of the joint indicates the child limb that is attached to it 
             * => ...Arm is the shoulder joint where the upper arm is attached, ...ForeArm is the elbow joint where the forearm is attached, etc.
             */
            if (child.name.Contains("LeftArm") || child.name.Contains("RightArm"))
            {
                /* 
                 * unfortunately, gazebo doesn't play well with joints having multiple DoFs / rotation axes (i.e. joint type ball, revolute2, universal)
                 * revolute2, universal let you set positions but seem to run into gimbal-lock-alike problems for local rotation axes of the joints
                 * specifically defining the two axes of rotation to be local y & z seems impossible 
                 * we have to split the natural shoulder joint (ball joint) into three individual revolute joints, each covering one axis of rotation for the shoulder 
                 */

                //Debug.Log(child.name + " : " + euler_angles);
                string topic_x_axis = "/" + this.avatar_name + "/avatar_ybot/" + child.name + "_x/set_position";
                string topic_y_axis = "/" + this.avatar_name + "/avatar_ybot/" + child.name + "_y/set_position";
                string topic_z_axis = "/" + this.avatar_name + "/avatar_ybot/" + child.name + "_z/set_position";

                // TEST
                // TEST end

                euler_angles = euler_angles * Mathf.Deg2Rad;
                ROSBridgeService.Instance.websocket.Publish(topic_x_axis, new Vector3Msg(euler_angles.x, 0, 0));
                ROSBridgeService.Instance.websocket.Publish(topic_y_axis, new Vector3Msg(-euler_angles.z, 0, 0));
                ROSBridgeService.Instance.websocket.Publish(topic_z_axis, new Vector3Msg(euler_angles.y, 0, 0));
            }
            else if (child.name.Contains("LeftForeArm") || child.name.Contains("RightForeArm"))
            {
                euler_angles = new Vector3(euler_angles.y, euler_angles.x, euler_angles.z);
                euler_angles = euler_angles * Mathf.Deg2Rad;
                ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(euler_angles.x, euler_angles.y, euler_angles.z));
            }
            else if (child.name.Contains("UpLeg") || child.name.Contains("Leg") || child.name.Contains("Foot"))
            {
                euler_angles = euler_angles * Mathf.Deg2Rad;
                ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(euler_angles.x, euler_angles.y, euler_angles.z));
            }
        }
    }

    private void PublishJointPIDParams()
    {
        Transform joints_parent = this.transform.Find("avatar_rig").Find("mixamorig_Hips");
        Transform[] children = joints_parent.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child == joints_parent) continue;

            string topic = "/" + this.avatar_name + "/avatar_ybot/" + child.name + "/set_pid_params";
            
            // default is (100f ,50f ,10f)
            ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(2000f, 2000f, 200f));
        }
    }

    private void PublishJointPIDPositionTarget()
    {
        Transform joints_parent = this.transform.Find("avatar_rig").Find("mixamorig_Hips");
        Transform[] children = joints_parent.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child == joints_parent) continue;

            //Quaternion rot_diff = Quaternion.FromToRotation(child.parent.forward, child.forward);
            Quaternion rot_diff = Quaternion.Inverse(child.parent.rotation) * child.rotation;

            Vector3 euler_angles = rot_diff.eulerAngles;
            euler_angles.x = euler_angles.x % 360;
            euler_angles.y = euler_angles.y % 360;
            euler_angles.z = euler_angles.z % 360;

            euler_angles.x = euler_angles.x > 180 ? euler_angles.x - 360 : euler_angles.x;
            euler_angles.y = euler_angles.y > 180 ? euler_angles.y - 360 : euler_angles.y;
            euler_angles.z = euler_angles.z > 180 ? euler_angles.z - 360 : euler_angles.z;

            //Vector3 euler_angles_rad = euler_angles * Mathf.Deg2Rad;

            string topic = "/" + this.avatar_name + "/avatar_ybot/" + child.name + "/set_pid_position_target";
            /* 
             * in case the naming seems confusing, the ...Arm joints are actually placed at the shoulder and the ...Shoulder joints are closer to the spine for the ybot type of rigged model
             * the name of the joint indicates the child limb that is attached to it 
             * => ...Arm is the shoulder joint where the upper arm is attached, ...ForeArm is the elbow joint where the forearm is attached, etc.
             */
            if (child.name.Contains("LeftArm") || child.name.Contains("RightArm"))
            {
                /* 
                 * unfortunately, gazebo doesn't play well with joints having multiple DoFs / rotation axes (i.e. joint type ball, revolute2, universal)
                 * revolute2, universal let you set positions but seem to run into gimbal-lock-alike problems for local rotation axes of the joints
                 * specifically defining the two axes of rotation to be local y & z seems impossible 
                 * we have to split the natural shoulder joint (ball joint) into three individual revolute joints, each covering one axis of rotation for the shoulder 
                 */

                //Debug.Log(child.name + " : " + euler_angles);
                string topic_x_axis = "/" + this.avatar_name + "/avatar_ybot/" + child.name + "_x/set_pid_position_target";
                string topic_y_axis = "/" + this.avatar_name + "/avatar_ybot/" + child.name + "_y/set_pid_position_target";
                string topic_z_axis = "/" + this.avatar_name + "/avatar_ybot/" + child.name + "_z/set_pid_position_target";

                // TEST
                // TEST end

                euler_angles = euler_angles * Mathf.Deg2Rad;
                ROSBridgeService.Instance.websocket.Publish(topic_x_axis, new Vector3Msg(euler_angles.x, 0, 0));
                ROSBridgeService.Instance.websocket.Publish(topic_y_axis, new Vector3Msg(-euler_angles.z, 0, 0));
                ROSBridgeService.Instance.websocket.Publish(topic_z_axis, new Vector3Msg(euler_angles.y, 0, 0));
            }
            else if (child.name.Contains("LeftForeArm") || child.name.Contains("RightForeArm"))
            {
                euler_angles = new Vector3(euler_angles.y, euler_angles.x, euler_angles.z);
                euler_angles = euler_angles * Mathf.Deg2Rad;
                ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(euler_angles.x, euler_angles.y, euler_angles.z));
            }
            else if (child.name.Contains("UpLeg") || child.name.Contains("Leg") || child.name.Contains("Foot") || child.name.Contains("Shoulder"))
            {
                euler_angles = euler_angles * Mathf.Deg2Rad;
                ROSBridgeService.Instance.websocket.Publish(topic, new Vector3Msg(euler_angles.x, euler_angles.y, euler_angles.z));
            }
        }
    }

    private void PublishModelPose()
    {
        string topic = "/gazebo/set_model_state";
        Vector3 model_position = GazeboSceneManager.Unity2GzVec3(this.avatar_rig.transform.position + this.avatar_rig.transform.rotation * this.gazebo_model_pos_offset);
        Quaternion model_rotation = GazeboSceneManager.Unity2GzQuaternion(this.avatar_rig.transform.rotation);
        ModelStateMsg model_state_msg = new ModelStateMsg(
            this.avatar_name,
            new PoseMsg(
                new PointMsg(model_position.x, model_position.y, model_position.z),
                new QuaternionMsg(model_rotation.x, model_rotation.y, model_rotation.z, model_rotation.w)
            ),
            new Vector3Msg(1f, 1f, 1f),
            new TwistMsg()
        );
        ROSBridgeService.Instance.websocket.Publish(topic, model_state_msg);
    }
}
