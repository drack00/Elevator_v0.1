using UnityEngine;
using System.Collections;

public class UIGizmo : MonoBehaviour
{
    public Transform referenceTransform;
    public Vector3 relativePosition;
    public Vector3 relativeRotation;
    public bool restrictY = false;

    private void OnEnable()
    {
        transform.position = !restrictY ?
            referenceTransform.TransformPoint(relativePosition) :
            referenceTransform.TransformPoint(new Vector3(relativePosition.x, transform.position.y, relativePosition.z));
        transform.rotation = !restrictY ?
            referenceTransform.rotation * Quaternion.Euler(relativeRotation) :
            referenceTransform.rotation * Quaternion.Euler(new Vector3(0f, relativeRotation.y, 0f));
    }
    private void Update ()
    {
        transform.position = !restrictY ? 
            referenceTransform.TransformPoint(relativePosition) : 
            referenceTransform.TransformPoint(new Vector3(relativePosition.x, transform.position.y, relativePosition.z));
        transform.rotation = !restrictY ? 
            referenceTransform.rotation * Quaternion.Euler(relativeRotation) :
            referenceTransform.rotation * Quaternion.Euler(new Vector3(0f, relativeRotation.y, 0f));
	}
}
