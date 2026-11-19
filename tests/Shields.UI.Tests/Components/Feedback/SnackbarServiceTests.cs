using Shields.UI.Components.Feedback;

namespace Shields.UI.Tests.Components.Feedback;

public class SnackbarServiceTests
{
    private ManualClock clock = null!;
    private SnackbarService service = null!;

    [SetUp]
    public void CreateService()
    {
        clock = new ManualClock();
        service = new SnackbarService(clock);
    }

    [TearDown]
    public void DisposeService() => service.Dispose();

    [Test]
    public void DefaultDuration_IsFiveSeconds() =>
        Assert.That(SnackbarService.DefaultDuration, Is.EqualTo(TimeSpan.FromSeconds(5)));

    [Test]
    public void Show_RaisesChangedAndSetsCurrent()
    {
        var changes = 0;
        service.Changed += () => changes++;

        service.Show("Entry added");

        Assert.Multiple(() =>
        {
            Assert.That(service.Current?.Text, Is.EqualTo("Entry added"));
            Assert.That(changes, Is.EqualTo(1));
        });
    }

    [Test]
    public void Dismiss_RemovesTheMessageAndRaisesChangedOnlyWhenThereWasOne()
    {
        var changes = 0;
        service.Show("Entry added");
        service.Changed += () => changes++;

        service.Dismiss();
        service.Dismiss();

        Assert.Multiple(() =>
        {
            Assert.That(service.Current, Is.Null);
            Assert.That(changes, Is.EqualTo(1));
        });
    }

    [Test]
    public void Show_RemovesTheMessageAfterTheDuration()
    {
        service.Show("Entry added", duration: TimeSpan.FromSeconds(2));

        clock.Advance(TimeSpan.FromSeconds(2) - TimeSpan.FromMilliseconds(1));
        var early = service.Current;
        clock.Advance(TimeSpan.FromMilliseconds(1));

        Assert.Multiple(() =>
        {
            Assert.That(early, Is.Not.Null);
            Assert.That(service.Current, Is.Null);
        });
    }

    [Test]
    public void InfiniteDuration_KeepsTheMessageUntilDismissed()
    {
        service.Show("Entry added", duration: Timeout.InfiniteTimeSpan);

        clock.Advance(TimeSpan.FromHours(1));

        Assert.That(service.Current, Is.Not.Null);
    }

    [Test]
    public async Task InvokeActionAsync_RunsTheActionOnceAndDismisses()
    {
        var fired = 0;
        service.Show("Entry deleted", "Undo", () =>
        {
            fired++;
            return Task.CompletedTask;
        });
        var shown = service.Current!;

        await service.InvokeActionAsync(shown);
        await service.InvokeActionAsync(shown);

        Assert.Multiple(() =>
        {
            Assert.That(fired, Is.EqualTo(1));
            Assert.That(service.Current, Is.Null);
        });
    }

    [Test]
    public async Task InvokeActionAsync_OnAReplacedMessage_DoesNothingToTheNewOne()
    {
        var oldFired = 0;
        var newFired = 0;
        service.Show("First", "Undo", () =>
        {
            oldFired++;
            return Task.CompletedTask;
        });
        var stale = service.Current!;
        service.Show("Second", "Redo", () =>
        {
            newFired++;
            return Task.CompletedTask;
        });

        await service.InvokeActionAsync(stale);

        Assert.Multiple(() =>
        {
            Assert.That(oldFired, Is.Zero);
            Assert.That(newFired, Is.Zero);
            Assert.That(service.Current?.Text, Is.EqualTo("Second"));
        });
    }

    [Test]
    public void Show_ReplacingAMessageNeverRunsItsAction()
    {
        var fired = 0;
        service.Show("First", "Undo", () =>
        {
            fired++;
            return Task.CompletedTask;
        });

        service.Show("Second");
        clock.Advance(SnackbarService.DefaultDuration);

        Assert.That(fired, Is.Zero);
    }

    [Test]
    public void NegativeDuration_ThrowsAndLeavesTheShowingMessage()
    {
        service.Show("First", duration: Timeout.InfiniteTimeSpan);

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Show("Second", duration: TimeSpan.FromSeconds(-1)));

        Assert.That(service.Current?.Text, Is.EqualTo("First"));
    }

    [TestCase("  ", true)]
    [TestCase(null, true)]
    [TestCase("Undo", false)]
    public void ActionLabelOrHandlerMissing_DropsTheAction(string? label, bool hasHandler)
    {
        service.Show("Entry added", label, hasHandler ? () => Task.CompletedTask : null);

        Assert.Multiple(() =>
        {
            Assert.That(service.Current!.ActionLabel, Is.Null);
            Assert.That((object?)service.Current.OnAction, Is.Null);
        });
    }
}
