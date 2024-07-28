using Sandbox.Events;

namespace Softsplit;

public partial class PhysGunComponent : InputWeaponComponent,
	IGameEventHandler<EquipmentDeployedEvent>
{
	[Property] public float MinTargetDistance { get; set; } = 0.0f;
	[Property] public float MaxTargetDistance { get; set; } = 10000.0f;
	[Property] public float LinearFrequency { get; set; } = 20.0f;
	[Property] public float LinearDampingRatio { get; set; } = 1.0f;
	[Property] public float AngularFrequency { get; set; } = 20.0f;
	[Property] public float AngularDampingRatio { get; set; } = 1.0f;
	[Property] public float TargetDistanceSpeed { get; set; } = 25.0f;
	[Property] public float RotateSpeed { get; set; } = 0.125f;
	[Property] public float RotateSnapAt { get; set; } = 45.0f;

	public const string GrabbedTag = "grabbed";

	public PhysicsBody HeldBody { get; private set; }
	public Vector3 HeldPos { get; private set; }
	public Rotation HeldRot { get; private set; }
	public Vector3 HoldPos { get; private set; }
	public Rotation HoldRot { get; private set; }
	public float HoldDistance { get; private set; }
	[Sync] public bool Grabbing { get; private set; }

	[Sync] public bool BeamActive { get; set; }
	[Sync] public GameObject GrabbedObject { get; set; }
	[Sync] public HighlightOutline GrabbedObjectHighlight { get; set; }
	[Sync] public int GrabbedBone { get; set; }
	[Sync] public Vector3 GrabbedPos { get; set; }

	/// <summary>
	/// Accessor for the aim ray.
	/// </summary>

	protected Ray WeaponRay => Equipment.Owner.AimRay;
	Beam beam;
	protected override void OnStart()
	{
		beam = Components.Get<Beam>();
	}

	bool rotating = false;

	protected override void OnUpdate()
	{
		if ( rotating )
		{
			rotating = GrabbedObject != null;
		}
		Equipment.Owner.lockCamera = rotating;
		beam.enabled = Grabbing && GrabbedObject != null;
		if ( GrabbedObjectHighlight != null ) GrabbedObjectHighlight.Enabled = Grabbing && GrabbedObject != null;
		if ( Grabbing && GrabbedObject != null )
		{
			beam.CreateEffect( Effector.Muzzle.Transform.Position, GrabbedObject.Transform.Local.PointToWorld( GrabbedPos / GrabbedObject.Transform.Scale ), Effector.Muzzle.Transform.World.Forward );
			beam.Base = Effector.Muzzle.Transform.Position;
			if ( GrabbedObjectHighlight == null ) GrabbedObjectHighlight = GrabbedObject.Components.Get<HighlightOutline>( true );
		}

		if ( IsProxy ) return;

		if ( !HeldBody.IsValid() )
			return;

		// if ( GrabbedObject.Root.Components.Get<PlayerPawn>().IsValid() )
		//	return;

		var velocity = HeldBody.Velocity;
		Vector3.SmoothDamp( HeldBody.Position, HoldPos, ref velocity, 0.075f, Time.Delta );
		HeldBody.Velocity = velocity;

		var angularVelocity = HeldBody.AngularVelocity;
		Rotation.SmoothDamp( HeldBody.Rotation, HoldRot, ref angularVelocity, 0.075f, Time.Delta );
		HeldBody.AngularVelocity = angularVelocity;
	}

	protected IEquipment Effector
	{
		get
		{
			if ( IsProxy || !Equipment.ViewModel.IsValid() )
				return Equipment;

			return Equipment.ViewModel;
		}
	}

	protected override void OnInputUpdate()
	{
		var eyePos = WeaponRay.Position;
		var eyeDir = WeaponRay.Forward;
		var eyeRot = Rotation.From( new Angles( 0.0f, Equipment.Owner.EyeAngles.yaw, 0.0f ) );

		if ( Input.Pressed( "Attack1" ) )
		{
			Equipment.Owner?.BodyRenderer?.Set( "b_attack", true );

			if ( !Grabbing )
				Grabbing = true;
		}

		bool grabEnabled = Grabbing && Input.Down( "Attack1" );
		bool wantsToFreeze = Input.Pressed( "Attack2" );

		if ( GrabbedObject.IsValid() && wantsToFreeze )
		{
			Equipment.Owner?.BodyRenderer?.Set( "b_attack", true );
		}

		BeamActive = grabEnabled;

		if ( grabEnabled )
		{
			if ( HeldBody.IsValid() )
			{
				UpdateGrab( eyePos, eyeRot, eyeDir, wantsToFreeze );
			}
			else
			{
				TryStartGrab( eyePos, eyeRot, eyeDir );
			}
		}
		else if ( Grabbing )
		{
			GrabEnd();
		}

		if ( Grabbing && Input.Pressed( "reload" ) )
		{
			TryUnfreezeAll( eyePos, eyeRot, eyeDir );
		}

		if ( BeamActive )
		{
			Input.MouseWheel = 0;
		}

		Equipment.Owner.Inventory.cantSwitch = GrabbedObject != null;
	}

	[Broadcast]
	private void TryUnfreezeAll( Vector3 eyePos, Rotation eyeRot, Vector3 eyeDir )
	{
		var tr = Scene.Trace.Ray( eyePos, eyePos + eyeDir * MaxTargetDistance )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.Run();

		if ( !tr.Hit || !tr.GameObject.IsValid() || tr.Component is MapCollider ) return;

		var rootEnt = tr.GameObject.Root;
		if ( !rootEnt.IsValid() ) return;

		var weldContexts = GetAllConnectedWelds( rootEnt );

		bool unfrozen = false;

		Log.Info( weldContexts.Count );


		for ( int i = 0; i < weldContexts.Count; i++ )
		{
			var body = weldContexts[i].Components.Get<Rigidbody>().PhysicsBody;
			if ( !body.IsValid() ) continue;

			if ( body.BodyType == PhysicsBodyType.Static )
			{
				body.BodyType = PhysicsBodyType.Dynamic;
				unfrozen = true;
			}
		}

		if ( unfrozen )
		{
			// var freezeEffect = Particles.Create( "particles/physgun_freeze.vpcf" );
			// freezeEffect.SetPosition( 0, tr.EndPosition );
		}
	}

	public static List<GameObject> GetAllConnectedWelds( GameObject gameObject )
	{
		Component component = gameObject.Components.Get<JointContext>();

		if ( !component.IsValid() )
			return null;

		var result = new List<JointContext>();
		var visited = new HashSet<Component>();

		CollectWelds( component, result, visited );

		List<GameObject> returned = new();

		foreach ( JointContext jointContext in result )
		{
			returned.Add( jointContext.GameObject );
		}

		return returned;
	}

	private static void CollectWelds( Component component, List<JointContext> result, HashSet<Component> visited )
	{
		if ( visited.Contains( component ) )
		{
			return;
		}

		visited.Add( component );

		var jointContexts = component.Components.GetAll<JointContext>();

		result.AddRange( jointContexts );

		foreach ( var jointContext in jointContexts )
		{
			if ( jointContext.connectedObject != null )
			{
				CollectWelds( jointContext.connectedObject, result, visited );
			}
		}
	}


	private void TryStartGrab( Vector3 eyePos, Rotation eyeRot, Vector3 eyeDir )
	{

		var tr = Scene.Trace.Ray( eyePos, eyePos + eyeDir * MaxTargetDistance )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "debris", "nocollide" )
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.Run();

		if ( !tr.Hit || !tr.GameObject.IsValid() || tr.Component is MapCollider || tr.StartedSolid || tr.Tags.Contains( "map" ) ) return;
		var rootEnt = tr.GameObject.Root;
		var body = tr.Body;



		if ( !body.IsValid() || tr.GameObject.Parent.IsValid() )
		{
			if ( rootEnt.IsValid() && (tr.Component as Rigidbody)?.PhysicsBody.PhysicsGroup != null )
			{
				body = (tr.Component as Rigidbody).PhysicsBody.PhysicsGroup.BodyCount > 0 ? (tr.Component as Rigidbody).PhysicsBody.PhysicsGroup.GetBody( 0 ) : null;
			}
		}

		if ( !body.IsValid() )
			return;

		//
		// Don't move keyframed, unless it's a player
		//
		if ( body.BodyType == PhysicsBodyType.Keyframed && tr.Component is not PlayerPawn )
			return;

		//
		// Unfreeze
		//
		if ( body.BodyType == PhysicsBodyType.Static )
		{
			body.BodyType = PhysicsBodyType.Dynamic;
		}

		if ( rootEnt.Tags.Has( GrabbedTag ) )
			return;

		GrabInit( body, eyePos, tr.EndPosition, eyeRot );

		GrabbedObject = rootEnt;
		GrabbedObject.Network.TakeOwnership();

		if ( GetAllConnectedWelds( GrabbedObject ) != null )
		{
			foreach ( GameObject g in GetAllConnectedWelds( GrabbedObject ) )
			{
				g?.Network.TakeOwnership();
			}
		}

		GrabbedPos = tr.GameObject.Transform.World.PointToLocal( tr.EndPosition );


		GrabbedObject.Tags.Add( GrabbedTag );
		GrabbedObject.Tags.Add( $"{GrabbedTag}{Equipment.Owner.SteamId}" );

		GrabbedPos = body.Transform.PointToLocal( tr.EndPosition );
		GrabbedBone = body.GroupIndex;
	}


	private void UpdateGrab( Vector3 eyePos, Rotation eyeRot, Vector3 eyeDir, bool wantsToFreeze )
	{
		if ( wantsToFreeze )
		{
			if ( HeldBody.BodyType == PhysicsBodyType.Dynamic )
			{
				HeldBody.BodyType = PhysicsBodyType.Static;
			}

			if ( GrabbedObject.IsValid() )
			{
				// var freezeEffect = Particles.Create( "particles/physgun_freeze.vpcf" );
				// freezeEffect.SetPosition( 0, HeldBody.Transform.PointToWorld( GrabbedPos ) );
			}

			GrabEnd();
			return;
		}

		MoveTargetDistance( Input.MouseWheel.y * TargetDistanceSpeed );

		rotating = Input.Down( "Use" );
		bool snapping = false;

		if ( rotating )
		{
			DoRotate( eyeRot, Input.MouseDelta * RotateSpeed );
			snapping = Input.Down( "Run" );
		}

		GrabMove( eyePos, eyeDir, eyeRot, snapping );
	}

	private void GrabInit( PhysicsBody body, Vector3 startPos, Vector3 grabPos, Rotation rot )
	{
		if ( !body.IsValid() )
			return;

		GrabEnd();

		Grabbing = true;
		HeldBody = body;
		HoldDistance = Vector3.DistanceBetween( startPos, grabPos );
		HoldDistance = HoldDistance.Clamp( MinTargetDistance, MaxTargetDistance );

		HeldRot = rot.Inverse * HeldBody.Rotation;
		HeldPos = HeldBody.Transform.PointToLocal( grabPos );

		HoldPos = HeldBody.Position;
		HoldRot = HeldBody.Rotation;

		HeldBody.Sleeping = false;
		HeldBody.AutoSleep = false;
	}
	[Broadcast]
	private void GrabEnd()
	{
		if ( GrabbedObject == null ) return;

		if ( HeldBody.IsValid() )
		{
			HeldBody.AutoSleep = true;
		}

		if ( GrabbedObject.IsValid() )
		{
			GrabbedObject.Tags.Remove( GrabbedTag );
			GrabbedObject.Tags.Remove( $"{GrabbedTag}{Equipment.Owner.SteamId}" );
		}

		GrabbedObjectHighlight ??= GrabbedObject.Components.Get<HighlightOutline>();

		if ( GrabbedObjectHighlight.IsValid() )
			GrabbedObjectHighlight.Enabled = false;

		GrabbedObject = null;
		GrabbedObjectHighlight = null;

		HeldBody = null;
		Grabbing = false;
	}

	private void GrabMove( Vector3 startPos, Vector3 dir, Rotation rot, bool snapAngles )
	{
		if ( !HeldBody.IsValid() )
			return;

		HoldPos = startPos - HeldPos * HeldBody.Rotation + dir * HoldDistance;

		if ( GrabbedObject.Root.Components.TryGet<PlayerPawn>( out var player ) )
		{
			var velocity = player.CharacterController.Velocity;
			Vector3.SmoothDamp( player.Transform.Position, HoldPos, ref velocity, 0.075f, Time.Delta );
			player.CharacterController.Velocity = velocity;
			player.CharacterController.IsOnGround = false;

			return;
		}

		HoldRot = rot * HeldRot;

		if ( snapAngles )
		{
			var angles = HoldRot.Angles();

			HoldRot = Rotation.From(
				MathF.Round( angles.pitch / RotateSnapAt ) * RotateSnapAt,
				MathF.Round( angles.yaw / RotateSnapAt ) * RotateSnapAt,
				MathF.Round( angles.roll / RotateSnapAt ) * RotateSnapAt
			);
		}
	}

	private void MoveTargetDistance( float distance )
	{
		HoldDistance += distance;
		HoldDistance = HoldDistance.Clamp( MinTargetDistance, MaxTargetDistance );
	}

	public void DoRotate( Rotation eye, Vector3 input )
	{
		var localRot = eye;
		localRot *= Rotation.FromAxis( Vector3.Up, input.x * RotateSpeed );
		localRot *= Rotation.FromAxis( Vector3.Right, input.y * RotateSpeed );
		localRot = eye.Inverse * localRot;

		HeldRot = localRot * HeldRot;
	}
}
