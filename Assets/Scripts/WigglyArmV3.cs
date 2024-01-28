using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using static UnityEngine.Rendering.CoreUtils;

/// <summary>
/// The arm is made up of small 'sections' hinged together (hand at one end, shoulder at another
/// </summary>
public class WigglyArmV3 : MonoBehaviour, IPunObservable
{
    public float sectionLength = 0.2f;
    public float armPhysicalLength = 5;

    [SerializeField] private GameObject sectionPrefab;
    [SerializeField] private Transform player;
    public Rigidbody2D hand;
    [SerializeField] private Rigidbody2D shoulder;
    [SerializeField] private LineRenderer lineRenderer;

    public PhotonView photonView;


    Vector3[] points; // For the line renderer
    public List<Rigidbody2D> arm = new();
    public List<Rigidbody2D> extension = new();

    private int armItems;
    private void Start()
    {
        armItems = Mathf.CeilToInt(armPhysicalLength / sectionLength);
        SetupArm();
        InitLineRenderer();
    }

    private void InitLineRenderer()
    {
        points = new Vector3[armItems + 2/*for the hand*/];
        lineRenderer.positionCount = armItems;
    }

    private bool IsMine()
    {
        return photonView.IsMine || !PhotonNetwork.IsConnected;
    }

    private void Update()
    {
        UpdateLineRenderer();
    }

    public void UpdateLineRenderer()
    {
        for (int i = 0; i < armItems; i++)
            points[i] = arm[i].transform.position;


        //Hand points
        points[armItems] = hand.transform.position;
        points[armItems + 1] = hand.transform.position + hand.transform.up * sectionLength;

        lineRenderer.positionCount = armItems + 2;
        lineRenderer.SetPositions(points);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            for (int i = 0; i < armItems; i++)
                stream.SendNext(arm[i].position);
            stream.SendNext(hand.position);
        }
        else
        {
            for (int i = 0; i < armItems; i++)
                arm[i].position = (Vector2)stream.ReceiveNext();
            
            hand.position = (Vector2)stream.ReceiveNext(); 

            UpdateLineRenderer();
        }
    }

    public void SetupArm()
    {
        Debug.Log($"Making {armItems} sections");

        Rigidbody2D last = shoulder;
        for (int i = 0; i < armItems; i++)
        {
            Debug.Log($"Calcing item after position at {last.position}");
            last = CreateAndLinkSection(arm, last, CalcStartPosition(last));
            Debug.Log($"New item is position at {last.position}");
        }
        LinkHand(last, CalcStartPosition(last));
    }

    private Vector2 CalcStartPosition(Rigidbody2D last)
    {
        return (Vector2)last.transform.position + (Vector2)player.up * sectionLength;
    }

    public Vector2 GetTargetHandPos()
    {
        return (Vector2)(arm.Last().transform.position + hand.transform.up * sectionLength);
    }

    private void LinkHand(Rigidbody2D last, Vector2 position)
    {
        hand.transform.position = position;
        LinkSection(last, hand);
    }

    private Rigidbody2D CreateAndLinkSection(List<Rigidbody2D> list, Rigidbody2D last, Vector2 position)
    {
        var next = CreateSection(list, position);
        LinkSection(last, next);
        return next;
    }

    private Rigidbody2D CreateSection(List<Rigidbody2D> list, Vector2 position)
    {
        Debug.Log($"Add section at position {position}");
        GameObject newSection = Instantiate(sectionPrefab, transform);
        newSection.transform.position = position;

        newSection.name = (list == arm ? "Arm " : "Extension ") + list.Count.ToString();

        Rigidbody2D newRigidbody = newSection.GetComponent<Rigidbody2D>();
        list.Add(newRigidbody);
        newRigidbody.drag = 0.0f;
        Debug.Log($"Rigidbody has position {newRigidbody.transform.position}");
        return newRigidbody;
    }

    public void LinkSection(Rigidbody2D first, Rigidbody2D second) {
        Debug.Log($"Link section at {first.position} to section at {second.position}");
        // Connecting this joint to the last
        HingeJoint2D joint = second.GetComponent<HingeJoint2D>();
        joint.connectedBody = first;
        joint.useLimits = false;
        joint.useMotor = false;
    }

    private void DestroySection(Rigidbody2D section)
    {
        Destroy(section.gameObject);
    }


    private void OnDrawGizmos()
    {
        if (arm.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < armItems; i++)
                Gizmos.DrawWireSphere(arm[i].transform.position, 0.2f);
            Gizmos.color = Color.red;
        }
    }

}