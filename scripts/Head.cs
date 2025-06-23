using Godot;
using System;

// Kelas Head, turunan dari Node3D, untuk menangani mekanik mengambil dan meletakkan objek
public partial class Head : Node3D
{
	private RayCast3D _raycast; // Raycast untuk mendeteksi objek di depan kamera
	private Node3D _hand; // Node sebagai "tangan" tempat objek dipegang
	private RigidBody3D _heldObject = null; // Objek yang sedang dipegang
	private Node _originalParent = null; // Parent asli dari objek yang dipegang

	public override void _Ready()
	{
		// Ambil node RayCast3D dari dalam Camera3D
		_raycast = GetNode<RayCast3D>("Camera3D/RayCast3D");
		// Ambil node Hand sebagai tempat objek dipegang
		_hand = GetNode<Node3D>("Hand");
	}

	public override void _Process(double delta)
	{
		// Jika sedang memegang objek, posisikan objek selalu di tangan
		if (_heldObject != null)
		{
			_heldObject.GlobalTransform = _hand.GlobalTransform;
			_heldObject.LinearVelocity = Vector3.Zero; // Hentikan gerakan objek
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Jika tombol "interact" ditekan
		if (@event.IsActionPressed("interact"))
		{
			if (_heldObject == null)
				TryPickUp(); // Jika tidak memegang apa-apa, coba ambil objek
			else
				DropObject(); // Jika sudah memegang, lepas objek
		}
	}

	// Fungsi untuk mencoba mengambil objek di depan kamera
	private void TryPickUp()
	{
		if (_raycast.IsColliding())
		{
			var collider = _raycast.GetCollider();
			// Hanya bisa mengambil objek RigidBody3D yang ada di grup "pickables"
			if (collider is RigidBody3D rb && rb.IsInGroup("pickables"))
			{
				_heldObject = rb;
				_heldObject.Freeze = true; // Bekukan fisika objek saat dipegang
				_heldObject.CollisionLayer = 2; // Ubah layer agar tidak bertabrakan dengan player
				_heldObject.GlobalTransform = _hand.GlobalTransform; // Posisikan di tangan

				// Simpan parent asli objek
				_originalParent = _heldObject.GetParent();

				// Lepaskan dari parent lama sebelum dipindah ke Hand
				if (_originalParent != null)
					_originalParent.RemoveChild(_heldObject);

				// Tambahkan objek ke node Hand
				_hand.AddChild(_heldObject);
			}
		}
	}

	// Fungsi untuk melepaskan objek yang sedang dipegang
	private void DropObject()
	{
		if (_heldObject != null)
		{
			// Lepaskan dari parent Hand sebelum dipindah ke parent asli
			var currentParent = _heldObject.GetParent();
			if (currentParent != null)
				currentParent.RemoveChild(_heldObject);

			// Kembalikan ke parent asli (misal: node "Pickables" di scene utama)
			if (_originalParent != null)
				_originalParent.AddChild(_heldObject);

			_heldObject.GlobalTransform = _hand.GlobalTransform; // Posisikan objek di depan tangan
			_heldObject.Freeze = false; // Aktifkan kembali fisika objek
			_heldObject.CollisionLayer = 1; // Kembalikan layer ke semula
			_heldObject.LinearVelocity = new Vector3(0.1f, 3, 0.1f); // Beri sedikit dorongan ke atas saat dilepas
			_heldObject = null; // Reset status pegang
			_originalParent = null; // Reset parent asli
		}
	}

	// Properti untuk mengecek apakah sedang memegang objek
	public bool IsHoldingObject => _heldObject != null;
}
