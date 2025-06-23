using Godot;
using System;

// Kelas NPC, turunan dari Area3D, untuk NPC yang bisa berdialog dengan player
public partial class NPC : Area3D
{
	[Export] public Godot.Collections.Array<NodePath> LabelPaths; // Path ke label dialog (atur di Inspector)
	[Export] public NodePath PanelContainerPath; // Path ke panel dialog (atur di Inspector)

	private Label[] _labels; // Array label dialog
	private PanelContainer _panelContainer; // Panel dialog
	private int _dialogIndex = 0; // Indeks dialog saat ini
	private bool _playerInArea = false; // Status apakah player di area NPC

	private AnimationPlayer _animPlayer; // Untuk animasi NPC

	public override void _Ready()
	{
		// Koneksikan sinyal Area3D untuk deteksi player masuk/keluar area
		BodyEntered += _on_body_entered;
		BodyExited += _on_body_exited;

		// Ambil semua label dari path yang sudah diatur di Inspector
		_labels = new Label[LabelPaths.Count];
		for (int i = 0; i < LabelPaths.Count; i++)
		{
			_labels[i] = GetNode<Label>(LabelPaths[i]);
			_labels[i].Visible = false; // Sembunyikan semua label di awal
		}

		// Ambil panel dialog jika path sudah diatur
		if (PanelContainerPath != null)
			_panelContainer = GetNode<PanelContainer>(PanelContainerPath);

		if (_panelContainer != null)
			_panelContainer.Visible = false; // Sembunyikan panel di awal

		// Cari AnimationPlayer (ganti path sesuai struktur scene kamu)
		_animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (_animPlayer == null)
		{
			// Jika AnimationPlayer ada di dalam model GLB, misal: "npc/AnimationPlayer"
			_animPlayer = GetNodeOrNull<AnimationPlayer>("npc/AnimationPlayer");
		}

		// Mainkan animasi idle jika ada
		if (_animPlayer != null && _animPlayer.HasAnimation("AnimsPack1/idle"))
		{
			_animPlayer.Play("AnimsPack1/idle");
		}
	}

	// Fungsi dipanggil saat player masuk area NPC
	private void _on_body_entered(Node body)
	{
		if (body is PlayerController)
		{
			_playerInArea = true; // Tandai player ada di area
			_dialogIndex = 0; // Reset dialog ke awal
			if (_panelContainer != null)
				_panelContainer.Visible = true; // Tampilkan panel dialog
			if (_labels.Length > 0)
				_labels[0].Visible = true; // Tampilkan dialog pertama
		}
	}

	// Fungsi dipanggil saat player keluar area NPC
	private void _on_body_exited(Node body)
	{
		if (body is PlayerController)
		{
			_playerInArea = false; // Player sudah keluar area
			if (_panelContainer != null)
				_panelContainer.Visible = false; // Sembunyikan panel dialog
			foreach (var label in _labels)
				label.Visible = false; // Sembunyikan semua label
		}
	}

	public override void _Process(double delta)
	{
		// Jika player di area dan menekan tombol "interact"
		if (_playerInArea && Input.IsActionJustPressed("interact"))
		{
			if (_labels.Length == 0) return;
			// Cegah interaksi jika sudah di label terakhir
			if (_dialogIndex >= _labels.Length)
				return;

			_labels[_dialogIndex].Visible = false; // Sembunyikan dialog saat ini
			_dialogIndex++; // Lanjut ke dialog berikutnya
			if (_dialogIndex < _labels.Length)
			{
				_labels[_dialogIndex].Visible = true; // Tampilkan dialog berikutnya
			}
			else
			{
				if (_panelContainer != null)
					_panelContainer.Visible = false; // Sembunyikan panel jika dialog habis
				// Optional: _dialogIndex++; // Jangan tambah lagi, biar tetap di batas
			}
		}
	}
}
