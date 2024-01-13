using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class CanvasControl : Control
{
    private int count = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // this.QueueRedraw();
    }

    public override void _Draw()
    {
        Color color = Colors.White; // Red color
        var radius = Math.Min(this.Size.X, this.Size.Y) / 2 - 50; // Radius of the circle
        float startAngle = 0.0f; // Start angle in radians
        float endAngle = Mathf.Pi * 2; // End angle in radians (full circle)
        int pointCount = 64; // Number of points on the circle (increase for smoother circle)
        bool antialiased = true; // Smooth the line
        var position = new Vector2(this.Size.X / 2, this.Size.Y / 2);

        DrawArc(position, radius, startAngle, endAngle, pointCount, color, 1.0f, antialiased);

        // draw smaller circles on bigger circle
        var balance = Globals.Ring?.LoadBalance();
        var balance2 = (balance ?? Enumerable.Empty<KeyValuePair<string, string>>()).GroupBy(_ => _.Value);
        int count = (balance2?.Count() ?? 0) + (balance2?.SelectMany(_ => _).Count() ?? 0);
        int i = 0;
        var debugPosition = new Vector2(position.X - 50, position.Y - ThemeDB.FallbackFontSize);
        foreach (var pair in balance2)
        {
            debugPosition = new Vector2(debugPosition.X, debugPosition.Y + ThemeDB.FallbackFontSize);
            DrawString(ThemeDB.FallbackFont, debugPosition, $"{pair.Key}: {pair.Count()}", HorizontalAlignment.Left, -1, ThemeDB.FallbackFontSize, Colors.White);
            foreach (var w in pair)
            {
                var angle = i * Mathf.Pi / (count / 2);
                var x = Mathf.Cos(angle) * radius;
                var y = Mathf.Sin(angle) * radius;
                var center = new Vector2(x, y) + position;
                DrawCircle(center, 5, Colors.Cyan);
                i++;
            }

            {
                var angle = i * Mathf.Pi / (count / 2);
                var x = Mathf.Cos(angle) * radius;
                var y = Mathf.Sin(angle) * radius;
                var center = new Vector2(x, y) + position;
                DrawCircle(center, 10, Colors.Violet);
            }
            i++;
        }
    }
}
