namespace AvaloniaSilkEffects.Sonnet;

internal static partial class SonnetExtendedDrawLists
{
    // 58: rows of repeating wave-crest arcs across the full bleed width.
    private static void DrawWaveScrolls(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var bleed = Bleed(width, height, radius);
        for (var row = 0; row < 4; row += 1) {
            var y = -radius * 0.3 + row * radius * 0.22;
            var crestR = radius * 0.1;
            var step = crestR * 2.1;
            var count = Math.Ceiling((bleed.x * 2) / step);
            for (var i = 0; i < count; i += 1) {
                var x = -bleed.x + i * step + (row % 2) * crestR;
                target.Arc(x, y, crestR, Math.PI, TAU)
                    .Stroke(color: (i + row) % 3 == 0 ? secondary : primary, width: row == 1 ? 2 : 1, alpha: 0.42 - row * 0.06);
            }
        }
        for (var i = 0; i < 6; i += 1) {
            target.Circle(
                (Hash(seed, i, 263) - 0.5) * bleed.x * 1.4,
                -radius * 0.55 + Hash(seed, i, 269) * radius * 0.25,
                1.6).Fill(color: secondary, alpha: 0.4);
        }
    }

    // 59: nautilus shell — sampled log spiral with chamber dividers, open outer tip.
    private static void DrawNautilus(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        const double turns = 2.6;
        const int steps = 90;
        var startAngle = Hash(seed, 0, 271) * TAU;
        target.MoveTo(Math.Cos(startAngle) * radius * 0.04, Math.Sin(startAngle) * radius * 0.04);
        for (var i = 1; i <= steps; i += 1) {
            var t = i / (double)steps;
            var angle = startAngle + t * turns * TAU;
            var r = radius * (0.04 + t * 0.5);
            target.LineTo(Math.Cos(angle) * r, Math.Sin(angle) * r * 0.94);
        }
        target.Stroke(color: primary, width: 2.5, alpha: 0.65);
        // Chamber dividers radiate from the spiral core at growing radii.
        for (var i = 1; i <= 8; i += 1) {
            var t = i / 9d;
            var angle = startAngle + t * turns * TAU;
            var r = radius * (0.04 + t * 0.5);
            target.MoveTo(Math.Cos(angle) * r * 0.55, Math.Sin(angle) * r * 0.52)
                .LineTo(Math.Cos(angle) * r, Math.Sin(angle) * r * 0.94)
                .Stroke(color: secondary, width: 1, alpha: 0.35);
        }
        target.Circle(0, 0, radius * 0.05).Stroke(color: primary, width: 1.5, alpha: 0.5);
    }

    // 60: coral branch grown from the bottom with forked limbs and tip buds.
    private static void DrawCoralBranch(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var baseX = (seed % 2 == 0 ? -1 : 1) * radius * 0.12;

        Fork(baseX, radius * 0.62, -Math.PI / 2d, radius * 0.34, 3, 0, 0);
        // A few detached polyps drifting nearby.
        for (var i = 0; i < 5; i += 1) {
            target.Circle(
                baseX + (Hash(seed, i, 281) - 0.5) * radius * 0.9,
                radius * (0.3 + Hash(seed, i, 283) * 0.3),
                1.8).Fill(color: primary, alpha: 0.35);
        }

        return;

        void Fork(double x, double y, double angle, double len, double width_, int depth, int limb)
        {
            while (true)
            {
                var ex = x + Math.Cos(angle) * len;
                var ey = y + Math.Sin(angle) * len;
                target.MoveTo(x, y)
                    .QuadraticCurveTo(x + Math.Cos(angle + 0.3) * len * 0.5, y + Math.Sin(angle + 0.3) * len * 0.5, ex, ey)
                    .Stroke(color: depth == 0 ? secondary : primary, width: width_, alpha: 0.6 - depth * 0.12);
                target.Circle(ex, ey, width_ * 0.9).Fill(color: secondary, alpha: 0.5);
                if (depth < 2)
                {
                    var spread = 0.55 + Hash(seed, limb, 277) * 0.3;
                    Fork(ex, ey, angle - spread, len * 0.62, Math.Max(1, width_ - 1), depth + 1, limb * 2 + 1);
                    x = ex;
                    y = ey;
                    angle = angle + spread * 0.8;
                    len = len * 0.68;
                    width_ = Math.Max(1, width_ - 1);
                    depth = depth + 1;
                    limb = limb * 2 + 2;
                    continue;
                }

                break;
            }
        }
    }

    // 61: lighthouse on a low rock, twin light beams fanning out, open sea arcs.
    private static void DrawLighthouseBeam(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var direction = seed % 2 == 0 ? 1 : -1;
        var bx = -radius * 0.3 * direction;
        var baseY = radius * 0.42;
        // Tower: tapered trapezoid outline, no base bar.
        target.MoveTo(bx - radius * 0.09, baseY)
            .LineTo(bx - radius * 0.05, baseY - radius * 0.42)
            .LineTo(bx + radius * 0.05, baseY - radius * 0.42)
            .LineTo(bx + radius * 0.09, baseY)
            .Stroke(color: primary, width: 2, alpha: 0.6);
        target.Rectangle(bx - radius * 0.07, baseY - radius * 0.52, radius * 0.14, radius * 0.1)
            .Stroke(color: primary, width: 1.5, alpha: 0.55);
        target.Circle(bx, baseY - radius * 0.47, radius * 0.025).Fill(color: secondary, alpha: 0.9);
        // Twin beams fan to the open right side.
        for (var beam = 0; beam < 2; beam += 1) {
            var spread = radius * (0.1 + beam * 0.12);
            target.MoveTo(bx, baseY - radius * 0.47)
                .LineTo(bx + direction * radius * 0.85, baseY - radius * 0.47 - spread)
                .Stroke(color: secondary, width: 1.5, alpha: 0.4 - beam * 0.1);
            target.MoveTo(bx, baseY - radius * 0.47)
                .LineTo(bx + direction * radius * 0.85, baseY - radius * 0.47 + spread)
                .Stroke(color: secondary, width: 1.5, alpha: 0.4 - beam * 0.1);
        }
        // Sea arcs below, unconnected.
        for (var i = 0; i < 5; i += 1) {
            var x = -radius * 0.6 + i * radius * 0.3;
            target.Arc(x, radius * 0.56, radius * 0.1, Math.PI, TAU)
                .Stroke(color: primary, width: 1, alpha: 0.35);
        }
    }

    // 62: compass rose with long cardinal needles and a partial degree arc.
    private static void DrawCompassRose(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var roseR = radius * 0.42;
        for (var i = 0; i < 8; i += 1) {
            var angle = (i / 8d) * TAU - Math.PI / 2d;
            var isLong = i % 2 == 0;
            var len = isLong ? roseR : roseR * 0.55;
            var halfWidth = isLong ? 0.09 : 0.06;
            // Needle = thin triangle from center.
            var tx = Math.Cos(angle) * len;
            var ty = Math.Sin(angle) * len;
            var lx = Math.Cos(angle + halfWidth) * roseR * 0.16;
            var ly = Math.Sin(angle + halfWidth) * roseR * 0.16;
            var rx = Math.Cos(angle - halfWidth) * roseR * 0.16;
            var ry = Math.Sin(angle - halfWidth) * roseR * 0.16;
            target.MoveTo(lx, ly).LineTo(tx, ty).LineTo(rx, ry)
                .Stroke(color: isLong ? primary : secondary, width: isLong ? 2 : 1, alpha: isLong ? 0.65 : 0.45);
            if (isLong && i % 4 == 0) {
                target.MoveTo(lx, ly).LineTo(tx, ty).LineTo(rx, ry)
                    .Fill(color: primary, alpha: 0.1);
            }
        }
        target.Circle(0, 0, roseR * 0.14).Stroke(color: primary, width: 1.5, alpha: 0.6);
        target.Circle(0, 0, roseR * 0.05).Fill(color: secondary, alpha: 0.8);
        // Degree ticks only along one open arc — deliberately not a closed dial.
        var arcStart = Hash(seed, 0, 293) * TAU;
        for (var i = 0; i <= 24; i += 1) {
            var angle = arcStart + (i / 24d) * Math.PI * 1.2;
            var inner = roseR * 1.12;
            var outer = inner + (i % 6 == 0 ? 10 : 5);
            target.MoveTo(Math.Cos(angle) * inner, Math.Sin(angle) * inner)
                .LineTo(Math.Cos(angle) * outer, Math.Sin(angle) * outer)
                .Stroke(color: primary, width: 1, alpha: 0.4);
        }
    }

    // 63: three abstract sailboats with curved sails and open water dashes.
    private static void DrawSailRegatta(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var boat = 0; boat < 3; boat += 1) {
            var scale = 1 - boat * 0.24;
            var bx = (boat - 1) * radius * 0.44 + (Hash(seed, boat, 307) - 0.5) * radius * 0.08;
            var by = radius * (0.28 - boat * 0.12);
            var mastH = radius * 0.34 * scale;
            target.MoveTo(bx, by).LineTo(bx, by - mastH)
                .Stroke(color: primary, width: 2, alpha: 0.6);
            // Curved sail via quadratic leech.
            target.MoveTo(bx, by - mastH)
                .QuadraticCurveTo(bx + radius * 0.2 * scale, by - mastH * 0.55, bx, by - mastH * 0.08)
                .Stroke(color: boat == 1 ? secondary : primary, width: 1.5, alpha: 0.55);
            target.MoveTo(bx, by - mastH * 0.92)
                .LineTo(bx - radius * 0.13 * scale, by - mastH * 0.1)
                .LineTo(bx, by - mastH * 0.1)
                .Stroke(color: primary, width: 1, alpha: 0.4);
            // Hull: shallow arc, open at both ends.
            target.MoveTo(bx - radius * 0.15 * scale, by)
                .QuadraticCurveTo(bx, by + radius * 0.07 * scale, bx + radius * 0.15 * scale, by)
                .Stroke(color: primary, width: 2, alpha: 0.55);
        }
        for (var i = 0; i < 7; i += 1) {
            var x = -radius * 0.66 + i * radius * 0.22;
            var y = radius * (0.42 + (i % 2) * 0.05);
            target.MoveTo(x, y).LineTo(x + radius * 0.1, y)
                .Stroke(color: secondary, width: 1, alpha: 0.35);
        }
    }

    // 64: three rising bubble columns with highlight arcs on the large ones.
    private static void DrawBubbleRise(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var column = 0; column < 3; column += 1) {
            var x = (column - 1) * radius * 0.34 + (Hash(seed, column, 311) - 0.5) * radius * 0.12;
            var count = 5 + column;
            for (var i = 0; i < count; i += 1) {
                var t = i / (double)count;
                var y = radius * 0.55 - t * radius * 1.05;
                var wobble = (Hash(seed, column * 10 + i, 313) - 0.5) * radius * 0.08;
                var r = radius * (0.02 + t * 0.055);
                target.Circle(x + wobble, y, r)
                    .Stroke(color: i % 3 == 0 ? secondary : primary, width: 1, alpha: 0.35 + t * 0.35);
                if (r > radius * 0.05) {
                    target.Arc(x + wobble, y, r * 0.55, Math.PI * 1.1, Math.PI * 1.6)
                        .Stroke(color: secondary, width: 1, alpha: 0.5);
                }
            }
        }
    }

    // 65: overlapping organic tide-pool rings with pebble dots inside.
    private static void DrawTidePools(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var pool = 0; pool < 4; pool += 1) {
            var px = (Hash(seed, pool, 317) - 0.5) * radius * 0.7;
            var py = (Hash(seed, pool, 331) - 0.2) * radius * 0.5;
            var pr = radius * (0.16 + Hash(seed, pool, 337) * 0.12);
            target.MoveTo(px + pr, py);
            target.BezierCurveTo(px + pr, py - pr * 0.7, px + pr * 0.4, py - pr, px, py - pr * 0.9);
            target.BezierCurveTo(px - pr * 0.7, py - pr * 0.8, px - pr, py - pr * 0.2, px - pr * 0.9, py + pr * 0.3);
            target.BezierCurveTo(px - pr * 0.6, py + pr * 0.8, px + pr * 0.2, py + pr, px + pr * 0.6, py + pr * 0.7);
            target.BezierCurveTo(px + pr * 0.95, py + pr * 0.5, px + pr, py + pr * 0.3, px + pr, py);
            target.Stroke(color: pool % 2 == 0 ? primary : secondary, width: 1.5, alpha: 0.45);
            for (var pebble = 0; pebble < 3; pebble += 1) {
                target.Circle(
                    px + (Hash(seed, pool * 4 + pebble, 347) - 0.5) * pr,
                    py + (Hash(seed, pool * 4 + pebble, 349) - 0.5) * pr * 0.7,
                    1.6 + pebble).Fill(color: secondary, alpha: 0.45);
            }
        }
    }

    // 66: tall seaweed blades swaying from the bottom with drifting air bubbles.
    private static void DrawSeaweedSway(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var blade = 0; blade < 6; blade += 1) {
            var x = -radius * 0.55 + blade * radius * 0.22 + (Hash(seed, blade, 353) - 0.5) * radius * 0.06;
            var h = radius * (0.4 + Hash(seed, blade, 359) * 0.3);
            var sway = (blade % 2 == 0 ? 1 : -1) * radius * 0.12;
            target.MoveTo(x, radius * 0.62)
                .BezierCurveTo(x + sway, radius * 0.62 - h * 0.4, x - sway, radius * 0.62 - h * 0.7, x + sway * 0.5, radius * 0.62 - h)
                .Stroke(color: blade % 2 == 0 ? primary : secondary, width: 2, alpha: 0.5 - blade * 0.03);
            target.Circle(x + sway * 0.5, radius * 0.62 - h, 2).Fill(color: secondary, alpha: 0.5);
        }
        for (var i = 0; i < 6; i += 1) {
            target.Circle(
                (Hash(seed, i, 367) - 0.5) * radius * 1.1,
                -radius * 0.5 + Hash(seed, i, 373) * radius * 0.5,
                1.4 + Hash(seed, i, 379) * 1.6).Stroke(color: primary, width: 1, alpha: 0.35);
        }
    }

    // 67: layered horizontal current lines with scattered fish chevrons.
    private static void DrawDeepCurrent(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var bleed = Bleed(width, height, radius);
        for (var line = 0; line < 5; line += 1) {
            var y = -radius * 0.4 + line * radius * 0.2;
            var lift = (line % 2 == 0 ? 1 : -1) * radius * 0.06;
            target.MoveTo(-bleed.x, y)
                .BezierCurveTo(-radius * 0.3, y + lift, radius * 0.3, y - lift, bleed.x, y)
                .Stroke(color: line == 2 ? secondary : primary, width: line == 2 ? 2 : 1, alpha: 0.32 + (line % 3) * 0.07);
        }
        // Fish chevrons swim against the current, offset per seed.
        for (var i = 0; i < 6; i += 1) {
            var x = (Hash(seed, i, 383) - 0.5) * radius * 1.2;
            var y = -radius * 0.3 + Hash(seed, i, 389) * radius * 0.6;
            var s = 5 + Hash(seed, i, 397) * 4;
            var flip = i % 2 == 0 ? 1 : -1;
            target.MoveTo(x - s * flip, y - s * 0.5).LineTo(x, y).LineTo(x - s * flip, y + s * 0.5)
                .Stroke(color: secondary, width: 1.5, alpha: 0.55);
        }
    }
}
