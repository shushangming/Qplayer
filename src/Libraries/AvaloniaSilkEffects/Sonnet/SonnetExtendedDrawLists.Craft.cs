namespace AvaloniaSilkEffects.Sonnet;

internal static partial class SonnetExtendedDrawLists
{
    // 76: faceted origami crane in line art with two lightly filled folds.
    private static void DrawOrigamiCrane(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var direction = seed % 2 == 0 ? 1 : -1;
        var s = radius * 0.34;
        // Body diamond.
        target.MoveTo(0, -s * 0.3).LineTo(direction * s * 0.5, 0).LineTo(0, s * 0.35).LineTo(-direction * s * 0.5, 0)
            .LineTo(0, -s * 0.3)
            .Stroke(color: primary, width: 2, alpha: 0.6);
        // Raised wing.
        target.MoveTo(0, -s * 0.3).LineTo(-direction * s * 0.15, -s * 0.95).LineTo(direction * s * 0.28, -s * 0.1)
            .Stroke(color: primary, width: 1.5, alpha: 0.5);
        target.MoveTo(0, -s * 0.3).LineTo(-direction * s * 0.15, -s * 0.95).LineTo(-direction * s * 0.42, s * 0.02)
            .Fill(color: primary, alpha: 0.07);
        // Neck + head.
        target.MoveTo(direction * s * 0.5, 0)
            .LineTo(direction * s * 0.78, -s * 0.62)
            .LineTo(direction * s * 0.98, -s * 0.5)
            .Stroke(color: secondary, width: 1.5, alpha: 0.6);
        // Tail.
        target.MoveTo(-direction * s * 0.5, 0).LineTo(-direction * s * 0.85, -s * 0.5)
            .Stroke(color: primary, width: 1.5, alpha: 0.5);
        // Crease lines.
        target.MoveTo(0, -s * 0.3).LineTo(0, s * 0.35).Stroke(color: secondary, width: 1, alpha: 0.3);
        target.MoveTo(-direction * s * 0.5, 0).LineTo(direction * s * 0.5, 0)
            .Stroke(color: secondary, width: 1, alpha: 0.3);
    }

    // 77: paper plane with a segmented looping trail behind it.
    private static void DrawPaperPlaneTrail(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var direction = seed % 2 == 0 ? 1 : -1;
        var px = radius * 0.4 * direction;
        var py = -radius * 0.28;
        var s = radius * 0.16;
        // Plane: two folded triangles.
        target.MoveTo(px + direction * s, py).LineTo(px - direction * s * 0.8, py - s * 0.55).LineTo(px - direction * s * 0.35, py)
            .LineTo(px + direction * s, py)
            .Stroke(color: primary, width: 2, alpha: 0.65);
        target.MoveTo(px + direction * s, py).LineTo(px - direction * s * 0.35, py).LineTo(px - direction * s * 0.8, py + s * 0.4)
            .Stroke(color: secondary, width: 1.5, alpha: 0.5);
        // Trail: three arc segments with deliberate gaps.
        for (var seg = 0; seg < 3; seg += 1) {
            var start = Math.PI * (0.1 + seg * 0.55);
            target.Arc(px - direction * radius * 0.35, py + radius * 0.3, radius * (0.34 + seg * 0.06), start, start + Math.PI * 0.4)
                .Stroke(color: seg == 1 ? secondary : primary, width: 1.5, alpha: 0.45 - seg * 0.08);
        }
    }

    // 78: woven band — vertical strips pass over/under two horizontals via gaps.
    private static void DrawWeaveBand(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var bandY = new[] { -radius * 0.12, radius * 0.12 };
        const int strips = 7;
        // Horizontal strips first (behind), each broken at every other crossing.
        for (var row = 0; row < bandY.Length; row++) {
            var y = bandY[row];
            for (var i = 0; i < strips; i += 1) {
                var x0 = -radius * 0.63 + i * radius * 0.18;
                if ((i + row) % 2 == 0) {
                    target.MoveTo(x0 + radius * 0.02, y).LineTo(x0 + radius * 0.16, y)
                        .Stroke(color: row == 0 ? primary : secondary, width: 5, alpha: 0.4);
                } else {
                    target.MoveTo(x0 - radius * 0.05, y).LineTo(x0 + radius * 0.02, y)
                        .Stroke(color: row == 0 ? primary : secondary, width: 5, alpha: 0.4);
                    target.MoveTo(x0 + radius * 0.16, y).LineTo(x0 + radius * 0.23, y)
                        .Stroke(color: row == 0 ? primary : secondary, width: 5, alpha: 0.4);
                }
            }
        }
        // Vertical strips on top at the gapped crossings.
        for (var i = 0; i < strips; i += 1) {
            var x = -radius * 0.54 + i * radius * 0.18;
            var overRow = i % 2;
            var y = bandY[overRow];
            target.MoveTo(x, y - radius * 0.05).LineTo(x, y + radius * 0.05)
                .Stroke(color: primary, width: 6, alpha: 0.6);
            target.MoveTo(x, bandY[1 - overRow] - radius * 0.03).LineTo(x, bandY[1 - overRow] + radius * 0.03)
                .Stroke(color: secondary, width: 2, alpha: 0.3);
        }
    }

    // 79: figure-eight knot drawn in segments with crossing gaps for over/under.
    private static void DrawKnotLoop(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var s = radius * 0.34;
        // Left loop, broken where the right loop passes over.
        target.MoveTo(0, 0);
        target.BezierCurveTo(-s * 0.9, -s * 0.9, -s * 1.5, -s * 0.2, -s * 0.8, s * 0.28);
        target.Stroke(color: primary, width: 3, alpha: 0.6);
        target.MoveTo(-s * 0.62, s * 0.34);
        target.BezierCurveTo(-s * 0.3, s * 0.44, -s * 0.12, s * 0.2, 0, 0);
        target.Stroke(color: primary, width: 3, alpha: 0.6);
        // Right loop, broken where the left loop passes over.
        target.MoveTo(s * 0.12, -s * 0.08);
        target.BezierCurveTo(s * 0.6, -s * 0.6, s * 1.4, -s * 0.3, s * 0.9, s * 0.2);
        target.Stroke(color: secondary, width: 3, alpha: 0.6);
        target.MoveTo(s * 0.72, s * 0.26);
        target.BezierCurveTo(s * 0.4, s * 0.4, s * 0.05, s * 0.14, -s * 0.06, s * 0.04);
        target.Stroke(color: secondary, width: 3, alpha: 0.6);
        // Loose ends drifting out.
        target.MoveTo(0, 0).BezierCurveTo(-s * 0.2, s * 0.5, -s * 0.4, s * 0.8, -s * 0.3, s * 1.1)
            .Stroke(color: primary, width: 2, alpha: 0.4);
        target.MoveTo(s * 0.06, -s * 0.02).BezierCurveTo(s * 0.3, -s * 0.5, s * 0.5, -s * 0.8, s * 0.42, -s * 1.05)
            .Stroke(color: secondary, width: 2, alpha: 0.4);
    }

    // 80: cross-stitch sampler rows that fade toward the edges.
    private static void DrawStitchSampler(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var row = 0; row < 4; row += 1) {
            var y = -radius * 0.36 + row * radius * 0.24;
            var count = 7 - Math.Abs(row - 1.5);
            for (var i = 0; i < count; i += 1) {
                var x = (i - (count - 1) / 2d) * radius * 0.16 + (row % 2) * radius * 0.08;
                var edgeFade = 1 - Math.Abs(i - (count - 1) / 2d) / (count / 2d + 0.5);
                Stitch(x, y, 4 + (row % 2), (i + row) % 3 == 0 ? secondary : primary, 0.25 + edgeFade * 0.4);
            }
        }

        return;

        void Stitch(double x, double y, double size, uint color, double alpha) {
            target.MoveTo(x - size, y - size).LineTo(x + size, y + size).Stroke(color: color, width: 1.5, alpha: alpha);
            target.MoveTo(x + size, y - size).LineTo(x - size, y + size).Stroke(color: color, width: 1.5, alpha: alpha);
        }
    }

    // 81: folded fan — ribs from a pivot, double guard arc, open at the top.
    private static void DrawFoldedFan(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var pivotY = radius * 0.42;
        const int ribs = 11;
        const double spread = Math.PI * 0.9;
        for (var i = 0; i < ribs; i += 1) {
            var angle = -Math.PI / 2d - spread / 2d + (i / (double)(ribs - 1)) * spread;
            var len = radius * (0.5 + Math.Sin((i / (double)(ribs - 1)) * Math.PI) * 0.12);
            target.MoveTo(0, pivotY)
                .LineTo(Math.Cos(angle) * len, pivotY + Math.Sin(angle) * len)
                .Stroke(color: i % 2 == 0 ? primary : secondary, width: i == 5 ? 2.5 : 1.5, alpha: 0.5);
        }
        target.Arc(0, pivotY, radius * 0.5, -Math.PI / 2d - spread / 2d, -Math.PI / 2d + spread / 2d)
            .Stroke(color: primary, width: 2, alpha: 0.45);
        target.Arc(0, pivotY, radius * 0.58, -Math.PI / 2d - spread / 2d + 0.06, -Math.PI / 2d + spread / 2d - 0.06)
            .Stroke(color: secondary, width: 1, alpha: 0.3);
        target.Circle(0, pivotY, radius * 0.03).Fill(color: secondary, alpha: 0.8);
    }

    // 82: curling gift ribbon — sampled spiral with a parallel echo stroke.
    private static void DrawRibbonCurl(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var cx = radius * 0.15 * (seed % 2 == 0 ? 1 : -1);
        var start = Hash(seed, 0, 449) * TAU;
        foreach (var echo in new[] { 0, 1 }) {
            var offset = echo * radius * 0.035;
            target.MoveTo(cx + Math.Cos(start) * radius * 0.06, -radius * 0.1 + Math.Sin(start) * radius * 0.06 + offset);
            const int steps = 64;
            for (var i = 1; i <= steps; i += 1) {
                var t = i / (double)steps;
                var angle = start + t * TAU * 2.4;
                var r = radius * (0.06 + t * 0.42);
                target.LineTo(cx + Math.Cos(angle) * r, -radius * 0.1 + Math.Sin(angle) * r * 0.8 + offset);
            }
            target.Stroke(color: echo == 0 ? primary : secondary, width: echo == 0 ? 3 : 1, alpha: echo == 0 ? 0.55 : 0.3);
        }
        // Loose end flicks upward.
        var endAngle = start + TAU * 2.4;
        var ex = cx + Math.Cos(endAngle) * radius * 0.48;
        var ey = -radius * 0.1 + Math.Sin(endAngle) * radius * 0.38;
        target.MoveTo(ex, ey).QuadraticCurveTo(ex + radius * 0.1, ey - radius * 0.12, ex + radius * 0.16, ey - radius * 0.04)
            .Stroke(color: primary, width: 2, alpha: 0.5);
    }

    // 83: three overlapping patchwork triangles with hand-built inner stripes.
    private static void DrawPatchworkTrio(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var configs = new[] {
            (x: -radius * 0.22, y: -radius * 0.05, s: radius * 0.3, up: true),
            (x: radius * 0.18, y: -radius * 0.12, s: radius * 0.24, up: false),
            (x: radius * 0.05, y: radius * 0.2, s: radius * 0.2, up: true)
        };
        for (var index = 0; index < configs.Length; index++) {
            var (x, y, s, up) = configs[index];
            var topY = up ? y - s * 0.6 : y;
            var baseY = up ? y + s * 0.4 : y + s;
            target.MoveTo(x, topY).LineTo(x + s * 0.55, baseY).LineTo(x - s * 0.55, baseY).LineTo(x, topY)
                .Stroke(color: index == 1 ? secondary : primary, width: 2, alpha: 0.55);
            // Inner stripes are computed inside the silhouette — no mask needed.
            for (var stripe = 1; stripe <= 3; stripe += 1) {
                var t = stripe / 4d;
                var sy = topY + (baseY - topY) * t;
                var half = s * 0.55 * (up ? t : 1 - t);
                target.MoveTo(x - half, sy).LineTo(x + half, sy)
                    .Stroke(color: index == 1 ? primary : secondary, width: 1, alpha: 0.35);
            }
        }
    }

    // 84: dreamcatcher — ring, radial web to an off-center hub, hanging feathers.
    private static void DrawDreamcatcher(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var ringR = radius * 0.32;
        var cy = -radius * 0.12;
        target.Circle(0, cy, ringR).Stroke(color: primary, width: 2, alpha: 0.6);
        var hubX = radius * 0.05;
        var hubY = cy - radius * 0.03;
        for (var i = 0; i < 8; i += 1) {
            var angle = (i / 8d) * TAU + 0.2;
            var rimX = Math.Cos(angle) * ringR * 0.92;
            var rimY = cy + Math.Sin(angle) * ringR * 0.92;
            target.MoveTo(rimX, rimY).LineTo(hubX, hubY)
                .Stroke(color: i % 2 == 0 ? primary : secondary, width: 1, alpha: 0.4);
        }
        target.Circle(hubX, hubY, radius * 0.035).Stroke(color: secondary, width: 1.5, alpha: 0.6);
        // Three hanging strings with feather barbs; bottom stays open.
        for (var i = -1; i <= 1; i += 1) {
            var sx = i * ringR * 0.55;
            var topY = cy + Math.Sqrt(Math.Max(0, ringR * ringR - sx * sx));
            var len = radius * (0.22 + (1 - Math.Abs(i)) * 0.12 + Hash(seed, i + 1, 457) * 0.06);
            target.MoveTo(sx, topY).LineTo(sx, topY + len).Stroke(color: primary, width: 1, alpha: 0.45);
            var featherY = topY + len;
            target.MoveTo(sx, featherY).LineTo(sx, featherY + radius * 0.12)
                .Stroke(color: secondary, width: 1.5, alpha: 0.55);
            for (var barb = 1; barb <= 3; barb += 1) {
                var by = featherY + barb * radius * 0.03;
                var bl = radius * 0.04 * (1 - barb * 0.18);
                target.MoveTo(sx, by).LineTo(sx - bl, by + radius * 0.02)
                    .Stroke(color: secondary, width: 1, alpha: 0.4);
                target.MoveTo(sx, by).LineTo(sx + bl, by + radius * 0.02)
                    .Stroke(color: secondary, width: 1, alpha: 0.4);
            }
        }
    }

    // 85: tassel curtain — staggered hanging threads with bead tips from a short bar.
    private static void DrawTasselDrop(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var barY = -radius * 0.4;
        target.MoveTo(-radius * 0.3, barY).LineTo(radius * 0.3, barY)
            .Stroke(color: primary, width: 2, alpha: 0.4);
        const int threads = 7;
        for (var i = 0; i < threads; i += 1) {
            var x = -radius * 0.27 + i * radius * 0.09;
            var len = radius * (0.3 + Hash(seed, i, 461) * 0.35);
            var sway = (Hash(seed, i, 463) - 0.5) * radius * 0.08;
            target.MoveTo(x, barY)
                .QuadraticCurveTo(x + sway, barY + len * 0.6, x + sway * 0.6, barY + len)
                .Stroke(color: i % 2 == 0 ? primary : secondary, width: 1.5, alpha: 0.45);
            target.Circle(x + sway * 0.6, barY + len + 3, 2.5)
                .Fill(color: i % 3 == 0 ? secondary : primary, alpha: 0.6);
        }
    }
}
