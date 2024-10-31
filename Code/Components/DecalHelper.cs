/// <summary>
/// A component that makes sure a decal is attached to a physics body.
/// Good enough until we get official support for decals on bones.
/// </summary>
public sealed class DecalHelper : Component
{
	/// <summary>
	/// The physics body to parent to.
	/// </summary>
	public PhysicsBody Body { get; set; }

	/// <summary>
	/// The offset from the physics body.
	/// </summary>
	[Property] public Transform Offset { get; set; }

	protected override void OnStart()
	{
		Offset = Body.Transform.ToLocal( WorldTransform );

		Body.GetComponent().Transform.OnTransformChanged += AttachToBody;
	}

	protected override void OnDestroy()
	{
		if ( !Body.IsValid() )
		{
			return;
		}
		
		Body.GetComponent().Transform.OnTransformChanged -= AttachToBody;
	}

	void AttachToBody()
	{
		WorldTransform = Body.Transform.ToWorld( Offset );
	}
}
