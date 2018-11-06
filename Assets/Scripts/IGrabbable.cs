using UnityEngine;
using System.Collections;

namespace com.rak.BeatMaker
{

    public enum GrabType { Joint, VelocityMatch }

    public interface IGrabbable
    {
        void Grab(IGrabber grabber);
        void UnGrab();
        bool IsGrabbed();
    }

    public interface IGrabber
    {
        Rigidbody GetRigidBody();
        Vector3 GetJointAnchorPoint();
    }
}

