[Library( "weapon_fists", Title = "Fists" )]
partial class Fists : Weapon
{

	public override bool CanReload()
	{
		return false;
	}

	private void Attack( )
	{
		if(MeleeAttack())
		{
			OnMeleeHit();
		}
		else
		{
			OnMeleeMiss();
		}
	}

	public override void AttackPrimary()
	{
		Attack();
	}

	public override void AttackSecondary()
	{
		Attack();
	}

	private bool MeleeAttack()
	{
		var ray = Owner.AimRay;

		var forward = ray.Forward;
		forward = forward.Normal;

		bool hit = false;

		foreach ( var tr in TraceMelee( ray.Position, ray.Position + forward * 80, 20.0f ) )
		{
			if ( !tr.Hit ) continue;

			CreateImpactEffects( tr );

			hit = true;
			///OWWWWW
		}

		return hit;
	}

	[Broadcast]
	private void OnMeleeMiss()
	{
		Owner.ModelRenderer?.Set("b_attack",true);
		ViewModel?.Set( "b_attack_has_hit", false );
		ViewModel?.Set( "b_attack", true );
	}

	[Broadcast]
	private void OnMeleeHit()
	{
		Owner.ModelRenderer?.Set("b_attack",true);
		ViewModel?.Set( "b_attack_has_hit", true );
		ViewModel?.Set( "b_attack", true );
	}
}
