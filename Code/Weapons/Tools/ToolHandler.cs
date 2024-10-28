[Library( "tool_handler", Title = "Tool Handler" )]
public class ToolHandler : Weapon
{
	[ConVar( "tool_current" )] public static string CurrentTool { get; set; } = "Weld";
	public BaseTool ActiveTool {get;set;}
	public override void DoEnabled()
	{
		base.DoEnabled();
		UpdateTool();
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		ViewModel?.Set( "b_deploy", true );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( "attack1" );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;

		var trace = TraceTool(Owner.AimRay.Position,Owner.AimRay.Position+Owner.AimRay.Forward*5000);

		if(!trace.Hit)
			return;

		Owner.ModelRenderer?.Set( "b_attack", true );
		ViewModel?.Set( "b_attack", true );
		ToolEffects();
		//Sound.Play( "weapons/rust_pistol/sound/rust_pistol.shoot.sound", Muzzle.WorldPosition );
		ActiveTool?.Primary(trace);
		
	}

	[Broadcast]
	void ToolEffects()
	{
		Owner.ModelRenderer?.Set( "b_attack", true );
		ViewModel?.Set( "b_attack", true );
		WorldModel?.Set( "b_attack", true );
	}

	public override void AttackSecondary()
	{
		TimeSinceSecondaryAttack = 0;

		Owner.ModelRenderer?.Set( "b_attack", true );
		ViewModel?.Set( "b_attack", true );

		var trace = TraceTool(Owner.AimRay.Position,Owner.AimRay.Position+Owner.AimRay.Forward*5000);

		if(!trace.Hit)
			return;

		Owner.ModelRenderer?.Set( "b_attack", true );
		ViewModel?.Set( "b_attack", true );
		ToolEffects();
		//Sound.Play( "weapons/rust_pistol/sound/rust_pistol.shoot.sound", Muzzle.WorldPosition );
		ActiveTool?.Secondary(trace);
	}

	public void UpdateTool()
	{
		ActiveTool?.Destroy();

		var comp = TypeLibrary.GetType( CurrentTool );
		Components.Create( comp, true );
		
		ActiveTool = Components.Get<BaseTool>();
	}

	public SceneTraceResult TraceTool( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var trace = Scene.Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.IgnoreGameObjectHierarchy( Owner.GameObject )
				.Size( radius );

		var tr = trace.Run();

		return tr;
	}
}
