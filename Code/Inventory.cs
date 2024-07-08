using System;
using System.Collections.Generic;

public class Inventory
{
	Player player;
	List<Tool> tools;

	public Inventory( Player player )
	{
		this.player = player;
		tools = new();
	}

	public Tool? GetSlot( int slot )
	{
		if ( slot > tools.Count - 1 )
			return null;
		return tools[slot];
	}
	public int Count()
	{
		return tools.Count;
	}
	public int GetActiveSlot()
	{
		return tools.IndexOf( player.ActiveChild );
	}

	public bool CanAdd( Tool entity )
	{
		if ( !entity.IsValid() )
			return false;

		return !IsCarryingType( entity.GetType() );
	}

	public bool Add( Tool entity, bool makeActive = true )
	{
		if ( !entity.IsValid() )
			return false;

		if ( IsCarryingType( entity.GetType() ) )
			return false;

		if ( tools.Count == 9 )
		{
			tools.Remove( tools[8] );
		}
		/*if ( makeActive )
		{
			player.ActiveChild = GetSlot(Count()-1);
		}*/
		entity.Owner = player;
		tools.Add( entity );
		return true;
	}

	public bool IsCarryingType( Type t ) // TODO: implement this
	{
		return false;
	}
	public bool Drop( Tool ent ) // TODO: implement this
	{
		return false;
	}
}
