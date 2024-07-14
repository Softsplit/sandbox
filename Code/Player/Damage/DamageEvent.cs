using Sandbox.Events;

namespace Softsplit;

[Flags]
public enum HitboxTags
{
	None = 0,
	Head = 1,
	Chest = 2,
	Stomach = 4,
	Clavicle = 8,
	Arm = 16,
	Hand = 32,
	Leg = 64,
	Ankle = 128,
	Spine = 256,
	Neck = 512,

	UpperBody = Neck | Chest | Clavicle,
	LowerBody = Stomach
}

[Flags]
public enum DamageFlags
{
	None = 0,

	/// <summary>
	/// This was a knife attack.
	/// </summary>
	Melee = 1,

	/// <summary>
	/// This was some kind of explosion.
	/// </summary>
	Explosion = 2,

	/// <summary>
	/// The victim fell.
	/// </summary>
	FallDamage = 4,

	/// <summary>
	/// The victim was burned.
	/// </summary>
	Burn = 8,

	/// <summary>
	/// Was the attacker in the air when doing this damage?
	/// </summary>
	AirShot = 16,
}

/// <summary>
/// Event dispatched when a <see cref="HealthComponent"/> takes damage.
/// </summary>
/// <param name="Attacker">Who was the attacker?</param>
/// <param name="Damage">How much damage?</param>
/// <param name="Inflictor">What caused this damage? Can be a weapon, grenade, etc.</param>
/// <param name="Position">The point of the damage. Normally where you were hit.</param>
/// <param name="Force">The force of the damage.</param>
/// <param name="Hitbox">What hitbox did we hit?</param>
/// <param name="Flags">Extra data that we can pass around. Like if it's a blind-shot, mid-air shot, through smoke shot, etc.</param>
public record DamageInfo( Component Attacker, float Damage, Component Inflictor = null,
	Vector3 Position = default, Vector3 Force = default,
	HitboxTags Hitbox = default, DamageFlags Flags = DamageFlags.None )
{
	/// <summary>
	/// Who took damage?
	/// </summary>
	public Component Victim { get; init; }

	/// <inheritdoc cref="DamageFlags.Melee"/>
	public bool WasMelee => Flags.HasFlag( DamageFlags.Melee );

	/// <inheritdoc cref="DamageFlags.Explosion"/>
	public bool WasExplosion => Flags.HasFlag( DamageFlags.Explosion );

	/// <inheritdoc cref="DamageFlags.FallDamage"/>
	public bool WasFallDamage => Flags.HasFlag( DamageFlags.FallDamage );

	/// <summary>
	/// How long since this damage info event happened?
	/// </summary>
	public RealTimeSince TimeSinceEvent { get; init; } = 0;

	public override string ToString()
	{
		return $"\"{Attacker}\" - \"{Victim}\" with \"{Inflictor}\" ({Damage} damage)";
	}
}

/// <summary>
/// Event dispatched on the host when something takes damage, so it can be modified.
/// </summary>
public abstract class ModifyDamageEvent : IGameEvent
{
	public DamageInfo DamageInfo { get; set; }

	/// <summary>
	/// Clears all health and armor damage from this event.
	/// </summary>
	public void ClearDamage()
	{
		DamageInfo = DamageInfo with { Damage = 0f };
	}

	/// <summary>
	/// Scales health damage by the given multiplier.
	/// </summary>
	public void ScaleDamage( float scale )
	{
		DamageInfo = DamageInfo with { Damage = DamageInfo.Damage * scale };
	}

	/// <summary>
	/// Adds a flag to this damage event.
	/// </summary>
	/// <param name="flag"></param>
	public void AddFlag( DamageFlags flag )
	{
		DamageInfo = DamageInfo with { Flags = DamageInfo.Flags | flag };
	}

	/// <summary>
	/// Removes a flag from this damage event.
	/// </summary>
	/// <param name="flag"></param>
	public void WithoutFlag( DamageFlags flag )
	{
		DamageInfo = DamageInfo with { Flags = DamageInfo.Flags & flag };
	}
}

/// <summary>
/// Event dispatched on the host when this object is about to take damage, so it can be modified.
/// </summary>
public class ModifyDamageTakenEvent : ModifyDamageEvent
{
}

/// <summary>
/// Event dispatched on the host when this object is about to deal damage, so it can be modified.
/// </summary>
public class ModifyDamageGivenEvent : ModifyDamageEvent
{
}

/// <summary>
/// Event dispatched on the host when any object is about to deal damage, so it can be modified.
/// </summary>
public class ModifyDamageGlobalEvent : ModifyDamageEvent
{

}

/// <summary>
/// Event dispatched on a root object containing a <see cref="HealthComponent"/> that took damage.
/// </summary>
/// <param name="Damage">Information about the damage.</param>
public record DamageTakenEvent( DamageInfo DamageInfo ) : IGameEvent;

/// <summary>
/// Event dispatched to everything when a <see cref="HealthComponent"/> takes damage.
/// </summary>
/// <param name="DamageInfo"></param>
public record DamageTakenGlobalEvent( DamageInfo DamageInfo ) : IGameEvent;

/// <summary>
/// Event dispatched on a root object that inflicted damage on another object.
/// </summary>
/// <param name="Damage">Information about the damage.</param>
public record DamageGivenEvent( DamageInfo DamageInfo ) : IGameEvent;

/// <summary>
/// Event dispatched in the scene when a <see cref="HealthComponent"/> died after taking damage.
/// </summary>
/// <param name="Damage">Information about the killing blow.</param>
public record KillEvent( DamageInfo DamageInfo ) : IGameEvent;

public static class SceneTraceExtensions
{
	public static HitboxTags GetHitboxTags( this SceneTraceResult tr )
	{
		if ( tr.Hitbox is null ) return HitboxTags.None;

		var tags = HitboxTags.None;

		foreach ( var tag in tr.Hitbox.Tags )
		{
			if ( Enum.TryParse<HitboxTags>( tag, true, out var hitboxTag ) )
			{
				tags |= hitboxTag;
			}
		}

		return tags;
	}
}
