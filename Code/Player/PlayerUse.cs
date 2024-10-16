public sealed class PlayerUse : Component
{
	[RequireComponent] public PlayerController PlayerController { get; set; }
	[RequireComponent] public Player Player { get; set; }

	IPressable pressed;

	protected override void OnUpdate()
	{
		if ( !Player.Network.IsOwner )
			return;

		var lookingAt = TryGetLookedAt( 0.0f );
		lookingAt ??= TryGetLookedAt( 2.0f );
		lookingAt ??= TryGetLookedAt( 4.0f );
		lookingAt ??= TryGetLookedAt( 8.0f );

		if ( Input.Pressed( "use" ) )
		{
			if ( lookingAt is IPressable button )
			{
				button.Press( new IPressable.Event( this ) );
				pressed = button;
			}
		}

		if ( Input.Released( "use" ) )
		{
			if ( pressed is not null )
			{
				pressed.Release( new IPressable.Event( this ) );
				pressed = default;
			}
		}
	}

	protected override void OnDisabled()
	{
		pressed?.Release( new IPressable.Event( this ) );

		base.OnDisabled();
	}

	object TryGetLookedAt( float radius )
	{
		var eyeTrace = Scene.Trace
						.Ray( Scene.Camera.Transform.World.ForwardRay, 150 )
						.IgnoreGameObjectHierarchy( GameObject )
						.Radius( radius )
						.Run();

		if ( !eyeTrace.Hit ) return default;
		if ( !eyeTrace.GameObject.IsValid() ) return default;

		var button = eyeTrace.GameObject.Components.Get<IPressable>();
		if ( button is not null && button.CanPress( new IPressable.Event( this ) ) ) return button;

		return default;
	}
}
