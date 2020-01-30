using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteAvatarVisuals : MonoBehaviour {

    public float opacity = 0.1f;

    private GameObject hips;
    private GameObject spine;
    private GameObject spine1;
    private GameObject spine1Visuals;
    private GameObject spine2;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void IdentifyGameObjects()
    {
        hips = GameObject.Find("user_avatar_e26d79c4_f1cd_47a3_b78e_4764b7ffa254::avatar_ybot::mixamorig_Hips");
        spine = GameObject.Find("user_avatar_e26d79c4_f1cd_47a3_b78e_4764b7ffa254::avatar_ybot::mixamorig_Spine");
        spine1 = GameObject.Find("user_avatar_e26d79c4_f1cd_47a3_b78e_4764b7ffa254::avatar_ybot::mixamorig_Spine1");
        spine1Visuals = GameObject.Find("user_avatar_e26d79c4_f1cd_47a3_b78e_4764b7ffa254::avatar_ybot::mixamorig_Spine1::VIS_Alpha_Surface.010");
        spine2 = GameObject.Find("user_avatar_e26d79c4_f1cd_47a3_b78e_4764b7ffa254::avatar_ybot::mixamorig_Spine2");
    }

    public void SetOpacity(float opacity)
    {
        this.opacity = opacity;

        //Renderer[] renderers = this.GetComponentsInChildren<Renderer>();
        Renderer[] renderers = new Renderer[] {hips.GetComponentInChildren<Renderer>(), spine.GetComponentInChildren<Renderer>(),
            spine1.GetComponentInChildren<Renderer>(), spine1Visuals.GetComponentInChildren<Renderer>(), spine2.GetComponentInChildren<Renderer>() };
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.GetComponent<ParticleSystem>() != null)
                continue;
            foreach (Material material in renderer.materials)
            {
                Color color = material.color;
                color.a = opacity;
                material.SetColor("_Color", color);
                this.SetRenderModeTransparent(material);
            }
        }
    }

    private void SetRenderModeTransparent(Material material)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
}
