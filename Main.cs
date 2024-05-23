using System;
using System.Collections.Immutable;
using Godot;
using System.Linq;

public partial class Main : Node
{
    // Called when the node enters the scene tree for the first time.

    private PackedScene _ballScene = GD.Load<PackedScene>("res://Ball.tscn");

    [Signal]
    public delegate void BallDeathEventHandler(bool isPlayer = false);

    public override void _Ready()
    {
        GetNode<Control>("FailedUI").Visible = false;
        GetNode<Control>("SucceedUI").Visible = false;

        foreach (var spawnLocation in GetNode<Node>("SpawnLocations").GetChildren().OfType<MeshInstance3D>()
                     .Select((value, i) => new {i, value}))
        {
            var ball = _ballScene.Instantiate<Ball>();
            if (spawnLocation.i == 0)
                ball.Initialize(spawnLocation.value.Position, true);
            else
                ball.Initialize(spawnLocation.value.Position);
            AddChild(ball);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var balls = GetChildren().OfType<Ball>().OrderBy(x => Random.Shared.Next()).ToImmutableList();
        foreach (var ball in balls.Select((value, i) => new {i, value}))
        {
            if (IsInstanceValid(ball.value.CurrentFollowing)) continue;
            var otherBalls = balls.Where(x => x != ball.value && ball.value.IsOnFloor())
                .OrderBy(x => Random.Shared.Next()).ToImmutableList();
            if (otherBalls.IsEmpty) continue;
            ball.value.CurrentFollowing = otherBalls.First();
            GD.Print($"{ball.value} following to {ball.value.CurrentFollowing}");
        }
    }

    public void OnDiePlaneBodyEntered(Node3D node)
    {
        if (node is not Ball ball) return;

        ball.QueueFree();
        EmitSignal(SignalName.BallDeath, ball.IsPlayer);

        if (ball.IsPlayer
            && !GetNode<Control>("FailedUI").Visible
            && !GetNode<Control>("SucceedUI").Visible)
        {
            GetNode<Control>("FailedUI").Visible = true;
            return;
        }

        if (GetChildren().OfType<Ball>().Count() <= 2
            && !GetNode<Control>("FailedUI").Visible
            && !GetNode<Control>("SucceedUI").Visible)
        {
            GetNode<Control>("SucceedUI").Visible = true;
        }
    }

    public void OnRetryButtonPressed()
    {
        GetTree().ReloadCurrentScene();
    }
}