namespace AvaloniaSilkEffects.Sonnet;

internal static partial class SonnetExtendedDrawLists
{
    // 68: symmetric waveform mirrored around an open center axis.
    private static void DrawSoundWave(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var bleed = Bleed(width, height, radius);
        const int steps = 72;
        foreach (var mirror in new[] { -1, 1 }) {
            target.MoveTo(-bleed.x, 0);
            for (var i = 1; i <= steps; i += 1) {
                var t = i / (double)steps;
                var x = -bleed.x + t * bleed.x * 2;
                var y = mirror * Math.Sin(t * TAU * 5 + seed * 0.13) * radius * 0.22 * Envelope(t);
                target.LineTo(x, y);
            }
            target.Stroke(color: mirror < 0 ? primary : secondary, width: mirror < 0 ? 2 : 1, alpha: 0.55);
        }
        target.MoveTo(-bleed.x, 0).LineTo(bleed.x, 0).Stroke(color: primary, width: 1, alpha: 0.18);
        return;
        double Envelope(double t) => Math.Sin(t * Math.PI) * (0.4 + 0.6 * Hash(seed, Math.Floor(t * 12 + 0.5), 401));
    }

    // 69: vinyl record — grooved arcs with gaps (never closed rings) and a tonearm.
    private static void DrawVinylGrooves(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var groove = 0; groove < 7; groove += 1) {
            var r = radius * (0.2 + groove * 0.08);
            var gapAt = Hash(seed, groove, 409) * TAU;
            target.Arc(0, 0, r, gapAt, gapAt + TAU * 0.86)
                .Stroke(color: groove % 3 == 0 ? secondary : primary, width: groove % 3 == 0 ? 2 : 1, alpha: 0.3 + groove * 0.05);
        }
        target.Circle(0, 0, radius * 0.12).Stroke(color: primary, width: 2, alpha: 0.6);
        target.Circle(0, 0, radius * 0.03).Fill(color: secondary, alpha: 0.8);
        // Tonearm sweeps in from a corner pivot.
        var pivotX = radius * 0.62;
        var pivotY = -radius * 0.52;
        target.Circle(pivotX, pivotY, radius * 0.035).Stroke(color: primary, width: 2, alpha: 0.6);
        target.MoveTo(pivotX, pivotY)
            .LineTo(radius * 0.18, -radius * 0.1)
            .Stroke(color: primary, width: 3, alpha: 0.5);
        target.Circle(radius * 0.18, -radius * 0.1, 3).Fill(color: secondary, alpha: 0.8);
    }

    // 70: equalizer bars blooming along a shallow bottom arc, no baseline frame.
    private static void DrawEqualizerBloom(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        const int bars = 17;
        for (var i = 0; i < bars; i += 1) {
            var t = i / (double)(bars - 1);
            var x = -radius * 0.66 + t * radius * 1.32;
            var arcY = radius * 0.5 - Math.Sin(t * Math.PI) * radius * 0.12;
            var h = radius * (0.08 + Math.Sin(t * Math.PI) * 0.3 * (0.5 + Hash(seed, i, 419) * 0.8));
            target.MoveTo(x, arcY).LineTo(x, arcY - h)
                .Stroke(color: i % 4 == 0 ? secondary : primary, width: 3, alpha: 0.5 + Math.Sin(t * Math.PI) * 0.25);
            target.Circle(x, arcY - h - 4, 1.6).Fill(color: secondary, alpha: 0.55);
        }
    }

    // 71: five eighth notes stepping along an arc, joined by one open beam.
    private static void DrawNoteArc(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var headR = radius * 0.045;
        var points = new List<(double x, double y)>();
        for (var i = 0; i < 5; i += 1) {
            var t = i / 4d;
            var x = -radius * 0.55 + t * radius * 1.1;
            var y = radius * 0.22 - Math.Sin(t * Math.PI * 0.9) * radius * 0.34;
            points.Add((x, y));
            target.Circle(x, y, headR).Fill(color: i == 2 ? secondary : primary, alpha: 0.8);
            target.MoveTo(x + headR, y).LineTo(x + headR, y - radius * 0.16)
                .Stroke(color: i == 2 ? secondary : primary, width: 2, alpha: 0.65);
        }
        // Beam connecting the stem tops, left open past the last note.
        target.MoveTo(points[0].x + headR, points[0].y - radius * 0.16);
        for (var i = 1; i < points.Count; i += 1) {
            target.LineTo(points[i].x + headR, points[i].y - radius * 0.16);
        }
        target.LineTo(points[4].x + radius * 0.12, points[4].y - radius * 0.13);
        target.Stroke(color: primary, width: 3, alpha: 0.5);
        // A stray flag curling off the first note.
        target.MoveTo(points[0].x + headR, points[0].y - radius * 0.16)
            .QuadraticCurveTo(points[0].x + radius * 0.1, points[0].y - radius * 0.1, points[0].x + radius * 0.06, points[0].y - radius * 0.02)
            .Stroke(color: secondary, width: 1.5, alpha: 0.5);
    }

    // 72: tuning fork with sound rings emanating on both sides.
    private static void DrawTuningFork(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var fx = (seed % 2 == 0 ? -1 : 1) * radius * 0.08;
        var topY = -radius * 0.4;
        var prongW = radius * 0.05;
        var prongGap = radius * 0.1;
        var uY = radius * 0.02;
        // Two prongs + U bend + handle, all one open path broken per segment.
        target.MoveTo(fx - prongGap / 2d - prongW, topY).LineTo(fx - prongGap / 2d - prongW, uY)
            .Stroke(color: primary, width: 2.5, alpha: 0.65);
        target.MoveTo(fx + prongGap / 2d + prongW, topY).LineTo(fx + prongGap / 2d + prongW, uY)
            .Stroke(color: primary, width: 2.5, alpha: 0.65);
        target.Arc(fx, uY, prongGap / 2d + prongW, 0, Math.PI)
            .Stroke(color: primary, width: 2.5, alpha: 0.65);
        target.MoveTo(fx, uY + prongGap / 2d + prongW).LineTo(fx, radius * 0.42)
            .Stroke(color: primary, width: 3, alpha: 0.6);
        target.Circle(fx, radius * 0.46, radius * 0.035).Stroke(color: secondary, width: 2, alpha: 0.6);
        // Vibration arcs left and right of the prongs.
        for (var side = -1; side <= 1; side += 2) {
            for (var ring = 0; ring < 3; ring += 1) {
                var r = radius * (0.14 + ring * 0.1);
                var cx = fx + side * radius * 0.06;
                var cy = topY + radius * 0.1;
                target.Arc(cx, cy, r, side < 0 ? Math.PI * 0.6 : -Math.PI * 0.4, side < 0 ? Math.PI * 1.4 : Math.PI * 0.4)
                    .Stroke(color: ring == 1 ? secondary : primary, width: 1.5, alpha: 0.42 - ring * 0.1);
            }
        }
    }

    // 73: piano keys riding a shallow ribbon curve — alternating long/short bars.
    private static void DrawPianoRibbon(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var bleed = Bleed(width, height, radius);
        // Ribbon guide curve, open at both ends.
        target.MoveTo(-bleed.x, radius * 0.18)
            .BezierCurveTo(-radius * 0.3, -radius * 0.05, radius * 0.3, radius * 0.3, bleed.x, radius * 0.05)
            .Stroke(color: primary, width: 1, alpha: 0.25);
        const int keys = 12;
        for (var i = 0; i < keys; i += 1) {
            var t = i / (double)(keys - 1);
            var x = -radius * 0.6 + t * radius * 1.2;
            var baseY = radius * 0.18 + Math.Sin(t * Math.PI) * -radius * 0.1 + t * -radius * 0.06;
            var black = i % 12 is 1 or 3 or 6 or 8 or 10;
            var len = radius * (black ? 0.14 : 0.24);
            target.MoveTo(x, baseY).LineTo(x, baseY - len)
                .Stroke(color: black ? secondary : primary, width: black ? 4 : 3, alpha: black ? 0.7 : 0.45);
        }
    }

    // 74: metronome with tilted pendulum and motion echo arcs.
    private static void DrawMetronome(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        const int cx = 0;
        var baseY = radius * 0.4;
        var topY = -radius * 0.36;
        // Tapered body outline, base left open.
        target.MoveTo(cx - radius * 0.2, baseY).LineTo(cx - radius * 0.06, topY)
            .Stroke(color: primary, width: 2, alpha: 0.6);
        target.MoveTo(cx + radius * 0.2, baseY).LineTo(cx + radius * 0.06, topY)
            .Stroke(color: primary, width: 2, alpha: 0.6);
        target.MoveTo(cx - radius * 0.06, topY).LineTo(cx + radius * 0.06, topY)
            .Stroke(color: primary, width: 2, alpha: 0.6);
        // Pendulum.
        var tilt = (Hash(seed, 0, 431) - 0.5) * 0.9;
        var pivotY = radius * 0.16;
        var tipX = cx + Math.Sin(tilt) * radius * 0.5;
        var tipY = pivotY - Math.Cos(tilt) * radius * 0.5;
        target.Circle(cx, pivotY, radius * 0.03).Fill(color: secondary, alpha: 0.85);
        target.MoveTo(cx, pivotY).LineTo(tipX, tipY).Stroke(color: secondary, width: 2, alpha: 0.7);
        target.Rectangle(tipX - 4, tipY - 4, 8, 8).Fill(color: secondary, alpha: 0.7);
        // Echo arcs sweeping with the pendulum.
        for (var i = 0; i < 3; i += 1) {
            var r = radius * (0.24 + i * 0.12);
            target.Arc(cx, pivotY, r, -Math.PI / 2d - 0.5 - i * 0.1, -Math.PI / 2d + 0.5 + i * 0.1)
                .Stroke(color: primary, width: 1, alpha: 0.3 - i * 0.06);
        }
    }

    // 75: five staff lines undulating across the bleed with a few free notes.
    private static void DrawStaffWave(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var bleed = Bleed(width, height, radius);
        for (var line = 0; line < 5; line += 1) {
            var y0 = -radius * 0.16 + line * radius * 0.08;
            var lift = (line % 2 == 0 ? 1 : -1) * radius * 0.05;
            target.MoveTo(-bleed.x, y0)
                .BezierCurveTo(-radius * 0.3, y0 + lift, radius * 0.3, y0 - lift, bleed.x, y0)
                .Stroke(color: primary, width: 1, alpha: 0.3 + (line == 2 ? 0.15 : 0));
        }
        for (var i = 0; i < 4; i += 1) {
            var x = -radius * 0.45 + i * radius * 0.3 + (Hash(seed, i, 439) - 0.5) * radius * 0.08;
            var y = -radius * 0.16 + Math.Floor(Hash(seed, i, 443) * 5) * radius * 0.08;
            target.Circle(x, y, radius * 0.032).Fill(color: i % 2 == 0 ? secondary : primary, alpha: 0.85);
            target.MoveTo(x + radius * 0.032, y).LineTo(x + radius * 0.032, y - radius * 0.14)
                .Stroke(color: i % 2 == 0 ? secondary : primary, width: 1.5, alpha: 0.6);
        }
    }
}
