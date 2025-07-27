using Godot;
using System;

public partial class WindowManager : Node
{
    public static Vector2 ViewportSize { get; private set; }

    public override void _Ready()
    {
        UpdateViewportSize();
        GetViewport().Connect("size_changed", Callable.From(UpdateViewportSize));
    }

    private void UpdateViewportSize()
    {
        ViewportSize = GetViewport().GetVisibleRect().Size;
        GD.Print("Viewport size updated: " + ViewportSize);
    }
}