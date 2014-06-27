// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;

/// <summary>
/// GUI helper class to get a consistent layout
/// </summary>
public class MBGUI
{
    static public bool HasChanged; // used for working with transform values like name, position etc...
    static Keyframe[] CurveClipboard=new Keyframe[0]; // used to copy&paste curves
    public static int _DefaultWidth = 280;

    static GUIStyle mStyleHeader;
    

    public static void DoHeader(string caption)
    {
        if (mStyleHeader == null) {
            mStyleHeader = new GUIStyle(GUI.skin.GetStyle("button"));
            mStyleHeader.fontStyle = FontStyle.Bold;
        }
        Color c = GUI.backgroundColor;
        GUI.backgroundColor= new Color(0.7f, 0.7f, 0.8f);
        GUILayout.Label(caption, mStyleHeader);
        GUI.backgroundColor = c;

    }

    public static Vector3[] DoVector3Array(string text, Vector3[] points,bool isPolygon, ref bool folderopen)
    {
        folderopen = EditorGUILayout.Foldout(folderopen, text);
        if (folderopen) {
            int step = (isPolygon) ? 1 : 2;
            int size = DoIntField("Size", "Array size", points.Length / step) * step;
            if (size!=points.Length) {
                Vector3[] tmp=points;
                points = new Vector3[size];
                if (size>0 && tmp.Length>0)
                    System.Array.Copy(tmp, points, Mathf.Min(size,tmp.Length));
            }
            
            for (int i = 0; i < points.Length; i+=step) {
                if (isPolygon)
                    points[i] = DoVector3Field("Point " + i, points[i]);
                else {
                    EditorGUILayout.BeginHorizontal();
                    DoLabel("Line " + ((i / 2) + 1)+": ");
                    points[i] = DoVector3Field("From", points[i]);
                    points[i+1] = DoVector3Field("To", points[i+1]);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        return points;
    }

    public static void DoLabel(GUIContent content)
    {
        GUILayout.Label(content, GUILayout.ExpandWidth(false));        
    }

    public static void DoLabel(string text)
    {
        DoLabel(new GUIContent(text));
    }

    public static void DoLabel(string text, int width, bool bold)
    {
        GUIStyle style=(bold) ? EditorStyles.boldLabel : EditorStyles.label;
        GUILayout.Label(new GUIContent(text), style,new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(width) });
    }

    public static LayerMask DoLayerMask(string text, string tooltip, LayerMask mask)
    {
        // TBD
        return mask;
    }

    public static bool DoButton(GUIContent content, bool enabled, bool expand)
    {
        GUI.enabled = enabled;
        bool res = GUILayout.Button(content, GUILayout.ExpandWidth(expand));
        GUI.enabled = true;
        return res;
    }

    public static bool DoButton(string text, string tooltip, bool enabled)
    {
        return DoButton(new GUIContent(text, tooltip), enabled, false);
    }

    public static bool DoLabelButton(GUIContent content, bool enabled)
    {
        return DoLabelButton(content, _DefaultWidth, enabled);
    }

    public static bool DoLabelButton(GUIContent content, int width, bool enabled)
    {
        GUI.enabled = enabled;
        bool res = GUILayout.Button(content, EditorStyles.label, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(width) });
        GUI.enabled = true;
        return res;
    }

    public static float DoFloatFieldSmall(string text, string tooltip, float value)
    {
        return DoFloatFieldSmall(new GUIContent(text, tooltip), value, true);
    }
    public static float DoFloatFieldSmall(GUIContent content, float value, bool round)
    {
        GUILayout.Toggle(false, content, "Label", GUILayout.ExpandWidth(false));
        HasChanged = false;
        float o = value;
        value = EditorGUILayout.FloatField(value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MaxWidth(80) });
        if (o != value) {
            HasChanged = true;
            if (round)
                return (int)(value * 100) / 100.0f;
        }
        return value;
    }

    public static float DoFloatField(string text, string tooltip, float value)
    {
        return DoFloatField(text, tooltip, value, true);
    }


    // HasChanged aware
    public static float DoFloatField(string text, string tooltip, float value, bool round)
    {
        HasChanged = false;
        float o = value;
        
        value = EditorGUILayout.FloatField(new GUIContent(text, tooltip), value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth) });
        if (o != value) {
            HasChanged = true;
            if (round)
                return (int)(value * 100) / 100.0f;
        }
        return value;
    }


    public static float DoFloatSliderSmall(GUIContent content, float value, float min, float max)
    {
        GUILayout.Toggle(false, content, "Label", GUILayout.ExpandWidth(false));
        float v = EditorGUILayout.Slider(value, min, max, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MaxWidth(140) });
        return (int)(v * 100) / 100.0f;
    }
    public static float DoFloatSlider(string text, string tooltip, float value, float min, float max)
    {
        float v = EditorGUILayout.Slider(new GUIContent(text, tooltip), value, min, max, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth) });
        return (int)(v * 100) / 100.0f;
    }

    public static bool DoToggle(string text, string tooltip, bool value)
    {
        return EditorGUILayout.Toggle(new GUIContent(text, tooltip), value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth) });
    }

    public static bool DoToggle(string text, string tooltip,bool value, out bool hasChanged)
    {
        bool vnew = EditorGUILayout.Toggle(new GUIContent(text, tooltip), value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth), GUILayout.Height(16) });
        hasChanged=(vnew!=value);
        return vnew;
    }
    public static bool DoToggle(Texture tex, string tooltip,bool value, out bool hasChanged)
    {
        bool vnew = GUILayout.Toggle(value, new GUIContent(tex,tooltip),GUILayout.ExpandWidth(false));
        hasChanged = (vnew != value);
        return vnew;
    }

    public static bool DoToggleButton(GUIContent content, bool value)
    {
        return GUILayout.Toggle(value,content,"button",GUILayout.ExpandWidth(false));
    }

    // HasChanged aware
    public static Vector2 DoVector2Field(string text, Vector2 value)
    {
        HasChanged = false;
        Vector2 o = value;
        value=EditorGUILayout.Vector2Field(text, value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth) });
        if (o != value)
            HasChanged = true;
        return value;
    }

    // HasChanged aware
    public static Vector3 DoVector3Field(string text, Vector3 value)
    {
        HasChanged = false;
        Vector3 o = value;
        value = EditorGUILayout.Vector3Field(text, value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth) });
        if (o != value)
            HasChanged = true;
        return value;
    }

    public static Object DoObjectField(string text, string tooltip,Object obj, System.Type objtype)
    {
        return EditorGUILayout.ObjectField(new GUIContent(text, tooltip), obj, objtype, true,new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth) });
    }

    public static System.Enum DoEnumField(string text, string tooltip, System.Enum value)
    {
        return EditorGUILayout.EnumPopup(new GUIContent(text, tooltip), value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth) });
    }

    //public static string DoTextField(string text, string tooltip, string value) { return DoTextField(text,tooltip,value);}

    // HasChanged aware
    public static string DoTextField(string text,string tooltip, string value)
    {
        HasChanged = false;
        string o = value;
        value=EditorGUILayout.TextField(new GUIContent(text, tooltip), value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth) });
        if (o != value)
            HasChanged = true;
        return value;
    }

    public static int DoIntFieldSmall(string text, string tooltip, int value)
    {
        GUILayout.Toggle(false, new GUIContent(text,tooltip), "Label", GUILayout.ExpandWidth(false));
        HasChanged = false;
        int o = value;
        value= EditorGUILayout.IntField(value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MaxWidth(80) });
        if (o != value) {
            HasChanged = true;
            return o;
        }
        return value;
    }

    // HasChanged aware
    public static int DoIntField(string text, string tooltip, int value)
    {
        HasChanged = false;
        int o = value;
        value=EditorGUILayout.IntField(new GUIContent(text, tooltip), value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(_DefaultWidth)});
        if (o != value)
            HasChanged = true;
        return value;
    }

    public static Color DoColorField(string text, string tooltip,Color value)
    {
        return EditorGUILayout.ColorField(new GUIContent(text, tooltip), value, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(200) });
    }

    public static AnimationCurve DoCurve(string text, string tooltip,AnimationCurve curve, float init0, float init1)
    {
        if (curve == null)
            curve = AnimationCurve.Linear(0, init0, 1, init1);
        curve = EditorGUILayout.CurveField(new GUIContent(text, tooltip), curve, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(400) });

        if (MBGUI.DoButton("Clear", "Reset curve", true)) 
            curve = AnimationCurve.Linear(0, init0, 1, init1);
        if (MBGUI.DoButton("Copy", "Copy curve data", true)) {
            CurveClipboard = curve.keys;
        }
        if (MBGUI.DoButton("Paste", "Paste curve data", CurveClipboard.Length > 0)) {
            curve = AnimationCurve.Linear(0, init0, 1, init1); // to force update
            curve.keys = CurveClipboard;
            CurveClipboard = new Keyframe[0];
        }
        
        return curve;
    }

    public static void LimitCurveValue (ref AnimationCurve curve, float min, float max)
    {
        Keyframe[] keys = curve.keys;
        
        for (int i=0;i<keys.Length;i++)
            keys[i].value = Mathf.Clamp(keys[i].value, min, max);
        curve.keys = keys;
    }

    public static void DoMinMax(string text, string tooltip, ref float minValue, ref float maxValue, float min, float max)
    {
        EditorGUILayout.MinMaxSlider(new GUIContent(text, tooltip), ref minValue, ref maxValue, min, max, new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.MinWidth(200)});
    }

    public static MBSendMessageTarget DoEditorEvent(string text, string tooltip, MBSendMessageTarget smEvent)
    {
        
        GameObject tgt = (smEvent != null) ? smEvent.Target : null;
        string mth = (smEvent != null) ? smEvent.MethodName : "";
        EditorGUILayout.BeginHorizontal();
            tgt = DoObjectField(text, tooltip, tgt, typeof(GameObject)) as GameObject;
            mth = DoTextField("Method", "Method Name to call", mth);
        EditorGUILayout.EndHorizontal();
        if ((tgt != null || !string.IsNullOrEmpty(mth)) && smEvent==null) 
                smEvent = new MBSendMessageTarget();
        if (smEvent!=null) {
            smEvent.Target = tgt;
            smEvent.MethodName = mth;
            smEvent.Prepare();
        }
        
        return smEvent;
    }

}
