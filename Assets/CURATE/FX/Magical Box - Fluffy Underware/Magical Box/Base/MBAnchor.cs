// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Class to create visible hierarchies and emitter pooling within Magical Box
/// </summary>
/// <remarks>
/// Though any GameObject can be used to group emitters (as long as it's inside a ParticleSystem's child hierarchy),
/// Anchors are shown in the editor window and draw their own gizmo icon.
/// They also include pooling manager capabilities for easy emitter pooling
/// </remarks>
public class MBAnchor : MBObject
{

    #region ### Inspector Fields ###
    /// <summary>
    /// Whether this anchor acts as a pooling manager
    /// </summary>
    public bool PoolingEnabled = false;
    /// <summary>
    /// Whether spawned emitters will despawn automatically once they stop playing
    /// </summary>
    public bool AutoDespawn = true;
    /// <summary>
    /// Minimum number of items in the pool
    /// </summary>
    public int MinPoolSize = 1;
    /// <summary>
    /// Maximum number of items int the pool
    /// </summary>
    public int MaxPoolSize = 1;
    /// <summary>
    /// Determines the behaviour when the pool exceeds MaxPoolSize
    /// </summary>
    public MBAnchorPoolExceededMode OnMaxPoolSize = MBAnchorPoolExceededMode.Ignore;
    /// <summary>
    /// The number of items processed at once when the pool need to grow or shrink
    /// </summary>
    public int AllocationBlockSize = 1;
    /// <summary>
    /// Whether the pool should automatically cull items that exceeds MaxPoolSize
    /// </summary>
    public bool AutoCull = true;
    /// <summary>
    /// Time in seconds between culling actions
    /// </summary>
    public float CullingSpeed = 1.0f;

    #endregion

    #region ### Public Properties ###

    /// <summary>
    /// Gets the number of items currently waiting in the pool
    /// </summary>
    public int InStock { get { return mStock.Count; } }
    /// <summary>
    /// Gets the number of items currently spawned from this pool
    /// </summary>
    public int Spawned { get { return mSpawned.Count; } }

    /// <summary>
    /// The Source emitter used for pooling.
    /// </summary>
    /// <remarks>Only available when PoolingEnabled is true</remarks>
    public MBEmitter PoolSource { get { return (PoolingEnabled) ? (MBEmitter)Children[0] : null; } }

    #endregion
    #region ### Private Fields ###

    Stack<MBEmitter> mStock = new Stack<MBEmitter>();
    List<MBEmitter> mSpawned = new List<MBEmitter>();
    float mLastCull;

    #endregion
    #region ### Unity Callbacks ###

    IEnumerator Start()
    {
        if (PoolingEnabled) {
            while (!ParticleSystem.IsInitialized)
                yield return new WaitForEndOfFrame();
            if (PoolSource)
                PreparePool();
        }
    }

    void LateUpdate()
    {
        if (PoolingEnabled) {
            if (AutoCull && InStock > MaxPoolSize && Time.time - mLastCull > CullingSpeed) {
                Cull(true);
                mLastCull = Time.time;
            }
        }
    }

    #endregion

    #region ### Public Methods ###

    /// <summary>
    /// Remove all instances held by the pool
    /// </summary>
    public void ClearPool()
    {
        foreach (MBObject o in mStock)
            o.Destroy();
        foreach (MBObject o in mSpawned)
            o.Destroy();
        mStock.Clear();
        mSpawned.Clear();
    }

    /// <summary>
    /// Shrink the stock to match MaxPoolSize
    /// </summary>
    public void Cull() { Cull(false); }

    /// <summary>
    /// Shrink the stock to match MaxPoolSize
    /// </summary>
    /// <param name="smartCull">if true a maximum of AllocationBlockSize items are culled</param>
    public void Cull(bool smartCull)
    {
        int toCull = (smartCull) ? Mathf.Min(AllocationBlockSize, mStock.Count - MaxPoolSize) : mStock.Count - MaxPoolSize;

        while (toCull-- > 0)
            mStock.Pop().Destroy();
    }

    /// <summary>
    /// Despawn an item and add it back to the stock
    /// </summary>
    /// <param name="item"></param>
    public void Despawn(MBEmitter item)
    {
        if (!PoolingEnabled || !PoolSource) return;
        if (IsSpawned(item)) {
#if UNITY_4_0
            item.gameObject.SetActive(false);
#else
            item.gameObject.active = false;
#endif
            item.EmitterStopsPlaying -= callbackAutoDespawn;
            item.name = PoolSource.name + "_stock";
            item.Transform.position = Vector3.zero;
            mSpawned.Remove(item);
            mStock.Push(item);
        }
    }
    /// <summary>
    /// Despawn all items currently spawned from this pool
    /// </summary>
    public void DespawnAll()
    {
        while (mSpawned.Count > 0)
            Despawn(mSpawned[0]);
    }

    /// <summary>
    /// Whether an item is managed by this pool
    /// </summary>
    /// <param name="item">an item</param>
    /// <returns>true if item is managed by this pool</returns>
    public bool IsManaged(MBEmitter item)
    {
        if (mSpawned.Contains(item) || mStock.Contains(item))
            return true;
        else
            return false;
    }

    /// <summary>
    /// Whether an item is spawned by this pool
    /// </summary>
    /// <param name="item">an item</param>
    /// <returns>true if the item is spawned by this pool</returns>
    public bool IsSpawned(MBEmitter item)
    {
        return mSpawned.Contains(item);
    }

    /// <summary>
    /// Clear all instances and repopulate the pool to match MinPoolSize
    /// </summary>
    public void PreparePool()
    {
        ClearPool();
        Populate(MinPoolSize);
    }

    /// <summary>
    /// Spawn an instance, make it active and add it to the Spawned list
    /// </summary>
    /// <returns>the instance spawned</returns>
    public MBEmitter Spawn()
    {
        if (!PoolingEnabled || !PoolSource) return null;
        MBEmitter item = null;
        // if we ran out of objects, create some
        if (InStock == 0) {
            if (Spawned < MaxPoolSize || OnMaxPoolSize == MBAnchorPoolExceededMode.Ignore)
                Populate(AllocationBlockSize);
        }
        // maybe we've got objects ready now
        if (InStock > 0) 
            item = mStock.Pop();
        // or maybe we want to reuse the oldest spawned item
        else if (OnMaxPoolSize == MBAnchorPoolExceededMode.Reuse) {
            item = mSpawned[0];
            mSpawned.RemoveAt(0);
        }

        if (item != null) {
            mSpawned.Add(item);
#if UNITY_4_0
            item.gameObject.SetActive(true);
#else
            item.gameObject.active = true;
#endif
            item.mbReloadHierarchy();
            item.name = PoolSource.name + "_clone";
            item.Transform.localPosition = Vector3.zero;
            if (AutoDespawn)
                item.EmitterStopsPlaying += new MBEventHandler(callbackAutoDespawn);
        }
        return item;
    }

    #endregion

    #region ### Privates ###

    void callbackAutoDespawn(MBEvent e)
    {
        Despawn((MBEmitter)e.Context);
    }

    /// <summary>
    /// Create instances and add them to the stock
    /// </summary>
    void Populate(int no)
    {
        if (!PoolingEnabled || !PoolSource) return;
        while (no-- > 0) {
            MBEmitter o = ParticleSystem.AddEmitter(PoolSource, this);
#if UNITY_4_0
            o.gameObject.SetActive(false);
#else
            o.gameObject.active = false;
#endif
            o.Transform.position = Transform.position;
            o.name = PoolSource.name + "_stock";
            o.mbIsPooled = true;
            mStock.Push(o);
        }
    }

    #endregion


    [System.Obsolete("MBAnchor.AddEmitter is obsolete. Use MBParticleSystem.AddEmitter (parent) instead!")]
    public MBEmitter AddEmitter() 
    {
        return ParticleSystem.AddEmitter(this); 
    }


    [System.Obsolete("MBAnchor.AddEmitter is obsolete. Use MBParticleSystem.AddEmitter (source,parent) instead!")]
    public MBEmitter AddEmitter(MBEmitter source)
    {
        return ParticleSystem.AddEmitter(source, this); 
    }

}

/// <summary>
/// Determining reaction when MaxPoolSize is exceeded
/// </summary>
[System.Serializable]
public enum MBAnchorPoolExceededMode : int
{
    /// <summary>
    /// MaxPoolSize will be ignored
    /// </summary>
    Ignore = 0,
    /// <summary>
    /// Spawning will fail when MaxPoolSize is exceeded
    /// </summary>
    StopSpawning = 1,
    /// <summary>
    /// Already spawned items will be returned when MaxPoolSize is exceeded
    /// </summary>
    Reuse = 2
}



