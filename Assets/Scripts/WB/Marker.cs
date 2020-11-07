using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Painter))]
public class Marker : MonoBehaviour
{
    protected Rigidbody mRigidbody;
    private Vector3 startPosition;
    private Quaternion starRotation;

    [SerializeField]
    private Color color = Color.white;

    [SerializeField]
    private MeshRenderer[] colouredParts = null;

    [SerializeField]
    private PaintReceiver paintReceiver = null;

    protected void Awake()
    {
        if(paintReceiver == null)
        {
            paintReceiver = GameObject.FindObjectOfType<PaintReceiver>();
        }

        mRigidbody = GetComponent<Rigidbody>();

        startPosition = mRigidbody.position;
        starRotation = mRigidbody.rotation;

        if (colouredParts != null)
        {
            foreach (MeshRenderer renderer in colouredParts)
            {
                renderer.material.color = color;
            }
        }
        else
        {
            Debug.LogWarning("colouredParts is null!");
        }

        if (TryGetComponent(out Painter painter))
        {
            painter.Initialize(paintReceiver);
            painter.ChangeColour(color);
        }
        else
		{
            Debug.LogError("Can't find Painter component");
		}
    }
}
