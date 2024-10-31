using Sandbox.Diagnostics;

public sealed class PlayerInventory : Component, IPlayerEvent
{
	[RequireComponent] public Player Player { get; set; }

	[Sync] public List<BaseWeapon> Weapons { get; set; } = new();
	[Sync] public BaseWeapon ActiveWeapon { get; set; }

	public void GiveDefaultWeapons()
	{
		Pickup( "prefabs/weapons/physgun/w_physgun.prefab", true );
		Pickup( "prefabs/weapons/fists/w_fists.prefab", true );
		Pickup( "prefabs/weapons/flashlight/w_flashlight.prefab", false );
		Pickup( "prefabs/weapons/pistol/w_pistol.prefab", false );
		Pickup( "prefabs/weapons/mp5/w_mp5.prefab", false );
		Pickup( "prefabs/weapons/shotgun/w_shotgun.prefab", false );
		Pickup( "prefabs/weapons/toolgun/w_toolgun.prefab", false );
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( ActiveWeapon is PhysGun physgun && physgun.GrabbedObject.IsValid() )
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

		if ( Input.MouseWheel != 0 ) SwitchActiveSlot( (int)Input.MouseWheel.y );
	}

	private void Pickup( string prefabName )
	{
		var prefab = GameObject.Clone( prefabName, new CloneConfig { Parent = GameObject, StartEnabled = false } );
		prefab.NetworkSpawn( false, Network.Owner );

		var weapon = prefab.Components.Get<BaseWeapon>( true );
		Assert.NotNull( weapon );

		BroadcastPickup( weapon );

		IPlayerEvent.Post( e => e.OnWeaponAdded( Player, weapon ) );
	}

	[Broadcast]
	private void BroadcastPickup( BaseWeapon weapon )
	{
		Weapons.Add( weapon );
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
			ActiveWeapon.GameObject.Enabled = true;
	}

	public BaseWeapon GetSlot( int i )
	{
		if ( Weapons.Count <= i ) return null;
		if ( i < 0 ) return null;

		return Weapons[i];
	}

	public int GetActiveSlot()
	{
		var aw = ActiveWeapon;
		var count = Weapons.Count;

		for ( int i = 0; i < count; i++ )
		{
			if ( Weapons[i] == aw )
				return i;
		}

		return -1;
	}

	public void SwitchActiveSlot( int idelta )
	{
		var count = Weapons.Count;
		if ( count == 0 ) return;

		var slot = GetActiveSlot();
		var nextSlot = slot + idelta;

		while ( nextSlot < 0 ) nextSlot += count;
		while ( nextSlot >= count ) nextSlot -= count;

		SetActiveSlot( nextSlot );
	}

	void IPlayerEvent.OnSpawned( Player player )
	{
		if ( player != Player )
			return;

		GiveDefaultWeapons();
		SetActiveSlot( 0 );
	}

	void IPlayerEvent.OnDied( Player player )
	{
		if ( player != Player )
			return;

		Weapons.ForEach( x => x.DestroyGameObject() );
	}

	[ConCmd ("select_weapon")]
	public static void Select_Weapon(string name)
	{
		PlayerInventory pI = Player.FindLocalPlayer().Inventory;
		for(int i = 0; i < pI.Weapons.Count; i++)
		{
			if( DisplayInfo.For(pI.Weapons[i]).Name != name)
				continue;
			pI.SetActiveSlot(i);
			return;
		}
	}
}
