// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class with useful functions
/// </summary>
public class MBUtility
{
    /// <summary>
    /// Draws a gizmo arc
    /// </summary>
    /// <param name="min">Minimum angle in radians</param>
    /// <param name="max">Maxixum angle in radians</param>
    /// <param name="size">Size of the arc</param>
    public static void DrawGizmoArc(float min, float max, float size)
    {
        float r = min;
        List<Vector2> points = new List<Vector2>();
        points.Add(Vector2.zero);
        while (r < max) {
            points.Add(new Vector2(size*Mathf.Cos(r + Mathf.PI / 2), size*Mathf.Sin(r + Mathf.PI / 2)));
            r += 0.3f;
        }
        points.Add(new Vector2(size*Mathf.Cos(max + Mathf.PI / 2), size*Mathf.Sin(max + Mathf.PI / 2)));
        points.Add(Vector2.zero);
        DrawGizmoPolygon(points.ToArray());
    }

    /// <summary>
    /// Draws a gizmo arc
    /// </summary>
    /// <param name="min">Minimum angle in radians</param>
    /// <param name="max">Maxixum angle in radians</param>
    public static void DrawGizmoArc(float min, float max)
    {
        DrawGizmoArc(min, max, 1f);
    }

    /// <summary>
    /// Draws a gizmo arrow
    /// </summary>
    /// <param name="from">center position</param>
    /// <param name="dir">extend</param>
    /// <param name="length">length</param>
    public static void DrawGizmoArrow(Vector3 from, Vector3 dir, float length)
    {
        Vector3 to=from + dir * length;
        Gizmos.DrawLine(from, to);
        Vector3 r = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 200, 0) * new Vector3(0, 0, 1);
        Vector3 l = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 160, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(to, l * 0.25f);
        Gizmos.DrawRay(to, r * 0.25f);
    }

    /// <summary>
    /// Draws a gizmo circle
    /// </summary>
    /// <param name="radius">radius in units</param>
    public static void DrawGizmoCircle(float radius)
    {
        List<Vector2> points = new List<Vector2>();

        for (float r = 0; r <= Mathf.PI * 2; r += Mathf.PI * 2 / 36) {
            points.Add(new Vector2(Mathf.Cos(r) * radius, Mathf.Sin(r) * radius));
        }
        DrawGizmoPolygon(points.ToArray());
    }
    /// <summary>
    /// Draws a gizmo polygon
    /// </summary>
    /// <param name="points">an array of points</param>
    public static void DrawGizmoPolygon(Vector3[] points)
    {
        for (int i = 0; i < points.Length - 1; i++) {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
    /// <summary>
    /// Draws a gizmo polygon
    /// </summary>
    /// <param name="points">an array of points</param>
    public static void DrawGizmoPolygon(Vector2[] points)
    {
        for (int i = 0; i < points.Length - 1; i++) {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
    /// <summary>
    /// Draws a gizmo rectangle
    /// </summary>
    /// <param name="rect">rectangle coordinates</param>
    public static void DrawGizmoRect(Rect rect)
    {
        Vector2[] p = new Vector2[5] { new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMin, rect.yMin) };
        DrawGizmoPolygon(p);
    }

    /// <summary>
    /// Gets the first matching component of parentTypes from transform or one of its parents
    /// </summary>
    /// <param name="transform">The transform you want the object from</param>
    /// <param name="types">a list of allowed types</param>
    /// <returns>a component with a matching type from types[] or null</returns>
    public static Component Get(Transform transform, params System.Type[] types)
    {
        Transform T = transform;
        if (T && types.Length > 0) {
            Component Result = null;
            while (T) {
                foreach (System.Type type in types) {
                    if (type != null)
                        Result = T.GetComponent(type.Name);
                    if (Result)
                        return Result;
                }
                T = T.parent;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the first matching component of parentTypes from transform or one of its parents
    /// </summary>
    /// <typeparam name="T">a wanted type</typeparam>
    /// <param name="transform">The transform you want the object from</param>
    /// <returns>a component of type T or null</returns>
    public static T Get<T>(Transform transform) where T : Component
    {
        return (T)Get(transform, typeof(T));
    }


    /// <summary>
    /// Gets all objects of a given type from an object list
    /// </summary>
    /// <typeparam name="T">the desired type</typeparam>
    /// <param name="list">an object list, e.g. Selection.objects</param>
    /// <returns>a list of objects of Type T</returns>
    public static List<T> GetAll<T>(Object[] list) where T : Object
    {
        List<T> res = new List<T>();
        foreach (Object o in list)
            if (o is T)
                res.Add((T)o);
        return res;
    }

    /// <summary>
    /// Gets all scene objects of a given type
    /// </summary>
    /// <typeparam name="T">Type of wanted objects</typeparam>
    /// <returns>a list of all scene Objects of Type T</returns>
    public static List<T> GetAllFromScene<T>() where T : Object
    {
        Object[] olst = GameObject.FindSceneObjectsOfType(typeof(T));
        List<T> res = new List<T>();
        foreach (Object o in olst)
            if (o is T)
                res.Add((T)o);
        return res;
    }

    public static Vector2 GetGameViewDimensions()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }

    /// <summary>
    /// Gets the first parent with a matching component of parentTypes
    /// </summary>
    /// <param name="transform">The transform you want the parent of</param>
    /// <param name="parentTypes">a list of allowed parent types</param>
    /// <returns>a component with a matching type from parentTypes[] or null</returns>
    public static Component GetParent(Transform transform, params System.Type[] parentTypes)
    {
        Transform T = transform.parent;
        if (T && parentTypes.Length>0) {
            Component Result = null;
            while (T){
                foreach (System.Type type in parentTypes){
                    if (type!=null)
                        Result = T.GetComponent(type.Name);
                    if (Result)
                        return Result;
                }
                T=T.parent;
            }
        }
        
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">a wanted parent type</typeparam>
    /// <param name="transform">The transform you want the parent of</param>
    /// <returns>a component of type T or null</returns>
    public static T GetParent<T>(Transform transform) where T:Component
    {
        return (T)GetParent(transform, typeof(T));
    }

    /// <summary>
    /// Gets the first matching component of a certain type from child gameobjects
    /// </summary>
    /// <remarks>Smart mode means the first matching child of each tree-leaf. If there's a different gameobject in between, it will be ignored</remarks>
    /// <typeparam name="T">a Type derived from Component</typeparam>
    /// <param name="transform">The parent</param>
    /// <param name="smartSearch">Whether smart mode is used</param>
    /// <param name="onlyEnabled">true to return only components of active gameobjects</param>
    public static List<T> GetChildren<T>(Transform transform, bool smartSearch, bool onlyEnabled) where T:MonoBehaviour
    {
        List<T> result = new List<T>();
        foreach (Transform child in transform) {
#if UNITY_4_0
            if (child.gameObject && (child.gameObject.activeSelf || !onlyEnabled)) {
#else
            if (child.gameObject && (child.gameObject.active || !onlyEnabled)) {
#endif
                T cmp = child.GetComponent<T>();
                if (cmp)
                    result.Add(cmp);
                else
                    if (smartSearch) 
                           result.AddRange(GetChildren<T>(child, smartSearch, onlyEnabled));
                
            }
        }
        return result;
    }

    /// <summary>
    /// Gets a single direct child-component by its gameObject's name
    /// </summary>
    /// <typeparam name="T">a Type derived from component</typeparam>
    /// <param name="name">the GameObject's name</param>
    /// <param name="transform">The parent</param>
    /// <param name="onlyEnabled">true to return only a component of an active GameObject</param>
    /// <returns></returns>
    public static T GetChild<T>(string name, Transform transform, bool onlyEnabled) where T : MonoBehaviour
    {
        foreach (Transform child in transform) {
            if (child.name.Equals(name,System.StringComparison.CurrentCultureIgnoreCase)){
                T comp = child.GetComponent<T>();
#if UNITY_4_0
                if (comp && (child.gameObject.activeSelf || !onlyEnabled))
#else
                if (comp && (child.gameObject.active || !onlyEnabled))
#endif
                    return comp;
                }
        }
        return null;
    }

    /// <summary>
    /// Gets a single direct child-component of an active GameObject by its name
    /// </summary>
    /// <typeparam name="T">a Type inherited from component</typeparam>
    /// <param name="transform">The parent</param>
    /// <param name="name">the GameObject's name</param>
    public static T GetChild<T>(string name, Transform transform) where T : MonoBehaviour
    {
        return GetChild<T>(name,transform, true);
    }

    /// <summary>
    /// Gets a random sign
    /// </summary>
    /// <returns>either 1 or -1</returns>
    public static float RandomSign ()
    {
	    return Random.value < 0.5f? -1 : 1;
    }

    public static Texture2D SaveTexture(Texture2D tex, string path)
    {
        if (!string.IsNullOrEmpty(path)) {
            Texture2D t = new Texture2D(tex.width, tex.height);
            t.SetPixels(tex.GetPixels());
            t.Apply();
#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IPHONE && !UNITY_WEBPLAYER
            System.IO.File.WriteAllBytes(path, t.EncodeToPNG());
#endif
            return t;
        }
        return null;
    }

    /// <summary>
    /// Multiply two Vector3
    /// </summary>
    public static Vector3 Scale(Vector3 a, Vector3 b)
    {
        return (new Vector3(a.x * b.x, a.y * b.y, a.z * b.z));
    }

}
