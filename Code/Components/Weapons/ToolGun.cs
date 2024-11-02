[Spawnable, Library( "tool_gun" )]
public class ToolGun : BaseWeapon
{
	[ConVar( "tool_current" )] public static string CurrentTool { get; set; } = "Weld";
	public BaseTool ActiveTool { get; set; }

	string lastTool;
	protected override void OnEnabled()
	{
		base.OnEnabled();
		UpdateTool();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if(lastTool != CurrentTool)
		{
			UpdateTool();
		}
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( "attack1" );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		var trace = TraceTool( Owner.AimRay.Position, Owner.AimRay.Position + Owner.AimRay.Forward * 5000 );

		if ( !trace.Hit )
			return;

		BroadcastAttack();

		ViewModel?.Renderer?.Set( "b_attack", true );

		//ToolEffects();
		ActiveTool?.Primary( trace );
	}

	[Broadcast]
	private void BroadcastAttack()
	{
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
	}
	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		var trace = TraceTool( Owner.AimRay.Position, Owner.AimRay.Position + Owner.AimRay.Forward * 5000 );

		if ( !trace.Hit )
			return;

		BroadcastAttack();

		ViewModel?.Renderer?.Set( "b_attack", true );

		//ToolEffects();
		ActiveTool?.Secondary( trace );
	}

	public void UpdateTool()
	{
		lastTool = CurrentTool;

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
