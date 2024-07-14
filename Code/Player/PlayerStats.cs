namespace Softsplit;

/// <summary>
/// Player Stats.
/// </summary>
public partial class PlayerStats : Component
{
    private PlayerPawn Player { get; set; }

    [Property]
    public List<GameObject> spawnedPropsList = new List<GameObject>();

    protected override void OnAwake()
    {
        base.OnAwake();
        Player = this.Components.Get<PlayerPawn>();
    }

    protected override void OnUpdate()
    {
        if ( !Player.IsLocallyControlled )
            return;

        if ( Input.Pressed( "prop_undo" ) )
        {
            if ( spawnedPropsList.Count > 0 )
            {
                spawnedPropsList.Last().Destroy();
                spawnedPropsList.RemoveAt( spawnedPropsList.IndexOf( spawnedPropsList.Last() ) );
            }
        }
    }
}