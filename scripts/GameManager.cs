using Godot;

public partial class GameManager : Node
{
	[Export] public NodePath FinishAreaPath;
	[Export] public NodePath WinLabelPath;

	private Label _winLabel;

	public override void _Ready()
	{
		// Ambil node FinishArea dan sambungkan sinyal
		var finishArea = GetNode<Area3D>(FinishAreaPath);
		finishArea.BodyEntered += _on_finish_area_body_entered;

	}

	private void _on_finish_area_body_entered(Node body)
	{
		// Cek apakah yang masuk adalah player
		if (body is PlayerController)
		{
			ShowWinMessage();
		}
	}

	private void ShowWinMessage()
	{
		GD.Print("Selamat! Kamu Menang!");
		var winScene = GD.Load<PackedScene>("res://scenes/win.tscn");
		var winInstance = winScene.Instantiate<Control>();
		AddChild(winInstance);
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
	}
}
