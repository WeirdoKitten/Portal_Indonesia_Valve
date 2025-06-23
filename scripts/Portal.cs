using Godot;
using System;

public partial class Portal : Area3D
{
	[Export] public string TargetGroup;

	public override void _Ready()
	{
		BodyEntered += _on_body_entered;
	}

	private void _on_body_entered(Node3D body)
	{

		if (body is PlayerController player)
		{
			// Teleport hanya jika player benar-benar baru masuk area
			var portals = GetTree().GetNodesInGroup(TargetGroup);
			if (portals.Count == 0) return;

			// Ambil portal target pertama
			if (portals[0] is Node3D targetPortal)
			{
				Vector3 offset = -targetPortal.GlobalTransform.Basis.Z * 0.5f;
				player.GlobalTransform = new Transform3D(
					targetPortal.GlobalTransform.Basis,
					targetPortal.GlobalTransform.Origin + offset
				);

				// Sinkronkan rotasi kamera agar tidak glitch
				player.SyncLookRotationWithTransform();
			}
		}
	}
}
