using Godot;

public partial class Ball : CharacterBody3D
{
    [Export] public float MaxSpeed = 50.0f;

    [Export] public float Acceleration = 10.0f;

    public bool IsPlayer { get; private set; }

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public Ball? CurrentFollowing { get; set; }

    public void Initialize(Vector3 position, bool isPlayer = false)
    {
        IsPlayer = isPlayer;
        Position = position;

        if (!IsPlayer) return;

        var text = new Label3D();
        text.Text = "Player";
        text.Position = new Vector3(0, 0.5f, 0);
        text.RotationDegrees = new Vector3(-90, 0, 0);
        AddChild(text);
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor())
            velocity.Y -= _gravity * (float) delta;

        if (IsPlayer)
        {
            var inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
            if (direction != Vector3.Zero)
            {
                velocity.X = Mathf.MoveToward(Velocity.X, direction.X * MaxSpeed, Acceleration * (float) delta);
                velocity.Z = Mathf.MoveToward(Velocity.Z, direction.Z * MaxSpeed, Acceleration * (float) delta);
            }
            else
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Acceleration * (float) delta);
                velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Acceleration * (float) delta);
            }
        }
        else
        {
            // velocity.X = Mathf.MoveToward(Velocity.X, 0, Acceleration * (float) delta);
            // velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Acceleration * (float) delta);
            if (IsInstanceValid(CurrentFollowing))
            {
                var direction = Position.DirectionTo(CurrentFollowing!.Position);
                velocity.X = Mathf.MoveToward(velocity.X, direction.X * MaxSpeed, Acceleration * (float) delta);
                velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * MaxSpeed, Acceleration * (float) delta);
            }
        }

        Velocity = velocity;

        MoveAndSlide();

        if (CurrentFollowing?.IsOnFloor() == false)
        {
            CurrentFollowing = null;
        }

        for (var i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);

            if (collision.GetCollider() is not Ball ball) continue;

            ball.Velocity = velocity;

            ball.CurrentFollowing = null;
        }
    }
}