public partial class PhysGun
{
	LegacyParticleSystem Beam;
	LegacyParticleSystem EndNoHit;

	Vector3 lastBeamPos;
	GameObject lastGrabbedObject;

	[Broadcast]
	protected virtual void KillEffects()
	{
		// BeamLight?.Delete();
		Beam?.GameObject.Destroy();
		Beam = null;
		// BeamLight = null;
		EndNoHit?.GameObject?.Destroy();
		EndNoHit = null;

		DisableHighlights( lastGrabbedObject );
		lastGrabbedObject = null;
	}

	void DisableHighlights( GameObject gameObject )
	{
		if ( gameObject.IsValid() )
		{
			foreach ( var child in gameObject.Children )
			{
				if ( !child.Components.Get<ModelRenderer>().IsValid() )
					continue;

				if ( child.Components.TryGet<HighlightOutline>( out var childglow ) )
				{
					childglow.Enabled = false;
				}
			}

			if ( gameObject.Components.TryGet<HighlightOutline>( out var glow ) )
			{
				glow.Enabled = false;
			}
		}
	}

	protected virtual void UpdateEffects()
	{
		if ( Owner == null || !Beaming || !Owner.GameObject.IsDescendant( GameObject ) )
		{
			KillEffects();
			return;
		}
		if ( grabbed && !GrabbedObject.IsValid() )
		{
			DisableHighlights( lastGrabbedObject );
		}

		var startPos = Owner.EyeTransform.Position;
		var dir = Owner.EyeTransform.Forward;

		var tr = Scene.Trace.Ray( startPos, startPos + dir * MaxTargetDistance )
			.UseHitboxes()
			.IgnoreGameObject( Owner.GameObject )
			.WithAllTags( "solid" )
			.WithoutTags( "player" ) 
			.Run();

		Beam ??= CreateBeam( tr.EndPosition );

		Beam.WorldPosition = Muzzle.Position;
		Beam.WorldRotation = Muzzle.Rotation;

		if ( GrabbedObject.IsValid() && !GrabbedObject.Tags.Contains( "world" ) )
		{
			var physGroup = HeldBody.PhysicsGroup;

			if ( physGroup != null && GrabbedBone >= 0 )
			{
				var physBody = physGroup.GetBody( GrabbedBone );
				if ( physBody != null )
				{
					Beam.SceneObject.SetControlPoint( 1, physBody.Transform.PointToWorld( GrabbedPos ) );
				}
			}
			else
			{
				Beam.SceneObject.SetControlPoint( 1, HeldBody.Transform.PointToWorld( GrabbedPos ) );
			}

			lastBeamPos = HeldBody.Position + HeldBody.Rotation * GrabbedPos;

			EndNoHit?.GameObject.Destroy();
			EndNoHit = null;

			if ( GrabbedObject.Components.Get<ModelRenderer>().IsValid() )
			{
				lastGrabbedObject = GrabbedObject;

				var glow = GrabbedObject.Components.GetOrCreate<HighlightOutline>();
				glow.Enabled = true;
				glow.Width = 0.25f;
				glow.Color = new Color( 4f, 50.0f, 70.0f, 1.0f );
				glow.ObscuredColor = new Color( 4f, 50.0f, 70.0f, 0.0005f );

				foreach ( var child in lastGrabbedObject.Children )
				{
					if ( !child.Components.Get<ModelRenderer>().IsValid() )
						continue;

					glow = child.Components.GetOrCreate<HighlightOutline>();
					glow.Enabled = true;
					glow.Color = new Color( 0.1f, 1.0f, 1.0f, 1.0f );
				}
			}
		}
		else
		{
			lastBeamPos = tr.EndPosition; Vector3.Lerp( lastBeamPos, tr.EndPosition, Time.Delta * 10 );
			Beam.SceneObject.SetControlPoint( 1, lastBeamPos );

			EndNoHit ??= CreateParticleSystem( "particles/physgun_end_nohit.vpcf", new Transform( lastBeamPos ), 0 );

			EndNoHit.SceneObject.SetControlPoint( 0, lastBeamPos );
			EndNoHit.WorldPosition = lastBeamPos;
		}

	}

	LegacyParticleSystem CreateBeam( Vector3 endPos )
	{
		LegacyParticleSystem beam = CreateParticleSystem( "particles/physgun_beam.vpcf", new Transform( endPos ), 0 );

		return beam;
	}

	void IPlayerEvent.OnDied()
	{
		KillEffects();
	}

	void FreezeEffects()
	{
		CreateParticleSystem( "particles/physgun_freeze.vpcf", new Transform( lastBeamPos ), 4 );
	}
}
