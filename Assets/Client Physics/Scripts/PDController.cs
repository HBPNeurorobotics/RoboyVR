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
    public Vector3 oldVelocity;
    Vector3 integral = Vector3.zero;
    Vector3 previousIntegral = Vector3.zero;
    int integralReset = 50;
    int integralCounter = 0;
    Vector3 oldError = Vector3.zero;

    public Rigidbody rigidbody;

    public float kp = 0.5f;
    public float ki = 0.1f;
    public float kd = 0.2f;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {

    }

    Vector3 CalculatePIDError(Vector3 error)
    {
        float dt = Time.fixedDeltaTime;
        Vector3 derivative = (error - oldError) / dt;
        previousIntegral = integral;
        integral += error * dt;

        integralCounter++;
        if (integralCounter > integralReset)
        {
            integral -= previousIntegral;
        }
        oldError = error;
        return kp * error + ki * integral + kd * derivative;
    }


    void ForceBackwardsPDController(Vector3 targetPosition, Vector3 targetVelocity)
    {
        float dt = Time.fixedDeltaTime;
        float g = 1 / (1 + derivativeGain * dt + proportionalGain * dt * dt);
        float ksg = proportionalGain * g;
        float kdg = (derivativeGain + proportionalGain * dt) * g;
        Vector3 currentPosition = transform.position;
        Vector3 currentVelocity = rigidbody.velocity;
        //Euler Step
        //Vector3 F = CalculatePIDError(targetPosition - currentPosition) + CalculatePIDError(targetPosition - currentVelocity);
        Vector3 F = (targetPosition - currentPosition) * ksg + (targetVelocity - currentVelocity) * kdg;
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
