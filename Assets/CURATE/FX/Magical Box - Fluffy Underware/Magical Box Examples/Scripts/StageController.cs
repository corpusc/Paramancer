using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class StageController : MonoBehaviour {

    public MBParticleSystem ParticleSystem;
    public string txtKeys;
    public Texture2D Logo;
    
    Vector3 orgPos;
    Quaternion orgRot;

	// Use this for initialization
	void Start () {
        if (txtKeys.Length==0)
            txtKeys= "\"WASD/Cursors\": Move   \"Fire2\": Turn   \"Space\": Reset   \"F4\":Toggle Fullscreen";
        orgPos = transform.position;
        orgRot = transform.rotation;
        if (!ParticleSystem)
            ParticleSystem = (MBParticleSystem)FindObjectOfType(typeof(MBParticleSystem));
	}

    // Update is called once per frame
    void Update()
    {

        // look around
        if (Input.GetButton("Fire2")) {
            float x = 500 * Input.GetAxis("Mouse X") * Time.deltaTime;
            float y = 500 * Input.GetAxis("Mouse Y") * Time.deltaTime;
            transform.Rotate(Vector3.up, x,Space.World);
            transform.Rotate(Vector3.left, y,Space.Self);
        }
        // move around
        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * 4, 0, Input.GetAxis("Vertical") * Time.deltaTime * 4, Space.Self);
        // Reset?
        if (Input.GetKeyDown(KeyCode.Space)) {
            transform.position = orgPos;
            transform.rotation = orgRot;
        }
        // Toggle Fullscreen?
        if (Input.GetKeyDown(KeyCode.F4))
            Screen.fullScreen = !Screen.fullScreen;
        

	}

    void OnGUI()
    {
        int w=Screen.width/4;
        GUI.DrawTexture(new Rect(5, Screen.height-w, w,w), Logo);
        GUILayout.BeginArea(new Rect(w+20,Screen.height-40,Screen.width-w-20,40));
        
            GUILayout.Label(txtKeys);
            if (ParticleSystem) 
                GUILayout.Label(string.Format("Particles processed / rendered: {0} / {1} (max: {2})", ParticleSystem.ParticleCount,ParticleSystem.ParticlesRendered, ParticleSystem.ParticlesRenderedMax));
            GUILayout.EndArea();
        
    }
    

}
