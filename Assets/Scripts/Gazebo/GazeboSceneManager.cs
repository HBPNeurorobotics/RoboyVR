using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using ROSBridgeLib.geometry_msgs;

public class GazeboSceneManager : MonoBehaviour {

    public enum GZ_LIGHT_TYPE
    {
        POINT = 1,
        SPOT = 2,
        DIRECTIONAL = 3,
        UNKNOWN = 4
    }

    private bool avatarInScene = false;
    private string avatarId = "";

    // scene access
    private string scene_name_ = null;
    private GameObject models_parent = null;
    private GameObject lights_parent = null;
    private GameObject joints_parent = null;

    // loading models
    public string NRPModelsSubpath = "Models/nrp_models";
    private Dictionary<string, string> nrp_models_subpaths = new Dictionary<string, string>();

    public Material CollisionMaterial = null;

	// Use this for initialization
	void Start ()
    {
        Camera.main.transform.position = new Vector3(6, 3, 6);
        Camera.main.transform.LookAt(new Vector3());

        this.InitModelSubpaths();
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(avatarId == "")
        {
            avatarId = GzBridgeManager.Instance.avatarId;
        }	
	}

    #region ON_MESSAGE_FUNCTIONS

    public bool OnSceneMsg(JSONNode json_scene)
    {
        // clear scene before building new one
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform)
            children.Add(child.gameObject);
        children.ForEach(child => Destroy(child));

        // name
        if (json_scene["name"] != null)
            scene_name_ = json_scene["name"];

        // ambient color
        JSONClass ambient = json_scene["ambient"].AsObject;
        if (ambient != null)
        {
            Color color_ambient = new Color(ambient["r"].AsFloat, ambient["g"].AsFloat, ambient["b"].AsFloat, ambient["a"].AsFloat);
            RenderSettings.ambientLight = color_ambient;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        }

        // shadow settings

        // background color
        JSONClass background = json_scene["background"].AsObject;
        if (ambient != null)
        {
            Color color_background = new Color(background["r"].AsFloat, background["g"].AsFloat, background["b"].AsFloat, background["a"].AsFloat);
            Camera.main.backgroundColor = color_background;
        }

        // lights
        lights_parent = new GameObject("lights");
        lights_parent.transform.SetParent(this.gameObject.transform, false);

        JSONArray json_lights = json_scene["light"].AsArray;
        foreach (JSONNode json_light in json_lights)
        {
            this.CreateLightFromJSON(json_light, lights_parent.transform);
        }

        // models
        models_parent = new GameObject("models");
        models_parent.transform.SetParent(this.gameObject.transform, false);

        JSONArray json_models = json_scene["model"].AsArray;
        foreach (JSONNode json_model in json_models)
        {
            string modelName = json_model["name"];
            string avatar = "user_avatar_" + avatarId;
            if (modelName == avatar)
            {
                avatarInScene = true;
            }
            this.SetModelFromJSON(json_model, models_parent.transform);
        }

        // joints
        joints_parent = new GameObject("joints");
        joints_parent.transform.SetParent(this.gameObject.transform, false);

        JSONArray json_joints = json_scene["joint"].AsArray;
        foreach (JSONNode json_joint in json_joints)
        {
            this.SetJointFromJSON(json_joint, joints_parent.transform);
        }

        if (!avatarInScene)
        {
            /**
             * Instantiate the avatar with the following message if it doesn't already exists in the scene
             * "{\"op\": \"publish\", \"topic\": \"" + "~/factory" + "\", \"msg\": {\"name\":\"Test\",\"type\":\"user_avatar_basic\",\"createEntity\":1,\"position\":{\"x\":5,\"y\":0,\"z\":0},\"orientation\":{\"w\":1,\"x\":0,\"y\":0,\"z\":0}}" + "}";
             */
            GzBridgeManager.Instance.m_GzBridge.Publish(GzFactoryPublisher.GetMessageTopic(), new GzFactoryMsg("user_avatar_" + avatarId, "user_avatar_basic", new PointMsg(5, 0, 0), new QuaternionMsg(0, 0, 0, 1)));
            avatarInScene = true;
        }

        return true;
    }

    public bool OnPoseInfoMsg(JSONNode json_pose_info)
    {
        string name = json_pose_info["name"];
        GameObject gameobject = GameObject.Find(name);

        if (gameobject != null)
        {
            this.SetPoseFromJSON(json_pose_info, gameobject);

            return true;
        }

        return false;
    }

    public bool OnModelInfoMsg(JSONNode json_model_info)
    {
        //Debug.Log("model info: " + json_model_info.ToString());
        this.SetModelFromJSON(json_model_info, this.models_parent.transform);

        return true;
    }

    public bool OnMaterialMsg(JSONNode json_material)
    {
        //Debug.Log("material msg: " + json_material.ToString());

        return true;
    }

    #endregion //ON_MESSAGE_FUNCTIONS

    #region CREATE_SCENE_ELEMENTS

    private void CreateLightFromJSON(JSONNode json_light, Transform parent_transform)
    {
        string light_name = json_light["name"];
        GameObject light_gameobject = new GameObject(light_name);
        light_gameobject.transform.SetParent(parent_transform, false);
        Light light_component = light_gameobject.AddComponent<Light>();

        // pose
        JSONNode json_pose = json_light["pose"];
        if (json_pose != null)
        {
            this.SetPoseFromJSON(json_pose, light_gameobject);
        }

        // color
        JSONNode json_diffuse = json_pose["diffuse"];
        if (json_diffuse != null)
        {
            Color color_diffuse = new Color(json_diffuse["r"].AsFloat, json_diffuse["g"].AsFloat, json_diffuse["b"].AsFloat, json_diffuse["a"].AsFloat);
            light_component.color = color_diffuse;
        }
        //TODO: what to do with specular part of gazebo light?
        
        // light type
        JSONNode light_type = json_light["type"];
        if (light_type != null)
        {
            if (light_type.AsInt == (int)GZ_LIGHT_TYPE.POINT)
            {
                light_component.type = LightType.Point;
                light_component.range = json_light["range"].AsFloat;
            }
            else if (light_type.AsInt == (int)GZ_LIGHT_TYPE.SPOT)
            {
                light_component.type = LightType.Spot;
                light_component.range = json_light["range"].AsFloat;
                light_component.spotAngle = Mathf.Rad2Deg * json_light["spot_outer_angle"].AsFloat;

                JSONNode json_direction = json_light["direction"];
                Vector3 direction = new Vector3(json_direction["x"].AsFloat, json_direction["y"].AsFloat, json_direction["z"].AsFloat);
                light_gameobject.transform.LookAt(light_gameobject.transform.position + Gz2UnityVec3(direction));
            }
            else if (light_type.AsInt == (int)GZ_LIGHT_TYPE.DIRECTIONAL)
            {
                light_component.type = LightType.Directional;

                JSONNode json_direction = json_light["direction"];
                Vector3 direction = new Vector3(json_direction["x"].AsFloat, json_direction["y"].AsFloat, json_direction["z"].AsFloat);
                light_gameobject.transform.LookAt(light_gameobject.transform.position + Gz2UnityVec3(direction));
            }
        }

        // intensity (from gazebo attenuation factors)
        // equation taken from https://docs.blender.org/manual/en/dev/render/blender_render/lighting/lights/light_attenuation.html
        if (json_light["attenuation_linear"] != null && json_light["attenuation_quadratic"] != null)
        {
            float E = 1.0f;
            float D = 1.0f;
            float r = 1.0f;
            float L = json_light["attenuation_linear"].AsFloat;
            float Q = json_light["attenuation_quadratic"].AsFloat;

            float intensity = E * (D / (D + L * r)) * (Mathf.Pow(D, 2.0f) / (Mathf.Pow(D, 2.0f) + Q * Mathf.Pow(r, 2.0f)));
            light_component.intensity = intensity;
        }

        // shadows
        bool cast_shadows = json_light["cast_shadows"].AsBool;
        if (cast_shadows)
        {
            light_component.shadows = LightShadows.Soft;
        }
        else
        {
            light_component.shadows = LightShadows.None;
        }
    }

    private void CreateMeshFromJSON(JSONNode json_mesh, Transform parent_transform, JSONNode json_model_scale)
    {
        string json_mesh_uri = json_mesh["filename"];
        string mesh_uri_type = json_mesh_uri.Substring(0, json_mesh_uri.IndexOf("://"));
        // need "file" or "model" to load
        if (mesh_uri_type != "file" && mesh_uri_type != "model")
            return;

        string json_uri_path = json_mesh_uri.Substring(json_mesh_uri.IndexOf("://") + 3);
        string model_name = json_uri_path.Split('/')[0];

        try
        {
            string model_subpath = this.nrp_models_subpaths[model_name] + json_uri_path.Substring(model_name.Length);
            string mesh_uri = "Assets/" + this.NRPModelsSubpath + "/" + model_subpath;

            // import Mesh
            GameObject mesh_prefab = null;
            mesh_prefab = (GameObject)AssetDatabase.LoadAssetAtPath(mesh_uri, typeof(UnityEngine.Object));
            if (mesh_prefab == null)
            {
                Debug.LogWarning("Could not import model! (" + mesh_uri + ")");
            }

            if (mesh_prefab != null)
            {
                GameObject mesh_gameobject = Instantiate(mesh_prefab);
                mesh_gameobject.transform.SetParent(parent_transform, false);
                // unity model import is turned 180 degrees around local Y?
                mesh_gameobject.transform.localRotation = Quaternion.Euler(mesh_gameobject.transform.localRotation.eulerAngles.x, 180.0f, mesh_gameobject.transform.localRotation.eulerAngles.z);

                // adjust materials
                List<Renderer> mesh_renderers = new List<Renderer>();
                //mesh_renderers.Add(mesh_gameobject.GetComponent<Renderer>());
                foreach (Renderer renderer in mesh_gameobject.GetComponentsInChildren<MeshRenderer>())
                {
                    mesh_renderers.Add(renderer);
                }
                foreach (MeshRenderer mesh_renderer in mesh_renderers)
                {
                    //if (mesh_renderer.materials != null)
                    //{
                    //    for (int i = 0; i < mesh_renderer.materials.Length; i = i + 1)
                    //    {
                    //        Material material = mesh_renderer.materials[i];
                    //        if (material.name.Contains("PBR"))
                    //        {
                    //            string material_uri = "Assets/Materials/NRP/" + this.nrp_models_subpaths[model_name] + "/" + material.name;
                    //            if (material_uri.Contains(" (Instance)"))
                    //            {
                    //                material_uri = material_uri.Remove(material_uri.IndexOf(" (Instance)"));
                    //            }
                    //            material_uri = material_uri + ".mat";
                    //            Material material_preset = AssetDatabase.LoadAssetAtPath(material_uri, typeof(UnityEngine.Material)) as Material;
                    //            if (material_preset != null)
                    //            {
                    //                Debug.Log("CreateMeshFromJSON() - material " + material_preset + " for " + mesh_renderer.gameObject.name + "(uri: " + material_uri + ")");
                    //                /*List<Renderer> renderers = new List<Renderer>();
                    //                if (gameobject.GetComponent<Renderer>() != null) renderers.Add(gameobject.GetComponent<Renderer>());
                    //                foreach (Renderer renderer in gameobject.GetComponentsInChildren<Renderer>())
                    //                {
                    //                    renderers.Add(renderer);
                    //                }
                    //                gameobject.GetComponent<Renderer>().materials[0] = material_preset;*/
                    //                mesh_renderer.material = Instantiate(material_preset);
                    //            }
                    //            else
                    //            {
                    //                Debug.LogWarning("Could not load " + material_uri);
                    //            }
                    //        }
                    //    }
                    //}

                    if (mesh_renderer.material != null)
                    {
                        Material material = mesh_renderer.material;
                        if (material.name.Contains("PBR"))
                        {
                            string material_uri = "Assets/Materials/NRP/" + this.nrp_models_subpaths[model_name] + "/" + material.name;
                            if (material_uri.Contains(" (Instance)"))
                            {
                                material_uri = material_uri.Remove(material_uri.IndexOf(" (Instance)"));
                            }
                            material_uri = material_uri + ".mat";
                            Material material_preset = AssetDatabase.LoadAssetAtPath(material_uri, typeof(UnityEngine.Material)) as Material;
                            if (material_preset != null)
                            {
                                //Debug.Log("CreateMeshFromJSON() - material " + material_preset + " for " + mesh_renderer.gameObject.name + "(uri: " + material_uri + ")");
                                mesh_renderer.material = Instantiate(material_preset);
                            }
                            else
                            {
                                Debug.LogWarning("Could not load " + material_uri);
                            }
                        }
                    }
                }
            }
        }
        catch (KeyNotFoundException e)
        {
            Debug.Log("CreateMeshFromJSON() - model subpath for '" + model_name + "' is not in dictionary");
        }
        catch (Exception e)
        {
            Debug.Log("CreateMeshFromJSON() - model subpath for '" + model_name + "' is not in dictionary");
        }
    }

    #endregion //CREATE_SCENE_ELEMENTS

    #region SET_SCENE_ELEMENTS
    // create new or update existing scene elements

    private void SetModelFromJSON(JSONNode json_model, Transform parent_transform = null)
    {
        string model_name = json_model["name"];
        GameObject model_gameobject = GameObject.Find(model_name);
        if (model_gameobject == null)
        {
            model_gameobject = new GameObject(model_name);
            model_gameobject.transform.SetParent(parent_transform, false);
        }

        // pose
        JSONNode json_pose = json_model["pose"];
        if (json_pose != null)
        {
            this.SetPoseFromJSON(json_pose, model_gameobject);
            //model_gameobject.transform.localRotation *= Quaternion.AngleAxis(180f, Vector3.up);  // necessary, gazebo or conversion (gazebo->unity) quirk?
        }

        // scale
        JSONNode json_model_scale = json_model["scale"];
        model_gameobject.transform.localScale = new Vector3(json_model_scale["x"].AsFloat, json_model_scale["z"].AsFloat, json_model_scale["y"].AsFloat);

        // links
        Transform links_parent_transform = model_gameobject.transform.Find("links");
        GameObject links_parent = links_parent_transform == null ? new GameObject("links") : links_parent_transform.gameObject;
        links_parent.transform.SetParent(model_gameobject.transform, false);

        JSONArray json_links = json_model["link"].AsArray;
        foreach (JSONNode json_link in json_links)
        {
            this.SetLinkFromJSON(json_link, links_parent.transform, json_model_scale);
        }
    }

    private void SetLinkFromJSON(JSONNode json_link, Transform parent_transform, JSONNode json_model_scale)
    {
        string link_name = json_link["name"];

        // get or create link gameobject
        Transform link_gameobject_transform = parent_transform.Find(link_name);
        GameObject link_gameobject = link_gameobject_transform == null ? new GameObject(link_name) : link_gameobject_transform.gameObject;
        link_gameobject.transform.SetParent(parent_transform, false);

        // pose
        JSONNode json_pose = json_link["pose"];
        if (json_pose != null)
        {
            this.SetPoseFromJSON(json_pose, link_gameobject);
        }

        // visuals
        JSONArray json_visuals = json_link["visual"].AsArray;
        if (json_visuals.Count > 0)
        {
            Transform visuals_parent_transform = link_gameobject.transform.Find("visuals");
            GameObject visuals_parent = visuals_parent_transform == null ? new GameObject("visuals") : visuals_parent_transform.gameObject;
            visuals_parent.transform.SetParent(link_gameobject.transform, false);
            
            foreach (JSONNode jsoon_visual in json_visuals)
            {
                this.SetVisualFromJSON(jsoon_visual, visuals_parent.transform, json_model_scale);
            }
        }

        // collisions
        JSONArray json_collisions = json_link["collision"].AsArray;
        if (json_collisions.Count > 0)
        {
            Transform collisions_parent_transform = link_gameobject.transform.Find("collisions");
            GameObject collisions_parent = collisions_parent_transform == null ? new GameObject("collisions") : collisions_parent_transform.gameObject;
            collisions_parent.transform.SetParent(link_gameobject.transform, false);
            
            foreach (JSONNode json_collision in json_collisions)
            {
                this.SetCollisionFromJSON(json_collision, collisions_parent.transform, json_model_scale);
            }
        }

        // sensors
        JSONArray json_sensors = json_link["sensor"].AsArray;
        if (json_sensors.Count > 0)
        {
            Transform sensors_parent_transform = link_gameobject.transform.Find("sensors");
            GameObject sensors_parent = sensors_parent_transform == null ? new GameObject("sensors") : sensors_parent_transform.gameObject;
            sensors_parent.transform.SetParent(link_gameobject.transform, false);
            
            foreach (JSONNode json_sensor in json_sensors)
            {
                this.SetSensorFromJSON(json_sensor, sensors_parent.transform);
            }
        }
    }

    private GameObject SetVisualFromJSON(JSONNode json_visual, Transform parent_transform, JSONNode json_model_scale)
    {
        if (json_visual["geometry"] == null)
        {
            // visual without geometry can be ignored
            return null;
        }

        string visual_name = json_visual["name"];

        Transform visual_gameobject_transform = parent_transform.Find(visual_name);
        GameObject visual_gameobject = visual_gameobject_transform == null ? new GameObject(visual_name) : visual_gameobject_transform.gameObject;
        visual_gameobject.transform.SetParent(parent_transform, false);

        // pose
        JSONNode json_pose = json_visual["pose"];
        if (json_pose != null)
        {
            this.SetPoseFromJSON(json_pose, visual_gameobject);
        }

        // cast shadows
        bool json_cast_shadows = json_visual["cast_shadows"].AsBool;
        if (json_cast_shadows)
        {
            //TODO: implement
        }

        // receive shadows
        //TODO: implement

        // geometry
        JSONNode json_geometry = json_visual["geometry"];
        if (json_geometry != null)
        {
            JSONNode json_material = json_visual["material"];
            this.SetGeometryFromJSON(json_geometry, json_material, visual_gameobject.transform, json_model_scale);
        }

        return visual_gameobject;
    }

    private GameObject SetGeometryFromJSON(JSONNode json_geometry, JSONNode json_material, Transform parent_transform, JSONNode json_model_scale)
    {
        // geometry
        GameObject geometry_gameobject = null;
        if (json_geometry["box"] != null)
        {
            Transform geometry_gameobject_transform = parent_transform.Find("Cube");
            geometry_gameobject = geometry_gameobject_transform == null ? GameObject.CreatePrimitive(PrimitiveType.Cube) : geometry_gameobject_transform.gameObject;
            geometry_gameobject.transform.SetParent(parent_transform, false);

            JSONNode json_size = json_geometry["box"].AsObject["size"];
            geometry_gameobject.transform.localScale = new Vector3(json_size["x"].AsFloat / json_model_scale["x"].AsFloat,
                json_size["z"].AsFloat / json_model_scale["z"].AsFloat,
                json_size["y"].AsFloat / json_model_scale["y"].AsFloat);
        }
        else if (json_geometry["cylinder"] != null)
        {
            Transform geometry_gameobject_transform = parent_transform.Find("Cylinder");
            geometry_gameobject = geometry_gameobject_transform == null ? GameObject.CreatePrimitive(PrimitiveType.Cylinder) : geometry_gameobject_transform.gameObject;
            geometry_gameobject.transform.SetParent(parent_transform, false);

            float radius = json_geometry["cylinder"].AsObject["radius"].AsFloat;
            float length = json_geometry["cylinder"].AsObject["length"].AsFloat;

            // rescale (unity standard sizes differ from gazebo)
            geometry_gameobject.transform.localScale = new Vector3(radius * 2, length / 2, radius * 2);
        }
        else if (json_geometry["sphere"] != null)
        {
            Transform geometry_gameobject_transform = parent_transform.Find("Sphere");
            geometry_gameobject = geometry_gameobject_transform == null ? GameObject.CreatePrimitive(PrimitiveType.Sphere) : geometry_gameobject_transform.gameObject;
            geometry_gameobject.transform.SetParent(parent_transform, false);

            JSONNode json_radius = json_geometry["sphere"].AsObject["radius"];
            geometry_gameobject.transform.localScale = new Vector3(json_radius.AsFloat * 2, json_radius.AsFloat * 2, json_radius.AsFloat * 2);
        }
        else if (json_geometry["plane"] != null)
        {
            Transform geometry_gameobject_transform = parent_transform.Find("Plane");
            geometry_gameobject = geometry_gameobject_transform == null ? GameObject.CreatePrimitive(PrimitiveType.Plane) : geometry_gameobject_transform.gameObject;
            geometry_gameobject.transform.SetParent(parent_transform, false);

            JSONNode json_normal = json_geometry["plane"].AsObject["normal"];
            geometry_gameobject.transform.up = new Vector3(json_normal["x"].AsFloat, json_normal["z"].AsFloat, json_normal["y"].AsFloat);

            JSONNode json_size = json_geometry["plane"].AsObject["size"];
            geometry_gameobject.transform.localScale = new Vector3(json_size["x"].AsFloat, 1.0f, json_size["y"].AsFloat);
        }
        else if (json_geometry["mesh"] != null)
        {
            //Debug.Log("creating mesh " + json_geometry["mesh"].AsObject["filename"].ToString() + " with material " + json_material.ToString());
            this.CreateMeshFromJSON(json_geometry["mesh"], parent_transform, json_model_scale);
        }

        if (geometry_gameobject != null)
        {
            geometry_gameobject.transform.SetParent(parent_transform, false);
            // don't need collisions
            Destroy(geometry_gameobject.GetComponent<Collider>());
        }

        // material
        //Debug.Log("material: " + json_material.ToString());
        this.SetMaterialFromJSON(json_material, geometry_gameobject);

        return geometry_gameobject;
    }

    private void SetCollisionFromJSON(JSONNode json_collision, Transform parent_transform, JSONNode json_model_scale)
    {
        // visuals
        Transform visuals_parent_transform = parent_transform.Find("visuals");
        GameObject visuals_parent_gameobject = visuals_parent_transform == null ? new GameObject("visuals") : visuals_parent_transform.gameObject;
        visuals_parent_gameobject.transform.SetParent(parent_transform, false);

        JSONArray json_visuals = json_collision["visual"].AsArray;
        foreach (JSONNode json_visual in json_visuals)
        {
            GameObject visual_gameobject = this.SetVisualFromJSON(json_visual, visuals_parent_gameobject.transform, json_model_scale);
            if (visual_gameobject != null && visual_gameobject.GetComponentInChildren<Renderer>() != null)
            {
                visual_gameobject.GetComponentInChildren<Renderer>().material = this.CollisionMaterial;
                // deactivate collision visuals by default
                visual_gameobject.GetComponentInChildren<Renderer>().enabled = false;
            }
        }
    }

    private void SetMaterialFromJSON(JSONNode json_material, GameObject gameobject)
    {
        if (gameobject == null) return;

        JSONNode json_material_script = json_material["script"];
        if (json_material_script != null)
        {
            string material_subpath = json_material_script["name"];
            //Debug.Log("SetMaterialFromJSON() - applying material " + json_material_script["name"] + " for " + gameobject.name);
            string material_uri = "Assets/Materials/" + material_subpath + ".mat";
            Material material_preset = AssetDatabase.LoadAssetAtPath(material_uri, typeof(UnityEngine.Material)) as Material;
            
            if (material_preset != null && gameobject.GetComponent<Renderer>() != null)
            {
                gameobject.GetComponent<Renderer>().materials[0] = material_preset;
            }
            else
            {
                Debug.LogWarning("Could not load " + material_uri);
            }
        }
    }

    private void SetSensorFromJSON(JSONNode json_sensor, Transform parent_transform)
    {
        string sensor_name = json_sensor["name"];

        Transform sensor_transform = parent_transform.Find(sensor_name);
        GameObject sensor_gameobject = sensor_transform == null ? new GameObject(sensor_name) : sensor_transform.gameObject;
        sensor_gameobject.transform.SetParent(parent_transform, false);

        // pose
        JSONNode json_pose = json_sensor["pose"];
        if (json_pose != null)
        {
            this.SetPoseFromJSON(json_pose, sensor_gameobject);
        }

        string type = json_sensor["type"];
        if (type == "camera")
        {
            sensor_gameobject.transform.rotation *= Quaternion.AngleAxis(90, Vector3.up); // gazebo camera sensor faces towards +x, unity's towards +z
            sensor_gameobject.AddComponent<Camera>();
            sensor_gameobject.GetComponent<Camera>().enabled = false;
        }
    }

    private void SetJointFromJSON(JSONNode json_joint, Transform parent_transform)
    {
        //Debug.Log("json joint: " + json_joint.ToString());
    }

    private void SetPoseFromJSON(JSONNode json_pose, GameObject gameobject)
    {
        // rotation
        JSONNode json_rotation = json_pose["orientation"];
        Quaternion rotation = Gz2UnityQuaternion(new Quaternion(json_rotation["x"].AsFloat, json_rotation["y"].AsFloat, json_rotation["z"].AsFloat, json_rotation["w"].AsFloat));
        // position
        JSONNode json_position = json_pose["position"];
        Vector3 position = Gz2UnityVec3(new Vector3(json_position["x"].AsFloat, json_position["y"].AsFloat, json_position["z"].AsFloat));
        // scale
        JSONNode json_scale = json_pose["scale"];
        if (json_scale != null)
        {
            Debug.Log("SetPoseFromJSON - " + name + ", json_scale: " + json_scale.ToString());
        }
        // reference frame
        JSONNode json_reference_frame = json_pose["reference_frame"];
        if (json_reference_frame != null)
        {
            Debug.Log("SetPoseFromJSON - " + name + ", json_reference_frame: " + json_reference_frame.ToString());
        }
        // The user's avatar takes the rotation directly from the VR headset and not from the server to countervail latency and prevent glitches
        if (!gameobject.name.Equals("user_avatar_" + GzBridgeManager.Instance.avatarId))
        {
            gameobject.transform.localRotation = rotation /* Quaternion.AngleAxis(180f, Vector3.up)*/;
        }
        gameobject.transform.localPosition = position;
    }

    #endregion //SET_SCENE_ELEMENTS

    #region HELPER_FUNCTIONS

    /// <summary>
    /// Saves model names and their relative paths in the NRP models repository into a dictionary.
    /// This is done to avoid having to flatten the model folder hierarchy (usually done by Gazebo) so the models can be found at their actual location inside the repository folder structure.
    /// </summary>
    private bool InitModelSubpaths()
    {
        // load model list file
        string model_list_file_path = Application.dataPath + "/" + this.NRPModelsSubpath + "/_rpmbuild/models.txt";
        StreamReader stream_reader = new StreamReader(model_list_file_path, Encoding.Default);
        using (stream_reader)
        {
            try
            {
                string line = stream_reader.ReadLine();
                if (line != null)
                {
                    do
                    {
                        if (line.LastIndexOf('/') == line.Length-1)
                        {
                            line = line.Remove(line.Length - 1);
                        }
                        string[] model_subpath_directories = line.Split('/');
                        string model_name = model_subpath_directories[model_subpath_directories.Length - 1];

                        try
                        {
                            this.nrp_models_subpaths.Add(model_name, line);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message + " (model: " + model_name + ", line: " + line);
                        }

                        line = stream_reader.ReadLine();
                    }
                    while (line != null);
                }
                stream_reader.Close();

                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }
    }

    #region Convert function from gazebo to unity and vice versa.
    
    /// <summary>
    /// Converts a vector in gazebo coordinate frame to unity coordinate frame.
    /// </summary>
    /// <param name="gazeboPos">Vector in gazebo coordinate frame.</param>
    /// <returns>Vector in unity coordinate frame.</returns>
    private Vector3 Gz2UnityVec3(Vector3 gazeboPos)
    {
        return new Vector3(gazeboPos.x, gazeboPos.z, gazeboPos.y);
    }

    /// <summary>
    /// Converts a quaternion in gazebo coordinate frame to unity coordinate frame.
    /// </summary>
    /// <param name="gazeboRot">Quaternion in gazebo coordinate frame.</param>
    /// <returns>Quaternion in unity coordinate frame.</returns>
    private Quaternion Gz2UnityQuaternion(Quaternion gazeboRot)
    {
        Quaternion tempRot = new Quaternion(-gazeboRot.x, -gazeboRot.z, -gazeboRot.y, gazeboRot.w);
        
        Quaternion finalRot = tempRot;

        return finalRot;
    }

    /// <summary>
    /// Converts a vector in unity coordinate frame to gazebo coordinate frame.
    /// </summary>
    /// <param name="unityPos">Vector in unity coordinate frame.</param>
    /// <returns>Vector in gazebo coordinate frame.</returns>
    private Vector3 Unity2GzVec3(Vector3 unityPos)
    {
        return new Vector3(unityPos.x, unityPos.z, unityPos.y);
    }

    /// <summary>
    /// Converts a quaternion in unity coordinate frame to gazebo coordinate frame.
    /// </summary>
    /// <param name="unityRot">Quaternion in unity coordinate frame.</param>
    /// <returns>Quaternion in gazebo coordinate frame.</returns>
    private Quaternion Unity2GzQuaternion(Quaternion unityRot)
    {
        Quaternion rotX = Quaternion.AngleAxis(180f, Vector3.right);
        Quaternion rotZ = Quaternion.AngleAxis(180f, Vector3.forward);

        Quaternion tempRot = unityRot * rotX * rotZ;

        Quaternion finalRot = new Quaternion(-tempRot.x, -tempRot.z, -tempRot.y, tempRot.w);

        return finalRot;
    }
    #endregion

    #endregion //HELPER_FUNCTIONS
}
