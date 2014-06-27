// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Provides a mesh edge emitter type
/// </summary>
[MBEmitterTypeInfo(Menu = "Mesh/Edge")]
public class MBMeshEdgeEmitter : MBLineEmitter
{
    #region ### Inspector Fields ###
    public MeshFilter Mesh;
    #endregion

    Quaternion mOldRot;
    Vector3 mOldScale;
    MeshFilter mOldMesh;

    /// <summary>
    /// Reads all necessary mesh information
    /// </summary>
    public void ReadMesh()
    {
        Hashtable Hash=new Hashtable();
        List<Vector3> pts = new List<Vector3>();
        if (Mesh != mOldMesh || Transform.rotation != mOldRot || Scale != mOldScale) {
            if (Mesh) {
                int[] tris = Mesh.sharedMesh.triangles;
                Vector3[] verts = Mesh.sharedMesh.vertices;
                for (int i = 0; i < verts.Length; i++)
                    verts[i].Scale(Scale);

                for (int t = 0; t < tris.Length; t += 3) {
                    if (!Hash.ContainsKey(Key(tris[t], tris[t + 1]))) {
                        Hash.Add(Key(tris[t], tris[t + 1]), null);
                        pts.Add(verts[tris[t]]);
                        pts.Add(verts[tris[t + 1]]);
                    }
                    if (!Hash.ContainsKey(Key(tris[t + 1], tris[t + 2]))) {
                        Hash.Add(Key(tris[t + 1], tris[t + 2]), null);
                        pts.Add(verts[tris[t + 1]]);
                        pts.Add(verts[tris[t + 2]]);
                    }
                    if (!Hash.ContainsKey(Key(tris[t + 2], tris[t]))) {
                        Hash.Add(Key(tris[t + 2], tris[t]), null);
                        pts.Add(verts[tris[t + 2]]);
                        pts.Add(verts[tris[t]]);
                    }
                }
            }
            mOldMesh = Mesh;
            mOldRot = Transform.rotation;
            mOldScale = Scale;
            Points = pts.ToArray();
        }
    }

    string Key(int vt1, int vt2)
    {
        if (vt1<vt2)
            return vt1.ToString() + "-" + vt2.ToString();
        else
            return vt2.ToString() + "-" + vt1.ToString();
    }

    public override void Validate()
    {
        base.Validate();
        ReadMesh();
    }

    public override void OnPlay()
    {
        base.OnPlay();
    }

    public override void Reset()
    {
        base.Reset();
        ReadMesh();
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();
        Gizmos.color = GizmoColor1;
        Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, Vector3.one);
        for (int i = 0; i < Points.Length - 1; i += 2) {
            Gizmos.DrawLine(Points[i], Points[i + 1]);
        }

    }

}

