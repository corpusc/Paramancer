// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;

/// <summary>
/// Magical About Box ;)
/// </summary>
public class MBEditorAbout : EditorWindow {

    Texture2D mTexLogo;

    public static void Open()
    {
        var win=GetWindow<MBEditorAbout>(true, "About Magical Box",true);
        win.maxSize = new Vector2(300, 265);
        win.minSize = win.maxSize;
    }

  
    void OnEnable()
    {
        mTexLogo=Resources.LoadAssetAtPath("Assets/Magical Box/Editor/Resources/Logo.png",typeof(Texture2D)) as Texture2D;
    }
 
    void OnGUI()
    {
        if (mTexLogo!=null)
            GUI.DrawTexture(new Rect(0, 0, 300, 145), mTexLogo);
        GUILayout.BeginArea(new Rect(0,150,300,300));
        GUILayout.Label("Version: " + MBParticleSystem.VERSION);
        GUILayout.Label("(c) 2011-2013 Fluffy Underware");
        GUILayout.Label("");
        GUILayout.Label("Special thanks to:");
        
        GUILayout.Label("Peter Rigby @ Rigzsoft (http://www.rigzsoft.co.uk)");
        GUILayout.Label("Quazistax (http://quazistax.blogspot.com)");
        if (GUILayout.Button("Close"))
            Close();
        GUILayout.EndArea();
    }

}
