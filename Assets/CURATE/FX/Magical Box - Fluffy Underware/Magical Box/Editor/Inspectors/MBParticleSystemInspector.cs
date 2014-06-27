// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;


/// <summary>
/// Inspector class of MBParticleSystem
/// </summary>
[CustomEditor(typeof(MBParticleSystem))]
public class MBParticleSystemInspector : Editor 
{

    MBParticleSystem ParticleSystem { get { return target as MBParticleSystem; } }
    
    void OnEnable()
    {
        MBEditor.Select(Selection.activeGameObject);
    }

    void OnDisable()
    {
        if (Selection.activeGameObject)
            MBEditor.Select(Selection.activeGameObject);
    }
    
    void OnSceneGUI()
    {
        MBEditorUtility.OnEditorSceneGUI();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Texture2D atlas = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Texture Atlas"), ParticleSystem.TextureAtlas, typeof(Texture2D),false);

        if (atlas != ParticleSystem.TextureAtlas)
            ParticleSystem.SetTextureAtlas(AssetDatabase.GetAssetPath(atlas));
        
        if (ParticleSystem.TextureAtlas) {
            GUILayout.Label(ParticleSystem.TextureAtlasPath);
        }

        if (ParticleSystem && PrefabUtility.GetPrefabType(ParticleSystem) != PrefabType.Prefab) {
            GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
            //    Repaint();
                if (MBGUI.DoButton(new GUIContent("Play"), !ParticleSystem.Playing, true)) 
                    ParticleSystem.Play();
                if (MBGUI.DoButton(new GUIContent("Stop"),ParticleSystem.Playing,true))
                    ParticleSystem.Stop();
                if (MBGUI.DoButton(new GUIContent("Halt"), ParticleSystem.Playing, true))
                    ParticleSystem.Halt();
                GUILayout.EndHorizontal();

            if (GUILayout.Button("Edit"))
                MBEditor.ShowWindow();

            if (MBGUI.DoButton(new GUIContent("Synchronize"), true, true))
                MBEditorCommands.SyncHierarchy();

            
            GUILayout.EndVertical();
            
            
            
            
        }
        
    }

    
    
}
