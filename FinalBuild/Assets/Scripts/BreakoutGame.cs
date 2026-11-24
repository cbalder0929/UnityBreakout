using System.Collections.Generic;
using UnityEngine;

namespace Breakout
{
    public sealed class BreakoutGame : MonoBehaviour
    {
        const float Radius = .16f, Speed = 7f, PaddleY = -4f, HalfPaddle = 1.15f;
        sealed class Brick { public Rect bounds; public GameObject visual; public int points; }
        readonly List<Brick> bricks = new List<Brick>();
        Sprite sprite, ballSprite;
        Transform paddle, ball;
        Vector2 position, velocity;
        int score, lives;
        bool launched, ended, paused;
        GUIStyle heading, text;

        void Start()
        {
            Application.targetFrameRate = 120;
            var camera = Camera.main;
            if (camera == null)
            {
                var obj = new GameObject("Main Camera", typeof(Camera));
                obj.tag = "MainCamera";
                camera = obj.GetComponent<Camera>();
            }
            camera.orthographic = true;
            camera.orthographicSize = 6.2f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(.025f, .04f, .09f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
            Box("Left wall", new Vector2(-8.12f, 0), new Vector2(.24f, 10), Color.gray);
            Box("Right wall", new Vector2(8.12f, 0), new Vector2(.24f, 10), Color.gray);
            Box("Ceiling", new Vector2(0, 5.12f), new Vector2(16.48f, .24f), Color.gray);
            paddle = Box("Paddle", new Vector2(0, PaddleY), new Vector2(HalfPaddle * 2, .28f), Color.cyan);
            ball = Box("Ball", Vector2.zero, Vector2.one * Radius * 2, Color.white);
            var circle = new Texture2D(32, 32);
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                circle.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) <= 15.5f ? Color.white : Color.clear);
            circle.Apply();
            ballSprite = Sprite.Create(circle, new Rect(0, 0, 32, 32), Vector2.one * .5f, 32);
            ball.GetComponent<SpriteRenderer>().sprite = ballSprite;
            ResetGame();
        }

        Transform Box(string objectName, Vector2 point, Vector2 size, Color color)
        {
            var obj = new GameObject(objectName);
            obj.transform.SetParent(transform);
            obj.transform.position = point;
            obj.transform.localScale = size;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            return obj.transform;
        }

        void ResetGame()
        {
            foreach (var brick in bricks) Destroy(brick.visual);
            bricks.Clear();
            score = 0; lives = 3; ended = paused = false;
            paddle.position = new Vector2(0, PaddleY);
            for (int row = 0; row < 5; row++)
            for (int col = 0; col < 10; col++)
            {
                var point = new Vector2(-6.75f + col * 1.5f, 3.65f - row * .65f);
                var visual = Box("Brick " + row + "-" + col, point, new Vector2(1.38f, .48f), Color.HSVToRGB(.02f + row * .12f, .7f, 1));
                bricks.Add(new Brick { bounds = new Rect(point - new Vector2(.69f, .24f), new Vector2(1.38f, .48f)), visual = visual.gameObject, points = (5 - row) * 10 });
            }
            Serve();
        }

        void Serve()
        {
            launched = false;
            velocity = Vector2.zero;
            position = new Vector2(paddle.position.x, PaddleY + .14f + Radius + .03f);
            ball.position = position;
        }

        void Update()
        {
            Camera.main.orthographicSize = Mathf.Max(6.2f, 8.6f / Camera.main.aspect);
            if (Input.GetKeyDown(KeyCode.R)) ResetGame();
            if (Input.GetKeyDown(KeyCode.Escape) && !ended) paused = !paused;
            if (paused || ended) return;
            float direction = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direction--;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direction++;
            float x = paddle.position.x + direction * 10f * Time.deltaTime;
            if (Input.GetMouseButton(0)) x = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            paddle.position = new Vector2(Mathf.Clamp(x, -8 + HalfPaddle, 8 - HalfPaddle), PaddleY);
            if (!launched)
            {
                position = new Vector2(paddle.position.x, PaddleY + .14f + Radius + .03f);
                if (Input.GetKeyDown(KeyCode.Space)) { launched = true; velocity = new Vector2(.35f, 1).normalized * Speed; }
            }
            else
            {
                // Substeps prevent tunneling through the thin paddle and bricks.
                float remaining = Mathf.Min(Time.deltaTime, .1f);
                while (remaining > 0 && launched && !ended)
                {
                    float step = Mathf.Min(remaining, 1f / 240f);
                    TickBall(step);
                    remaining -= step;
                }
            }
            ball.position = position;
        }

        void TickBall(float dt)
        {
            Vector2 previous = position;
            position += velocity * dt;
            if (position.x < -8 + Radius) { position.x = -8 + Radius; velocity.x = Mathf.Abs(velocity.x); }
            if (position.x > 8 - Radius) { position.x = 8 - Radius; velocity.x = -Mathf.Abs(velocity.x); }
            if (position.y > 5 - Radius) { position.y = 5 - Radius; velocity.y = -Mathf.Abs(velocity.y); }
            float top = PaddleY + .14f + Radius;
            if (velocity.y < 0 && previous.y >= top && position.y <= top)
            {
                float hitX = Mathf.Lerp(previous.x, position.x, Mathf.InverseLerp(previous.y, position.y, top));
                if (Mathf.Abs(hitX - paddle.position.x) <= HalfPaddle + Radius)
                {
                    position.y = top;
                    float offset = Mathf.Clamp((hitX - paddle.position.x) / HalfPaddle, -1, 1);
                    velocity = new Vector2(offset * .85f, 1).normalized * Speed;
                }
            }
            for (int i = bricks.Count - 1; i >= 0; i--)
            {
                var brick = bricks[i];
                var bounds = brick.bounds;
                bounds.xMin -= Radius; bounds.xMax += Radius;
                bounds.yMin -= Radius; bounds.yMax += Radius;
                if (!bounds.Contains(position)) continue;
                if (previous.y >= bounds.yMax) { position.y = bounds.yMax; velocity.y = Mathf.Abs(velocity.y); }
                else if (previous.y <= bounds.yMin) { position.y = bounds.yMin; velocity.y = -Mathf.Abs(velocity.y); }
                else if (previous.x <= bounds.xMin) { position.x = bounds.xMin; velocity.x = -Mathf.Abs(velocity.x); }
                else { position.x = bounds.xMax; velocity.x = Mathf.Abs(velocity.x); }
                score += brick.points;
                Destroy(brick.visual);
                bricks.RemoveAt(i);
                if (bricks.Count == 0) ended = true;
                break;
            }
            if (position.y < -5.5f)
            {
                lives--;
                if (lives == 0) ended = true;
                else Serve();
            }
        }

        void OnGUI()
        {
            if (heading == null)
            {
                heading = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                text = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            }
            GUI.Label(new Rect(0, 8, Screen.width, 40), "BREAKOUT", heading);
            GUI.Label(new Rect(0, 45, Screen.width, 30), "Score: " + score + "     Lives: " + lives + "     Blocks: " + bricks.Count, text);
            GUI.Label(new Rect(0, Screen.height - 42, Screen.width, 32), "A/D or arrows: move | Hold mouse: move | Space: launch | Esc: pause | R: restart", text);
            string message = ended ? (bricks.Count == 0 ? "YOU WIN!" : "GAME OVER") + "\nPress R to play again" : paused ? "PAUSED\nPress Esc to resume" : !launched ? "Press Space to launch" : "";
            if (message.Length > 0) GUI.Label(new Rect(0, Screen.height * .53f, Screen.width, 100), message, heading);
        }

        void OnDestroy()
        {
            if (sprite != null) { Destroy(sprite.texture); Destroy(sprite); }
            if (ballSprite != null) { Destroy(ballSprite.texture); Destroy(ballSprite); }
        }
    }
}
