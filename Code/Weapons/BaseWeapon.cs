using Sandbox.Citizen;

public class BaseWeapon : Component
{
	[Property] public string DisplayName { get; set; } = "My Weapon";
	[Property] public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.HoldItem;
	[Property] public CitizenAnimationHelper.Hand Hand { get; set; } = CitizenAnimationHelper.Hand.Right;
	[Property] public string ParentBoneName { get; set; } = "hold_R";
	[Property] public Transform BoneOffset { get; set; } = new Transform( 0 );

	[Property] public SkinnedModelRenderer WorldModel { get; set; }
	[Property] public SkinnedModelRenderer ViewModel { get; set; }

	[Property] public GameObject WorldModelMuzzle { get; set; }
	[Property] public GameObject ViewModelMuzzle { get; set; }

	public GameObject Muzzle => IsProxy ? WorldModelMuzzle : ViewModelMuzzle;

	GameObject _parentBone;

	public GameObject ParentBone
	{
		get
		{
			if ( !_parentBone.IsValid() )
			{
				_parentBone = Owner.ModelRenderer.GetBoneObject( ParentBoneName );
			}

			return _parentBone;
		}
	}

	public Player Owner { get; set; }
	public bool UseWorldModel => Owner.BodyController.UseWorldModel;

	protected override void OnUpdate()
	{
		Update();

		if(WorldModel.IsValid())
			WorldModel.RenderType = UseWorldModel ? ModelRenderer.ShadowRenderType.On : ModelRenderer.ShadowRenderType.ShadowsOnly;
		
		if(ViewModel.IsValid)
		{
			var renderers = ViewModel.Components.GetAll<ModelRenderer>(FindMode.EverythingInSelfAndDescendants);
			foreach(var renderer in renderers)
			{
				renderer.RenderType = ModelRenderer.ShadowRenderType.Off;
			}
		}
			
		GameObject.NetworkInterpolation = false;

		if ( !Owner.IsValid() )
			return;

		var body = Owner.Body.Components.Get<SkinnedModelRenderer>();
		body.Set( "holdtype", (int)HoldType );
		body.Set( "holdtype_handedness", (int)Hand );

		// TR: From what I see original Sandbox has a sine wave bob
		// AS: Yeah it does.

		var obj = body.GetBoneObject( ParentBoneName );
		if ( obj.IsValid() )
		{
			GameObject.Parent = obj;
			GameObject.LocalTransform = BoneOffset.WithScale( 1f );
		}

		if ( IsProxy )
			return;

		OnControl();
	}

	public virtual void Spawn()
	{
	}

	public virtual void OnControl()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void DoEnabled()
	{
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		DoEnabled();
	}
}
