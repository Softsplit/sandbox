﻿[Spawnable, Library( "weapon_fists", Title = "Fists" )]
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
			}
			else if ( tr.GameObject.Components.TryGet<Player>( out var player ) )
			{
				player.TakeDamage( 25 );
			}

			// TODO: Make other non-host clients able to apply impulse too
			if ( tr.Body.IsValid() )
			{
				tr.Body.ApplyImpulseAt( tr.EndPosition, forward * 100 );
			}
		}

		return hit;
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
