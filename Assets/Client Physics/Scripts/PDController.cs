using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDController : MonoBehaviour
{
    public float proportionalGain;
    public float derivativeGain;
    public float frequency = 0.9f;
    public float damping = 0.3f;

    Vector3 positionOfDestination;
    Quaternion orientationOfDestination;
    Vector3 velocityOfDestination;

    public Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        proportionalGain = 8f;
        derivativeGain = 1f;

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
        Vector3 F = (Pdes - Pt0) * proportionalGain + (Vdes - Vt0) * derivativeGain;
        rigidbody.AddForce(F);
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
        
        rigidbody.AddTorque(pidv);
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
