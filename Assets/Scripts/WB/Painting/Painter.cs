using Photon.Pun;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

internal enum DrawingCode
{
	DRAWLINE,
	CREATESPLASH
}

internal class DrawCommand 
{
	public int ID { get; set; }
	public DrawingCode Code { get; private set; }

	public DrawCommand(DrawingCode code)
	{
		Code = code;
	}
};

internal class DrawLineCommand : DrawCommand
{
	public Vector2 StartUVPosition { get; private set; }
	public Vector2 EndUVPosition { get; private set; }
	public float StartStampRotation { get; private set; }
	public float EndStampRotation { get; private set; }

	public DrawLineCommand(Vector2 startUVPosition, Vector2 endUVPosition, float startStampRotation, float endStampRotation) : base(DrawingCode.DRAWLINE)
	{
		StartUVPosition = startUVPosition;
		EndUVPosition = endUVPosition;
		StartStampRotation = startStampRotation;
		EndStampRotation = endStampRotation;
	}
}

internal class CreateSplashCommand : DrawCommand
{
	public Vector2 UVPosition { get; private set; }
	public float StampRotation { get; private set; }

	public CreateSplashCommand(Vector2 uvPosition, float stampRotation) : base(DrawingCode.CREATESPLASH)
	{
		UVPosition = uvPosition;
		StampRotation = stampRotation;
	}
}

public class Painter : MonoBehaviourPun, IPunObservable
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

	private const uint MaxSizeCommands = 50;
	private List<DrawCommand> Commands = new List<DrawCommand>();
	private List<DrawCommand> LastAppliedCommands = new List<DrawCommand>();

	private int LastCommandId = 0;

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
		paintReceiverCollider = newPaintReceiver.GetComponent<MeshCollider>();
		if (paintReceiverCollider == null)
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
		if (photonView.IsMine || !PhotonNetwork.InRoom)
		{
			UpdateDrawing();
		}

		ApplyCommands();
	}

	public void ChangeColour(Color newColor)
	{
		color = newColor;
	}

	private void UpdateDrawing()
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
				AddCommand(new DrawLineCommand(lastDrawPosition.Value, hit.textureCoord, lastAngle, currentAngle));
			}
			else
			{
				AddCommand(new CreateSplashCommand(hit.textureCoord, currentAngle));
			}

			lastAngle = currentAngle;

			lastDrawPosition = hit.textureCoord;
		}
		else if (lastDrawPosition != null)
		{
			lastDrawPosition = null;
		}
	}

	private void AddCommand(DrawCommand command)
	{
		if (Commands.Count >= MaxSizeCommands)
		{
			Commands.RemoveAt(0);
		}
		command.ID = LastCommandId;
		++LastCommandId;
		Commands.Add(command);
	}

	private void ApplyCommands()
	{
		List<DrawCommand> needToApply = Commands.ExceptBy(LastAppliedCommands, x => x.ID).ToList();
		List<DrawCommand> needToRemove = new List<DrawCommand>();
		if (needToApply.Count > 0)
		{
			foreach (DrawCommand command in needToApply)
			{
				bool hasChange = false;
				switch (command.Code)
				{
					case DrawingCode.DRAWLINE:
						{
							DrawLineCommand cmd = command as DrawLineCommand;
							if (cmd != null)
							{
								hasChange = paintReceiver.DrawLine(stamp, cmd.StartUVPosition, cmd.EndUVPosition, cmd.StartStampRotation, cmd.EndStampRotation, color, spacing);
							}
						}
						break;

					case DrawingCode.CREATESPLASH:
						{
							CreateSplashCommand cmd = command as CreateSplashCommand;
							if (cmd != null)
							{
								hasChange = paintReceiver.CreateSplash(cmd.UVPosition, stamp, color, cmd.StampRotation);
							}
						}
						break;
				}

				if(!hasChange)
				{
					needToRemove.Add(command);
				}
			}

			Commands = Commands.ExceptBy(needToRemove, x => x.ID).ToList();
			LastAppliedCommands = Commands.GetRange(0, Commands.Count);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext((int)LastCommandId);
			stream.SendNext(LastAppliedCommands.Count);
			foreach(DrawCommand cmd in LastAppliedCommands)
			{
				stream.SendNext(cmd.Code);
				stream.SendNext((int)cmd.ID);
				if (cmd.Code == DrawingCode.CREATESPLASH)
				{
					CreateSplashCommand cpcmd = cmd as CreateSplashCommand;

					stream.SendNext(cpcmd.UVPosition);
					stream.SendNext(cpcmd.StampRotation);
				}
				else if (cmd.Code == DrawingCode.DRAWLINE)
				{
					DrawLineCommand dlcmd = cmd as DrawLineCommand;

					stream.SendNext(dlcmd.StartUVPosition);
					stream.SendNext(dlcmd.EndUVPosition);
					stream.SendNext(dlcmd.StartStampRotation);
					stream.SendNext(dlcmd.EndStampRotation);
				}
			}
		}
		else
		{
			LastCommandId = (int)stream.ReceiveNext();
			int count = (int)stream.ReceiveNext();
			if(count > 0)
			{
				Commands.Clear();

				for(int i = 0; i < count; ++i)
				{
					DrawingCode code = (DrawingCode)stream.ReceiveNext();
					int ID = (int)stream.ReceiveNext();
					if (code == DrawingCode.CREATESPLASH)
					{
						Vector2 uvpos = (Vector2)stream.ReceiveNext();
						float r = (float)stream.ReceiveNext();

						CreateSplashCommand cpcmd = new CreateSplashCommand(uvpos, r);
						cpcmd.ID = ID;
						Commands.Add(cpcmd);
					}
					else if (code == DrawingCode.DRAWLINE)
					{
						Vector2 startUvpos = (Vector2)stream.ReceiveNext();
						Vector2 endUvpos = (Vector2)stream.ReceiveNext();
						float r1 = (float)stream.ReceiveNext();
						float r2 = (float)stream.ReceiveNext();

						DrawLineCommand dlcmd = new DrawLineCommand(startUvpos, endUvpos, r1, r2);
						dlcmd.ID = ID;
						Commands.Add(dlcmd);
					}
				}
			}
		}
	}
}
