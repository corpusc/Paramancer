// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;


/// <summary>
/// Inspector class of MBEmitter
/// </summary>
[CustomEditor(typeof(MBEmitter))]
public class MBEmitterInspector : Editor
{
    MBEmitter Emitter { get { return (target as MBEmitter); } }

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

        if (!Emitter.AutoPlay && PrefabUtility.GetPrefabType(Emitter) != PrefabType.Prefab) {
            GUILayout.BeginHorizontal();
                if (GUILayout.Button("Play"))
                    Emitter.Play();
                if (GUILayout.Button("Stop"))
                    Emitter.Stop();
                if (GUILayout.Button("Halt"))
                    Emitter.Halt();
            GUILayout.EndHorizontal();
        }

        if (Emitter.Layer && Emitter.Layer.Material && Emitter.Layer.Material.shader) {
            Emitter.LayerShaderName = Emitter.Layer.Material.shader.name;
        }
        else
            Emitter.LayerShaderName = string.Empty;
    }

    
}

