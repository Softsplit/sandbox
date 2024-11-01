[Spawnable, Library( "weapon_flashlight" )]
partial class Flashlight : BaseWeapon
{
	private SpotLight worldLight;
	private SpotLight viewLight;

	[Sync] private bool LightEnabled { get; set; } = true;

	TimeSince timeSinceLightToggled;

	protected override void OnStart()
	{
		base.OnStart();

		worldLight = GetComponentInChildren<SpotLight>();
		viewLight = ViewModel?.GetComponentInChildren<SpotLight>();
	}

	public override void OnControl()
	{
		base.OnControl();

		bool toggle = Input.Pressed( "flashlight" ) || Input.Pressed( "attack1" );

		if ( timeSinceLightToggled > 0.1f && toggle )
		{
			LightEnabled = !LightEnabled;

			Sound.Play( LightEnabled ? "flashlight-on" : "flashlight-off", WorldPosition );

			if ( worldLight.IsValid() )
			{
				worldLight.Enabled = LightEnabled;
			}

			if ( viewLight.IsValid() )
			{
				viewLight.Enabled = LightEnabled;
			}

			timeSinceLightToggled = 0;
		}
	}

	public override bool CanReload()
	{
		return false;
	}

	public override void AttackSecondary()
	{
		if ( MeleeAttack() )
		{
			OnMeleeHit();
		}
		else
		{
			OnMeleeMiss();
		}

		BroadcastAttackSecondary();
	}

	[Broadcast]
	private void BroadcastAttackSecondary()
	{
		Sound.Play( "rust_flashlight.attack", WorldPosition );
	}

	private bool MeleeAttack()
	{
		var ray = Owner.AimRay;

		var forward = ray.Forward;
		forward = forward.Normal;

		bool hit = false;

		foreach ( var tr in TraceMelee( ray.Position, ray.Position + forward * 80, 20.0f ) )
		{
			tr.Surface.DoBulletImpact( tr );

			hit = true;

			if ( !tr.GameObject.IsValid() ) continue;

			if ( tr.GameObject.Components.TryGet<PropHelper>( out var prop ) )
			{
				prop.Damage( 25 );
			}
			else if ( tr.GameObject.Components.TryGet<Player>( out var player ) )
			{
				player.TakeDamage( 25 );
			}

			if ( tr.Body.IsValid() )
			{
				if ( tr.Body.GetComponent() is Rigidbody rigidbody )
				{
					BroadcastApplyImpulseAt( rigidbody, tr.EndPosition, forward * 80 * 100 / tr.Body.Mass );
				}
				else if ( tr.Body.GetComponent() is ModelPhysics modelPhysics )
				{
					BroadcastApplyImpulseAt( modelPhysics, tr.EndPosition, forward * 80 * 100 );
				}
			}
		}

		return hit;
	}

	[Broadcast]
	private void BroadcastApplyImpulseAt( Component body, Vector3 position, Vector3 force )
	{
		if ( !Networking.IsHost ) return;

		if ( body is Rigidbody rigidbody )
		{
			rigidbody.ApplyImpulseAt( position, force );
		}
		else if ( body is ModelPhysics modelPhysics )
		{
			modelPhysics.PhysicsGroup.ApplyImpulse( force / modelPhysics.PhysicsGroup.Mass, true );
		}
	}

	private void OnMeleeMiss()
	{
		ViewModel?.Renderer?.Set( "attack", true );
	}

	private void OnMeleeHit()
	{
		ViewModel?.Renderer?.Set( "attack_hit", true );
	}

	private void Activate()
	{
		if ( worldLight.IsValid() )
		{
			worldLight.Enabled = LightEnabled;
		}

		if ( viewLight.IsValid() )
		{
			viewLight.Enabled = LightEnabled;
		}
	}

	private void Deactivate()
	{
		if ( worldLight.IsValid() )
		{
			worldLight.Enabled = false;
		}

		if ( viewLight.IsValid() )
		{
			viewLight.Enabled = false;
		}
	}

	protected override void OnEnabled()
	{
		Activate();

		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		Deactivate();

		base.OnDisabled();
	}
}
