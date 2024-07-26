using Softsplit.UI;

namespace Softsplit;

public sealed class Rope : ToolComponent
{
	GameObject object1;
	HighlightOutline object1Outline;
	Vector3 point1Direction;
	Vector3 point1;

	RopeMenu RopeMenu;

	protected override void Start()
	{
		ToolName = "Rope";
		ToolDes = "Rope objects together. Right click to snap.";
		RopeMenu = Components.Get<RopeMenu>(true);
	}

	protected override void Update()
	{
		if ( object1Outline != null )
		{
			object1Outline.Enabled = object1 != null;
			if ( object1 == null ) object1Outline = null;
		}

		object1Outline = object1?.Components.Get<HighlightOutline>( true );
	}

	protected override void PrimaryAction()
	{
		var hit = Trace();

		if ( hit.Hit && hit.GameObject?.Name != "Map" )
		{
			if ( hit.GameObject == object1 || hit.Body == null ) return;

			Recoil( hit.EndPosition );

			Vector3 localPoint = hit.GameObject.Transform.World.PointToLocal( hit.EndPosition );

			if ( object1 == null )
			{
				point1 = localPoint;
				point1Direction = hit.Normal;
				object1 = hit.GameObject;
			}
			else
			{
				CreateRope( PlayerState.Local.Pawn.GameObject, object1, point1, hit.GameObject, localPoint, RopeMenu.Width, RopeMenu.Color, RopeMenu.MinLength, RopeMenu.MaxLength );
				object1 = null;
			}
		}
	}


	protected override void SecondaryAction()
	{
		base.SecondaryAction();

		var hit = Trace();
		if ( hit.Hit && hit.GameObject?.Name != "Map" )
		{
			if ( object1 == null )
			{
				RemoveRope( hit.GameObject );
				Recoil( hit.EndPosition );
			}
			else
			{
				GameObject object1G = object1;

				object1G.Transform.Rotation = Rotation.FromToRotation( point1Direction, -hit.Normal ) * object1G.Transform.Rotation;

				Vector3 pointWorld = object1G.Transform.World.PointToWorld( point1 );

				object1G.Transform.Position += hit.EndPosition - pointWorld;

				CreateRope( PlayerState.Local.Pawn.GameObject, object1, point1, hit.GameObject, hit.EndPosition, RopeMenu.Width, RopeMenu.Color, RopeMenu.MinLength, RopeMenu.MaxLength );

				object1 = null;
			}
		}
	}

	[Broadcast]
	public static void CreateRope( GameObject player, GameObject object1, Vector3 point1Pos, GameObject object2, Vector3 point2Pos, float width, Color color, float minLength, float maxLength )
	{
		RopeContext ropeContext1 = object1?.Components.Create<RopeContext>();
		ropeContext1.Width = width;
		ropeContext1.Color = color;
		ropeContext1.MinLength = minLength;
		ropeContext1.MaxLength = maxLength;
		ropeContext1.MainRope = true;

		ropeContext1.point1 = point1Pos;
		ropeContext1.point2 = point2Pos;

		RopeContext ropeContext2 = object2?.Components.Create<RopeContext>();
		ropeContext2.connectedObject = ropeContext1;
		ropeContext2.body = object2?.Components.Get<Rigidbody>()?.PhysicsBody;

		ropeContext1.connectedObject = ropeContext2;
		ropeContext1.body = object1?.Components.Get<Rigidbody>()?.PhysicsBody;

		PlayerPawn owner = player?.Components.Get<PlayerPawn>();
		if ( owner == PlayerState.Local?.PlayerPawn )
		{
			Log.Info( "crap" );

			PlayerState.Thing thing = new()
			{
				components = new List<Component>
				{
					ropeContext1,
					ropeContext2
				}
			};

			owner.PlayerState?.SpawnedThings?.Add( thing );
		}
	}

	[Broadcast]
	public static void RemoveRope( GameObject gameObject )
	{
		if ( !Networking.IsHost )
			return;

		IEnumerable<RopeContext> ropeContext = gameObject?.Components.GetAll<RopeContext>();

		while ( ropeContext.Any() )
		{
			RopeContext ropeToRemove = ropeContext?.ElementAt( 0 );
			if ( ropeToRemove.MainRope ) ropeToRemove?.ropeJoint?.Remove();
			else ropeToRemove?.ropeJoint?.Remove();

			ropeToRemove?.connectedObject?.Destroy();
			ropeToRemove?.Destroy();
		}
	}

	public static Vector3 FindMidpoint( Vector3 vector1, Vector3 vector2 )
	{
		return new Vector3(
			(vector1.x + vector2.x) / 2,
			(vector1.y + vector2.z) / 2,
			(vector1.y + vector2.z) / 2
		);
	}
}
