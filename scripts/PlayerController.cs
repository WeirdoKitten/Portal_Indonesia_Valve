using Godot;
using System;

[GlobalClass]
public partial class PlayerController : CharacterBody3D
{
	[ExportGroup("Speeds")]
	[Export] public float LookSpeed { get; set; } = 0.005f; // Kecepatan rotasi kamera
	[Export] public float BaseSpeed { get; set; } = 7.0f; // Kecepatan jalan normal
	[Export] public float JumpVelocity { get; set; } = 4.5f; // Kecepatan lompatan
	[Export] public float SprintSpeed { get; set; } = 10.0f; // Kecepatan lari

	[ExportGroup("Shooting")]
	[Export] public PackedScene BulletNormalScene { get; set; } // Prefab peluru biasa
	[Export] public PackedScene BulletAScene { get; set; } // Prefab peluru portal A
	[Export] public PackedScene BulletBScene { get; set; } // Prefab peluru portal B

	[Export]
	public NodePath HeartUIPath; // Path ke UI heart (nyawa)

	[Export] public NodePath AnimationPlayerPath; // Path ke AnimationPlayer

	private Vector2 _lookRotation = Vector2.Zero; // Menyimpan rotasi kamera (X: vertikal, Y: horizontal)
	private float _moveSpeed = 0.0f; // Kecepatan gerak saat ini

	private Head _head; // Script Head (kamera + tangan)
	private Control _heartUI; // UI heart
	private AnimationPlayer _animPlayer; // AnimationPlayer untuk animasi player

	public int Heart = 3; // Jumlah nyawa player

	public override void _Ready()
	{
		_head = GetNode<Head>("Head"); // Ambil node Head
		_lookRotation.Y = Rotation.Y; // Set rotasi horizontal awal
		_lookRotation.X = _head.Rotation.X; // Set rotasi vertikal awal

		Input.MouseMode = Input.MouseModeEnum.Captured; // Kunci mouse ke tengah layar

		if (HeartUIPath != null)
			_heartUI = GetNode<Control>(HeartUIPath); // Ambil UI heart

		if (AnimationPlayerPath != null)
			_animPlayer = GetNode<AnimationPlayer>(AnimationPlayerPath); // Ambil AnimationPlayer
		_animPlayer.Play("AnimsPack1/idle"); // Mainkan animasi idle di awal
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Rotasi kamera dengan mouse
		if (@event is InputEventMouseMotion mouseMotion)
			RotateLook(mouseMotion.Relative);

		// Menembak portal A dengan Q
		if (Input.IsActionJustPressed("fire_portal_a"))
			FireBullet(Bullet.PortalType.A);

		// Menembak portal B dengan E
		if (Input.IsActionJustPressed("fire_portal_b"))
			FireBullet(Bullet.PortalType.B);

		// Menembak biasa dengan klik kiri (fire)
		if (Input.IsActionJustPressed("fire"))
			FireBullet(Bullet.PortalType.Normal);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// Tambahkan gravitasi jika tidak di tanah
		if (!IsOnFloor())
		{
			Vector3 gravity = new Vector3(0, -8.9f, 0);
			Velocity += gravity * dt;
		}

		// Lompat jika di tanah dan tombol jump ditekan
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);
			if (_animPlayer != null)
			{
				_animPlayer.SpeedScale = 1.0f;
				_animPlayer.Play("AnimsPack1/jump");
			}
		}

		// Tentukan kecepatan jalan/lari
		_moveSpeed = Input.IsActionPressed("sprint") ? SprintSpeed : BaseSpeed;

		// Ambil input arah gerak (WASD)
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		Vector3 moveDir = (GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		// Logika animasi berjalan/berlari/idle
		if (_animPlayer != null)
		{
			if (moveDir != Vector3.Zero)
			{
				if (Input.IsActionPressed("sprint"))
				{
					_animPlayer.SpeedScale = 1.0f;
					_animPlayer.Play("AnimsPack1/run");
				}
				else
				{
					_animPlayer.SpeedScale = 1.0f;
					_animPlayer.Play("AnimsPack1/walk");
				}
			}
			else
			{
				// Animasi idle jika tidak bergerak
				if (_animPlayer.IsPlaying() && 
					(_animPlayer.CurrentAnimation == "AnimsPack1/walk" || _animPlayer.CurrentAnimation == "AnimsPack1/run"))
				{
					_animPlayer.Play("AnimsPack1/idle");
				}
			}
		}

		// Gerakkan player
		if (moveDir != Vector3.Zero)
		{
			Velocity = new Vector3(moveDir.X * _moveSpeed, Velocity.Y, moveDir.Z * _moveSpeed);
		}
		else
		{
			// Perlambat player secara halus saat tidak ada input
			Velocity = new Vector3(
				Mathf.MoveToward(Velocity.X, 0, _moveSpeed),
				Velocity.Y,
				Mathf.MoveToward(Velocity.Z, 0, _moveSpeed)
			);
		}

		// Gerakkan player dan cek tabrakan
		MoveAndSlide();

		// Dorong objek RigidBody3D jika bertabrakan
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			if (collision.GetCollider() is RigidBody3D rigid)
			{
				// Hitung arah dorongan (dari player ke objek)
				Vector3 pushDir = collision.GetNormal();
				// Tambahkan gaya dorong ke rigidbody
				rigid.ApplyImpulse(-pushDir * 0.5f, Vector3.Zero); // 0.5f bisa diubah sesuai kebutuhan
			}
		}
	}

	// Fungsi untuk rotasi kamera dan kepala player
	private void RotateLook(Vector2 rotInput)
	{
		_lookRotation.X -= rotInput.Y * LookSpeed;
		_lookRotation.X = Mathf.Clamp(_lookRotation.X, Mathf.DegToRad(-85), Mathf.DegToRad(85));
		_lookRotation.Y -= rotInput.X * LookSpeed;
		Rotation = new Vector3(0, _lookRotation.Y, 0);
		_head.Rotation = new Vector3(_lookRotation.X, 0, 0);
	}

	// Ubah FireBullet agar menerima tipe portal
	private void FireBullet(Bullet.PortalType portalType)
	{
		// Tidak bisa menembak jika sedang mengangkat objek
		if (_head != null && _head.IsHoldingObject)
		{
			GD.Print("Tidak bisa menembak saat mengangkat objek!");
			return;
		}

		PackedScene bulletScene = BulletNormalScene;
		if (portalType == Bullet.PortalType.A)
			bulletScene = BulletAScene;
		else if (portalType == Bullet.PortalType.B)
			bulletScene = BulletBScene;

		var bullet = bulletScene.Instantiate<Bullet>();

		if (bullet == null)
		{
			GD.PrintErr("Player cannot fire: BulletScene not set!");
			return;
		}

		if (_animPlayer != null)
		{
			_animPlayer.SpeedScale = 5.0f;
			_animPlayer.Play("AnimsPack1/attack");
		}

		Marker3D spawnPoint = _head.GetNodeOrNull<Marker3D>("BulletSpawnPoint");
		if (spawnPoint == null)
		{
			GD.PrintErr("Player cannot fire: BulletSpawnPoint node not found!");
			return;
		}

		bullet.GlobalTransform = spawnPoint.GlobalTransform;
		bullet.Type = portalType; // Set tipe portal
		GetTree().Root.AddChild(bullet);

		GD.Print($"Fired bullet for portal {portalType}!");
	}

	// Fungsi untuk menerima damage (mengurangi heart)
	public void TakeDamage(int amount)
	{
		Heart -= amount;
		GD.Print($"Player heart: {Heart}");

		// Update UI heart
		if (_heartUI != null)
		{
			var hbox = _heartUI.GetNodeOrNull<HBoxContainer>("HBoxContainer");
			if (hbox != null)
			{
				// Hapus heart dari kanan ke kiri
				if (Heart == 2)
				{
					var heart3 = hbox.GetNodeOrNull<TextureRect>("Heart3");
					if (heart3 != null)
						heart3.QueueFree();
				}
				else if (Heart == 1)
				{
					var heart2 = hbox.GetNodeOrNull<TextureRect>("Heart2");
					if (heart2 != null)
						heart2.QueueFree();
				}
				else if (Heart == 0)
				{
					var heart1 = hbox.GetNodeOrNull<TextureRect>("Heart1");
					if (heart1 != null)
						heart1.QueueFree();
					// Tambahkan logika game over jika perlu
				}
			}
		}

		// Jika heart habis, tampilkan death menu dan pause game
		if (Heart <= 0)
		{
			GD.Print("Player mati! Tampilkan death menu...");
			var deathScene = GD.Load<PackedScene>("res://scenes/death.tscn");
			var deathInstance = deathScene.Instantiate<Control>();
			GetTree().Root.AddChild(deathInstance);
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GetTree().Paused = true;
		}
	}

	public void SyncLookRotationWithTransform()
	{
		// Sinkronkan _lookRotation dengan rotasi player dan head saat ini
		_lookRotation.Y = Rotation.Y;
		if (_head != null)
			_lookRotation.X = _head.Rotation.X;
	}
}
