using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using System;
using System.Numerics;

namespace Plants;

public class MinigiocoBreakout : MinigiocoBase
{
    public override string Nome => "Breakout";
    public override string Descrizione => "Distruggi tutti i mattoni con la palla!";
    public override TipoMinigioco Tipo => TipoMinigioco.Breakout;

    private float paddleX;
    private float paddleY;
    private const int PADDLE_W = 80;
    private const int PADDLE_H = 12;
    private const int PADDLE_Y_OFFSET = 60;

    private Vector2 ballPos;
    private Vector2 ballVel;
    private const float BALL_SPEED = 400f;
    private const float BALL_RADIUS = 8f;

    private const int BRICK_ROWS = 5;
    private const int BRICK_COLS = 8;
    private const int BRICK_W = 60;
    private const int BRICK_H = 20;
    private const int BRICK_PADDING = 8;
    private const int BRICK_TOP_OFFSET = 80;

    private bool[,] bricks;
    private int bricksRemaining;

    private bool ballLaunched = false;
    private const float LAUNCH_COOLDOWN = 1f;
    private float launchTimer = 0f;

    private readonly Color atariGreen = new Color(0, 255, 65, 255);
    private readonly Color atariDim = new Color(0, 180, 45, 255);

    public MinigiocoBreakout() : base() { }

    protected override void OnAvvia()
    {
        tempoTotale = 60f;
        punteggioMassimo = BRICK_ROWS * BRICK_COLS;

        bricks = new bool[BRICK_ROWS, BRICK_COLS];
        bricksRemaining = 0;
        for (int r = 0; r < BRICK_ROWS; r++)
            for (int c = 0; c < BRICK_COLS; c++)
            {
                bricks[r, c] = true;
                bricksRemaining++;
            }

        paddleX = sw / 2f - PADDLE_W / 2f;
        paddleY = sh - PADDLE_Y_OFFSET;
        ballPos = new Vector2(paddleX + PADDLE_W / 2f, paddleY - BALL_RADIUS - 2f);
        ballVel = Vector2.Zero;
        ballLaunched = false;
        launchTimer = 0f;
    }

    protected override void UpdateGioco(float dt)
    {
        paddleX = Math.Clamp(Input.GetMouseX() - PADDLE_W / 2f, 20, sw - PADDLE_W - 20);

        if (!ballLaunched)
        {
            launchTimer += dt;
            if (launchTimer >= LAUNCH_COOLDOWN &&
                (Input.IsMouseButtonPressed(MouseButton.Left) || Input.IsKeyPressed(KeyboardKey.Space)))
            {
                ballLaunched = true;
                float angle = MathHelper.Lerp(2.5f, 3.8f, RandomHelper.Float(0f, 1f));
                ballVel = new Vector2(MathF.Cos(angle) * BALL_SPEED, MathF.Sin(angle) * BALL_SPEED);
            }
        }

        if (ballLaunched)
        {
            ballPos += ballVel * dt;

            if (ballPos.X - BALL_RADIUS <= 20)
            {
                ballPos.X = 20 + BALL_RADIUS;
                ballVel.X = Math.Abs(ballVel.X);
            }
            if (ballPos.X + BALL_RADIUS >= sw - 20)
            {
                ballPos.X = sw - 20 - BALL_RADIUS;
                ballVel.X = -Math.Abs(ballVel.X);
            }
            if (ballPos.Y - BALL_RADIUS <= 30)
            {
                ballPos.Y = 30 + BALL_RADIUS;
                ballVel.Y = Math.Abs(ballVel.Y);
            }

            if (ballVel.Y > 0 &&
                ballPos.Y + BALL_RADIUS >= paddleY &&
                ballPos.Y - BALL_RADIUS <= paddleY + PADDLE_H &&
                ballPos.X >= paddleX &&
                ballPos.X <= paddleX + PADDLE_W)
            {
                ballPos.Y = paddleY - BALL_RADIUS;
                float hitPos = (ballPos.X - paddleX) / PADDLE_W;
                float angle = MathHelper.Lerp(2.8f, 3.5f, hitPos);
                float speed = ballVel.Length();
                ballVel = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed);
                if (ballVel.Y < 0) ballVel.Y = -ballVel.Y;
            }

            CheckBrickCollisions();

            if (ballPos.Y > sh + BALL_RADIUS)
            {
                ballLaunched = false;
                launchTimer = 0f;
                ballPos = new Vector2(paddleX + PADDLE_W / 2f, paddleY - BALL_RADIUS - 2f);
                ballVel = Vector2.Zero;

                if (bricksRemaining <= 0)
                {
                    Termina(true);
                    return;
                }
            }
        }
        else
        {
            ballPos = new Vector2(paddleX + PADDLE_W / 2f, paddleY - BALL_RADIUS - 2f);
        }
    }

    private void CheckBrickCollisions()
    {
        int totalBricksW = BRICK_COLS * BRICK_W + (BRICK_COLS - 1) * BRICK_PADDING;
        int startX = (sw - totalBricksW) / 2;

        for (int r = 0; r < BRICK_ROWS; r++)
        {
            for (int c = 0; c < BRICK_COLS; c++)
            {
                if (!bricks[r, c]) continue;

                int brickX = startX + c * (BRICK_W + BRICK_PADDING);
                int brickY = BRICK_TOP_OFFSET + r * (BRICK_H + BRICK_PADDING);

                float closestX = Math.Clamp(ballPos.X, brickX, brickX + BRICK_W);
                float closestY = Math.Clamp(ballPos.Y, brickY, brickY + BRICK_H);

                float dx = ballPos.X - closestX;
                float dy = ballPos.Y - closestY;

                if (dx * dx + dy * dy <= BALL_RADIUS * BALL_RADIUS)
                {
                    bricks[r, c] = false;
                    bricksRemaining--;
                    punteggio++;

                    float overlapLeft = (ballPos.X + BALL_RADIUS) - brickX;
                    float overlapRight = (brickX + BRICK_W) - (ballPos.X - BALL_RADIUS);
                    float overlapTop = (ballPos.Y + BALL_RADIUS) - brickY;
                    float overlapBottom = (brickY + BRICK_H) - (ballPos.Y - BALL_RADIUS);

                    float minOverlapX = Math.Min(overlapLeft, overlapRight);
                    float minOverlapY = Math.Min(overlapTop, overlapBottom);

                    if (minOverlapX < minOverlapY)
                        ballVel.X = -ballVel.X;
                    else
                        ballVel.Y = -ballVel.Y;

                    if (bricksRemaining <= 0)
                    {
                        Termina(true);
                        return;
                    }
                    return;
                }
            }
        }
    }

    protected override void DrawGioco()
    {
        Graphics.DrawRectangleLines(18, 28, sw - 36, sh - 48, atariDim);

        int totalBricksW = BRICK_COLS * BRICK_W + (BRICK_COLS - 1) * BRICK_PADDING;
        int startX = (sw - totalBricksW) / 2;

        for (int r = 0; r < BRICK_ROWS; r++)
        {
            Color rowColor = r switch
            {
                0 => new Color(255, 100, 100, 255),
                1 => new Color(255, 180, 80, 255),
                2 => new Color(255, 255, 80, 255),
                3 => new Color(80, 255, 80, 255),
                _ => new Color(80, 180, 255, 255)
            };

            for (int c = 0; c < BRICK_COLS; c++)
            {
                if (!bricks[r, c]) continue;

                int bx = startX + c * (BRICK_W + BRICK_PADDING);
                int by = BRICK_TOP_OFFSET + r * (BRICK_H + BRICK_PADDING);

                Graphics.DrawRectangle(bx, by, BRICK_W, BRICK_H, rowColor);
                Graphics.DrawRectangle(bx, by, BRICK_W, 3, Color.White);
                Graphics.DrawRectangle(bx, by + BRICK_H - 3, BRICK_W, 3, new Color(0, 0, 0, 100));
            }
        }

        Graphics.DrawRectangle((int)paddleX, (int)paddleY, PADDLE_W, PADDLE_H, atariGreen);
        Graphics.DrawRectangleLines((int)paddleX, (int)paddleY, PADDLE_W, PADDLE_H, Color.White);

        if (ballLaunched || launchTimer > 0.3f)
        {
            Color ballColor = ballLaunched ? atariGreen : atariDim;
            Graphics.DrawCircleV(ballPos, BALL_RADIUS, ballColor);
            Graphics.DrawCircleLinesV(ballPos, BALL_RADIUS, Color.White);
        }

        if (!ballLaunched && launchTimer >= LAUNCH_COOLDOWN)
        {
            string hint = "CLICCA PER LANCIARE";
            int hintW = GuiTheme.MeasureText(hint, 12);
            GuiTheme.DrawText(hint, (sw - hintW) / 2, sh - 30, 12, atariDim);
        }
    }
}