using Godot;
using System;

// Kelas Enemy, turunan dari CharacterBody3D, untuk musuh yang bisa mengejar dan menyerang target
public partial class Enemy : CharacterBody3D
{
	[Export]
	public float Speed = 3.0f; // Kecepatan gerak musuh

	[Export]
	public Node3D Target; // Target yang akan dikejar (biasanya player)

	[Export]
	public float ChaseDistance = 15.0f; // Jarak maksimum pengejaran

	public float StunTime = 0.0f; // Waktu stun (tidak bisa bergerak)

	[Export]
	public float AttackCooldown = 5.0f; // Waktu jeda antar serangan (detik)
	private float _attackTimer = 0.0f; // Timer internal untuk jeda serangan

	[Export] public NodePath AnimationPlayerPath; // Path ke AnimationPlayer (diatur di Inspector)

	private NavigationAgent3D _navigationAgent; // Agent navigasi untuk pathfinding
	private AnimationPlayer _animPlayer; // Untuk memainkan animasi

	public override void _Ready()
	{
		// Ambil node NavigationAgent3D
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		// Ambil AnimationPlayer jika path sudah diatur
		if (AnimationPlayerPath != null)
			_animPlayer = GetNode<AnimationPlayer>(AnimationPlayerPath);
	}

	public override void _PhysicsProcess(double delta)
	{
		// Kurangi timer attack jika sedang cooldown
		if (_attackTimer > 0)
			_attackTimer -= (float)delta;

		// Jika sedang stun, tidak bisa bergerak
		if (StunTime > 0)
		{
			StunTime -= (float)delta;
			MoveAndSlide();
			return;
		}

		// Jika tidak ada target atau navigation agent, keluar
		if (Target == null || _navigationAgent == null)
			return;

		// Hitung jarak ke target
		float distanceToTarget = GlobalPosition.DistanceTo(Target.GlobalPosition);

		// Jika target dalam jarak pengejaran
		if (distanceToTarget <= ChaseDistance)
		{
			// Ambil posisi path berikutnya dari NavigationAgent
			Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
			// Hitung arah menuju path berikutnya
			Vector3 direction = GlobalPosition.DirectionTo(nextPathPosition);
			Vector3 calculatedVelocity = direction * Speed;

			// Set kecepatan musuh (Y tetap mengikuti gravitasi/velocity sebelumnya)
			Velocity = new Vector3(calculatedVelocity.X, Velocity.Y, calculatedVelocity.Z);

			// Hadapkan musuh ke arah target
			LookAt(Target.GlobalPosition, Vector3.Up);
			// Jika model membelakangi, putar 180 derajat pada sumbu Y
			RotateY(Mathf.Pi);

			MoveAndSlide(); // Gerakkan musuh

			UpdateTargetPosition(); // Update posisi target untuk pathfinding

			// Mainkan animasi jalan jika belum berjalan animasi lain
			if (_animPlayer != null && !_animPlayer.IsPlaying())
				_animPlayer.Play("AnimsPack2/walk");
		}
		else
		{
			// Jika target terlalu jauh, musuh diam di tempat
			Velocity = Vector3.Zero;
			MoveAndSlide();

			// Mainkan animasi idle
			if (_animPlayer != null)
				_animPlayer.Play("AnimsPack2/idle"); // Pastikan animasi idle tersedia
		}
	}

	// Update posisi target pada NavigationAgent
	private void UpdateTargetPosition()
	{
		if (Target != null)
			_navigationAgent.SetTargetPosition(Target.GlobalPosition);
	}

	// Fungsi ini dipanggil saat musuh mencapai target (harus dikoneksikan ke sinyal NavigationAgent3D)
	private void _on_navigation_agent_3d_target_reached()
	{
		// Jika masih cooldown, tidak bisa menyerang
		if (_attackTimer > 0)
			return;

		// Jika ada target, lakukan serangan
		if (Target != null)
		{
			// Cek apakah target punya fungsi TakeDamage
			var method = Target.GetType().GetMethod("TakeDamage");
			if (method != null)
			{
				method.Invoke(Target, new object[] { 1 }); // Panggil TakeDamage(1)
				_attackTimer = AttackCooldown; // Reset cooldown setelah menyerang

				// Mainkan animasi attack
				if (_animPlayer != null)
					_animPlayer.Play("AnimsPack2/attack");
			}
		}
	}

	// Fungsi untuk menerima knockback dan stun
	public void ApplyKnockback(Vector3 knockback, float stunDuration)
	{
		Velocity = new Vector3(knockback.X, Velocity.Y, knockback.Z); // Set velocity sesuai arah knockback
		StunTime = stunDuration; // Set waktu stun
	}
}
