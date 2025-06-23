using Godot;

public partial class ChangeSceneButton : Button
{
	[Export] public PackedScene TargetScene;

	public override void _Ready()
	{
		base._Ready(); 
		Pressed += OnButtonPressed;
	}

	private void OnButtonPressed()
	{
		if (TargetScene != null)
		{
			GetTree().ChangeSceneToPacked(TargetScene);
		}
		else
		{
			GetTree().Quit();
		}
	}
}
