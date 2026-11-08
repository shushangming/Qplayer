namespace AvaloniaSilkEffects.Sonnet;

/// <summary>Folia's 52 extended motifs, preserving the upstream seed/variant mapping (48–99).</summary>
internal static partial class SonnetExtendedDrawLists
{
    private const double TAU = Math.Tau;
    private delegate void Drawer(SonnetDrawList target, double radius, double width, double height,
        uint seed, uint primary, uint secondary);

    private static readonly Drawer[] Drawers =
    [
        DrawSpiralGalaxy,
        DrawCometTrail,
        DrawEclipseCorona,
        DrawMeteorShower,
        DrawOrbitSatellites,
        DrawAuroraRibbons,
        DrawCrescentHalo,
        DrawNebulaVeil,
        DrawStarMap,
        DrawLunarTide,
        DrawWaveScrolls,
        DrawNautilus,
        DrawCoralBranch,
        DrawLighthouseBeam,
        DrawCompassRose,
        DrawSailRegatta,
        DrawBubbleRise,
        DrawTidePools,
        DrawSeaweedSway,
        DrawDeepCurrent,
        DrawSoundWave,
        DrawVinylGrooves,
        DrawEqualizerBloom,
        DrawNoteArc,
        DrawTuningFork,
        DrawPianoRibbon,
        DrawMetronome,
        DrawStaffWave,
        DrawOrigamiCrane,
        DrawPaperPlaneTrail,
        DrawWeaveBand,
        DrawKnotLoop,
        DrawStitchSampler,
        DrawFoldedFan,
        DrawRibbonCurl,
        DrawPatchworkTrio,
        DrawDreamcatcher,
        DrawTasselDrop,
        DrawPendulumWave,
        DrawDominoArc,
        DrawGearCluster,
        DrawCircuitDelta,
        DrawSignalTower,
        DrawSpiralStair,
        DrawWaterfallLines,
        DrawPinwheel,
        DrawRippleDrop,
        DrawSuspensionBridge,
        DrawFieldLines,
        DrawPrismBeam,
        DrawEchoArcs,
        DrawKiteString,
    ];

    internal static SonnetDrawList Build(int variant, double width, double height, uint seed,
        uint primary, uint secondary)
    {
        if (variant is < 48 or > 99) throw new ArgumentOutOfRangeException(nameof(variant));
        var target = new SonnetDrawList();
        Drawers[variant - 48](target, Math.Min(width, height), width, height, seed, primary, secondary);
        return target;
    }

    private static double Hash(uint seed, double index, uint salt) => SonnetRandom.Hash01(seed, (int)index, salt);
    private static (double x, double y) Bleed(double width, double height, double radius) =>
        (Math.Max(radius * 0.92, width * 0.64), Math.Max(radius * 0.92, height * 0.64));
}
