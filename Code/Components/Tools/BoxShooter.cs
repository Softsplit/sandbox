[Library( "tool_boxgun", Title = "Box Shooter", Description = "Shoot boxes", Group = "fun" )]
public class BoxShooter : BaseTool
{
	RealTimeSince timeSinceShoot;

	string modelToShoot = "models/citizen_props/crate01.vmdl";

	public override bool Primary( SceneTraceResult trace )
	{
		ShootBox();

		return false;
	}

	public override bool Secondary( SceneTraceResult trace )
	{
		if ( timeSinceShoot > 0.05f )
		{
			timeSinceShoot = 0;

			ShootBox();
		}

		return false;
	}

	public override bool Reload( SceneTraceResult trace )
	{
		if ( trace.GameObject.Components.TryGet<PropHelper>( out var propHelper ) && !string.IsNullOrEmpty( propHelper.Prop.Model.Name ) )
		{
			modelToShoot = propHelper.Prop.Model.Name;
			Log.Trace( $"Shooting model: {modelToShoot}" );
		}

		return false;
	}

	void ShootBox()
	{
		var go = new GameObject()
		{
			WorldPosition = Owner.EyeTransform.Position + Owner.EyeTransform.Forward * 50,
			WorldRotation = Owner.EyeTransform.Rotation
		};

		var prop = go.AddComponent<Prop>();
		prop.Model = Model.Load( modelToShoot );
		prop.AddComponent<PropHelper>();

		if ( prop.Components.TryGet<Rigidbody>( out var rigidbody ) )
		{
			rigidbody.Velocity = Owner.EyeTransform.Forward * 1000;
		}

		go.NetworkSpawn();

		Sandbox.Services.Stats.Increment( "box.shoot", 1 );
	}
}
