using Sandbox;
using System.Linq;
using System;

namespace Softsplit;

public partial class PhysGunComponent
{
	//CapsuleLightEntity? BeamLight;
	LegacyParticleSystem Beam;
	LegacyParticleSystem EndNoHit;

	Vector3 lastBeamPos;
	GameObject lastGrabbedObject;

	protected virtual void KillEffects()
	{
		//BeamLight?.Delete();
		Beam?.GameObject.Destroy();
		Beam = null;
		BeamActive = false;
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

		if ( owner == null || !BeamActive || !owner.GameObject.IsDescendant(GameObject) )
		{
			KillEffects();
			return;
		}

		var startPos = owner.EyeTransform.Position;
		var dir = owner.EyeTransform.Forward;

		var tr = Scene.Trace.Ray( startPos, startPos + dir * MaxTargetDistance )
			.UseHitboxes()
			.IgnoreGameObject( owner.GameObject )
			.WithAllTags( "solid" )
			.Run();

		Beam ??= CreateBeam(tr.EndPosition);

		/*
		if ( !BeamLight.IsValid() )
		{
			BeamLight = new CapsuleLightEntity
			{
				Color = Color.FromBytes( 4, 20, 70 )
			};
		}
		*/

		Beam.WorldPosition = ViewModelMuzzle.WorldPosition;
		Beam.WorldRotation = ViewModelMuzzle.WorldRotation;

		if ( GrabbedObject.IsValid())// && !GrabbedObject.IsWorld )
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
				Beam.SceneObject.SetControlPoint(1,GrabbedObject.WorldPosition);
			}

			lastBeamPos = GrabbedObject.WorldPosition + GrabbedObject.WorldRotation * GrabbedPos;

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
			lastBeamPos = tr.EndPosition;// Vector3.Lerp( lastBeamPos, tr.EndPosition, Time.Delta * 10 );
			Beam.SceneObject.SetControlPoint(1,lastBeamPos);

			if ( EndNoHit == null )
				EndNoHit = Weapon.CreateParticleSystem( "particles/physgun_end_nohit.vpcf", lastBeamPos, Rotation.Identity, -1 );

			EndNoHit.SceneObject.SetControlPoint(0,lastBeamPos);
		}
		/*
		if ( BeamLight.IsValid() )
		{
			var muzzle = IsFirstPersonMode && ViewModelEntity.IsValid ? ViewModelEntity.GetAttachment( "muzzle" ) ?? default : GetAttachment( "muzzle" ) ?? default;
			var pos = muzzle.Position;

			BeamLight.CapsuleLength = ( pos - lastBeamPos ).Length * 0.5f;
			BeamLight.LightSize = 0.5f + ( MathF.Sin( Time.Now * 10 ) * 0.5f);
			BeamLight.Position = ( pos + lastBeamPos ) * 0.5f;
			BeamLight.Rotation = Rotation.LookAt( ( pos - lastBeamPos ).Normal );

			BeamLight.Color = Color.Lerp( BeamLight.Color, new Color( 0.1f, 1.0f, 1.0f, 1.0f ) * 0.039f, Time.Delta * 10 );
		}
		*/
	}

	LegacyParticleSystem CreateBeam(Vector3 endPos)
	{
		LegacyParticleSystem beam = Weapon.CreateParticleSystem( "particles/physgun_beam.vpcf", endPos, Rotation.Identity, -1, false );

		beam.ControlPoints = new();

		ParticleControlPoint mPCP = new ParticleControlPoint
		{
			StringCP = "Point #0",
			Value = ParticleControlPoint.ControlPointValueInput.Vector3,
			VectorValue = ViewModelMuzzle.WorldPosition
		};

		beam.ControlPoints.Add( mPCP );
		
		ParticleControlPoint ePCP = new ParticleControlPoint
		{
			StringCP = "Point #1",
			Value = ParticleControlPoint.ControlPointValueInput.Vector3,
			VectorValue = beam.WorldPosition
		};

		beam.ControlPoints.Add( ePCP );

		return beam;
	}
}
