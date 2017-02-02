using UnityEngine;
using System.Collections;

public class UIGizmo : MonoBehaviour
{
    public Transform referenceTransform;
    public Vector3 relativePosition;
    public Vector3 relativeRotation;

    private void OnEnable()
    {
        transform.position = referenceTransform.TransformPoint(relativePosition);
        transform.rotation = referenceTransform.rotation * Quaternion.Euler(relativeRotation);
    }
    private void Update ()
    {
        transform.position = referenceTransform.TransformPoint(relativePosition);
        transform.rotation = referenceTransform.rotation * Quaternion.Euler(relativeRotation);
    }
}
