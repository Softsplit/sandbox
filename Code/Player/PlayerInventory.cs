using Sandbox.Diagnostics;

namespace Softsplit;

/// <summary>
/// The player's inventory.
/// </summary>
public partial class PlayerInventory : Component
{
	[RequireComponent] PlayerPawn Player { get; set; }

	/// <summary>
	/// What equipment do we have right now?
	/// </summary>
	public IEnumerable<Equipment> Equipment => Player.Components.GetAll<Equipment>( FindMode.EverythingInSelfAndDescendants );

	/// <summary>
	/// A <see cref="GameObject"/> that will hold all of our equipment.
	/// </summary>
	[Property] public GameObject WeaponGameObject { get; set; }

	/// <summary>
	/// Can we unequip the current weapon so we have no equipment out?
	/// </summary>
	[Property] public bool CanUnequipCurrentWeapon { get; set; } = false;

	/// <summary>
	/// Gets the player's current weapon.
	/// </summary>
	private Equipment Current => Player.CurrentEquipment;

	public void Clear()
	{
		if ( !Networking.IsHost )
			return;

		foreach ( var wpn in Equipment )
		{
			wpn.GameObject.Destroy();
			wpn.Enabled = false;
		}
	}

	[Authority( NetPermission.HostOnly )]
	public void RefillAmmo()
	{
		foreach ( var wpn in Equipment )
		{
			if ( wpn.Components.Get<AmmoComponent>( FindMode.EnabledInSelfAndDescendants ) is { } ammo )
			{
				ammo.Ammo = ammo.MaxAmmo;
			}
		}
	}

	/// <summary>
	/// Try to drop the given held equipment item.
	/// </summary>
	/// <param name="weapon">Item to drop.</param>
	/// <param name="forceRemove">If we can't drop, remove it from the inventory anyway.</param>
	public void Drop( Equipment weapon, bool forceRemove = false )
	{
		using ( Rpc.FilterInclude( Connection.Host ) )
		{
			DropHost( weapon, forceRemove );
		}
	}

	[Broadcast]
	private void DropHost( Equipment weapon, bool forceRemove )
	{
		if ( !Networking.IsHost )
			return;

		if ( !weapon.IsValid() )
			return;

		var canDrop = GameMode.Instance.Get<EquipmentDropper>() is { };

		if ( canDrop )
		{
			var tr = Scene.Trace.Ray( new Ray( Player.AimRay.Position, Player.AimRay.Forward ), 128 )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "trigger" )
				.Run();

			var position = tr.Hit ? tr.HitPosition + tr.Normal * weapon.Resource.WorldModel.Bounds.Size.Length : Player.AimRay.Position + Player.AimRay.Forward * 32f;
			var rotation = Rotation.From( 0, Player.EyeAngles.yaw + 90, 90 );

			var baseVelocity = Player.CharacterController.Velocity;
			var droppedWeapon = DroppedEquipment.Create( weapon.Resource, position, rotation, weapon );

			if ( !tr.Hit )
			{
				droppedWeapon.Rigidbody.Velocity = baseVelocity + Player.AimRay.Forward * 200.0f + Vector3.Up * 50;
				droppedWeapon.Rigidbody.AngularVelocity = Vector3.Random * 8.0f;
			}
		}

		if ( canDrop || forceRemove )
		{
			RemoveWeapon( weapon );
		}
	}

	protected override void OnUpdate()
	{
		if ( !Player.IsLocallyControlled )
			return;

		if ( Input.Pressed( "Drop" ) && Current.IsValid() )
		{
			Drop( Current );
			return;
		}

		for ( int i = 0; i < Equipment.ToList().Count; i++ )
		{
			if ( !Input.Pressed( $"Slot{i + 1}" ) )
				continue;

			Switch( Equipment.ToList()[i] );
			return;
		}

		var wheel = Input.MouseWheel;

		// gamepad input
		if ( Input.Pressed( "SlotNext" ) ) wheel.y = -1;
		if ( Input.Pressed( "SlotPrev" ) ) wheel.y = 1;

		if ( wheel.y == 0f ) return;

		var availableWeapons = Equipment.ToList();
		if ( availableWeapons.Count == 0 )
			return;

		var currentSlot = 0;
		for ( var index = 0; index < availableWeapons.Count; index++ )
		{
			var weapon = availableWeapons[index];
			if ( !weapon.IsDeployed )
				continue;

			currentSlot = index;
			break;
		}

		var slotDelta = wheel.y > 0f ? 1 : -1;
		currentSlot += slotDelta;

		if ( currentSlot < 0 )
			currentSlot = availableWeapons.Count - 1;
		else if ( currentSlot >= availableWeapons.Count )
			currentSlot = 0;

		var weaponToSwitchTo = availableWeapons[currentSlot];
		if ( weaponToSwitchTo == Current )
			return;

		Switch( weaponToSwitchTo );
	}

	public void SwitchToBest()
	{
		if ( !Equipment.Any() )
			return;

		Switch( Equipment.FirstOrDefault() );
	}

	public void HolsterCurrent()
	{
		Assert.True( !IsProxy || Networking.IsHost );
		Player.SetCurrentEquipment( null );
	}

	/// <summary>
	/// Tries to set the player's current weapon to a specific one, which has to be in the player's inventory.
	/// </summary>
	/// <param name="equipment"></param>
	public void Switch( Equipment equipment )
	{
		Assert.True( !IsProxy || Networking.IsHost );

		if ( !Equipment.Contains( equipment ) )
			return;

		Player.SetCurrentEquipment( equipment );
	}

	/// <summary>
	/// Removes the given weapon and destroys it.
	/// </summary>
	public void RemoveWeapon( Equipment equipment )
	{
		Assert.True( Networking.IsHost );

		if ( !Equipment.Contains( equipment ) ) return;

		if ( Current == equipment )
		{
			var otherEquipment = Equipment.Where( x => x != equipment );
			var orderedBySlot = otherEquipment;
			var targetWeapon = orderedBySlot.FirstOrDefault();

			if ( targetWeapon.IsValid() )
				Switch( targetWeapon );
		}

		equipment.GameObject.Destroy();
		equipment.Enabled = false;
	}

	/// <summary>
	/// Removes the given weapon (by its resource data) and destroys it.
	/// </summary>
	public void Remove( EquipmentResource resource )
	{
		var equipment = Equipment.FirstOrDefault( w => w.Resource == resource );
		if ( !equipment.IsValid() ) return;
		RemoveWeapon( equipment );
	}

	public Equipment Give( EquipmentResource resource, bool makeActive = true )
	{
		Assert.True( Networking.IsHost );

		// If we're in charge, let's make some equipment.
		if ( resource == null )
		{
			Log.Warning( "A player loadout without a equipment? Nonsense." );
			return null;
		}

		var pickupResult = CanTake( resource );

		if ( pickupResult == PickupResult.None )
			return null;

		// Don't let us have the exact same equipment
		if ( Has( resource ) )
			return null;

		if ( pickupResult == PickupResult.Swap )
		{
			var slotCurrent = Equipment.FirstOrDefault( equipment => equipment.Enabled );
			if ( slotCurrent.IsValid() )
				Drop( slotCurrent, true );
		}

		if ( !resource.MainPrefab.IsValid() )
		{
			Log.Error( $"equipment doesn't have a prefab? {resource}, {resource.MainPrefab}, {resource.ViewModelPrefab}" );
			return null;
		}

		// Create the equipment prefab and put it on the GameObject.
		var gameObject = resource.MainPrefab.Clone( new CloneConfig()
		{
			Transform = new(),
			Parent = WeaponGameObject
		} );
		var component = gameObject.Components.Get<Equipment>( FindMode.EverythingInSelfAndDescendants );
		gameObject.NetworkSpawn( Player.Network.OwnerConnection );
		component.OwnerId = Player.Id;

		if ( makeActive )
			Player.SetCurrentEquipment( component );

		return component;
	}
	public Equipment GiveTool( EquipmentResource resource, TypeDescription type, bool makeActive = true )
	{
		// Assert.True( Networking.IsHost ); idk what is doing
		// TODO: clean this code

		// If we're in charge, let's make some equipment.
		if ( type == null )
		{
			Log.Warning( "A player loadout without a equipment? Nonsense." );
			return null;
		}

		// Don't let us have the exact same equipment

		var pickupResult = CanTake( resource );

		if ( pickupResult == PickupResult.None )
			return null;

		if ( pickupResult == PickupResult.Swap )
		{
			var slotCurrent = Equipment.FirstOrDefault( equipment => equipment.Enabled );
			if ( slotCurrent.IsValid() )
				Drop( slotCurrent, true );
		}
		if ( !resource.MainPrefab.IsValid() )
		{
			Log.Error( $"equipment doesn't have a prefab? {resource}, {resource.MainPrefab}, {resource.ViewModelPrefab}" );
			return null;
		}
		// Create the equipment prefab and put it on the GameObject.
		var gameObject = resource.MainPrefab.Clone( new CloneConfig()
		{
			Transform = new(),
			Parent = WeaponGameObject
		} );
		var component = gameObject.Components.Create( type, false ) as Tools.Tool;
		gameObject.NetworkSpawn( Player.Network.OwnerConnection );
		gameObject.Parent = WeaponGameObject;
		component.OwnerId = Player.Id;
		component.Resource = resource;
		component.ModelRenderer = gameObject.Components.Get<SkinnedModelRenderer>( FindMode.EnabledInSelfAndChildren );
		component.Enabled = true;

		if ( makeActive )
			Player.SetCurrentEquipment( component );

		/*if ( component.Resource.Slot == EquipmentSlot.Special )
		{
			// Nothing
		}*/

		return component;
	}

	public bool Has( EquipmentResource resource )
	{
		return Equipment.Any( weapon => weapon.Enabled && weapon.Resource == resource );
	}

	public enum PickupResult
	{
		None,
		Pickup,
		Swap
	}

	public PickupResult CanTake( EquipmentResource resource )
	{
		return !Has( resource ) ? PickupResult.Pickup : PickupResult.Swap;
	}
}
