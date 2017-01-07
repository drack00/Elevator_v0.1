using UnityEngine;
using System.Collections;

public class SmoothMovement : MonoBehaviour
{
    public float smoothing;

    private Vector3 lastPosition;

    public void Awake()
    {
        lastPosition = transform.position;
    }

	public void Update ()
    {
        transform.position = Vector3.Lerp(lastPosition, transform.position, smoothing * Time.deltaTime);

        lastPosition = transform.position;
    }
}