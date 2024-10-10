using Sandbox.Citizen;

public class BaseWeapon : Component
{
	[Property] public string DisplayName { get; set; } = "My Weapon";
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.HoldItem;
	[Property] public string ParentBone { get; set; } = "hold_r";
	[Property] public Transform BoneOffset { get; set; } = new Transform( 0 );

	protected override void OnUpdate()
	{
		GameObject.NetworkInterpolation = false;

		var owner = GameObject.Components.GetInAncestorsOrSelf<Player>();
		if ( !owner.IsValid() ) return;

		var body = owner.Body.Components.Get<SkinnedModelRenderer>();
		body.Set( "holdtype", (int)HoldType );

		var obj = body.GetBoneObject( ParentBone );
		if ( obj.IsValid() )
		{
			GameObject.Parent = obj;
			GameObject.LocalTransform = BoneOffset.WithScale( 1 );
		}

		if ( IsProxy )
			return;

		OnControl( owner );
	}

	public virtual void OnControl( Player player )
	{
	}
}
