using UnityEngine;
using System.Collections.Generic;

public class Marker : MonoBehaviour
{
    protected Rigidbody mRigidbody;
    private Vector3 startPosition;
    private Quaternion starRotation;

    [SerializeField]
    private Color color;

    [SerializeField]
    private MeshRenderer[] colouredParts;


    [SerializeField]
    private Painter painter;

    [SerializeField]
    private PaintReceiver paintReceiver;

    protected void Awake()
    {
        mRigidbody = GetComponent<Rigidbody>();

        startPosition = mRigidbody.position;
        starRotation = mRigidbody.rotation;

        foreach (MeshRenderer renderer in colouredParts)
        {
            renderer.material.color = color;
        }

        painter.Initialize(paintReceiver);
        painter.ChangeColour(color);
    }
}
