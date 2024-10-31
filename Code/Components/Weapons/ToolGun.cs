[Library( "tool_gun", Title = "Tool Gun" )]
public class ToolGun : BaseWeapon
{
	[ConVar( "tool_current" )] public static string CurrentTool { get; set; } = "Weld";
	public BaseTool ActiveTool {get;set;}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		UpdateTool();
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;

		var trace = TraceTool(Owner.AimRay.Position,Owner.AimRay.Position+Owner.AimRay.Forward*5000);

		if(!trace.Hit)
			return;

		ViewModel?.Renderer?.Set( "b_attack", true );
		ToolEffects();
		//Sound.Play( "weapons/rust_pistol/sound/rust_pistol.shoot.sound", Muzzle.WorldPosition );
		ActiveTool?.Primary(trace);
	}

	[Broadcast]
	void ToolEffects()
	{
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
	}
	public override void AttackSecondary()
	{
		TimeSinceSecondaryAttack = 0;

		var trace = TraceTool(Owner.AimRay.Position,Owner.AimRay.Position+Owner.AimRay.Forward*5000);

		if(!trace.Hit)
			return;

		ViewModel?.Renderer?.Set( "b_attack", true );
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
