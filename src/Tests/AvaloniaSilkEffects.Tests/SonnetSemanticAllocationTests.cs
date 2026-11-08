using AvaloniaSilkEffects.Sonnet;

namespace AvaloniaSilkEffects.Tests;

public sealed class SonnetSemanticAllocationTests
{
    // Expected boundaries recorded from Folia's Intl.Segmenter word segmentation.
    [Theory]
    [InlineData("沿着光的轨迹", "沿着|光|的|轨迹")]
    [InlineData("世界， 再见！", "世界，| |再见！")]
    [InlineData("我们一起走过漫长的夜晚", "我们|一起|走过|漫长|的|夜晚")]
    [InlineData("明かりにあなたへ", "明かり|に|あなた|へ")]
    [InlineData("don’t stop 3.14!", "don’t| |stop| |3.14!")]
    [InlineData("星光👨‍👩‍👧‍👦闪耀", "星光👨‍👩‍👧‍👦|闪耀")]
    public void SegmentationMatchesReferenceAndPreservesTiming(string text, string expected)
    {
        var segments = SonnetProgramCompiler.BuildSemanticSegments(
            new SonnetLine(text, 1, 7, [new SonnetWordTiming(text, 1, 7)]));
        Assert.Equal(expected.Split('|'), segments.Select(segment => segment.Text));
        Assert.Equal(text, string.Concat(segments.Select(segment => segment.Text)));
        Assert.Equal(1, segments[0].StartTime);
        Assert.Equal(7, segments[^1].EndTime);
        Assert.All(segments, segment =>
        {
            Assert.Equal(text[segment.StartOffset..segment.EndOffset], segment.Text);
            Assert.Equal([0], segment.WordIndices);
        });
        for (var i = 1; i < segments.Count; i++)
            Assert.Equal(segments[i - 1].EndTime, segments[i].StartTime);
    }

    [Fact]
    public void ChineseSentenceDoesNotForceQuietShotAndReachesExtendedGeometry()
    {
        var reached = new HashSet<SonnetShotKind>();
        var geometryVariants = new HashSet<uint>();
        for (var seed = 0; seed < 256; seed++)
        {
            var program = SonnetProgramCompiler.Compile([
                new SonnetLine("沿着光的轨迹", 0, 5, [new("沿着光的轨迹", 0, 5)]),
                new SonnetLine("再见", 10, 12, [new("再见", 10, 12)])
            ], seed.ToString());
            var paragraph = program.Paragraphs[0];
            Assert.Equal(SonnetParagraphKind.Verse, paragraph.Kind);
            var shot = Assert.Single(paragraph.Shots);
            reached.Add(shot.Kind);
            if (shot.Kind is SonnetShotKind.TypeImpact or SonnetShotKind.FragmentCollage)
                geometryVariants.Add(SonnetRandom.Hash($"{program.Seed}:{paragraph.Id}") % 100);
        }
        Assert.Equal(7, reached.Count);
        Assert.Contains(geometryVariants, variant => variant >= 48);
    }
}
