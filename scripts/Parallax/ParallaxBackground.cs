using Godot;
using System;

public partial class ParallaxBackground : Godot.ParallaxBackground
{
	[Export] public float scrollSpeed = 50f;

	public override void _Process(double delta)
	{
		ScrollOffset += new Vector2((float)(scrollSpeed * delta), 0);
	}
}
