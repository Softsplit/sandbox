public class Physgun : Tool // https://github.com/Facepunch/sbox-scenestaging/blob/main/code/ExampleComponents/PlayerController.cs
{
	/// <summary>
	/// The higher this is, the "looser" the grip is when dragging objects
	/// </summary>
	[Property, Range( 1, 16 )] public float MovementSmoothness { get; set; } = 3.0f;

	PhysicsBody grabbedBody;
	Transform grabbedOffset;
	Vector3 localOffset;

	bool waitForUp = false;

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;
		if ( !IsUsing() )
			return;

		Transform aimTransform = Scene.Camera.Transform.World;

		if ( waitForUp && Input.Down( "attack1" ) )
		{
			return;
		}

		waitForUp = false;

		if ( grabbedBody is not null )
		{
			if ( Input.Down( "attack2" ) )
			{
				grabbedBody.BodyType = PhysicsBodyType.Keyframed;
				grabbedBody.Velocity = 0;
				grabbedBody.AngularVelocity = 0;

				grabbedOffset = default;
				grabbedBody = default;
				waitForUp = true;
				return;
			}

			var targetTx = aimTransform.ToWorld( grabbedOffset );

			var worldStart = grabbedBody.GetLerpedTransform( Time.Now ).PointToWorld( localOffset );
			var worldEnd = targetTx.PointToWorld( localOffset );

			//var delta = Scene.Camera.Transform.World.PointToWorld( new Vector3( 0, -10, -5 ) ) - worldStart;
			var delta = worldEnd - worldStart;
			for ( var f = 0.0f; f < delta.Length; f += 2.0f )
			{
				var size = 1 - f * 0.01f;
				if ( size < 0 ) break;

				Gizmo.Draw.Color = Color.Cyan;
				Gizmo.Draw.SolidSphere( worldStart + delta.Normal * f, size );
			}

			if ( !Input.Down( "attack1" ) )
			{
				grabbedOffset = default;
				grabbedBody = default;
			}
			else
			{
				return;
			}
		}

		if ( Input.Down( "attack2" ) )
		{
			Shoot();
			return;
		}

		var tr = Scene.Trace.Ray( Scene.Camera.Transform.Position, Scene.Camera.Transform.Position + Scene.Camera.Transform.Rotation.Forward * 1000 )
			.Run();

		if ( !tr.Hit || tr.Body is null )
			return;

		if ( Input.Down( "attack3" ) )
		{

			if ( Input.Pressed( "attack3" ) )
			{
				GameObject Cube = new GameObject();
				Cube.Transform.Position = tr.HitPosition;
				Cube.Components.Create<ModelRenderer>().Model = Model.Cube;
				Cube.Components.Create<BoxCollider>();
				Cube.Components.Create<Rigidbody>();

			}
			return;
		}

		if ( tr.Body.BodyType == PhysicsBodyType.Static )
			return;

		if ( Input.Down( "attack1" ) )
		{
			grabbedBody = tr.Body;
			localOffset = tr.Body.Transform.PointToLocal( tr.HitPosition );
			grabbedOffset = aimTransform.ToLocal( tr.Body.Transform );
			grabbedBody.BodyType = PhysicsBodyType.Dynamic;
			grabbedBody.MotionEnabled = true;
		}

	}
	private void Move( PhysicsBody body, global::Transform targetTx, float smooth)
	{
		body.SmoothMove( targetTx, smooth, Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		Transform aimTransform = Scene.Camera.Transform.World;

		if ( waitForUp && Input.Down( "attack1" ) )
		{
			return;
		}

		waitForUp = false;

		if ( grabbedBody is not null )
		{
			if ( Input.Down( "attack1" ) )
			{
				var targetTx = aimTransform.ToWorld( grabbedOffset );
				Move( grabbedBody, targetTx, Time.Delta * MovementSmoothness );
				// grabbedBody.SmoothMove( targetTx, Time.Delta * MovementSmoothness, Time.Delta );
				return;
			}
		}
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();


		if ( grabbedBody is null )
		{
			var tr = Scene.Trace.Ray( Scene.Camera.ScreenNormalToRay( 0.5f ), 1000.0f )
							.Run();

			if ( tr.Hit )
			{
				Gizmo.Draw.Color = Color.Cyan;
				Gizmo.Draw.SolidSphere( tr.HitPosition, 1 );
			}
		}
	}

	SoundEvent shootSound = Cloud.SoundEvent( "mdlresrc.toolgunshoot" );

	TimeSince timeSinceShoot;

	public void Shoot()
	{
		
	}
}