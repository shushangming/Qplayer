namespace AvaloniaSilkEffects.Sonnet;

internal static partial class SonnetExtendedDrawLists
{
    // 86: pendulum wave — strings and bobs at staggered phases along an arc.
    private static void DrawPendulumWave(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var pivotY = -radius * 0.42;
        const int bobs = 9;
        for (var i = 0; i < bobs; i += 1) {
            var x = -radius * 0.5 + (i / (double)(bobs - 1)) * radius;
            var len = radius * (0.4 + i * 0.035);
            var swing = Math.Sin(i * 0.9 + seed * 0.07) * 0.35;
            var bx = x + Math.Sin(swing) * len;
            var by = pivotY + Math.Cos(swing) * len;
            target.MoveTo(x, pivotY).LineTo(bx, by)
                .Stroke(color: primary, width: 1, alpha: 0.4);
            target.Circle(bx, by, 3.5 + (i % 3))
                .Fill(color: i % 2 == 0 ? secondary : primary, alpha: 0.7);
        }
        target.MoveTo(-radius * 0.58, pivotY).LineTo(radius * 0.58, pivotY)
            .Stroke(color: primary, width: 2, alpha: 0.35);
    }

    // 87: dominos toppling along an arc, each rotated a step further.
    private static void DrawDominoArc(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var count = 10;
        var arcR = radius * 0.55;
        for (var i = 0; i < count; i += 1) {
            var angle = Math.PI * 1.15 + (i / (double)(count - 1)) * Math.PI * 0.7;
            var bx = Math.Cos(angle) * arcR;
            var by = Math.Sin(angle) * arcR + radius * 0.5;
            var tilt = (i / (double)(count - 1)) * 1.1 * (seed % 2 == 0 ? 1 : -1);
            var w = radius * 0.035;
            var h = radius * 0.14;
            var cos = Math.Cos(tilt);
            var sin = Math.Sin(tilt);
            (double, double) Corner(double cx, double cy) => (bx + cx * cos - cy * sin, by + cx * sin + cy * cos);
            var (x1, y1) = Corner(-w, 0);
            var (x2, y2) = Corner(w, 0);
            var (x3, y3) = Corner(w, -h * 2);
            var (x4, y4) = Corner(-w, -h * 2);
            target.MoveTo(x1, y1).LineTo(x2, y2).LineTo(x3, y3).LineTo(x4, y4).LineTo(x1, y1)
                .Stroke(color: i % 3 == 0 ? secondary : primary, width: 1.5, alpha: 0.55);
        }
    }

    // 88: three intermeshed gears built from rings and radial teeth.
    private static void DrawGearCluster(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var gears = new[] {
            (x: 0d, y: 0d, r: radius * 0.26, teeth: 10),
            (x: radius * 0.42, y: -radius * 0.2, r: radius * 0.16, teeth: 8),
            (x: -radius * 0.4, y: radius * 0.22, r: radius * 0.13, teeth: 7)
        };
        for (var gi = 0; gi < gears.Length; gi++) {
            var gear = gears[gi];
            var color = gi == 1 ? secondary : primary;
            target.Circle(gear.x, gear.y, gear.r).Stroke(color: color, width: 2, alpha: 0.55);
            target.Circle(gear.x, gear.y, gear.r * 0.3).Stroke(color: color, width: 1.5, alpha: 0.45);
            var offset = Hash(seed, gi, 467) * TAU;
            for (var t = 0; t < gear.teeth; t += 1) {
                var angle = offset + (t / (double)gear.teeth) * TAU;
                target.MoveTo(gear.x + Math.Cos(angle) * gear.r, gear.y + Math.Sin(angle) * gear.r)
                    .LineTo(gear.x + Math.Cos(angle) * gear.r * 1.18, gear.y + Math.Sin(angle) * gear.r * 1.18)
                    .Stroke(color: color, width: 3, alpha: 0.5);
            }
        }
    }

    // 89: circuit traces with 45-degree bends and node pads, no board outline.
    private static void DrawCircuitDelta(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var bleed = Bleed(width, height, radius);
        const int lanes = 4;
        for (var lane = 0; lane < lanes; lane += 1) {
            var y = -radius * 0.36 + lane * radius * 0.24;
            var bendX = -radius * 0.3 + Hash(seed, lane, 479) * radius * 0.6;
            var drop = (lane % 2 == 0 ? 1 : -1) * radius * 0.08;
            target.MoveTo(-bleed.x, y)
                .LineTo(bendX - Math.Abs(drop), y)
                .LineTo(bendX, y + drop)
                .LineTo(bleed.x, y + drop)
                .Stroke(color: lane == 1 ? secondary : primary, width: 1.5, alpha: 0.45);
            target.Circle(bendX, y + drop, 3).Fill(color: secondary, alpha: 0.7);
            target.Circle(-bleed.x * 0.55, y, 2.5).Stroke(color: primary, width: 1, alpha: 0.5);
        }
    }

    // 90: signal tower mast with radiating wave arcs on both sides.
    private static void DrawSignalTower(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var baseY = radius * 0.45;
        var topY = -radius * 0.3;
        target.MoveTo(-radius * 0.12, baseY).LineTo(0, topY).LineTo(radius * 0.12, baseY)
            .Stroke(color: primary, width: 2, alpha: 0.6);
        for (var brace = 1; brace <= 3; brace += 1) {
            var y = baseY - (baseY - topY) * (brace / 4d);
            var half = radius * 0.12 * (1 - brace / 4.5);
            target.MoveTo(-half, y).LineTo(half, y - radius * 0.06)
                .Stroke(color: primary, width: 1, alpha: 0.4);
        }
        target.Circle(0, topY, radius * 0.03).Fill(color: secondary, alpha: 0.9);
        for (var side = -1; side <= 1; side += 2) {
            for (var ring = 0; ring < 3; ring += 1) {
                var r = radius * (0.12 + ring * 0.13);
                target.Arc(0, topY, r, side < 0 ? Math.PI * 0.75 : -Math.PI * 0.25, side < 0 ? Math.PI * 1.25 : Math.PI * 0.25)
                    .Stroke(color: ring == 1 ? secondary : primary, width: 1.5, alpha: 0.45 - ring * 0.1);
            }
        }
    }

    // 91: spiral staircase ascending as staggered tread/riser polylines.
    private static void DrawSpiralStair(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        const int steps = 12;
        var startAngle = Hash(seed, 0, 487) * TAU;
        for (var i = 0; i < steps; i += 1) {
            var angle = startAngle + i * 0.42;
            var r = radius * (0.14 + i * 0.04);
            var x = Math.Cos(angle) * r;
            var y = radius * 0.4 - i * radius * 0.055;
            var tread = radius * 0.09;
            target.MoveTo(x, y)
                .LineTo(x + Math.Cos(angle) * tread, y + Math.Sin(angle) * tread * 0.4)
                .Stroke(color: i % 3 == 0 ? secondary : primary, width: 2, alpha: 0.55);
            target.MoveTo(x + Math.Cos(angle) * tread, y + Math.Sin(angle) * tread * 0.4)
                .LineTo(x + Math.Cos(angle) * tread, y + Math.Sin(angle) * tread * 0.4 - radius * 0.055)
                .Stroke(color: primary, width: 1, alpha: 0.35);
        }
        // Central spine, open at the top.
        target.MoveTo(0, radius * 0.45).LineTo(0, -radius * 0.35)
            .Stroke(color: primary, width: 2, alpha: 0.3);
    }

    // 92: falling vertical streams of staggered length with splash arcs below.
    private static void DrawWaterfallLines(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var streams = 9;
        for (var i = 0; i < streams; i += 1) {
            var x = -radius * 0.5 + (i / (double)(streams - 1)) * radius + (Hash(seed, i, 491) - 0.5) * radius * 0.05;
            var topY = -radius * 0.6 + Hash(seed, i, 499) * radius * 0.15;
            var len = radius * (0.55 + Hash(seed, i, 503) * 0.35);
            target.MoveTo(x, topY).LineTo(x, topY + len)
                .Stroke(color: i % 3 == 0 ? secondary : primary, width: i % 3 == 0 ? 2 : 1, alpha: 0.4 + (i % 3) * 0.08);
        }
        for (var i = 0; i < 5; i += 1) {
            var x = -radius * 0.4 + i * radius * 0.2;
            target.Arc(x, radius * 0.5, radius * 0.07, Math.PI, TAU)
                .Stroke(color: secondary, width: 1, alpha: 0.4);
        }
    }

    // 93: four-blade pinwheel with curved sails around a hub.
    private static void DrawPinwheel(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var hubR = radius * 0.05;
        for (var blade = 0; blade < 4; blade += 1) {
            var angle = (blade / 4d) * TAU + Hash(seed, 0, 509) * 0.5;
            var tipR = radius * 0.5;
            var tx = Math.Cos(angle) * tipR;
            var ty = Math.Sin(angle) * tipR;
            var edgeAngle = angle + 0.7;
            target.MoveTo(Math.Cos(angle) * hubR, Math.Sin(angle) * hubR)
                .QuadraticCurveTo(
                    Math.Cos(edgeAngle) * tipR * 0.55, Math.Sin(edgeAngle) * tipR * 0.55,
                    tx, ty)
                .Stroke(color: blade % 2 == 0 ? primary : secondary, width: 2, alpha: 0.55);
            target.MoveTo(tx, ty)
                .LineTo(Math.Cos(angle + 0.45) * tipR * 0.62, Math.Sin(angle + 0.45) * tipR * 0.62)
                .Stroke(color: blade % 2 == 0 ? primary : secondary, width: 1.5, alpha: 0.4);
        }
        target.Circle(0, 0, hubR).Fill(color: secondary, alpha: 0.8);
        target.Circle(0, 0, radius * 0.56).Stroke(color: primary, width: 1, alpha: 0.15);
    }

    // 94: falling drop above broken ripple arcs — nothing touches the edges.
    private static void DrawRippleDrop(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var dx = (Hash(seed, 0, 521) - 0.5) * radius * 0.2;
        // Teardrop.
        target.MoveTo(dx, -radius * 0.52);
        target.BezierCurveTo(dx + radius * 0.07, -radius * 0.36, dx + radius * 0.06, -radius * 0.3, dx, -radius * 0.27);
        target.BezierCurveTo(dx - radius * 0.06, -radius * 0.3, dx - radius * 0.07, -radius * 0.36, dx, -radius * 0.52);
        target.Stroke(color: secondary, width: 2, alpha: 0.65);
        // Broken ripples: arc segments with gaps at alternating positions.
        for (var ring = 0; ring < 4; ring += 1) {
            var r = radius * (0.14 + ring * 0.13);
            var y = radius * 0.25;
            var gapAt = Hash(seed, ring, 523) * TAU;
            target.Arc(dx, y, r, gapAt, gapAt + TAU * 0.72)
                .Stroke(color: ring % 2 == 0 ? primary : secondary, width: ring == 0 ? 2 : 1, alpha: 0.5 - ring * 0.09);
        }
        // Impact crown.
        target.MoveTo(dx - radius * 0.05, radius * 0.2).LineTo(dx - radius * 0.02, radius * 0.12)
            .Stroke(color: primary, width: 1.5, alpha: 0.5);
        target.MoveTo(dx + radius * 0.05, radius * 0.2).LineTo(dx + radius * 0.02, radius * 0.12)
            .Stroke(color: primary, width: 1.5, alpha: 0.5);
    }

    // 95: suspension bridge — sagging main cable, two towers, open deck line.
    private static void DrawSuspensionBridge(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var bleed = Bleed(width, height, radius);
        var deckY = radius * 0.3;
        var towerX = radius * 0.34;
        var towerTop = -radius * 0.28;
        target.MoveTo(-bleed.x, deckY).LineTo(bleed.x, deckY)
            .Stroke(color: primary, width: 2, alpha: 0.5);
        foreach (var side in new[] { -1, 1 }) {
            var x = side * towerX;
            target.MoveTo(x - radius * 0.03, deckY).LineTo(x - radius * 0.03, towerTop)
                .Stroke(color: primary, width: 2, alpha: 0.55);
            target.MoveTo(x + radius * 0.03, deckY).LineTo(x + radius * 0.03, towerTop)
                .Stroke(color: primary, width: 2, alpha: 0.55);
            target.MoveTo(x - radius * 0.04, towerTop + radius * 0.08).LineTo(x + radius * 0.04, towerTop + radius * 0.08)
                .Stroke(color: secondary, width: 1.5, alpha: 0.45);
        }
        // Main cable: three quadratic spans, ends bleed off-canvas.
        target.MoveTo(-bleed.x, deckY - radius * 0.1)
            .QuadraticCurveTo(-towerX, towerTop - radius * 0.06, -towerX, towerTop)
            .Stroke(color: secondary, width: 1.5, alpha: 0.5);
        target.MoveTo(-towerX, towerTop)
            .QuadraticCurveTo(0, deckY - radius * 0.04, towerX, towerTop)
            .Stroke(color: secondary, width: 1.5, alpha: 0.5);
        target.MoveTo(towerX, towerTop)
            .QuadraticCurveTo(bleed.x, deckY - radius * 0.1, bleed.x, deckY - radius * 0.06)
            .Stroke(color: secondary, width: 1.5, alpha: 0.5);
        // Hangers along the center span.
        for (var i = 1; i < 7; i += 1) {
            var t = i / 7d;
            var x = -towerX + t * towerX * 2;
            var cableY = (1 - t) * (1 - t) * towerTop + 2 * (1 - t) * t * (deckY - radius * 0.04) + t * t * towerTop;
            target.MoveTo(x, cableY).LineTo(x, deckY)
                .Stroke(color: primary, width: 1, alpha: 0.35);
        }
    }

    // 96: magnetic field loops through two poles, mirrored top and bottom.
    private static void DrawFieldLines(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var poleGap = radius * 0.2;
        for (var loop = 0; loop < 4; loop += 1) {
            var bulge = radius * (0.2 + loop * 0.16);
            foreach (var mirror in new[] { -1, 1 }) {
                target.MoveTo(0, -poleGap);
                target.BezierCurveTo(
                    mirror * bulge, -poleGap - radius * 0.1,
                    mirror * bulge, poleGap + radius * 0.1,
                    0, poleGap);
                target.Stroke(color: loop % 2 == 0 ? primary : secondary, width: loop == 0 ? 2 : 1, alpha: 0.5 - loop * 0.08);
            }
        }
        target.Circle(0, -poleGap, radius * 0.04).Fill(color: secondary, alpha: 0.85);
        target.Circle(0, poleGap, radius * 0.04).Fill(color: primary, alpha: 0.85);
        target.MoveTo(-radius * 0.1, -poleGap).LineTo(radius * 0.1, -poleGap)
            .Stroke(color: secondary, width: 1.5, alpha: 0.5);
        target.MoveTo(-radius * 0.1, poleGap).LineTo(radius * 0.1, poleGap)
            .Stroke(color: primary, width: 1.5, alpha: 0.5);
    }

    // 97: prism splitting one inbound beam into a fanned spectrum.
    private static void DrawPrismBeam(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var bleed = Bleed(width, height, radius);
        var s = radius * 0.24;
        var topY = -s * 0.7;
        var baseY = s * 0.55;
        target.MoveTo(0, topY).LineTo(s * 0.8, baseY).LineTo(-s * 0.8, baseY).LineTo(0, topY)
            .Stroke(color: primary, width: 2, alpha: 0.6);
        target.MoveTo(0, topY).LineTo(s * 0.8, baseY).LineTo(-s * 0.8, baseY).LineTo(0, topY)
            .Fill(color: primary, alpha: 0.06);
        // Inbound beam from the left edge.
        var entryX = -s * 0.35;
        var entryY = s * 0.05;
        target.MoveTo(-bleed.x, entryY + radius * 0.12).LineTo(entryX, entryY)
            .Stroke(color: secondary, width: 2.5, alpha: 0.6);
        // Outbound fan to the right edge.
        for (var ray = 0; ray < 4; ray += 1) {
            var exitY = -radius * 0.1 + ray * radius * 0.09;
            target.MoveTo(s * 0.4, entryY - radius * 0.05)
                .LineTo(bleed.x, exitY)
                .Stroke(color: ray == 1 ? secondary : primary, width: 1.5, alpha: 0.45 - ray * 0.05);
        }
    }

    // 98: echo arcs bouncing between two unseen walls, offset per hop.
    private static void DrawEchoArcs(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        for (var hop = 0; hop < 5; hop += 1) {
            var side = hop % 2 == 0 ? -1 : 1;
            var cx = side * radius * 0.52;
            var cy = -radius * 0.35 + hop * radius * 0.18;
            var r = radius * (0.14 + hop * 0.045);
            target.Arc(cx, cy, r, side < 0 ? -Math.PI / 2d : Math.PI / 2d, side < 0 ? Math.PI / 2d : Math.PI * 1.5)
                .Stroke(color: hop % 2 == 0 ? primary : secondary, width: 2 - hop * 0.2, alpha: 0.55 - hop * 0.07);
            target.Circle(cx + (side < 0 ? r : -r) * 0.4, cy + r * 0.6, 1.8)
                .Fill(color: secondary, alpha: 0.5);
        }
    }

    // 99: diamond kite with cross spars, tail bows and a long free string.
    private static void DrawKiteString(SonnetDrawList target, double radius, double width, double height, uint seed, uint primary, uint secondary)
    {
        var kx = radius * 0.22 * (seed % 2 == 0 ? 1 : -1);
        var ky = -radius * 0.3;
        var kw = radius * 0.16;
        var kh = radius * 0.22;
        target.MoveTo(kx, ky - kh).LineTo(kx + kw, ky).LineTo(kx, ky + kh).LineTo(kx - kw, ky).LineTo(kx, ky - kh)
            .Stroke(color: primary, width: 2, alpha: 0.6);
        target.MoveTo(kx, ky - kh).LineTo(kx, ky + kh).Stroke(color: secondary, width: 1, alpha: 0.4);
        target.MoveTo(kx - kw, ky).LineTo(kx + kw, ky).Stroke(color: secondary, width: 1, alpha: 0.4);
        target.MoveTo(kx, ky - kh).LineTo(kx + kw, ky).LineTo(kx, ky + kh).LineTo(kx - kw, ky).LineTo(kx, ky - kh)
            .Fill(color: primary, alpha: 0.06);
        // String with sampled sag; tail bows are separate commands so the string
        // stays one continuous stroke (and grows in one piece).
        var bows = new List<(double x, double y)>();
        target.MoveTo(kx, ky + kh);
        var stringSteps = 24;
        for (var i = 1; i <= stringSteps; i += 1) {
            var t = i / (double)stringSteps;
            var x = kx - t * radius * 0.5 + Math.Sin(t * Math.PI * 2.2) * radius * 0.08;
            var y = ky + kh + t * radius * 0.6;
            target.LineTo(x, y);
            if (i == 7 || i == 13 || i == 19) bows.Add((x, y));
        }
        target.Stroke(color: primary, width: 1.5, alpha: 0.5);
        foreach (var (x, y) in bows) {
            var bowS = radius * 0.035;
            target.MoveTo(x, y).LineTo(x - bowS, y - bowS).LineTo(x, y - bowS * 0.3).LineTo(x + bowS, y - bowS)
                .LineTo(x, y)
                .Stroke(color: secondary, width: 1, alpha: 0.55);
        }
    }
}
