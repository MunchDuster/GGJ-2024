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
public class WigglyArm : MonoBehaviour, IPunObservable
{
    [SerializeField] private float sectionLength = 0.2f;
    [SerializeField] private float armLength = 5;
    [SerializeField] private GameObject sectionPrefab;
    [SerializeField] private Transform player;
    public Rigidbody2D hand;
    [SerializeField] private Rigidbody2D shoulder;
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private PhotonView photonView;

    
    Vector3[] points; // For the line renderer
    List<Rigidbody2D> sections = new();

    private void Start()
    {
        SetLength(armLength);
    }

    public float GetLength() => armLength;

    private void InitLineRenderer()
    {
        points = new Vector3[sections.Count + 2/*for the hand*/];
        lineRenderer.positionCount = points.Length;
    }

    private bool IsMine()
    {
        return photonView.IsMine || !PhotonNetwork.IsConnected;
    }

    private void Update()
    {
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        for (int i = 0; i < sections.Count; i++)
            points[i] = sections[i].position;

        //Hand points
        points[points.Length - 2] = hand.position;
        points[points.Length - 1] = hand.position + (Vector2)hand.transform.up * 0.2f;

        lineRenderer.SetPositions(points);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(sections.Select(section => section.position).ToArray());
        }
        else
        {
            var sectionPositions = (Vector2[])stream.ReceiveNext();

            if (sectionPositions.Length != sections.Count)
                AdjustSectionCount(sectionPositions); // Add/Remove missing sections and update line renderer

            for (int i = 0; i < sectionPositions.Length; i++)
                sections[i].position = sectionPositions[i];

            UpdateLineRenderer();
        }
    }

    private void AdjustSectionCount(Vector2[] sectionPositions)
    {
        if (sectionPositions.Length > sections.Count)
            for (int i = 0; i < sectionPositions.Length - sections.Count; i++)
                CreateSection();
        else
            for (int i = 0; i < sections.Count - sectionPositions.Length; i++)
                DeleteEndSection();

        JoinHand();
        InitLineRenderer();
    }

    public void SetLength(float length)
    {
        if(!IsMine())
        {
            Debug.LogError("Set length on remote controlled character");
            return;
        }

        ClearSections();
        PopulateSections(Mathf.RoundToInt(length / sectionLength));
        JoinHand();
        InitLineRenderer();
    }

    private void ClearSections()
    {
        int originalCount = sections.Count;
        for (int i = 0; i < originalCount; i++)
            DeleteEndSection();
    }

    private void DeleteEndSection()
    {
        Destroy(sections[sections.Count - 1].gameObject);
        sections.RemoveAt(sections.Count - 1);
    }

    private void PopulateSections(int count)
    {
        Debug.Log($"Making {count} sections");

        GameObject firstSection = Instantiate(sectionPrefab, transform);
        sections.Add(firstSection.GetComponent<Rigidbody2D>());

        HingeJoint2D joint = firstSection.GetComponent<HingeJoint2D>();
        joint.connectedBody = shoulder;

        for (int i = 1; i < count; i++)
            CreateSection();
    }

    private void CreateSection()
    {
        GameObject newSection = Instantiate(sectionPrefab, transform);
        sections.Add(newSection.GetComponent<Rigidbody2D>());

        // Connecting this joint to the last
        if (sections.Count < 2)
            return;

        HingeJoint2D joint = newSection.GetComponent<HingeJoint2D>();
        joint.connectedBody = sections[sections.Count - 2];
        newSection.transform.position = sections[sections.Count - 2].position + (Vector2)player.up * sectionLength;
    }

    private void JoinHand()
    {
        hand.transform.position = sections[sections.Count - 1].position + Vector2.up * sectionLength;
        hand.GetComponent<HingeJoint2D>().connectedBody = sections[sections.Count - 1];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < sections.Count; i++)
            Gizmos.DrawWireSphere(sections[i].transform.position, 0.2f);
        Gizmos.color = Color.red;
    }
}