// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;


/// <summary>
/// Provides a mesh face emitter type
/// </summary>
[MBEmitterTypeInfo(Menu = "Mesh/Surface")]
public class MBMeshSurfaceEmitter : MBEmitterType
{
    #region ### Inspector Fields ###
    public MeshFilter Mesh;
    
    public bool Ordered;
    public bool UseCenter;
    public MBTriangle[] Triangles = new MBTriangle[0];

    #endregion
    
    Quaternion mOldRot;
    Vector3 mOldScale;
    MeshFilter mOldMesh;

    int mLastTriID;

    public override Vector3 GetPosition(MBParticle PT)
    {
        if (Triangles.Length == 0) return Vector3.zero;
        if (Ordered) {
            if (++mLastTriID >= Triangles.Length)
                mLastTriID = 0;
        }
        else
            mLastTriID = Random.Range(0, Triangles.Length - 1);

        return (UseCenter) ? Triangles[mLastTriID].Center : Triangles[mLastTriID].RandomInside();
    }

    public override Vector3 GetHeading(MBParticle PT)
    {
        if (Heading == MBEmitterTypeHeading.MeshNormal) {
                int dir = InverseHeading ? -1 : 1;
                return Triangles[mLastTriID].Normal*dir;
        }
            else
                return base.GetHeading(PT);
    }


    /// <summary>
    /// Reads all necessary mesh information
    /// </summary>
    public void ReadMesh()
    {
        if (Mesh != mOldMesh || Transform.rotation != mOldRot || Scale != mOldScale) {

            if (Mesh) {
                Vector3[] verts=Mesh.sharedMesh.vertices;
                int[] tris = Mesh.sharedMesh.triangles;

                for (int i = 0; i < verts.Length; i++)
                    verts[i].Scale(Scale);
                Triangles = new MBTriangle[tris.Length / 3];
                int t = 0;
                for (int i = 0; i < tris.Length; i += 3) {
                    Triangles[t++] = new MBTriangle(verts[tris[i]], verts[tris[i + 1]], verts[tris[i + 2]]);
                }
            }
            else 
                Triangles = new MBTriangle[0];

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
        UseCenter = false;
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();   
        Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, Vector3.one);
                
        for (int i = 0; i < Triangles.Length; i++) {
            Gizmos.color = GizmoColor1;
            MBUtility.DrawGizmoPolygon(new Vector3[4] { Triangles[i].P0, Triangles[i].P1, Triangles[i].P2, Triangles[i].P0 });
            Gizmos.color = GizmoColor2;
            if (UseCenter)
                Gizmos.DrawCube(Triangles[i].Center, new Vector3(0.1f, 0.1f, 0.1f));
            if (Heading == MBEmitterTypeHeading.MeshNormal) {
                int dir = InverseHeading ? -1 : 1;
                Gizmos.DrawLine(Triangles[i].Center, Triangles[i].Center + Triangles[i].Normal * dir);
            }
        }


    }

}

[System.Serializable]
public class MBTriangle
{
    public Vector3 P0;
    public Vector3 P1;
    public Vector3 P2;
    public Vector3 Center;
    public Vector3 Normal;

    public MBTriangle(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        P0 = p0;
        P1 = p1;
        P2 = p2;
        Center=(P0+P1+P2)/3;
        Vector3 U = P1 - P0;
        Vector3 V = P2 - P0;
        Normal = new Vector3(U.y * V.z - U.z * V.y,
                           U.z * V.x - U.x * V.z,
                           U.x * V.y - U.y * V.x).normalized;
    }

    public Vector3 RandomInside()
    {
        float b0 = Random.value;
        float b1 = (1f - b0) * Random.value;
        float b2 = 1 - b0 - b1;
        return P0 * b0 + P1 * b1 + P2 * b2;
    }

}

