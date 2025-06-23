using Godot;

public partial class Death : Control
{
	public override void _Ready()
	{
		GetNode<Button>("PanelContainer/VBoxContainer/Restart").Pressed += OnRestartPressed;
		GetNode<Button>("PanelContainer/VBoxContainer/Quit").Pressed += OnQuitPressed;
		GetNode<Button>("PanelContainer/VBoxContainer/MainMenu").Pressed += OnMainMenuPressed;
	}

	private void OnRestartPressed()
	{
		Hide();
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnMainMenuPressed()
	{
		Hide();
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
