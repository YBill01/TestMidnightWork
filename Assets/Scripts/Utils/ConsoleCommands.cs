using IngameDebugConsole;
using System;
using UnityEngine;

public static class ConsoleCommands
{
	[ConsoleMethod("quit", "Quits the player application.")]
	public static void Quit()
	{
		App.Instance.Quit();
	}

	[ConsoleMethod("f_active", "UnitFactory swith activate/deactivate units containers.")]
	public static void FactoryActive()
	{
		UnitFactory.Active = !UnitFactory.Active;
	}


}