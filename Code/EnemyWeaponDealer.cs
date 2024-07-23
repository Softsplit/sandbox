using Sandbox;
using Softsplit;

public sealed class EnemyWeaponDealer : Component
{
	[Property] public EquipmentResource WeaponResource {get;set;}
	public Equipment Weapon {get;set;}
	[Property] public GameObject WeaponParent {get;set;}
	[Property] public ShootWeaponComponent Bullet {get;set;}
	[Property] public ReloadWeaponComponent Reload {get;set;}

	SkinnedModelRenderer skinnedModelRenderer {get;set;}

	protected override void OnStart()
	{
		skinnedModelRenderer = Components.Get<SkinnedModelRenderer>();
		ChangeWeapon();
	}

	public void ReloadWeapon()
	{
		Reload.ForceInput();
	}

	public bool WeaponHitsTarget(GameObject target)
	{
		return false;
	}

	public void ChangeWeapon(EquipmentResource weapon = null)
	{
		if(weapon!=null) WeaponResource = weapon;

		var gameObject = WeaponResource.MainPrefab.Clone( new CloneConfig()
		{
			Transform = new(),
			Parent = WeaponParent
		} );

		Weapon = gameObject.Components.Get<Equipment>( FindMode.EverythingInSelfAndDescendants );
		Weapon.ModelRenderer = skinnedModelRenderer;

		Bullet = Weapon.Components.GetInChildrenOrSelf<ShootWeaponComponent>();
		Reload = Weapon.Components.GetInChildrenOrSelf<ReloadWeaponComponent>();

		gameObject.NetworkSpawn();
	}
}
