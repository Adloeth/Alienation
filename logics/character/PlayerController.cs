using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
    [Export] private Vector2 minMaxSpeed = new Vector2(1, 5);
    [Export] private float mouseSensitivity = 1;
    [Export] private Node3D cameraHolder;
    [Export] private Node3D cameraNode;
    [Export] private Camera3D camera;
    [Export] private Label debugFPS;
    [Export] private Label debug;
    [Export] private TextureProgressBar staminaProgressBar;
    [Export] private float bobbingSpeed = 1;
    [Export] private float bobbingAmplitude = 1;
    [Export] private float moveBobbingSlope = 0.2f;
    [Export] private float moveBobbingAmplitude = 1f;
    [Export] private float moveBobbingAngleAmplitude = 1f;
    [Export] private float fovSpeedIncrease = 10;

    [Export] private float maxStamina = 100.0f;
    [Export] private Vector2 minMaxStaminaConsumptionSpeed = new Vector2(0, 8);
    [Export] private float staminaTimeToStartRegen = 10;
    [Export] private float staminaRegenSpeed = 1;
    [Export(PropertyHint.Range, "0,1,")] private float staminaRegenThreshold = 0.25f;
    private float stamina;
    private float staminaRegenTime;

    private Vector2 moveDir;
    private Vector2 viewDir;
    private float yVelocity;

    private float shakeTime;
    private float initialShakeTime;
    private float shakePower;

    private float neutralBobbing;
    private float moveBobbing;
    private float baseFov;

    private bool isMoving;
    public bool IsMoving => isMoving;

    float speed = 0;
    float CurrentSpeed => staminaRegenTime < 1 ? minMaxSpeed.X : Mathf.Lerp(minMaxSpeed.X, minMaxSpeed.Y, speed);
    float CurrentStaminaConsumption => staminaRegenTime < 1 ? 0 : Mathf.Lerp(minMaxStaminaConsumptionSpeed.X, minMaxStaminaConsumptionSpeed.Y, speed);
    float CurrentFov => IsMoving ? Mathf.Lerp(baseFov, baseFov + fovSpeedIncrease, speed) : baseFov;

    private Vector3 targetCameraAngle;
    private Vector3 targetCameraPos;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Input.MouseMode = Input.MouseModeEnum.Captured;
        staminaRegenTime = 1;
        stamina = maxStamina;
        baseFov = camera.Fov;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        float deltaF = (float)delta;
        debugFPS.Text = Engine.GetFramesPerSecond() + " FPS";

        isMoving = moveDir.LengthSquared() > 0;

        ProcessStamina(deltaF);
        ProcessMovement(deltaF);
        ProcessView();
        ProcessViewEffects(deltaF);
	}

    private void ProcessStamina(float delta)
    {
        if(staminaRegenTime < 1)
            staminaRegenTime += delta / staminaTimeToStartRegen;
        else if(stamina <= 0)
        {
            staminaRegenTime = 0;
            speed = 0;
        }
        
        float staminaLoss = IsMoving ? maxStamina / CurrentStaminaConsumption : 0;
        if(speed <= staminaRegenThreshold || !IsMoving)
            staminaLoss -= staminaRegenSpeed;

        stamina = Mathf.Clamp(stamina - staminaLoss * delta, 0, maxStamina);
        debug.Text = "Speed: " + speed + "\nRegenTime: " + staminaRegenTime + "\nInitLoss: " + (maxStamina / CurrentStaminaConsumption) + "\nLoss: " + staminaLoss;
        staminaProgressBar.Value = stamina / maxStamina;

        camera.Fov = Mathf.Lerp(camera.Fov, CurrentFov, speed > 0 ? 0.005f : 0.02f);
    }

    private void ProcessMovement(float delta)
    {
        Vector3 realDir = new Vector3();
        if(IsMoving)
        {
            Vector2 normDir = moveDir.Normalized();
            realDir = Basis.X * normDir.X + Basis.Z * normDir.Y;
        }

        if(IsOnFloor())
            yVelocity = 0;
        else
            yVelocity = Mathf.Clamp(yVelocity - 9.81f * delta, -200, 0);

        Vector3 velocity = new Vector3(realDir.X, yVelocity, realDir.Z) * CurrentSpeed;
        Velocity = velocity;
        MoveAndSlide();
    }

    private void ProcessView()
    {
        Vector2 viewVelocity = viewDir * mouseSensitivity * 0.1f;

        this.RotateY(viewVelocity.X);
        cameraHolder.RotateX(viewVelocity.Y);

        cameraHolder.RotationDegrees.Clamp(new Vector3(-80, 0, 0), new Vector3(80, 0, 0));

        viewDir = new Vector2(0,0);
    }

    private void ProcessViewEffects(float delta)
    {
        ProcessShaking(delta);
        ProcessBobbing(delta);

        cameraNode.Position = cameraNode.Position.Lerp(targetCameraPos, 0.1f);
        cameraNode.RotationDegrees = cameraNode.RotationDegrees.Lerp(targetCameraAngle, 0.1f);
    }

    private void ProcessShaking(float delta)
    {
        if(shakeTime <= 0)
            return;

        shakeTime -= delta;
        cameraNode.Position = new Vector3(GD.Randf() * 2 - 1, GD.Randf() * 2 - 1, GD.Randf() * 2 - 1) * 0.1f * Mathf.Lerp(0, shakePower, shakeTime / initialShakeTime);
    }

    private void ProcessBobbing(float delta)
    {
        neutralBobbing += delta * bobbingSpeed;
        moveBobbing += delta * CurrentSpeed;

        float z = 0;
        float y = 0;
        if(IsMoving)
        {
            y = CalcNegMoveBobbing(Utils.Mod(moveBobbing, (1.0f + moveBobbingSlope)), moveBobbingSlope, moveBobbingAmplitude);
            z = CalcPosMoveBobbing(Utils.Mod(moveBobbing, (1.0f + moveBobbingSlope) * 2), moveBobbingSlope, moveBobbingAngleAmplitude);
        }

        targetCameraPos = new Vector3(cameraNode.Position.X, y, cameraNode.Position.Z);
        targetCameraAngle = new Vector3(Mathf.Sin(neutralBobbing), Mathf.Sin(neutralBobbing * 2), z) * bobbingAmplitude;
    }

    /// <summary>
    /// This function enables a pattern of a cosine wave starting at zero, then rising to the amplitude `a` and falling linearly towards zero at a slope of `s`.
    /// It then reverses by calling CalcNegMoveBobbing. It does the same thing but the cosine wave falls to minus the amplitude `a` before rising linearly towards zero.
    /// When `x` is provided through the Mod function with a divisor of `(1 + a) * 2`, the pattern repeats infinitely.
    /// You could easily translate this function to a graphing calculator of your choice to see exactly what curve this generates.
    /// This function is used to simulate walking with a slow and smooth rise of the leg followed by a sharp step.
    /// </summary>
    /// <param name="x">The variable</param>
    /// <param name="s">The slope</param>
    /// <param name="a">The amplitude</param>
    /// <returns></returns>
    private float CalcPosMoveBobbing(float x, float s, float a)
    {
        if(x < 1)
            return (Mathf.Cos(x * Mathf.Pi) * a - a) / 2.0f;
        else if(x < 1 + s)
            return (x - 1) * (1.0f / s) * a - a;
        else
            return CalcNegMoveBobbing(x - 1 - s, s, a);
    }

    /// <summary>
    /// This function enables a pattern of a cosine wave starting at zero, then falling to minus amplitude `a` and rising linearly towards zero at a slope of `s`.
    /// When `x` is provided through the Mod function with a divisor of `1 + a`, the pattern repeats infinitely.
    /// You could easily translate this function to a graphing calculator of your choice to see exactly what curve this generates.
    /// This function is used to simulate walking with a slow and smooth rise of the leg followed by a sharp step.
    /// </summary>
    /// <param name="x">The variable</param>
    /// <param name="s">The slope</param>
    /// <param name="a">The amplitude</param>
    /// <returns></returns>
    private float CalcNegMoveBobbing(float x, float s, float a)
    {
        if(x < 1)
            return (-Mathf.Cos(x * Mathf.Pi) * a + a) / 2.0f;
        else if(x < 1 + s)
            return -(x - 1) * (1.0f / s) * a + a;
        else
            return 0;
    }

    public void Shake(float time, float power)
    {
        shakeTime = time;
        initialShakeTime = time;
        shakePower = power;
    }

    public override void _UnhandledKeyInput(InputEvent evnt)
    {
        if (evnt is InputEventKey eventKey)
        {
            if(eventKey.Keycode == Key.Z)
                moveDir = new Vector2(moveDir.X, eventKey.Pressed ? -1 : 0);
            else if(eventKey.Keycode == Key.S)
                moveDir = new Vector2(moveDir.X, eventKey.Pressed ? 1 : 0);
            else if(eventKey.Keycode == Key.Q)
                moveDir = new Vector2(eventKey.Pressed ? -1 : 0, moveDir.Y);
            else if(eventKey.Keycode == Key.D)
                moveDir = new Vector2(eventKey.Pressed ? 1 : 0, moveDir.Y);
            else if(eventKey.Keycode == Key.Escape)
                Input.MouseMode = Input.MouseModeEnum.Visible;
            else if(eventKey.Keycode == Key.K)
                Shake(3, 1);
        }
        
    }

    public override void _Input(InputEvent evnt)
    {
    }

    public override void _UnhandledInput(InputEvent evnt)
    {
        if(evnt is InputEventMouseMotion motion)
            viewDir = new Vector2(-Mathf.DegToRad(motion.Relative.X), -Mathf.DegToRad(motion.Relative.Y));
        else if(evnt is InputEventMouseButton button)
            if(button.ButtonMask == MouseButtonMask.Left && button.Pressed)
                Input.MouseMode = Input.MouseModeEnum.Captured;
            else if(button.Pressed && button.ButtonIndex == MouseButton.WheelUp && staminaRegenTime >= 1)
                speed = Mathf.Clamp(speed + 0.1f, 0, 1);
            else if(button.Pressed && button.ButtonIndex == MouseButton.WheelDown && staminaRegenTime >= 1)
                speed = Mathf.Clamp(speed - 0.1f, 0, 1);
    }
}
