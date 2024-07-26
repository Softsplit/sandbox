using Sandbox.Events;

namespace Softsplit;

/// <summary>
/// A weapon component that reacts to input actions.
/// </summary>
public abstract class InputWeaponComponent : EquipmentComponent,
	IGameEventHandler<EquipmentDeployedEvent>
{
	/// <summary>
	/// What input action are we going to listen for?
	/// </summary>
	[Property, Category( "Base" )] public List<string> InputActions { get; set; } = new() { "Attack1" };
	
	/// <summary>
	/// Should we perform the action when ALL input actions match, or any?
	/// </summary>
	[Property, Category( "Base" )] public bool RequiresAllInputActions { get; set; }

	/// <summary>
	/// ActionGraphs action so you can do stuff with visual scripting.
	/// </summary>
	[Property, Category( "Base" )] public Action<InputWeaponComponent> OnInputAction { get; set; }

	bool RunningWhileDeployed { get; set; }

	[Property] public bool ForceShoot {get;set;}
	[Property] public bool NotPlayerControlled {get;set;}

	void IGameEventHandler<EquipmentDeployedEvent>.OnGameEvent( EquipmentDeployedEvent eventArgs )
	{
		if ( Equipment?.Owner?.IsLocallyControlled ?? false )
		{
			RunningWhileDeployed = InputActions.Any( x => Input.Down( x ) );
		}
	}

	bool isDown = false;

	protected bool IsDown() => !NotPlayerControlled ? isDown : ForceShoot;

	public void ForceInput()
	{
		OnInput();
	}

	public void ForceInputDown()
	{
		OnInputDown();
	}

	/// <summary>
	/// Called when the input method succeeds.
	/// </summary>
	protected virtual void OnInput()
	{
		//
	}
	protected virtual void FixedUpdate()
	{

	}

	/// <summary>
	/// When the button is up
	/// </summary>
	protected virtual void OnInputUp()
	{
	}

	/// <summary>
	/// When the button is down
	/// </summary>
	protected virtual void OnInputDown()
	{
		//
	}

	protected virtual void OnInputUpdate()
	{
		//
	}

	protected override void OnFixedUpdate()
	{
		FixedUpdate();
		if(!NotPlayerControlled)
		{
			if ( !Equipment.IsValid())
			return;
		
			// Don't execute weapon components on weapons that aren't deployed.
			if ( !Equipment.IsDeployed)
				return;
			
			if ( !Equipment.Owner.IsValid())
				return;

			// We only care about input actions coming from the owning object.
			if ( !Equipment.Owner.IsLocallyControlled)
				return;
		}

		if ( InputActions.All( x => !Input.Down( x ) ) )
		{
			RunningWhileDeployed = false;
		}

		if ( RunningWhileDeployed )
			return;
		
		OnInputUpdate();
		
		bool matched = false;

		foreach ( var action in InputActions )
		{
			var down = Input.Down( action ) || ForceShoot;

			if ( RequiresAllInputActions && !down )
			{
				matched = false;
				break;
			}
			if ( down )
			{
				matched = true;
			}
		}
		
		if ( matched )
		{
			OnInput();
			OnInputAction?.Invoke( this );

			if ( !isDown )
			{
				OnInputDown();
				isDown = true;
			}
		}
		else
		{
			if ( isDown )
			{
				OnInputUp();
				isDown = false;
			}
		}
		
	}
}
