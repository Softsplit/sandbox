using Sandbox.Diagnostics;

public sealed class PlayerInventory : Component, IPlayerEvent
{
	[Property] public int SlotIndex { get; set; }

	[RequireComponent] public PlayerController PlayerController { get; set; }
	[RequireComponent] public Player Player { get; set; }

	public BaseWeapon ActiveWeapon { get; private set; }

	public List<BaseWeapon> Weapons => Scene.Components.GetAll<BaseWeapon>( FindMode.EverythingInSelfAndDescendants ).Where( x => x.Network.OwnerId == Network.OwnerId ).ToList();

	public void GiveDefaultWeapons()
	{
		Pickup( "prefabs/weapons/pistol.prefab" );
		Pickup( "prefabs/weapons/mp5.prefab" );
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
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

	private void Pickup( string prefabName )
	{
		var prefab = GameObject.Clone( prefabName, new CloneConfig { Parent = GameObject, StartEnabled = false } );
		prefab.NetworkSpawn( false, Network.Owner );

		var weapon = prefab.Components.Get<BaseWeapon>( true );
		Assert.NotNull( weapon );

		IPlayerEvent.Post( e => e.OnWeaponAdded( Player, weapon ) );

		weapon.Spawn();

		SetActiveSlot( Weapons.Count - 1 );
	}

	[Broadcast]
	public void SetActiveSlot( int slot )
	{
		SlotIndex = slot;

		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.GameObject.Enabled = false;
		}

		if ( slot >= Weapons.Count )
		{
			SlotIndex = Weapons.Count - 1;
			return;
		}

		ActiveWeapon = Weapons[slot];

		if ( ActiveWeapon.IsValid() )
		{
			ActiveWeapon.GameObject.Enabled = true;
			ActiveWeapon.DoEnabled();
		}
	}

	[Broadcast]
	public void SwitchActiveSlot( int idelta )
	{
		var count = Weapons.Count;
		if ( count == 0 ) return;

		var nextSlot = SlotIndex + idelta;

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

	[ConCmd( "give" )]
	public static void AddWeaponCommand( string weaponName )
	{
		Player.FindLocalPlayer().Components.Get<PlayerInventory>().Pickup( $"prefabs/weapons/{weaponName}.prefab" );
	}
}
