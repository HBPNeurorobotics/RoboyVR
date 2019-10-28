using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDController : MonoBehaviour
{
    public float proportionalGain;
    public float derivativeGain;

    Vector3 positionOfDestination;
    Quaternion orientationOfDestination;
    Vector3 velocityOfDestination;

    public Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {

    }

    void ForceBackwardsPDController(Vector3 Pdes, Vector3 Vdes)
    {
        float dt = Time.fixedDeltaTime;
        float g = 1 / (1 + derivativeGain * dt + proportionalGain * dt * dt);
        float ksg = proportionalGain * g;
        float kdg = (derivativeGain + proportionalGain * dt) * g;
        Vector3 Pt0 = transform.position;
        Vector3 Vt0 = rigidbody.velocity;
        Vector3 F = (Pdes - Pt0) * ksg + (Vdes - Vt0) * kdg;
        if (float.IsNaN(F.x) || float.IsNaN(F.y) || float.IsNaN(F.z))
        {
            return;
        }
        rigidbody.AddForce(F, ForceMode.Force);
    }


    void TorqueBackwardsPDController()
    {
        Quaternion desiredRotation = orientationOfDestination;
        float dt = Time.fixedDeltaTime;
        Vector3 x;
        float xMag;
        Quaternion q = desiredRotation * Quaternion.Inverse(transform.rotation);
        q.ToAngleAxis(out xMag, out x);
        x.Normalize();
        x *= Mathf.Deg2Rad;
        Vector3 pidv = proportionalGain * x * xMag - derivativeGain * rigidbody.angularVelocity;
        Quaternion rotInertia2World = rigidbody.inertiaTensorRotation * transform.rotation;
        pidv = Quaternion.Inverse(rotInertia2World) * pidv;
        pidv.Scale(rigidbody.inertiaTensor);
        pidv = rotInertia2World * pidv;

        if (float.IsNaN(pidv.x) || float.IsNaN(pidv.y) || float.IsNaN(pidv.z)){
            return;
        }
        rigidbody.AddTorque(pidv, ForceMode.Force);
    }

    public void SetDestination(Transform Pdes, Vector3 Vdes)
    {
        positionOfDestination = Pdes.position;
        orientationOfDestination = Pdes.rotation;
        velocityOfDestination = Vdes;

        ForceBackwardsPDController(positionOfDestination, velocityOfDestination);
        TorqueBackwardsPDController();
    }
}
