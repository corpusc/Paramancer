// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================

// UNCOMMENT THIS IF YOU WANT YOUR PARTICLES TO GENERATE NORMALS. YOU'LL NEED TO UNCOMMENT THE SAME IN MBEmitter.cs
//#define PARTICLES_USE_NORMALS

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MBLayer : MBObject
{
    #region ### Inspector Fields ###

    public int LayerThreshold = 100;
    public int LayerBlocksize = 20;
    /// <summary>
    /// Determines the rendering order
    /// </summary>
    public int RenderQueue;

    public bool FreezeWhenCulled;

    #endregion

    #region ### Public Properties ###

    /// <summary>
    /// Gets a list of emitters using our layer
    /// </summary>
    public List<MBEmitter> Emitter
    {
        get
        {
            List<MBEmitter> res = new List<MBEmitter>();
            if (ParticleSystem) {
                MBEmitter[] emlist=ParticleSystem.GetComponentsInChildren<MBEmitter>();
                foreach (MBEmitter em in emlist)
                    if (em.Layer == this)
                        res.Add(em);
            }
            return res;
        }
    }

    /// <summary>
    /// Gets or sets the material used by this particle system
    /// </summary>
    /// <remarks>Material's mainTexture will be written by GenerateTextureMap()</remarks>
    public Material Material
    {
        get
        {
            return (mMeshRenderer) ? mMeshRenderer.sharedMaterial : null;
        }
        set
        {
            mMeshRenderer.sharedMaterial = value;
            UpdateMaterial();
            if (RenderQueue == 0 && mMeshRenderer.sharedMaterial != null)
                RenderQueue = mMeshRenderer.sharedMaterial.renderQueue;
        }
    }
    
    /// <summary>
    /// Gets the number of particles currently been rendered by this particle system
    /// </summary>
    public int ParticlesRendered { get; private set; }

    /// <summary>
    /// Gets the maximum number of particles rendered since Play()
    /// </summary>
    /// <remarks>Calling Play() will reset this value. This is useful for determining a good treshold value for your particle system</remarks>
    public int ParticlesRenderedMax { get; private set; }

    #endregion

    #region ### Private Fields ###

    
    MeshFilter mMeshFilter;
    MeshRenderer mMeshRenderer;
    Mesh mMesh;
    Vector3[] mVertices;
    Vector2[] mUVs;
    Color[] mColors;
    int[] mTriangles;

    #if PARTICLES_USE_NORMALS
    Vector3[] mNormals;
    #endif

    int mBufferPtr;

    #endregion

    #region ### Unity Callbacks ###

    void Awake()
    {
        mMeshRenderer = GetComponent<MeshRenderer>();
        mMeshFilter = GetComponent<MeshFilter>();
        mMeshRenderer.castShadows = false;
        mMeshRenderer.receiveShadows = false;
        if (Application.isPlaying) {
            if (!mMeshFilter.mesh) {
                mMeshFilter.mesh = new Mesh();
                mMeshFilter.mesh.name = "Particlesystem";

            }
            mMesh = mMeshFilter.mesh;
        }
        else {
            if (!mMeshFilter.sharedMesh) {
                mMeshFilter.sharedMesh = new Mesh();
                mMeshFilter.sharedMesh.name = "Particlesystem";
            }
        }
        
        mbResetBuffers();
    }

    void Start()
    {
        // Needed to avoid problems with instantiation order
        foreach (MBEmitter em in Emitter)
            em.Layer = this;
    }

    protected override void OnEnable()
    {
         base.OnEnable();
        // Prepare Buffers
        mbResetBuffers();

        UpdateMaterial();

    }

    protected override void OnDisable()
    {
         base.OnDisable();
        if (!Application.isPlaying && mMesh) {
            GameObject.DestroyImmediate(mMesh, true);
        }
    }

    void OnDestroy()
    {
        foreach (MBEmitter em in Emitter)
            em.Layer = null;
    }
    

    #endregion

    #region ### Public Methods ###

    public override void SetParent(MBObject parent)
    {
        base.SetParent(parent);
        UpdateMaterial();
    }

    public void UpdateMaterial()
    {
        if (Material && ParticleSystem) {
            Material.mainTexture = ParticleSystem.TextureAtlas;
        }
    }

    public bool IsVisible()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(ParticleSystem.Camera);
        return GeometryUtility.TestPlanesAABB(planes, mMeshRenderer.bounds);
    }

    #endregion

    #region ### Internal Publics. Don't use them unless you know what you're doing ###
    /*! @name Internal Public
     * Don't use them unless you know what you're doing
     */
    //@{

    public void mbAddToRenderBuffer(ref Vector3[] vertices, ref Vector2[] uvs, ref Color[] colors, ref Vector3[] normals)
    {

        if (mBufferPtr == mVertices.Length)
            mbEnlargeBuffers(LayerBlocksize);

        vertices.CopyTo(mVertices, mBufferPtr);
        uvs.CopyTo(mUVs, mBufferPtr);
        colors.CopyTo(mColors, mBufferPtr);
#if PARTICLES_USE_NORMALS
        normals.CopyTo(mNormals, mBufferPtr);
#endif
        int i = (mBufferPtr / 4) * 6;
        mTriangles[i++] = mBufferPtr;
        mTriangles[i++] = mBufferPtr + 1;
        mTriangles[i++] = mBufferPtr + 2;
        mTriangles[i++] = mBufferPtr + 2;
        mTriangles[i++] = mBufferPtr + 3;
        mTriangles[i++] = mBufferPtr;

        mBufferPtr += 4;
    }
    
    public void mbAddToRenderBuffer(ref Vector3[] vertices, ref Vector2[] uvs, ref Color[] colors)
    {
        if (mBufferPtr == mVertices.Length)
            mbEnlargeBuffers(LayerBlocksize);

        vertices.CopyTo(mVertices, mBufferPtr);
        uvs.CopyTo(mUVs, mBufferPtr);
        colors.CopyTo(mColors, mBufferPtr);

        int i = (mBufferPtr / 4) * 6;
        mTriangles[i++] = mBufferPtr;
        mTriangles[i++] = mBufferPtr + 1;
        mTriangles[i++] = mBufferPtr + 2;
        mTriangles[i++] = mBufferPtr + 2;
        mTriangles[i++] = mBufferPtr + 3;
        mTriangles[i++] = mBufferPtr;

        mBufferPtr += 4;
    }
    

    public void mbOnBeginPlay()
    {
        ParticlesRenderedMax = 0;
        if (Material)
            Material.renderQueue = RenderQueue;
    }

    public void mbRender()
    {
        if (!mMesh) {
            if (!Application.isPlaying) {
                mMesh = new Mesh();
                mMesh.hideFlags = HideFlags.HideAndDontSave;
                mbEditorRender();
            }
            else {
                Debug.LogWarning("Magical Box: Missing mesh!");
                return;
            }
        }
        
        // Buffers are already filled by emitters. Here we just clean up and assign buffers
            ParticlesRendered = mBufferPtr / 4;
            mMesh.Clear();
            if (ParticlesRendered > 0) {
                // zero-out unused mesh areas
                for (int i = mBufferPtr; i < mVertices.Length; i++) {
                    mVertices[i] = mVertices[mBufferPtr - 1];
                }
                if (!Muted) {
                    // feed the mesh
                    mMesh.vertices = mVertices;
                    mMesh.uv = mUVs;
                    mMesh.colors = mColors;
                    mMesh.triangles = mTriangles;
#if PARTICLES_USE_NORMALS
            mMesh.normals = mNormals;
#endif
                }
            }
            
            if (FreezeWhenCulled && !IsVisible()) {
                foreach (MBEmitter em in Emitter)
                    em.mbIsFreezed = true;
            } else
                // prepare for next frame
                mBufferPtr = 0;
        
        ParticlesRenderedMax = Mathf.Max(ParticlesRendered, ParticlesRenderedMax);
    }

    public void mbEditorRender()
    {
        if (mMesh && Material) {
            if (!Material.mainTexture)
                UpdateMaterial();

            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(Transform.position, Transform.rotation, Transform.lossyScale);
            Graphics.DrawMesh(mMesh, matrix, Material, gameObject.layer);

        }
    }

    public void mbResetBuffers()
    {
        mVertices = new Vector3[LayerThreshold * 4];
        mUVs = new Vector2[LayerThreshold * 4];
        mColors = new Color[LayerThreshold * 4];
        mTriangles = new int[LayerThreshold * 6];
#if PARTICLES_USE_NORMALS
        mNormals = new Vector3[LayerThreshold * 4];
#endif
        mBufferPtr = 0;
        ParticlesRendered = 0;
    }

    #endregion

    #region ### Privates ###

    void mbEnlargeBuffers(int quads)
    {

        int newsize = mVertices.Length + quads * 4;

        Vector3[] vt = mVertices;
        mVertices = new Vector3[newsize];
        vt.CopyTo(mVertices, 0);

        Vector2[] uv = mUVs;
        mUVs = new Vector2[newsize];
        uv.CopyTo(mUVs, 0);

        Color[] co = mColors;
        mColors = new Color[newsize];
        co.CopyTo(mColors, 0);
#if PARTICLES_USE_NORMALS
        Vector3[] no = mNormals;
        mNormals = new Vector3[newsize];
        no.CopyTo(mNormals, 0);
#endif
        int[] tris = mTriangles;
        mTriangles = new int[mTriangles.Length + quads * 6];
        tris.CopyTo(mTriangles, 0);
    }

    public override void Purge()
    {
        base.Purge();
        if (mMesh) 
            mMesh.Clear();
        mbResetBuffers();
    }

    #endregion
}
