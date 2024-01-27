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
public class WigglyArmV2 : MonoBehaviour, IPunObservable
{
    public float sectionLength = 0.2f;
    public float armPhysicalLength = 5;

    [SerializeField] private float maxExtensionPhysicalLength = 15;

    [SerializeField] private GameObject sectionPrefab;
    [SerializeField] private Transform player;
    public Rigidbody2D hand;
    [SerializeField] private Rigidbody2D shoulder;
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private PhotonView photonView;

    
    Vector3[] points; // For the line renderer
    public List<Rigidbody2D> arm = new();
    public List<Rigidbody2D> extension = new();

    private int armItems;
    public int maxExtensionItems;
    public int actualExtensionItems = 0;

    private void Start()
    {
        armItems = Mathf.CeilToInt(armPhysicalLength / sectionLength);
        maxExtensionItems = Mathf.CeilToInt(maxExtensionPhysicalLength / sectionLength);
        SetupArm();
        InitLineRenderer();
    }

    private void InitLineRenderer()
    {
        points = new Vector3[armItems + maxExtensionItems + 2/*for the hand*/ + 1 /* trailing zero */];
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

        for(int i = 0; i < actualExtensionItems; i++)
            points[i + armItems] = extension[i].transform.position;

        int totalLength = armItems + actualExtensionItems;

        //Hand points
        points[totalLength] = hand.transform.position;
        points[totalLength + 1] = hand.transform.position + hand.transform.up * sectionLength;
        points[totalLength + 2] = Vector3.zero;

        if (actualExtensionItems > 0)
        {
            Debug.Log($"Line renderer has points {points}");
        }

        lineRenderer.positionCount = totalLength + 2;
        lineRenderer.SetPositions(points);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(arm.Select(e => e.position).ToArray());
            stream.SendNext(actualExtensionItems);
            stream.SendNext(extension.Take(actualExtensionItems).Select(e => e.position));
            stream.SendNext(hand.position);
        }
        else
        {
            var armPositions = (Vector2[])stream.ReceiveNext();
         
            for (int i = 0; i < armPositions.Length; i++)
                arm[i].position = armPositions[i];

            var newExtensionItems = (int)stream.ReceiveNext();
            var extensionPositions = (Vector2[])stream.ReceiveNext();
            var handPos = (Vector2)stream.ReceiveNext(); 
            UpdateExtensionLinkage(newExtensionItems, extensionPositions, handPos);

            UpdateLineRenderer();
        }
    }

    public void UpdateExtensionLinkage(int newItems, Vector2[] positions, Vector2 handPos)
    {
        Debug.Log($"Updating extension length to {newItems}");

        int currentItems = Math.Min(newItems, actualExtensionItems);
        for(int i = 0; i < currentItems; i++)
        {
            extension[i].position = positions[i];
        }

        Rigidbody2D last = currentItems == 0 ? arm.Last() : extension[currentItems - 1];
        if ( currentItems < newItems )
        {
            for (int i = currentItems; i < newItems; i++)
            {
                last = CreateAndLinkSection(extension, last, positions[i]);
                last.drag = 1.0f; // Add drag for extension to make it less sensitive
            }
        } 
        else if( currentItems < actualExtensionItems )
        {
            for(int i = currentItems; i < actualExtensionItems; i++)
            {
                DestroySection(extension[i]);
                extension[i] = null;
            }
            extension.RemoveRange(currentItems, actualExtensionItems - currentItems);
        }
        LinkHand(last, handPos);
        actualExtensionItems = newItems;
    }

    public void SetupArm()
    {
        if(!IsMine())
        {
            Debug.LogError("Set length on remote controlled character");
            return;
        }

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
            for (int i = 0; i < armPhysicalLength; i++)
                Gizmos.DrawWireSphere(arm[i].transform.position, 0.2f);
            Gizmos.color = Color.red;
            for (int i = 0; i < actualExtensionItems; i++)
                Gizmos.DrawWireSphere(extension[i].transform.position, 0.2f);
        }
    }

}