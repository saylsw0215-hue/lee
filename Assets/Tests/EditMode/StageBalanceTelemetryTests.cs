using HeroDefense.QA;
using NUnit.Framework;

namespace HeroDefense.Tests.EditMode
{
    public sealed class StageBalanceTelemetryTests
    {
        [Test] public void ValidReportRejectsNoNumbers(){var report=new StageBalanceTelemetry{duration=10,finalBaseHp=1,finalGold=0};report.waves.Add(new WaveBalanceTelemetry{startTime=0,endTime=10,baseHp=1,remainingGold=0});Assert.That(report.HasInvalidNumbers(),Is.False);}
        [Test] public void NaNAndNegativeEconomyAreDetected(){var report=new StageBalanceTelemetry{duration=float.NaN,finalBaseHp=-1,finalGold=-1};Assert.That(report.HasInvalidNumbers(),Is.True);}
        [Test] public void OutputPathIsTransientBuildArtifact(){string path=StageBalanceTelemetryWriter.PathFor("hero_test");StringAssert.StartsWith(System.IO.Path.Combine("Builds","Balance"),path);StringAssert.EndsWith(".json",path);}
    }
}
