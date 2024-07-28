namespace Softsplit;

public sealed class Weld : ToolComponent
{
	GameObject object1;
	HighlightOutline object1Outline;
	Vector3 point1Direction;
	Vector3 point1;

	protected override void Start()
	{
		ToolName = "Weld";
		ToolDes = "Weld objects together. Right click to snap.";
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
				CreateWeld( PlayerState.Local.Pawn.GameObject, object1, point1, hit.GameObject, localPoint );
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
				RemoveWeld( hit.GameObject );
				Recoil( hit.EndPosition );
			}
			else
			{
				GameObject object1G = object1;

				object1G.Transform.Rotation = Rotation.FromToRotation( point1Direction, -hit.Normal ) * object1G.Transform.Rotation;

				Vector3 pointWorld = object1G.Transform.World.PointToWorld( point1 );

				object1G.Transform.Position += hit.EndPosition - pointWorld;

				CreateWeld( PlayerState.Local.Pawn.GameObject, object1, point1, hit.GameObject, hit.EndPosition );

				object1 = null;
			}
		}
	}

	[Broadcast]
	public static void CreateWeld( GameObject player, GameObject object1, Vector3 point1Pos, GameObject object2, Vector3 point2Pos )
	{
		WeldContext weldContext1 = object1?.Components.Create<WeldContext>();
		weldContext1.MainWeld = true;

		weldContext1.point1 = point1Pos;
		weldContext1.point2 = point2Pos;

		WeldContext weldContext2 = object2?.Components.Create<WeldContext>();
		weldContext2.connectedObject = weldContext1;
		weldContext2.body = object2?.Components.Get<Rigidbody>()?.PhysicsBody;

		weldContext1.connectedObject = weldContext2;
		weldContext1.body = object1?.Components.Get<Rigidbody>()?.PhysicsBody;

		PlayerPawn owner = player?.Components.Get<PlayerPawn>();
		if ( owner == PlayerState.Local?.PlayerPawn )
		{
			Log.Info( "crap" );

			PlayerState.Thing thing = new()
			{
				components = new List<Component>
				{
					weldContext1,
					weldContext2
				}
			};

			owner.PlayerState?.SpawnedThings?.Add( thing );
		}
	}

	[Broadcast]
	public static void RemoveWeld( GameObject gameObject )
	{
		if ( !Networking.IsHost )
			return;

		IEnumerable<WeldContext> weldContext = gameObject?.Components.GetAll<WeldContext>();

		while ( weldContext.Any() )
		{
			WeldContext weldToRemove = weldContext?.ElementAt( 0 );
			if ( weldToRemove.MainWeld ) weldToRemove?.weldJoint?.Remove();
			else weldToRemove?.weldJoint?.Remove();

			weldToRemove?.connectedObject?.Destroy();
			weldToRemove?.Destroy();
		}
	}
}
