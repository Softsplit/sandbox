namespace Softsplit;
using Tools;

public partial class PlayerPawn
{
	public void UpdateTool()
	{
		if (this.CurrentEquipment is not Tool)
		{
			// return;
		}
		throw new NotImplementedException();
	}
}
