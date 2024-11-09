[Library( "tool_leafblower", Title = "Leaf Blower", Description = "Blow me", Group = "fun" )]
public partial class LeafBlowerTool : BaseTool
{
	protected virtual float Force => 128;
	protected virtual float MaxDistance => 512;
	protected virtual bool Massless => true;

	public override bool Primary( SceneTraceResult trace )
	{
		bool push = Input.Down( "attack1" );
		if ( !push && !Input.Down( "attack2" ) )
			return false;

		if ( !trace.Hit )
			return false;

		if ( !trace.GameObject.IsValid() )
			return false;

		if ( trace.Component is MapCollider )
			return false;

		var body = trace.Body;
		if ( !body.IsValid() )
			return false;

		var direction = trace.EndPosition - trace.StartPosition;
		var distance = direction.Length;
		var ratio = (1.0f - (distance / MaxDistance)).Clamp( 0, 1 ) * (push ? 1.0f : -1.0f);
		var force = direction * (Force * ratio);

		if ( Massless )
		{
			force *= body.Mass;
		}

		body.ApplyForceAt( trace.EndPosition, force );

		return false;
	}
}
