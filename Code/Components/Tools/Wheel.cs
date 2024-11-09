[Library( "tool_wheel", Description = "A wheel that you can turn on and off (but actually can't yet)", Group = "construction" )]
public class Wheel : BaseTool
{
	PreviewModel PreviewModel;
	RealTimeSince timeSinceDisabled;

	protected override void OnAwake()
	{
		if ( IsProxy )
			return;

		PreviewModel = new PreviewModel
		{
			ModelPath = "models/citizen_props/wheel01.vmdl",
			NormalOffset = 8f,
			RotationOffset = Rotation.From( new Angles( 0, 90, 0 ) ),
			FaceNormal = true
		};
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( timeSinceDisabled < Time.Delta * 5f || !Parent.IsValid() )
			return;

		var trace = Parent.BasicTraceTool();

		PreviewModel.Update( trace );
	}

	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit || !trace.GameObject.IsValid() )
			return false;

		if ( Input.Pressed( "attack1" ) )
		{
			if ( trace.Tags.Contains( "wheel" ) )
				return true;

			var wheel = GameManager.SpawnGetModel( "models/citizen_props/wheel01.vmdl", trace.HitPosition + trace.Normal * 8f, Rotation.LookAt( trace.Normal ) * Rotation.From( new Angles( 0, 90, 0 ) ) );

			PropHelper propHelper = wheel.Components.Get<PropHelper>();
			if ( !propHelper.IsValid() )
				return true;

			propHelper.Hinge( trace.GameObject, trace.EndPosition, trace.Normal );

			return true;
		}

		return false;
	}

	void PositionWheel( GameObject wheel, SceneTraceResult trace )
	{
		wheel.WorldPosition = trace.HitPosition + trace.Normal * 8f;
		wheel.WorldRotation = Rotation.LookAt( trace.Normal ) * Rotation.From( new Angles( 0, 90, 0 ) );
	}

	protected override void OnDestroy()
	{
		PreviewModel?.Destroy();
		base.OnDestroy();
	}

	public override void Disabled()
	{
		timeSinceDisabled = 0;
		PreviewModel?.Destroy();
	}
}
