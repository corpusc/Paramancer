using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ExplosionMat))]
[CanEditMultipleObjects]
public class ExplosionMatEditor : Editor {
	
	bool advanced = false;
	GUIContent[] octaveStrings = {new GUIContent("1"), new GUIContent("2"), new GUIContent("3"), new GUIContent("4"), new GUIContent("5")};
	int[] octaveNums = {1, 2, 3, 4, 5};
	GUIContent[] qualityStrings = {new GUIContent("Low"), new GUIContent("Medium"), new GUIContent("High")};
	int[] qualityNums = {0, 1, 2};
	
	SerializedProperty ramp;
	SerializedProperty noise;
	
	SerializedProperty alpha;
	SerializedProperty heat;
	SerializedProperty scrollSpeed;
	SerializedProperty frequency;
	
	SerializedProperty scattering;
	SerializedProperty quality;
	SerializedProperty octaves;
	
	// Get all the serialized properties for the internal values
	public void OnEnable() {
		heat = serializedObject.FindProperty("_heat");
		alpha = serializedObject.FindProperty("_alpha");
		scrollSpeed = serializedObject.FindProperty("_scrollSpeed");
		frequency = serializedObject.FindProperty("_frequency");
		ramp = serializedObject.FindProperty("_ramp");
		noise = serializedObject.FindProperty("_noise");
		scattering = serializedObject.FindProperty("_scattering");
		quality = serializedObject.FindProperty("_quality");
		octaves = serializedObject.FindProperty("_octaves");
	}
	
	public override void OnInspectorGUI() {
		// Cast target to an ExplosionMat
		ExplosionMat Mat = (ExplosionMat) target;
		serializedObject.Update(); // Always call this
		
		EditorGUIUtility.LookLikeInspector();
		
		// Check if any changes are made to the GUI
		EditorGUI.BeginChangeCheck();
			EditorGUILayout.Slider(alpha, 0, 1, new GUIContent("Alpha"));
			EditorGUILayout.PropertyField(heat, new GUIContent("Heat"));
			EditorGUILayout.PropertyField(scrollSpeed, new GUIContent("Noise Scroll Speed"));
			EditorGUILayout.PropertyField(frequency, new GUIContent("Noise Frequency"));
			
			EditorGUILayout.PropertyField(scattering, new GUIContent("Scattering"));
			EditorGUILayout.IntPopup(quality, qualityStrings, qualityNums, new GUIContent("Quality"));
			EditorGUILayout.IntPopup(octaves, octaveStrings, octaveNums, new GUIContent("Octaves"));
		
			if (advanced = EditorGUILayout.Foldout(advanced, "Advanced")) {
				EditorGUILayout.PropertyField(ramp, new GUIContent("Ramp Texture"));
				EditorGUILayout.PropertyField(noise, new GUIContent("Noise Texture"));
			}
		
			serializedObject.ApplyModifiedProperties(); // Always call this
		//Make shader update properties if editor has been changed
		if (EditorGUI.EndChangeCheck() || Event.current.commandName == "UndoRedoPerformed") {
			// Tell the ExplosionMat to update the shader properties
			Mat.UpdateShaderProperties();
		}
		
	}
}
