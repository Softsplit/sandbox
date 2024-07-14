namespace Softsplit;

public partial class PlayerPawn
{
	/// <summary>
	/// Is this player the currently possessed controller
	/// </summary>
	public bool IsViewer => IsPossessed;

	/// <summary>
	/// What are we called?
	/// </summary>
	public override string DisplayName => PlayerState.DisplayName;
	public override bool IsLocallyControlled => base.IsLocallyControlled && !PlayerState.IsBot;

	/// <summary>
	/// Called when possessed.
	/// </summary>
	public override void OnPossess()
	{
		CameraController.Mode = CameraMode.FirstPerson;
		CameraController.SetActive( true );
	}

	public override void OnDePossess()
	{
		CameraController.SetActive( false );
	}
}
