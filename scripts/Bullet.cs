using System;
using Godot;

// Kelas Bullet, turunan dari Area3D, digunakan untuk peluru
public partial class Bullet : Area3D
{
	[Export]
	public float Speed = 20.0f; // Kecepatan peluru (unit per detik)
	
	[Export]
	public float Lifetime = 4.0f; // Waktu maksimum hidup peluru (detik)
	
	private double _timeAlive = 0.0; // Menyimpan berapa lama peluru sudah hidup
	private PackedScene _portalAScene;
	private PackedScene _portalBScene;

	public enum PortalType { Normal, A, B } // Tambahkan Normal

	[Export]
	public PortalType Type = PortalType.Normal; // Default Normal

	public override void _Ready()
	{
		// Load portal scene sekali saja
		_portalAScene = GD.Load<PackedScene>("res://scenes/portala.tscn");
		_portalBScene = GD.Load<PackedScene>("res://scenes/portalb.tscn");
	}

	// Fungsi ini dipanggil setiap frame fisika, digunakan untuk menggerakkan peluru
	public override void _PhysicsProcess(double delta)
	{
		// Godot: -Z adalah arah depan node
		Vector3 forwardDirection = -GlobalTransform.Basis.Z; // Ambil arah depan peluru
		GlobalPosition += forwardDirection * Speed * (float)delta; // Gerakkan peluru ke depan

		_timeAlive += delta; // Tambah waktu hidup peluru
		if (_timeAlive >= Lifetime)
		{
			QueueFree(); // Hapus peluru jika sudah melewati waktu hidup maksimum
		}
	}
	
	// Fungsi callback sinyal body_entered (harus dikoneksikan di editor)
	private async void _on_body_entered(Node3D body)
	{
	if (Type == PortalType.A || Type == PortalType.B)
	{
		if (IsMapOrChild(body))
		{
			// Hapus portal lama jika ada
			string groupName = Type == PortalType.A ? "portal_a" : "portal_b";
			var portals = GetTree().GetNodesInGroup(groupName);
			foreach (Node portal in portals)
			{
				portal.QueueFree();
			}

			// Raycast dari posisi bullet ke arah gerak bullet
			var spaceState = GetWorld3D().DirectSpaceState;
			Vector3 from = GlobalPosition;
			Vector3 to = from + (-GlobalTransform.Basis.Z) * 1.0f;

			var query = PhysicsRayQueryParameters3D.Create(from, to);
			query.CollideWithAreas = false;
			query.CollideWithBodies = true;

			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				Vector3 hitPos = (Vector3)result["position"];
				Vector3 hitNormal = (Vector3)result["normal"];

				Node3D portal;
				if (Type == PortalType.A)
					portal = _portalAScene.Instantiate<Node3D>();
				else
					portal = _portalBScene.Instantiate<Node3D>();

				GetTree().CurrentScene.AddChild(portal);
				portal.GlobalPosition = hitPos;

				// Pilih up vector yang aman
				Vector3 up = Vector3.Up;
				if (hitNormal.Dot(Vector3.Up) > 0.99f || hitNormal.Dot(Vector3.Up) < -0.99f)
					up = Vector3.Forward; // Gunakan Forward jika normal hampir sejajar Up

				portal.LookAt(hitPos + hitNormal, up);

				// Tambahkan ke group
				portal.AddToGroup(groupName);
			}
		}
	}

		// Knockback hanya untuk peluru biasa
		if (Type == PortalType.Normal && body.IsInGroup("targets"))
		{
			if (body is CharacterBody3D characterBody)
			{
				// Hitung arah knockback berdasarkan arah peluru
				Vector3 bulletDirection = -GlobalTransform.Basis.Z.Normalized();
				Vector3 knockback = new Vector3(bulletDirection.X, 0, bulletDirection.Z).Normalized() * 20f; // Kekuatan knockback

				// Jika objek adalah Enemy, gunakan fungsi ApplyKnockback (dengan stun)
				if (characterBody is Enemy enemy)
				{
					enemy.ApplyKnockback(knockback, 0.2f); // Stun selama 0.2 detik
				}
			}

		}
		QueueFree(); // Hapus peluru setelah mengenai objek
	}

	// Fungsi helper untuk cek apakah node adalah Map atau child Map
	private bool IsMapOrChild(Node node)
	{
		while (node != null)
		{
			if (node.Name == "Map") return true;
			node = node.GetParent();
		}
		return false;
	}
}
