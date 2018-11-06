using UnityEngine;
using UnityEditor;


public enum GrabType { Joint, VelocityMatch }
public interface Grabbable
{
    RigidbodyConstraints constraintsWhenGrabbed { get; }
    RigidbodyConstraints originalConstraints { get; }
    GrabType grabType { get; }
    Rigidbody thisRigidBody { get; }
    bool grabbed { get; }
    Rigidbody grabber { get; }
    Vector3 anchorPoint { get; }


    bool Grab(GameObject gameObject);
    void UnGrab(GameObject gameObject);
}