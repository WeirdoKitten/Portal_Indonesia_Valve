using Godot;

public partial class Pause : Control
{
	public override void _Ready()
	{
		Hide();
		GetNode<Button>("PanelContainer/VBoxContainer/Resume").Pressed += OnResumePressed;
		GetNode<Button>("PanelContainer/VBoxContainer/Quit").Pressed += OnQuitPressed;
		GetNode<Button>("PanelContainer/VBoxContainer/Restart").Pressed += OnRestartPressed;
	}

	public void Resume()
	{
		Hide();
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
	}

	public void PauseGame()
	{
		Show();
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
	}

	public void TestPause()
	{
		if (Input.IsActionJustPressed("esc") && !GetTree().Paused)
		{
			PauseGame();
		}
		else if (Input.IsActionJustPressed("esc") && GetTree().Paused)
		{
			Resume();
		}
	}

	private void OnResumePressed()
	{
		Resume();
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}

	private void OnRestartPressed()
	{
		Resume();
		GetTree().ReloadCurrentScene();
	}

	public override void _Process(double delta)
	{
		TestPause();
	}
}
