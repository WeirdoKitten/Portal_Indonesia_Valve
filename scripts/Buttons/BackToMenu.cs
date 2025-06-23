using Godot;
using System;

public partial class BackToMenu : Button
{
	[Export] public string MenuScenePath = "res://scenes/menu.tscn"; // Bisa diubah dari Inspector

	public override void _Ready()
	{
		base._Ready(); 
		Pressed += OnBackPressed;
	}

	private void OnBackPressed()
	{
		if (ResourceLoader.Exists(MenuScenePath)) // Cek apakah path valid
		{
			GetTree().Paused = false;
			GetTree().ChangeSceneToFile(MenuScenePath);
		}
		else
		{
			GD.PrintErr("MainMenu.tscn tidak ditemukan! Periksa path-nya.");
		}
	}
}
