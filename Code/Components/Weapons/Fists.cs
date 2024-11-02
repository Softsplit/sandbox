[Spawnable, Library( "weapon_fists" )]
partial class Fists : BaseWeapon
{
	public override bool CanReload()
	{
		return false;
	}

	private void Attack( bool leftHand )
	{
		if ( MeleeAttack() )
		{
			OnMeleeHit( leftHand );
		}
		else
		{
			OnMeleeMiss( leftHand );
		}

		BroadcastAttack();
	}

	[Broadcast]
	private void BroadcastAttack()
	{
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
	}

	public override void AttackPrimary()
	{
		Attack( true );
	}

	public override void AttackSecondary()
	{
		Attack( false );
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

				if ( prop.Rigidbody.IsValid() )
				{
					BroadcastApplyImpulseAt( prop.Rigidbody, tr.EndPosition, forward * 80 * 100 / tr.Body.Mass );
				}
				else if ( prop.ModelPhysics.IsValid() )
				{
					BroadcastApplyImpulseAt( prop.ModelPhysics, tr.EndPosition, forward * 80 * 100 );
				}
			}
			else if ( tr.GameObject.Components.TryGet<Player>( out var player ) )
			{
				player.TakeDamage( 25 );
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

	private void OnMeleeMiss( bool leftHand )
	{
		ViewModel?.Renderer?.Set( "b_attack_has_hit", false );
		ViewModel?.Renderer?.Set( "b_attack", true );
		ViewModel?.Renderer?.Set( "holdtype_attack", leftHand ? 2 : 1 );
	}

	private void OnMeleeHit( bool leftHand )
	{
		ViewModel?.Renderer?.Set( "b_attack_has_hit", true );
		ViewModel?.Renderer?.Set( "b_attack", true );
		ViewModel?.Renderer?.Set( "holdtype_attack", leftHand ? 2 : 1 );
	}
}
