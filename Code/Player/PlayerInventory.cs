using Sandbox.Citizen;
using Sandbox.Diagnostics;

public sealed class PlayerInventory : Component, IPlayerEvent
{
	[RequireComponent] public PlayerController PlayerController { get; set; }
	[RequireComponent] public Player Player { get; set; }
	[Property] public int SlotIndex {get;set;}

	public BaseWeapon ActiveWeapon { get; private set; }

	public List<BaseWeapon> Weapons => Scene.Components.GetAll<BaseWeapon>( FindMode.EverythingInSelfAndDescendants ).Where( x => x.Network.OwnerId == Network.OwnerId ).ToList();

	public void GiveDefaultWeapons()
	{
	}

	protected override void OnFixedUpdate()
	{
		if(IsProxy)
			return;
		
		for(int i = 1; i < 9; i++)
		{
			if(!Input.Pressed($"Slot{i}")) continue;
			SwitchWeapon(i-1);
			break;
		}
	}

	public int WeaponsMod = 0;
	void Pickup( string prefabName )
	{
		WeaponsMod++;
		var prefab = GameObject.Clone( prefabName, new CloneConfig { Parent = GameObject, StartEnabled = false } );
		prefab.NetworkSpawn( false, Network.Owner );

		var weapon = prefab.Components.Get<BaseWeapon>( true );
		Assert.NotNull( weapon );

		IPlayerEvent.Post( e => e.OnWeaponAdded( Player, weapon ) );

		weapon.Spawn();
	}

	[Broadcast]
	public void SwitchWeapon( int Slot )
	{
		SlotIndex = Slot;
		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.GameObject.Enabled = false;
		}

		if(Slot >= Weapons.Count)
		{
			Player.ModelRenderer.Set( "holdtype", (int)CitizenAnimationHelper.HoldTypes.None );
			return;
		}

		ActiveWeapon = Weapons[Slot];

		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.GameObject.Enabled = true;
			ActiveWeapon.DoEnabled();
		}
	}

	void IPlayerEvent.OnSpawned( Player player )
	{
		if ( player != Player )
			return;

		GiveDefaultWeapons();
	}

	[ConCmd( "Add_Weapon" )]
	public static void AddWeaponCommand(string weaponName)
	{
		Player.FindLocalPlayer().Components.Get<PlayerInventory>().Pickup($"prefabs/weapons/{weaponName}.prefab");
	}
}
