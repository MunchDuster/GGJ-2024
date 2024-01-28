using UnityEngine;

public class TransformFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offsetPosition;

    private void LateUpdate()
    {
        transform.position = target.position + offsetPosition;
        transform.rotation = Quaternion.identity;
    }
}
