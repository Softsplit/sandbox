namespace Sandbox.Tools
{
	public partial class PreviewEntity : ModelRenderer
	{
		[Sync] public bool RelativeToNormal { get; set; } = true;
		[Sync] public bool OffsetBounds { get; set; } = false;
		[Sync] public Rotation RotationOffset { get; set; } = Rotation.Identity;
		[Sync] public Vector3 PositionOffset { get; set; } = Vector3.Zero;

		internal bool UpdateFromTrace( SceneTraceResult tr )
		{
			if ( !IsTraceValid( tr ) )
			{
				return false;
			}

			if ( RelativeToNormal )
			{
				Transform.Rotation = Rotation.LookAt( tr.Normal, tr.Direction ) * RotationOffset;
				Transform.Position = tr.EndPosition + Transform.Rotation * PositionOffset;
			}
			else
			{
				Transform.Rotation = Rotation.Identity * RotationOffset;
				Transform.Position = tr.EndPosition + PositionOffset;
			}

			if ( OffsetBounds )
			{
				Transform.Position += tr.Normal * Model.PhysicsBounds.Size * 0.5f;
			}

			return true;
		}

		protected virtual bool IsTraceValid( SceneTraceResult tr ) => tr.Hit;
	}
}
