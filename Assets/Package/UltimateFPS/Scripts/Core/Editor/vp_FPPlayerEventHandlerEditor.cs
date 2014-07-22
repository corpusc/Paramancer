/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPPlayerEventHandlerEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	simple interface for opening the event handler window or
//					performing an event dump to the console
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(vp_FPPlayerEventHandler))]

public class vp_FPPlayerEventHandlerEditor : Editor
{
	

	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		GUI.color = Color.white;
		
		GUILayout.Space(20);
		EditorGUILayout.BeginVertical();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		GUI.enabled = Application.isPlaying;
		if (GUILayout.Button("View Enabled Events"))
		{
			vp_EventDumpWindow.Create((vp_EventHandler)target);
		}
		if (GUILayout.Button("Dump to Console"))
		{
			Debug.Log("EVENT DUMP\n\n" + vp_EventDump.Dump((vp_EventHandler)target, new string[] { "vp_Message", "vp_Value", "vp_Attempt", "vp_Activity" }));
		}
		GUI.enabled = true;

		GUILayout.Space(20);

		EditorGUILayout.EndHorizontal();
		if (!Application.isPlaying)
		{
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.HelpBox("Events can be viewed when the application is playing. See the docs for more info about the Event System.", MessageType.Info);
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		GUILayout.Space(20);


	}

}

