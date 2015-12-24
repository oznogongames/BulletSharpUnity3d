﻿using System;
using UnityEngine;
using System.Collections;
using BulletSharp;

public class BPhysicsWorld : MonoBehaviour, IDisposable
{
    protected static BPhysicsWorld singleton;
    protected static bool _isDisposed = false;

    public static BPhysicsWorld Get() {
        if (singleton == null && !_isDisposed) {
            BPhysicsWorld[] ws = FindObjectsOfType<BPhysicsWorld>();
            if (ws.Length == 1) {
                singleton = ws[0];
            } else if (ws.Length == 0) {
                Debug.LogError("Need to add a dynamics world to the scene");
            } else {
                Debug.LogError("Found more than one dynamics world.");
                singleton = ws[0];
                for (int i = 1; i < ws.Length; i++) {
                    GameObject.Destroy(ws[i].gameObject);
                }
            }
        }
        if (singleton.World == null && !singleton.isDisposed) singleton._InitializePhysicsWorld();
        return singleton;
    }

    public DynamicsWorld World;

    protected int _frameCount;
    public int frameCount
    {
        get
        {
            return _frameCount;
        }
    }

    protected DebugDrawModes _debugDrawMode = DebugDrawModes.DrawWireframe;
    public DebugDrawModes DebugDrawMode {
        get { return _debugDrawMode; }
        set {
            _debugDrawMode = value;
            if (_doDebugDraw && World != null && World.DebugDrawer != null) {
                World.DebugDrawer.DebugMode = value;
            }
        }
    }

    protected bool _doDebugDraw = false;
    public bool DoDebugDraw {
        get { return _doDebugDraw; }
        set {
            if (_doDebugDraw != value && World != null) {
                if (value == true) {
                    BulletSharpExamples.DebugDrawUnity db = new BulletSharpExamples.DebugDrawUnity();
                    db.DebugMode = DebugDrawModes.DrawWireframe;
                    World.DebugDrawer = db;
                } else {
                    IDebugDraw db = World.DebugDrawer;
                    if (db != null && db is IDisposable) {
                        ((IDisposable)db).Dispose();
                    }
                    World.DebugDrawer = null;
                }
            }
            _doDebugDraw = value;
        }
    }

    public void OnDrawGizmos() {
        if (_doDebugDraw && World != null) {
            World.DebugDrawWorld();
        }
    }

    //It is critical that Awake be called before any other scripts call BPhysicsWorld.Get()
    //Set this script and any derived classes very early in script execution order.
    protected virtual void Awake() {
        _frameCount = 0;
        _isDisposed = false;
        singleton = BPhysicsWorld.Get();
    }

    protected virtual void FixedUpdate() {
        _frameCount++;
        World.StepSimulation(UnityEngine.Time.fixedTime);

        //collisions
        /*
        int numManifolds = World.Dispatcher.NumManifolds;
        for (int i = 0; i < numManifolds; i++)
        {
            PersistentManifold contactManifold = World.Dispatcher.GetManifoldByIndexInternal(i);
            CollisionObject a = contactManifold.Body0;
            CollisionObject b = contactManifold.Body1;
            Debug.LogFormat("Collision between {0},{1} numContacts={2}",a,b,contactManifold.NumContacts);
        }
        */
    }

    protected virtual void OnDestroy() {
        Debug.Log("Destroying Physics World");
        Dispose(false);
    }

    public bool isDisposed {
        get { return _isDisposed; }
    }

    protected virtual void _InitializePhysicsWorld() {
        _isDisposed = false;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        _isDisposed = true;
    }

    public bool AddRigidBody(BRigidBody rb) {
        if (!_isDisposed) {
            Debug.LogFormat("Adding {0} to world",rb);
            if (rb._BuildRigidBody())
            {
                World.AddRigidBody(rb.GetRigidBody());
            }
            return true;
        }
        return false;
    }

    public void RemoveRigidBody(BulletSharp.RigidBody rb) {
        if (!_isDisposed) {
            Debug.LogFormat("Removing {0} from world", rb);
            World.RemoveRigidBody(rb);
        }
    }
}
