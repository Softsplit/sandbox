using Sandbox.Physics;

namespace Softsplit;

public partial class PhysGunComponent : BaseWeapon
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

	protected Ray WeaponRay => Owner.AimRay;
	/*
	Beam beam;
	protected override void OnStart()
	{
		beam = Components.Get<Beam>();
	}
	*/

	bool rotating = false;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		UpdateEffects();
		
		if ( rotating )
		{
			rotating = GrabbedObject != null;
		}
		if(!Owner.IsValid())
			return;
		Owner.PlayerController.lockCamera = rotating;
		//beam.enabled = Grabbing && GrabbedObject != null;
		if ( GrabbedObjectHighlight != null ) GrabbedObjectHighlight.Enabled = Grabbing && GrabbedObject != null;
		if ( Grabbing && GrabbedObject != null )
		{
			//beam.CreateEffect( Effector.Muzzle.Transform.Position, GrabbedObject.Transform.Local.PointToWorld( GrabbedPos / GrabbedObject.Transform.Scale ), Effector.Muzzle.Transform.World.Forward );
			//beam.Base = Effector.Muzzle.Transform.Position;
			if ( GrabbedObjectHighlight == null ) GrabbedObjectHighlight = GrabbedObject.Components.Get<HighlightOutline>( true );
		}

		if ( IsProxy ) return;

		if ( !HeldBody.IsValid() )
			return;

		if ( GrabbedObject.Root.Components.Get<Player>().IsValid() )
			return;

		var velocity = HeldBody.Velocity;
		Vector3.SmoothDamp( HeldBody.Position, HoldPos, ref velocity, 0.075f, Time.Delta );
		HeldBody.Velocity = velocity;

		var angularVelocity = HeldBody.AngularVelocity;
		Rotation.SmoothDamp( HeldBody.Rotation, HoldRot, ref angularVelocity, 0.075f, Time.Delta );
		HeldBody.AngularVelocity = angularVelocity;
	}

	public override void OnControl()
	{
		var eyePos = WeaponRay.Position;
		var eyeDir = WeaponRay.Forward;
		var eyeRot = Rotation.From( new Angles( 0.0f, Owner.PlayerController.EyeAngles.yaw, 0.0f ) );

		if ( Input.Pressed( "Attack1" ) )
		{
			Owner?.ModelRenderer?.Set( "b_attack", true );

			if ( !Grabbing )
				Grabbing = true;
		}

		bool grabEnabled = Grabbing && Input.Down( "Attack1" );
		bool wantsToFreeze = Input.Pressed( "Attack2" );

		if ( GrabbedObject.IsValid() && wantsToFreeze )
		{
			Owner?.ModelRenderer?.Set( "b_attack", true );
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

		Owner.PlayerInventory.lockSwitch = GrabbedObject != null;
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

		var weldContexts = GetAllConnectedProps( rootEnt );

		bool unfrozen = false;


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

	public static List<GameObject> GetAllConnectedProps( GameObject gameObject )
	{
		PropHelper propHelper = gameObject.Components.Get<PropHelper>();

		if ( !propHelper.IsValid() )
			return null;

		var result = new List<PhysicsJoint>();
		var visited = new HashSet<PropHelper>();

		CollectWelds( propHelper, result, visited );

		List<GameObject> returned = new();

		foreach ( PhysicsJoint joint in result )
		{
			returned.Add( joint.Body1.GetGameObject());
			returned.Add( joint.Body2.GetGameObject());
		}

		return returned;
	}

	private static void CollectWelds( PropHelper propHelper, List<PhysicsJoint> result, HashSet<PropHelper> visited )
	{
		
		if ( visited.Contains( propHelper ) )
			return;

		visited.Add( propHelper );

		result.AddRange( propHelper.Joints );

		foreach ( var joint in propHelper.Joints )
		{
			GameObject jointObject = joint.Body1.GetGameObject();

			if(jointObject == propHelper.GameObject)
			{
				jointObject = joint.Body2.GetGameObject();
			}

			if(!jointObject.IsValid())
				return;

			PropHelper propHelper1 = jointObject.Components.Get<PropHelper>();

			if(!propHelper1.IsValid())
				return;
			
			CollectWelds( propHelper1, result, visited );
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
		if ( body.BodyType == PhysicsBodyType.Keyframed && tr.Component is not PlayerController )
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

		if ( GetAllConnectedProps( GrabbedObject ) != null )
		{
			foreach ( GameObject g in GetAllConnectedProps( GrabbedObject ) )
			{
				g?.Network.TakeOwnership();
			}
		}

		GrabbedPos = tr.GameObject.Transform.World.PointToLocal( tr.EndPosition );


		GrabbedObject.Tags.Add( GrabbedTag );
		GrabbedObject.Tags.Add( $"{GrabbedTag}{Owner.Network.Owner.SteamId}" );

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
			GrabbedObject.Tags.Remove( $"{GrabbedTag}{Owner.Network.Owner.SteamId}" );
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
		HoldPos = startPos - HeldPos * HeldBody.Rotation + dir.Normal * HoldDistance;

		if ( GrabbedObject.Root.Components.TryGet<PlayerController>( out var player ) )
		{
			var velocity = player.CharacterController.Velocity;
			Vector3.SmoothDamp( player.WorldPosition, HoldPos, ref velocity, 0.075f, Time.Delta );
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
