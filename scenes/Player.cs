using Godot;

public partial class Player : CharacterBody2D
{

	[Export] public float MaxSpeed = 250f;
	
	[Export] public float Friction = 0.2f;
	[Export] public float AccelerationFactor = 500f;
	[Export] public float DecelerationFactor = 200f;
	
	// Boost
	[Export] public float DashFactor = 2.5f;
	[Export] public Timer DashDurationTimer = new();
	[Export] public float DashDuration = 0.5f;
	[Export] public Timer DashCooldownTimer = new();
	[Export] public float DashCooldown = 1.5f;
	public bool CanDash = true;
	public bool IsDashing = false;
	
	[Export] public float MaxSkew = 0.1f;
	[Export] public float SkewSpeed = 3f;
	
	[Export] public float RotationSpeed = 1f;
	[Export] public float MaxRotation =  0.1f;
	[Export] public StringName LeftInput = "left";
	[Export] public StringName RightInput = "right";
	[Export] public StringName ForwardInput = "forward";
	[Export] public StringName BackwardInput = "backward";
	[Export] public StringName ShootInput = "shoot";
	
	[Export] public Texture2D PlayerTexture = GD.Load<Texture2D>("res://assets/graphic/playerShip1_orange.png");

	public bool CanShoot = true;
	
	// This could maybe be a global?
	public int Health = 5;
	
	
	private float _currentSpeed;
	private float _currentAccel;

	// Positioning and rotation
	private float _targetSkew;
	private Vector2 _direction;
	
	// Nodes
	private Sprite2D _playerSpriteNode;
	private Marker2D _laserStartPositionNode;
	private AudioStreamPlayer2D _laserStreamPlayerNode;
	private AudioStreamPlayer2D _damageStreamPlayerNode;
	
	private Timer _primaryWeaponTimerNode;

	[Signal]
	public delegate void ShootLaserEventHandler(Vector2 position, Vector2 originatingVelocity);
	
	
	public override void _Ready()
	{
		_getNodes();
		
		_registerPrimaryWeaponTimer();

		_initialiseDashTimers();
		_updateSpeedValues();
		
		_playerSpriteNode.Texture = PlayerTexture;
	}

	public override void _Process(double delta)
	{
		var deltaf = (float)delta;
		
		_handleInput();
		
		_calculateRotationAndSkew(deltaf);
		_skew(deltaf);
		_squash();
		
		_move(deltaf);

		_updateDebugUi();
	}

	public void PlayDamageSound()
	{
		_damageStreamPlayerNode.Play();
	}

	private void _getNodes()
	{
		_playerSpriteNode = GetNode<Sprite2D>("PlayerSprite");
		_laserStartPositionNode = GetNode<Marker2D>("LaserStartPosition");
		_laserStreamPlayerNode = GetNode<AudioStreamPlayer2D>("LaserSound");
		_damageStreamPlayerNode = GetNode<AudioStreamPlayer2D>("DamageSound");
		_primaryWeaponTimerNode = GetNode<Timer>("PrimaryWeaponTimer");
	}

	private void _setInitialPosition()
	{
		Position = new Vector2(WindowManager.ViewportSize.X / 2, WindowManager.ViewportSize.Y - 250f);;
	}
	
	private void _initialiseDashTimers()
	{
		
		DashDurationTimer.OneShot = true;
		DashDurationTimer.WaitTime = DashDuration;
		DashDurationTimer.Timeout += _onDashDurationTimerTimeout;
		AddChild(DashDurationTimer);
		
		DashCooldownTimer.OneShot = true;
		DashCooldownTimer.WaitTime = DashCooldown;
		DashCooldownTimer.Timeout += _onDashCooldownTimerTimeout;
		AddChild(DashCooldownTimer);
	}
	
	private void _registerPrimaryWeaponTimer()
	{
		_primaryWeaponTimerNode.Timeout += _onPrimaryWeaponTimerTimeout;	
	}

	private void _onDashDurationTimerTimeout()
	{
		IsDashing = false;
	}

	private void _onDashCooldownTimerTimeout()
	{
		CanDash = true;
	}

	private void _onPrimaryWeaponTimerTimeout()
	{
		CanShoot = true;
	}

	private void _handleInput()
	{
		_direction = Input.GetVector(LeftInput, RightInput, ForwardInput, BackwardInput);
		
		// Input.isActionPressed for continuous shooting - maybe for another special type of laser.
		if (Input.IsActionJustPressed("shoot") && CanShoot)
		{
			CanShoot = false;
			EmitSignal(SignalName.ShootLaser, _laserStartPositionNode.GlobalPosition, Velocity);
			_primaryWeaponTimerNode.Start();
			_laserStreamPlayerNode.Play();
		}
		
		_handleDashInput();
	}

	private void _handleDashInput()
	{
		// While dash is pressed, enable dash, keeping active while held down. If let go, stop dash.
		// Don't allow dashing before timer is finished. 
		if (Input.IsActionJustPressed("dash") && CanDash && DashDurationTimer.IsStopped())
		{
			CanDash = false;
			DashCooldownTimer.Start();
			IsDashing = true;
			DashDurationTimer.Start();
			_updateSpeedValues();
		}

		if (Input.IsActionJustReleased("dash") && IsDashing)
		{
			IsDashing = false;
			_updateSpeedValues();
		}
		
		
		// Add swoosh sound here.

	}

	private void _move(float deltaf)
	{
		
		Velocity = 
			_direction != Vector2.Zero ? 
				Velocity.MoveToward(_direction * _currentSpeed, _currentAccel * deltaf) :
				Velocity.MoveToward(Vector2.Zero, DecelerationFactor * Friction * deltaf);
		
		MoveAndSlide();
	}

	private void _updateSpeedValues()
	{
		_currentSpeed = IsDashing ? MaxSpeed * DashFactor : MaxSpeed;
		_currentAccel = IsDashing ? AccelerationFactor * DashFactor : AccelerationFactor;
	}

	private void _updateDebugUi()
	{
		if (!OS.HasFeature("debug")) return;
		GetTree().CallGroup("ui", "SetPlayerDebugInfo", CanDash, IsDashing, Velocity.Length());;
	}

	private void _calculateRotationAndSkew(float deltaf)
	{
		switch (_direction.X)
		{
			// going right
			case < 0:
				Rotation -= RotationSpeed * deltaf;
				_targetSkew = -MaxSkew;
				break;
			// going left
			case > 0:
				Rotation += RotationSpeed * deltaf;
				_targetSkew = MaxSkew;
				break;
			default:
				_targetSkew = 0f;
				// Smoothly rotate back to neutral when not pressing left/right
				Rotation = Mathf.MoveToward(Rotation, 0f, RotationSpeed * deltaf);
				break;
		}

		// Clamp the rotation so it doesn't exceed the max in either direction
		Rotation = Mathf.Clamp(Rotation, -MaxRotation, MaxRotation);
	}
	
	private void _squash()
	{
		// Squash sprite as well on horizontal axis
		var squash = 1f - Mathf.Abs(_targetSkew) * 0.2f;
		_playerSpriteNode.Scale = new Vector2(squash, 1f);
	}

	private void _skew(float deltaf)
	{
		// Current transform
		var xform = _playerSpriteNode.Transform;
		
		// Extract basis vectors
		var x = xform.X;
		var y = xform.Y;
		
		// Apply skew to x basis (skewing horizontally by changing the Y component of X)
		x.Y = Mathf.MoveToward(x.Y, _targetSkew, SkewSpeed * deltaf);
		
		// Reassign modified transform
		xform.X = x;
		_playerSpriteNode.Transform = xform;
	}
}
