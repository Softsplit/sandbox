[Library( "tool_leafblower", Title = "Leaf Blower", Description = "Blow me", Group = "fun" )]
public partial class LeafBlowerTool : BaseTool
{
	protected virtual float Force => 128;
	protected virtual float MaxDistance => 512;
	protected virtual bool Massless => true;

	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( !trace.GameObject.IsValid() )
			return false;

		if ( trace.Component is MapCollider )
			return false;

		var body = trace.Body;
		if ( !body.IsValid() )
			return false;

		var ratio = (1.0f - (trace.Direction / MaxDistance)).Clamp( 0, 1 ) * 1.0f;
		var force = trace.Direction * (Force * ratio);

		if ( Massless )
		{
			force *= body.Mass;
		}

		if ( trace.GameObject.Components.TryGet<PropHelper>( out var propHelper ) )
			propHelper.BroadcastAddForce( body.GroupIndex, force );

		return false;
	}

	public override bool Secondary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( !trace.GameObject.IsValid() )
			return false;

		if ( trace.Component is MapCollider )
			return false;

		var body = trace.Body;
		if ( !body.IsValid() )
			return false;

		var ratio = (1.0f - (trace.Direction / MaxDistance)).Clamp( 0, 1 ) * -1.0f;
		var force = trace.Direction * (Force * ratio);

		if ( Massless )
		{
			force *= body.Mass;
		}

		if ( trace.GameObject.Components.TryGet<PropHelper>( out var propHelper ) )
			propHelper.BroadcastAddForce( body.GroupIndex, force );

		return false;
	}
}
