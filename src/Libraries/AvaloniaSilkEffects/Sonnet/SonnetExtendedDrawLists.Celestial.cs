namespace AvaloniaSilkEffects.Sonnet;

internal static partial class SonnetExtendedDrawLists
{
    // 48: twin log-spiral arms with a bright core and free-floating star dust.
    private static void DrawSpiralGalaxy(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var arm = 0; arm < 2; arm += 1) {
            var offset = arm * Math.PI + Hash(seed, arm, 101) * 0.5;
            target.MoveTo(Math.Cos(offset) * radius * 0.06, Math.Sin(offset) * radius * 0.05);
            var steps = 56;
            for (var i = 1; i <= steps; i += 1) {
                var t = i / (double)steps;
                var angle = offset + t * Math.PI * 3.1;
                var r = radius * (0.06 + t * 0.62);
                target.LineTo(Math.Cos(angle) * r, Math.Sin(angle) * r * 0.72);
            }
            target.Stroke(color: arm == 0 ? primary : secondary, width: 2, alpha: 0.5 - arm * 0.12);
        }
        target.Circle(0, 0, radius * 0.07).Fill(color: primary, alpha: 0.7);
        target.Circle(0, 0, radius * 0.12).Stroke(color: primary, width: 1, alpha: 0.3);
        for (var i = 0; i < 14; i += 1) {
            var angle = Hash(seed, i, 103) * TAU;
            var r = radius * (0.2 + Hash(seed, i, 107) * 0.55);
            target.Circle(Math.Cos(angle) * r, Math.Sin(angle) * r * 0.72, 1.4 + Hash(seed, i, 109) * 2.2)
                .Fill(color: i % 3 == 0 ? secondary : primary, alpha: 0.3 + Hash(seed, i, 113) * 0.35);
        }
    }

    // 49: a comet head with three curved tail trails and cross sparkles.
    private static void DrawCometTrail(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var direction = seed % 2 == 0 ? 1 : -1;
        var hx = radius * 0.34 * direction;
        var hy = -radius * 0.18;
        for (var tail = 0; tail < 3; tail += 1) {
            var spread = (tail - 1) * radius * 0.12;
            target.MoveTo(hx - direction * radius * 0.04, hy + spread * 0.3)
                .BezierCurveTo(
                    hx - direction * radius * 0.35, hy + spread,
                    hx - direction * radius * 0.6, hy + radius * 0.16 + spread,
                    hx - direction * radius * (0.85 + tail * 0.06), hy + radius * 0.3 + spread * 1.2)
                .Stroke(color: tail == 1 ? secondary : primary, width: 3 - tail, alpha: 0.55 - tail * 0.12);
        }
        target.Circle(hx, hy, radius * 0.09).Fill(color: primary, alpha: 0.75);
        target.Circle(hx, hy, radius * 0.14).Stroke(color: primary, width: 1, alpha: 0.35);
        for (var i = 0; i < 5; i += 1) {
            var x = (Hash(seed, i, 127) - 0.5) * radius * 1.4;
            var y = radius * (0.1 + Hash(seed, i, 131) * 0.5);
            var s = 2.5 + Hash(seed, i, 137) * 2.5;
            target.MoveTo(x - s, y).LineTo(x + s, y).Stroke(color: secondary, width: 1, alpha: 0.45);
            target.MoveTo(x, y - s).LineTo(x, y + s).Stroke(color: secondary, width: 1, alpha: 0.45);
        }
    }

    // 50: eclipsed disc with an uneven corona of alternating rays.
    private static void DrawEclipseCorona(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var discR = radius * 0.24;
        target.Circle(0, 0, discR).Fill(color: primary, alpha: 0.16);
        target.Circle(0, 0, discR).Stroke(color: secondary, width: 2, alpha: 0.65);
        target.Circle(0, 0, discR * 1.14).Stroke(color: primary, width: 1, alpha: 0.25);
        var rays = 28;
        for (var i = 0; i < rays; i += 1) {
            var angle = (i / (double)rays) * TAU + Hash(seed, i, 139) * 0.08;
            var inner = discR * 1.2;
            var outer = radius * (i % 2 == 0 ? 0.6 : 0.42) * (0.85 + Hash(seed, i, 149) * 0.3);
            target.MoveTo(Math.Cos(angle) * inner, Math.Sin(angle) * inner)
                .LineTo(Math.Cos(angle) * outer, Math.Sin(angle) * outer)
                .Stroke(color: i % 4 == 0 ? secondary : primary, width: i % 2 == 0 ? 2 : 1, alpha: 0.3 + (i % 3) * 0.1);
        }
    }

    // 51: diagonal meteor streaks with glowing heads, all open-ended.
    private static void DrawMeteorShower(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var direction = seed % 2 == 0 ? 1 : -1;
        for (var i = 0; i < 8; i += 1) {
            var x = (Hash(seed, i, 151) - 0.5) * radius * 1.5;
            var y = -radius * 0.55 + Hash(seed, i, 157) * radius * 0.9;
            var len = radius * (0.2 + Hash(seed, i, 163) * 0.3);
            var dx = direction * len;
            var dy = len * 0.55;
            target.MoveTo(x, y).LineTo(x - dx, y - dy)
                .Stroke(color: primary, width: 2, alpha: 0.55);
            target.MoveTo(x - dx * 0.15, y - dy * 0.15 + 3).LineTo(x - dx * 0.85, y - dy * 0.85 + 3)
                .Stroke(color: secondary, width: 1, alpha: 0.3);
            target.Circle(x, y, 2 + Hash(seed, i, 167) * 2)
                .Fill(color: i % 2 == 0 ? secondary : primary, alpha: 0.7);
        }
    }

    // 52: broken orbit rings carrying small satellite diamonds.
    private static void DrawOrbitSatellites(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var ring = 0; ring < 3; ring += 1) {
            var r = radius * (0.28 + ring * 0.18);
            var gapStart = Hash(seed, ring, 173) * TAU;
            var segs = 3 + ring;
            for (var s = 0; s < segs; s += 1) {
                var start = gapStart + (s / (double)segs) * TAU;
                target.Arc(0, 0, r, start, start + (TAU / (double)segs) * 0.68)
                    .Stroke(color: ring == 1 ? secondary : primary, width: ring == 0 ? 2 : 1, alpha: 0.35 + ring * 0.08);
            }
            var satAngle = Hash(seed, ring, 179) * TAU;
            var sx = Math.Cos(satAngle) * r;
            var sy = Math.Sin(satAngle) * r;
            var d = 5 + ring * 2;
            target.MoveTo(sx, sy - d).LineTo(sx + d, sy).LineTo(sx, sy + d).LineTo(sx - d, sy).LineTo(sx, sy - d)
                .Fill(color: secondary, alpha: 0.75);
        }
        target.Circle(0, 0, radius * 0.06).Fill(color: primary, alpha: 0.8);
    }

    // 53: vertical aurora ribbons flowing down from the top, no edges.
    private static void DrawAuroraRibbons(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var band = 0; band < 4; band += 1) {
            var x0 = -radius * 0.6 + band * radius * 0.38 + (Hash(seed, band, 181) - 0.5) * radius * 0.1;
            var sway = (band % 2 == 0 ? 1 : -1) * radius * 0.2;
            target.MoveTo(x0, -radius * 0.75)
                .BezierCurveTo(
                    x0 + sway, -radius * 0.35,
                    x0 - sway, radius * 0.1,
                    x0 + sway * 0.6, radius * 0.55)
                .Stroke(color: band % 2 == 0 ? primary : secondary, width: 7 - band, alpha: 0.16 + band * 0.05);
            target.MoveTo(x0 + radius * 0.06, -radius * 0.7)
                .BezierCurveTo(
                    x0 + sway + radius * 0.06, -radius * 0.3,
                    x0 - sway + radius * 0.06, radius * 0.12,
                    x0 + sway * 0.6 + radius * 0.06, radius * 0.5)
                .Stroke(color: primary, width: 1, alpha: 0.3);
        }
        for (var i = 0; i < 8; i += 1) {
            target.Circle(
                (Hash(seed, i, 191) - 0.5) * radius * 1.5,
                -radius * 0.6 + Hash(seed, i, 193) * radius * 0.5,
                1.4).Fill(color: secondary, alpha: 0.5);
        }
    }

    // 54: crescent with halo ring and hanging star pendants.
    private static void DrawCrescentHalo(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var moonR = radius * 0.3;
        var cx = -radius * 0.12;
        var cy = -radius * 0.1;
        target.MoveTo(cx, cy - moonR);
        target.Arc(cx, cy, moonR, -Math.PI / 2d, Math.PI / 2d, false);
        target.QuadraticCurveTo(cx - moonR * 0.45, cy, cx, cy - moonR);
        target.Fill(color: primary, alpha: 0.55);
        target.Circle(cx, cy, moonR * 1.35).Stroke(color: secondary, width: 1, alpha: 0.3);
        target.Circle(cx, cy, moonR * 1.5).Stroke(color: primary, width: 1, alpha: 0.16);
        for (var i = 0; i < 3; i += 1) {
            var px = radius * (0.18 + i * 0.16);
            var topY = -radius * 0.5 + Hash(seed, i, 197) * radius * 0.1;
            var len = radius * (0.14 + Hash(seed, i, 199) * 0.12);
            target.MoveTo(px, topY).LineTo(px, topY + len).Stroke(color: primary, width: 1, alpha: 0.4);
            var sr = 4 + i;
            var sy = topY + len + sr;
            target.MoveTo(px, sy - sr).LineTo(px + sr * 0.25, sy - sr * 0.25)
                .LineTo(px + sr, sy).LineTo(px + sr * 0.25, sy + sr * 0.25)
                .LineTo(px, sy + sr).LineTo(px - sr * 0.25, sy + sr * 0.25)
                .LineTo(px - sr, sy).LineTo(px - sr * 0.25, sy - sr * 0.25)
                .LineTo(px, sy - sr)
                .Stroke(color: secondary, width: 1, alpha: 0.6);
        }
    }

    // 55: nested organic nebula veils — closed bezier blobs, no straight edges.
    private static void DrawNebulaVeil(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var blob = 0; blob < 4; blob += 1) {
            var bx = (Hash(seed, blob, 211) - 0.5) * radius * 0.5;
            var by = (Hash(seed, blob, 223) - 0.5) * radius * 0.4;
            var br = radius * (0.2 + blob * 0.1);
            var wobble = Hash(seed, blob, 227) * 0.6;
            target.MoveTo(bx + br, by);
            target.BezierCurveTo(bx + br, by - br * (0.6 + wobble * 0.3), bx + br * 0.5, by - br, bx, by - br * (0.9 - wobble * 0.2));
            target.BezierCurveTo(bx - br * 0.6, by - br * 0.8, bx - br, by - br * 0.3, bx - br * (0.85 + wobble * 0.2), by + br * 0.2);
            target.BezierCurveTo(bx - br * 0.7, by + br * 0.7, bx - br * 0.2, by + br, bx + br * 0.3, by + br * (0.8 + wobble * 0.2));
            target.BezierCurveTo(bx + br * 0.8, by + br * 0.6, bx + br, by + br * 0.4, bx + br, by);
            target.Stroke(color: blob % 2 == 0 ? primary : secondary, width: 1.5, alpha: 0.35 - blob * 0.04);
            if (blob < 2) target.Fill(color: primary, alpha: 0.05);
        }
        for (var i = 0; i < 10; i += 1) {
            target.Circle(
                (Hash(seed, i, 229) - 0.5) * radius * 1.2,
                (Hash(seed, i, 233) - 0.5) * radius * 1.0,
                1.2 + Hash(seed, i, 239) * 1.8).Fill(color: primary, alpha: 0.25 + Hash(seed, i, 241) * 0.3);
        }
    }

    // 56: survey-style star map — faint cross grid plus one bright constellation.
    private static void DrawStarMap(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var gx = 0; gx < 5; gx += 1) {
            for (var gy = 0; gy < 4; gy += 1) {
                var x = -radius * 0.6 + gx * radius * 0.3;
                var y = -radius * 0.45 + gy * radius * 0.3;
                target.MoveTo(x - 3, y).LineTo(x + 3, y).Stroke(color: primary, width: 1, alpha: 0.18);
                target.MoveTo(x, y - 3).LineTo(x, y + 3).Stroke(color: primary, width: 1, alpha: 0.18);
            }
        }
        var nodes = 6;
        var px = 0d;
        var py = 0d;
        for (var i = 0; i < nodes; i += 1) {
            var x = -radius * 0.5 + Hash(seed, i, 251) * radius;
            var y = -radius * 0.4 + Hash(seed, i, 257) * radius * 0.8;
            if (i > 0) {
                target.MoveTo(px, py).LineTo(x, y).Stroke(color: secondary, width: 1.5, alpha: 0.55);
            }
            target.Circle(x, y, 3).Fill(color: primary, alpha: 0.8);
            target.Circle(x, y, 6.5).Stroke(color: primary, width: 1, alpha: 0.3);
            px = x;
            py = y;
        }
    }

    // 57: moon above, open tide arcs below — a bridge between sky and sea.
    private static void DrawLunarTide(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var mx = radius * 0.28 * (seed % 2 == 0 ? 1 : -1);
        var my = -radius * 0.34;
        target.Circle(mx, my, radius * 0.16).Fill(color: primary, alpha: 0.2);
        target.Circle(mx, my, radius * 0.16).Stroke(color: primary, width: 2, alpha: 0.6);
        target.Arc(mx, my, radius * 0.24, Math.PI * 0.2, Math.PI * 0.8).Stroke(color: secondary, width: 1, alpha: 0.35);
        for (var row = 0; row < 4; row += 1) {
            var y = radius * (0.05 + row * 0.14);
            var arcs = 6 - row;
            for (var i = 0; i < arcs; i += 1) {
                var x = -radius * 0.62 + i * radius * 0.24 + (row % 2) * radius * 0.12;
                target.Arc(x, y, radius * 0.09, Math.PI, TAU)
                    .Stroke(color: row % 2 == 0 ? primary : secondary, width: row == 0 ? 2 : 1, alpha: 0.45 - row * 0.07);
            }
        }
    }
}
