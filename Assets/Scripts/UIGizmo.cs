using UnityEngine;
using System.Collections;

public class UIGizmo : MonoBehaviour
{
    public Transform referenceTransform;
    private Vector3 relativePosition;
    private Quaternion relativeRotation;

    void Awake()
    {
        relativePosition = transform.localPosition;
        relativeRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        transform.position = referenceTransform.TransformPoint(relativePosition);
        transform.rotation = referenceTransform.rotation * relativeRotation;
    }
    private void Update ()
    {
        transform.position = referenceTransform.TransformPoint(relativePosition);
        transform.rotation = referenceTransform.rotation * relativeRotation;
    }
}
