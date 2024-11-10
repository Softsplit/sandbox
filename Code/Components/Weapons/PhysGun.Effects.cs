public partial class PhysGun
{
	LegacyParticleSystem beam;
	LegacyParticleSystem endNoHit;

	GameObject lastGrabbedObject;

	[Broadcast]
	protected virtual void KillEffects()
	{
		// beamLight?.Destroy();
		beam?.GameObject.Destroy();
		beam = null;
		// beamLight = null;
		endNoHit?.GameObject?.Destroy();
		endNoHit = null;

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

	Vector3 lastBeamPos;

	protected virtual void UpdateEffects()
	{
		if ( !Owner.IsValid() || !Beaming || !Owner.GameObject.IsDescendant( GameObject ) )
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

		beam ??= CreateBeam( tr.EndPosition );

		if ( beam.IsValid() )
		{
			beam.WorldPosition = Attachment( "muzzle" ).Position;
			beam.WorldRotation = Attachment( "muzzle" ).Rotation;
		}

		if ( GrabbedObject.IsValid() && !GrabbedObject.Tags.Contains( "world" ) && HeldBody.IsValid() )
		{
			var physGroup = HeldBody.PhysicsGroup;

			if ( physGroup != null && GrabbedBone >= 0 )
			{
				var physBody = physGroup.GetBody( GrabbedBone );
				if ( physBody != null )
				{
					beam?.SceneObject.SetControlPoint( 1, physBody.Transform.PointToWorld( GrabbedPos ) );
				}
			}
			else
			{
				beam?.SceneObject.SetControlPoint( 1, HeldBody.Transform.PointToWorld( GrabbedPos ) );
			}

			lastBeamPos = HeldBody.Position + HeldBody.Rotation * GrabbedPos;

			endNoHit?.GameObject.Destroy();
			endNoHit = null;

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

			beam?.SceneObject.SetControlPoint( 1, lastBeamPos );

			endNoHit ??= Particles.MakeParticleSystem( "particles/physgun_end_nohit.vpcf", new Transform( lastBeamPos ), 0 );

			endNoHit.SceneObject.SetControlPoint( 0, lastBeamPos );
			endNoHit.WorldPosition = lastBeamPos;
		}
	}

	LegacyParticleSystem CreateBeam( Vector3 endPos )
	{
		LegacyParticleSystem beam = Particles.MakeParticleSystem( "particles/physgun_beam.vpcf", new Transform( endPos ), 0 );

		return beam;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsProxy )
			return;

		KillEffects();
	}

	void INetworkListener.OnDisconnected( Connection channel )
	{
		KillEffects();
	}

	void FreezeEffects()
	{
		Particles.MakeParticleSystem( "particles/physgun_freeze.vpcf", new Transform( lastBeamPos ), 4 );
	}
}
