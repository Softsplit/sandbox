using Sandbox;
using Softsplit;

public sealed class EnemyWeaponDealer : Component
{
	[Property] public EquipmentResource WeaponResource {get;set;}
	public Equipment Weapon {get;set;}
	[Property] public GameObject WeaponParent {get;set;}
	[Property] public ShootWeaponComponent Bullet {get;set;}
	[Property] public ReloadWeaponComponent Reload {get;set;}

	protected override void OnStart()
	{
		ChangeWeapon();
	}

	public void ReloadWeapon()
	{
		Reload.ForceInput();
	}

	public bool WeaponHitsTarget(GameObject target)
	{
		var MuzzleNoZ = Weapon.Muzzle.Transform.Position;
		MuzzleNoZ.z = 0;

		var TargetNoZ = target.Transform.Position;
		TargetNoZ.z = 0;

		var trace = Bullet.DoTraceBulletOne(Weapon.Muzzle.Transform.Position,Weapon.Muzzle.Transform.Position+(target.Transform.Position-Weapon.Muzzle.Transform.Position)*1024f, 1);
		
		Gizmo.Draw.Line(Weapon.Muzzle.Transform.Position,Weapon.Muzzle.Transform.Position+Weapon.Muzzle.Transform.World.Forward*1024f);

		return trace.GameObject == target || trace.GameObject.Parent == target;
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
		//Weapon.ModelRenderer = gameObject.Components.Get<SkinnedModelRenderer>();

		/*

		WHY DOESN'T THIS WORK S&BOX WHY
		Bullet = Weapon.Components.GetInChildrenOrSelf<ShootWeaponComponent>();
		Reload = Weapon.Components.GetInChildrenOrSelf<ReloadWeaponComponent>();
		
		*/

		//dumb hack

		IEnumerable<Component> components = Weapon.GameObject.Children[1].Components.GetAll();
		foreach(Component c in components)
		{
			if(c.GetType().ToString() == "Softsplit.ShootWeaponComponent")
			{
				Bullet = (ShootWeaponComponent)c;
				Bullet.NotPlayerControlled = true;
			}
			if(c.GetType().ToString() == "Softsplit.ReloadWeaponComponent")
			{
				Reload = (ReloadWeaponComponent)c;
			}
		}
		


		gameObject.NetworkSpawn();
	}
}
