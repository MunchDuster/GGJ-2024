using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static UnityEngine.ParticleSystem;

/// <summary>
/// The arm is made up of small 'sections' hinged together (hand at one end, shoulder at another
/// </summary>
public class WigglyArm : MonoBehaviour
{
    [SerializeField] private float torque = 5;
    [SerializeField] private float maxTorque = 5;
    [SerializeField] private float sectionLength = 0.2f;
    [SerializeField] private float armLength = 5;
    [SerializeField] private GameObject sectionPrefab;
    [SerializeField] private Rigidbody2D hand;
    [SerializeField] private Rigidbody2D shoulder;
    [SerializeField] private LineRenderer lineRenderer;

    Vector3[] points; // For the line renderer
    List<Rigidbody2D> sections = new();

    private void Awake()
    {
        SetLength(armLength);
    }

    private void InitLineRenderer()
    {
        points = new Vector3[sections.Count + 1/*for the hand*/];
        lineRenderer.positionCount = points.Length;
    }

    private void Update()
    {
        for (int i = 0; i < sections.Count; i++)
            points[i] = sections[i].position;

        points[points.Length - 1] = hand.position;

        lineRenderer.SetPositions(points);
    }

    public void SetLength(float length)
    {
        ClearSections();
        PopulateSections(Mathf.RoundToInt(length / sectionLength));
        JoinHand();
        InitLineRenderer();
    }

    private void ClearSections()
    {
        int originalCount = sections.Count;
        for (int i = 0; i < originalCount; i++)
        {
            Destroy(sections[0]);
            sections.RemoveAt(0);
        }
    }

    private void PopulateSections(int count)
    {
        Debug.Log($"Making {count} sections");

        GameObject firstSection = Instantiate(sectionPrefab, transform);
        sections.Add(firstSection.GetComponent<Rigidbody2D>());

        HingeJoint2D joint = firstSection.GetComponent<HingeJoint2D>();
        joint.connectedBody = shoulder;

        for (int i = 1; i < count; i++)
        {
            GameObject newSection = Instantiate(sectionPrefab, transform);
            sections.Add(newSection.GetComponent<Rigidbody2D>());

            // Connecting this joint to the last

            joint = newSection.GetComponent<HingeJoint2D>();
            joint.connectedBody = sections[i - 1];
            newSection.transform.position = sections[i - 1].position + Vector2.up * sectionLength;
        }
    }

    private void JoinHand()
    {
        hand.transform.position = sections[sections.Count - 1].position + Vector2.up * sectionLength;
        hand.GetComponent<HingeJoint2D>().connectedBody = sections[sections.Count - 1];
    }

    //private void FixedUpdate()
    //{
    //    //// Rotate each joint to point towards the mouse
    //    //for (int i = 0; i < joints.Length; i++)
    //    //{
    //    //    // if (i > 0) return;
    //    //    Vector3 directionToMouse3d = targetPosition - joints[i].transform.position;
    //    //    directionToMouse3d.z = 0;// Prevents normalization issues

    //    //    float angleError = Vector3.SignedAngle(directionToMouse3d.normalized, joints[i].transform.up, Vector3.forward);
    //    //    //float angleError = GetAngleDifference(targetAngle, currentAngle);
    //    //    float clampedTorque = Mathf.Min(maxTorque, Mathf.Abs(angleError * torque * Time.fixedDeltaTime)) * Mathf.Sign(angleError);
    //    //    SetMotorSpeed(joints[i], clampedTorque);
    //    //}
    //}

    /// <summary>
    /// Because unity sucks sometimes with setting variables
    /// </summary>
    private void SetMotorSpeed(HingeJoint2D joint, float speed)
    {
        JointMotor2D motor = joint.motor;
        motor.motorSpeed = speed;
        joint.motor = motor;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < sections.Count; i++)
            Gizmos.DrawWireSphere(sections[i].transform.position, 0.2f);
        Gizmos.color = Color.red;
    }

    private Vector3 VectorFromAngle(float angle)
        => new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
}