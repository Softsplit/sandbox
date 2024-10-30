using Sandbox;
using System.Linq;
using System;

namespace Softsplit;

public partial class PhysGun
{
	LegacyParticleSystem Beam;
	LegacyParticleSystem EndNoHit;

	Vector3 lastBeamPos;
	GameObject lastGrabbedObject;

	protected virtual void KillEffects()
	{
		//BeamLight?.Delete();
		Beam?.GameObject.Destroy();
		Beam = null;
		//BeamLight = null;

		EndNoHit?.GameObject.Destroy();
		EndNoHit = null;

		if ( lastGrabbedObject.IsValid() )
		{
			foreach ( var child in lastGrabbedObject.Children )
			{
				if ( !child.Components.Get<ModelRenderer>().IsValid() )
						continue;

				if ( child.Components.TryGet<HighlightOutline>( out var childglow ) )
				{
					childglow.Enabled = false;
				}
			}

			if ( lastGrabbedObject.Components.TryGet<HighlightOutline>( out var glow ) )
			{
				glow.Enabled = false;
			}

			lastGrabbedObject = null;
		}
	}

	protected virtual void UpdateEffects()
	{
		var owner = Owner;

		var startPos = owner.EyeTransform.Position;
		var dir = owner.EyeTransform.Forward;

		var tr = Scene.Trace.Ray( startPos, startPos + dir * MaxTargetDistance )
			.UseHitboxes()
			.IgnoreGameObject( owner.GameObject )
			.WithAllTags( "solid" )
			.Run();

		Beam ??= CreateBeam(tr.EndPosition);

		Beam.WorldPosition = Muzzle.Position;
		Beam.WorldRotation = Muzzle.Rotation;

		if ( GrabbedObject.IsValid() && !GrabbedObject.Tags.Contains("world") )
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
			lastBeamPos = Vector3.Lerp( lastBeamPos, tr.EndPosition, Time.Delta * 10 );
			Beam.SceneObject.SetControlPoint(1,lastBeamPos);

			if ( EndNoHit == null )
				EndNoHit = CreateParticleSystem( "particles/physgun_end_nohit.vpcf", new Transform( lastBeamPos ), 0 );

			EndNoHit.SceneObject.SetControlPoint(0,lastBeamPos);
		}
		
	}

	LegacyParticleSystem CreateBeam(Vector3 endPos)
	{
		LegacyParticleSystem beam = CreateParticleSystem( "particles/physgun_beam.vpcf", new Transform( endPos ), 0);

		return beam;
	}

	void IPlayerEvent.OnDied( Player player )
	{
		if ( player != Owner )
			return;

		KillEffects();
	}
}
