using Sandbox.Diagnostics;

public sealed class PlayerInventory : Component, IPlayerEvent
{
	[RequireComponent] public Player Player { get; set; }

	public bool lockSwitch;

	public BaseWeapon ActiveWeapon { get; private set; }

	[Sync] public List<BaseWeapon> Weapons { get; set; } = new();// => Scene.Components.GetAll<BaseWeapon>( FindMode.EverythingInSelfAndDescendants ).Where( x => x.Network.OwnerId == Network.OwnerId ).ToList();

	public void GiveDefaultWeapons()
	{
		Pickup( "prefabs/weapons/physgun.prefab", false );
		Pickup( "prefabs/weapons/toolgun.prefab", false );
		Pickup( "prefabs/weapons/pistol.prefab", false );
		Pickup( "prefabs/weapons/flashlight.prefab", false );
		Pickup( "prefabs/weapons/fists.prefab", false );
		Pickup( "prefabs/weapons/mp5.prefab", false );
	}

	protected override void OnUpdate()
	{
		if ( IsProxy || lockSwitch)
			return;

		if ( Input.Pressed( "slot1" ) ) SetActiveSlot( 0 );
		if ( Input.Pressed( "slot2" ) ) SetActiveSlot( 1 );
		if ( Input.Pressed( "slot3" ) ) SetActiveSlot( 2 );
		if ( Input.Pressed( "slot4" ) ) SetActiveSlot( 3 );
		if ( Input.Pressed( "slot5" ) ) SetActiveSlot( 4 );
		if ( Input.Pressed( "slot6" ) ) SetActiveSlot( 5 );
		if ( Input.Pressed( "slot7" ) ) SetActiveSlot( 6 );
		if ( Input.Pressed( "slot8" ) ) SetActiveSlot( 7 );
		if ( Input.Pressed( "slot9" ) ) SetActiveSlot( 8 );

		if ( Input.MouseWheel != 0 ) SwitchActiveSlot( (int)-Input.MouseWheel.y );
	}

	private void Pickup( string prefabName, bool equip = true )
	{
		var prefab = GameObject.Clone( prefabName, new CloneConfig { Parent = GameObject, StartEnabled = false } );
		prefab.NetworkSpawn( false, Network.Owner );

		var weapon = prefab.Components.Get<BaseWeapon>( true );
		weapon.Owner = Player;
		
		Assert.NotNull( weapon );

		IPlayerEvent.Post( e => e.OnWeaponAdded( Player, weapon ) );

		weapon.Spawn();

		Weapons.Add(weapon);

		if(equip)
			SetActiveSlot( Weapons.Count - 1 );
	}

	[Broadcast]
	public void SetActiveSlot( int i )
	{
		var weapon = GetSlot( i );
		if ( ActiveWeapon == weapon )
			return;

		if ( weapon == null )
			return;

		if ( ActiveWeapon.IsValid() )
			ActiveWeapon.GameObject.Enabled = false;

		ActiveWeapon = weapon;

		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.GameObject.Enabled = true;
			ActiveWeapon.DoEnabled();
		}
	}

	public BaseWeapon GetSlot( int i )
	{
		if ( Weapons.Count <= i ) return null;
		if ( i < 0 ) return null;

		return Weapons[i];
	}

	[Broadcast]
	public void SwitchActiveSlot( int idelta )
	{
		var count = Weapons.Count;
		if ( count == 0 ) return;

		var nextSlot = Weapons.IndexOf( ActiveWeapon ) + idelta;

		while ( nextSlot < 0 ) nextSlot += count;
		while ( nextSlot >= count ) nextSlot -= count;

		SetActiveSlot( nextSlot );
	}

	void IPlayerEvent.OnSpawned( Player player )
	{
		if ( player != Player )
			return;

		GiveDefaultWeapons();
	}

	public void SelectWeaponByName(string weaponName)
	{
		for(int i = 0; i < Weapons.Count; i++)
		{
			if(Weapons[i].GameObject.Name == weaponName)
			{
				SetActiveSlot(i);
				return;
			}
		}
	}

	[ConCmd( "give" )]
	public static void AddWeaponCommand( string weaponName )
	{
		Player.FindLocalPlayer().Components.Get<PlayerInventory>().Pickup( $"prefabs/weapons/{weaponName}.prefab" );
	}

	[ConCmd( "select" )]
	public static void SelectWeapon( string weaponName )
	{
		Player.FindLocalPlayer().Components.Get<PlayerInventory>().Pickup( $"prefabs/weapons/{weaponName}.prefab" );
	}
}
