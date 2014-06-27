// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Provides a mesh vertex emitter type
/// </summary>
[MBEmitterTypeInfo(Menu = "Mesh/Vertex")]
public class MBMeshVertexEmitter : MBEmitterType
{
    #region ### Inspector Fields ###
    public MeshFilter Mesh;
    public Vector3[] Points = new Vector3[0];
    public Vector3[] Normals = new Vector3[0];
    public bool Ordered;

    #endregion

    Quaternion mOldRot;
    Vector3 mOldScale;
    MeshFilter mOldMesh;

    int mLastVertexID;

    public override Vector3 GetPosition(MBParticle PT)
    {
        if (Points.Length == 0) return Vector3.zero;
        if (Ordered) {
            if (++mLastVertexID >= Points.Length)
                mLastVertexID = 0;
        }
        else
            mLastVertexID = Random.Range(0, Points.Length - 1);

        return Points[mLastVertexID];
    }

    public override Vector3 GetHeading(MBParticle PT)
    {
        if (Heading == MBEmitterTypeHeading.MeshNormal) {
            if (Normals.Length > 0) {
                int dir = InverseHeading ? -1 : 1;
                return Normals[mLastVertexID] * dir;
            }
            else
                return Vector3.zero;
        } else
            return base.GetHeading(PT);
    }


    /// <summary>
    /// Reads all necessary mesh information
    /// </summary>
    public void ReadMesh()
    {
        if (Mesh != mOldMesh || Transform.rotation != mOldRot || Scale != mOldScale) {

            if (Mesh) {
                Points = Mesh.sharedMesh.vertices;
                Normals = Mesh.sharedMesh.normals;
                
                for (int i = 0; i < Points.Length; i++)
                    Points[i].Scale(Scale);
            }
            else {
                Points = new Vector3[0];
                Normals = new Vector3[0];
            }

            mOldMesh = Mesh;
            mOldRot = Transform.rotation;
            mOldScale = Scale;
        }
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
        Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, Vector3.one);
        for (int i = 0; i < Points.Length - 1; i++) {
            Gizmos.color = GizmoColor1;
            Gizmos.DrawCube(Points[i], new Vector3(0.1f, 0.1f, 0.1f));
            if (Normals.Length > 0 && Heading==MBEmitterTypeHeading.MeshNormal) {
                Gizmos.color = GizmoColor2;
                int dir = InverseHeading ? -1 : 1;
                Gizmos.DrawLine(Points[i], Points[i] + Normals[i]*dir);
            }
        }

    }

}

