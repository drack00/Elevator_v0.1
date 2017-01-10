using UnityEngine;
using System.Collections;

public class SyncPosition : MonoBehaviour
{
    private Vector3 defaultPosition;
    private Vector3 defaultRotation;
    public Transform syncTransform;
    public Vector3 syncPosition;
    public Vector3 syncRotation;

    void Awake ()
    {
        defaultPosition = transform.localPosition;
        defaultRotation = transform.localEulerAngles;
	}
	void Update ()
    {
        transform.position = syncTransform.TransformPoint(syncPosition);
        transform.eulerAngles = syncTransform.eulerAngles + syncRotation;
	}
    void OnDisable()
    {
        transform.localPosition = defaultPosition;
        transform.localEulerAngles = defaultRotation;
    }
}
