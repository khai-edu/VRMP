using UnityEngine;

public class Painter : MonoBehaviour
{
    [SerializeField]
    private PaintMode paintMode = PaintMode.Draw;

    [SerializeField]
    private Transform paintingTransform = null;

    [SerializeField]
    private float raycastLength = 0.01f;

    [SerializeField]
    private Texture2D brush = null;

    [SerializeField]
    private float spacing = 1f;
    
    private float currentAngle = 0f;
    private float lastAngle = 0f;

    private PaintReceiver paintReceiver;
    private Collider paintReceiverCollider;

    private Stamp stamp = null;

    private Color color;

    private Vector2? lastDrawPosition = null;

    public void Initialize(PaintReceiver newPaintReceiver)
    {

        if (brush == null)
        {
            Debug.LogError("Initialization error: brush is null!");
            return;
        }

        stamp = new Stamp(brush);
        stamp.mode = paintMode;

        if (newPaintReceiver == null)
        {

            Debug.LogError("Initialization error: newPaintReceiver is null!");
            return;
        }

        paintReceiver = newPaintReceiver;
        paintReceiverCollider = newPaintReceiver.GetComponent<Collider>();
        if(paintReceiverCollider == null)
		{
            Debug.LogError("Initialization error: Collier not found in paint receiver!");
		}

        if (paintingTransform == null)
        {
            Debug.LogError("Initialization error: paintingTransform is null!");
        }
    }

    private void Update()
    {
        if (paintingTransform == null)
            return;

        if (paintReceiverCollider == null)
            return;

        currentAngle = -transform.rotation.eulerAngles.z;

        Ray ray = new Ray(paintingTransform.position, paintingTransform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * raycastLength);

        if (paintReceiverCollider.Raycast(ray, out hit, raycastLength))
        {
            if (lastDrawPosition.HasValue && lastDrawPosition.Value != hit.textureCoord)
            {
                paintReceiver.DrawLine(stamp, lastDrawPosition.Value, hit.textureCoord, lastAngle, currentAngle, color, spacing);
            }
            else
            {
                paintReceiver.CreateSplash(hit.textureCoord, stamp, color, currentAngle);
            }

            lastAngle = currentAngle;

            lastDrawPosition = hit.textureCoord;
        }
        else
        {
            lastDrawPosition = null;
        }
    }

    public void ChangeColour(Color newColor)
    {
        color = newColor;
    }

    public void SetRotation(float newAngle)
    {
        currentAngle = newAngle;
    }
}
