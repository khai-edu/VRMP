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
	public Vector2 StartUVPosition { get; private set; }
	public Vector2 EndUVPosition { get; private set; }
	public float StartStampRotation { get; private set; }
	public float EndStampRotation { get; private set; }

	public DrawCommand(DrawingCode code)
	{
		Code = code;
	}

	public static DrawCommand CreateDrawLineCommand(Vector2 startUVPosition, Vector2 endUVPosition, float startStampRotation, float endStampRotation)
	{
		DrawCommand cmd = new DrawCommand(DrawingCode.DRAWLINE);
		cmd.StartUVPosition = startUVPosition;
		cmd.EndUVPosition = endUVPosition;
		cmd.StartStampRotation = startStampRotation;
		cmd.EndStampRotation = endStampRotation;
		return cmd;
	}

	public static DrawCommand CreateCreateSplashCommand(Vector2 uvPosition, float stampRotation)
	{
		DrawCommand cmd = new DrawCommand(DrawingCode.CREATESPLASH);
		cmd.StartUVPosition = uvPosition;
		cmd.EndUVPosition = new Vector2(0.0f, 0.0f);
		cmd.StartStampRotation = stampRotation;
		cmd.EndStampRotation = 0.0f;
		return cmd;
	}
};

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

	private const int MaxSizeCommands = 10;
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
				AddCommand(DrawCommand.CreateDrawLineCommand(lastDrawPosition.Value, hit.textureCoord, lastAngle, currentAngle));
			}
			else
			{
				AddCommand(DrawCommand.CreateCreateSplashCommand(hit.textureCoord, currentAngle));
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
							hasChange = paintReceiver.DrawLine(stamp, command.StartUVPosition, command.EndUVPosition, command.StartStampRotation, command.EndStampRotation, color, spacing);
						}
						break;

					case DrawingCode.CREATESPLASH:
						{
							hasChange = paintReceiver.CreateSplash(command.StartUVPosition, stamp, color, command.StartStampRotation);
						}
						break;
				}

				if(!hasChange)
				{
					needToRemove.Add(command);
				}
			}

			Commands = Commands.ExceptBy(needToRemove, x => x.ID).ToList();

			if (Commands.Count > MaxSizeCommands)
			{
				int diff = Commands.Count - MaxSizeCommands;
				Commands.RemoveRange(0, diff);
			}

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
				stream.SendNext(cmd.StartUVPosition);
				stream.SendNext(cmd.EndUVPosition);
				stream.SendNext(cmd.StartStampRotation);
				stream.SendNext(cmd.EndStampRotation);
			}
		}
		else
		{
			LastCommandId = (int)stream.ReceiveNext();
			int count = (int)stream.ReceiveNext();
			if(count > 0)
			{
				List<DrawCommand> commandsFromOwner = new List<DrawCommand>();

				for(int i = 0; i < count; ++i)
				{
					DrawingCode code = (DrawingCode)stream.ReceiveNext();
					int ID = (int)stream.ReceiveNext();
					Vector2 startUvpos = (Vector2)stream.ReceiveNext();
					Vector2 endUvpos = (Vector2)stream.ReceiveNext();
					float r1 = (float)stream.ReceiveNext();
					float r2 = (float)stream.ReceiveNext();

					if (code == DrawingCode.CREATESPLASH)
					{
						DrawCommand cmd = DrawCommand.CreateCreateSplashCommand(startUvpos, r1);
						cmd.ID = ID;
						commandsFromOwner.Add(cmd);
					}
					else if (code == DrawingCode.DRAWLINE)
					{
						DrawCommand cmd = DrawCommand.CreateDrawLineCommand(startUvpos, endUvpos, r1, r2);
						cmd.ID = ID;
						commandsFromOwner.Add(cmd);
					}
				}

				IEnumerable<DrawCommand> newCommands = commandsFromOwner.ExceptBy(Commands, x => x.ID);
				Commands.AddRange(newCommands);
			}
		}
	}
}
