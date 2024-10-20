using Sandbox;

[Spawnable]
[Library( "weapon_flashlight", Title = "Flashlight" )]
partial class Flashlight : Weapon
{

	protected virtual Vector3 LightOffset => Vector3.Forward * 10;

	[Sync]
	private bool LightEnabled { get; set; } = true;

    [Property] public SpotLight spotLight { get; set; }

	TimeSince timeSinceLightToggled;

    /*
	private SpotLightEntity CreateLight()
	{
		var light = new SpotLightEntity
		{
			Enabled = true,
			DynamicShadows = true,
			Range = 512,
			Falloff = 1.0f,
			LinearAttenuation = 0.0f,
			QuadraticAttenuation = 1.0f,
			Brightness = 2,
			Color = Color.White,
			InnerConeAngle = 20,
			OuterConeAngle = 40,
			FogStrength = 1.0f,
			Owner = Owner,
			LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
		};

		return light;
	}
    */
    public override void Update()
    {
        spotLight.WorldPosition = ViewModel.WorldPosition;
        spotLight.WorldRotation = ViewModel.WorldRotation;
        spotLight.Enabled = LightEnabled;
    }
	public override void OnControl()
	{

		base.OnControl();

		bool toggle = Input.Pressed( "flashlight" ) || Input.Pressed( "attack1" );

		if ( timeSinceLightToggled > 0.1f && toggle )
		{
			LightEnabled = !LightEnabled;

			timeSinceLightToggled = 0;
		}
	}

	public override bool CanReload()
	{
		return false;
	}

	public override void AttackSecondary()
	{
		MeleeAttack();

		Log.Info("Shmuck");

		//PlaySound( "rust_flashlight.attack" );
	}

	private bool MeleeAttack()
	{
        MeleeAttackEffects();

		var ray = Owner.AimRay;
		
		var forward = ray.Forward;
		forward = forward.Normal;

		bool hit = false;

		foreach ( var tr in TraceMelee( ray.Position, ray.Position + forward * 80, 20.0f ) )
		{
			if ( !tr.Hit ) continue;

			CreateImpactEffects( tr );

			hit = true;

			//oooowwwwww - right here
		}

		return hit;
	}

	[Broadcast]
	private void MeleeAttackEffects()
	{
        Owner.ModelRenderer?.Set("b_attack",true);
		ViewModel.Set( "attack", true );
	}
}
