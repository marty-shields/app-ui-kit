using Shields.UI.Components.Actions;
using Shields.UI.Components.Lists;

namespace Shields.UI.Tests.Components.Lists;

public class ListRowTests : ComponentTestBase
{
    [Test]
    public void WithOnClick_RendersButtonThatRaisesIt()
    {
        var clicked = false;
        var cut = Context.Render<ListRow>(p => p
            .Add(x => x.Text, "Workouts")
            .Add(x => x.OnClick, () => clicked = true));

        cut.Find("button").Click();

        Assert.That(clicked, Is.True);
    }

    [Test]
    public void WithOnClick_EndsInAMutedChevron()
    {
        var cut = Context.Render<ListRow>(p => p
            .Add(x => x.Text, "Workouts")
            .Add(x => x.OnClick, () => { }));

        var chevron = cut.FindComponent<Icon>().Instance;

        Assert.That((chevron.Name, chevron.Tone, chevron.Size), Is.EqualTo((IconName.Chevron, IconTone.Muted, IconSize.Default)));
    }

    [Test]
    public void Aside_IsBeforeTheChevron()
    {
        var cut = Context.Render<ListRow>(p => p
            .Add(x => x.Text, "Dumbbell Curl")
            .Add(x => x.Aside, "Strength")
            .Add(x => x.OnClick, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("button").Children.TakeLast(2).Select(c => c.ClassName), Is.EqualTo(new[] { "aside", "chevron" }));
            Assert.That(cut.Find(".aside").TextContent, Is.EqualTo("Strength"));
        });
    }

    [Test]
    public void Aside_WithoutOnClick_IsAtTheEnd()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "Plank").Add(x => x.Aside, "Strength"));

        Assert.That(cut.Find("div.row").LastElementChild!.ClassName, Is.EqualTo("aside"));
    }

    [Test]
    public void WithoutOnClick_HasNoChevron() =>
        Assert.That(Context.Render<ListRow>(p => p.Add(x => x.Text, "Porridge oats")).FindAll(".chevron"), Is.Empty);

    [Test]
    public void TestId_IsOnTheRow_TappableOrNot()
    {
        var tappable = Context.Render<ListRow>(p => p.Add(x => x.Text, "A").Add(x => x.OnClick, () => { }).Add(x => x.TestId, "row"));
        var plain = Context.Render<ListRow>(p => p.Add(x => x.Text, "A").Add(x => x.TestId, "row"));

        Assert.Multiple(() =>
        {
            Assert.That(tappable.Find("[data-testid=row]").TagName, Is.EqualTo("BUTTON"));
            Assert.That(plain.Find("[data-testid=row]").TagName, Is.EqualTo("DIV"));
        });
    }

    [Test]
    public void NoAside_RendersNone() =>
        Assert.That(Context.Render<ListRow>(p => p.Add(x => x.Text, "Workouts")).FindAll(".aside"), Is.Empty);

    [Test]
    public void WithoutOnClick_RendersStaticRow()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "Tue 20 Oct 2026 · 18:05"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("button"), Is.Empty);
            Assert.That(cut.Find(".text").TextContent, Is.EqualTo("Tue 20 Oct 2026 · 18:05"));
        });
    }

    [Test]
    public void WithoutDetail_HasOneLine()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "Workouts").Add(x => x.OnClick, () => { }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".detail"), Is.Empty);
            Assert.That(cut.Find(".row").ClassList, Does.Not.Contain("two-line"));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Detail_ShowsUnderTheText(bool tappable)
    {
        var cut = Context.Render<ListRow>(p =>
        {
            p.Add(x => x.Text, "Barbell Back Squat").Add(x => x.Detail, "Last Tue 20 Oct · 70kg × 3");
            if (tappable)
            {
                p.Add(x => x.OnClick, () => { });
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".texts > span").Select(s => s.TextContent), Is.EqualTo(new[] { "Barbell Back Squat", "Last Tue 20 Oct · 70kg × 3" }));
            Assert.That(cut.Find(".row").ClassList, Does.Contain("two-line"));
        });
    }

    [Test]
    public void TextAndDetail_AreMarkedWrapOk_InBothVariants()
    {
        foreach (var clickable in new[] { true, false })
        {
            var cut = Context.Render<ListRow>(p =>
            {
                p.Add(x => x.Text, "Barbell Back Squat").Add(x => x.Detail, "Last Tue");
                if (clickable)
                {
                    p.Add(x => x.OnClick, () => { });
                }
            });

            Assert.That(cut.FindAll(".text[data-wrap-ok], .detail[data-wrap-ok]"), Has.Count.EqualTo(2), $"clickable: {clickable}");
        }
    }

    [Test]
    public void Css_WrapsTextAndDetail_AndKeepsAsideAndChevronWhole()
    {
        var css = RepoFiles.Read("src", "Shields.UI", "Components", "Lists", "ListRow.razor.css");

        Assert.Multiple(() =>
        {
            foreach (var selector in new[] { ".text", ".detail" })
            {
                Assert.That(Rule(css, selector), Does.Contain("overflow-wrap: anywhere;"), selector);
                Assert.That(Rule(css, selector), Does.Not.Contain("text-overflow"), selector);
                Assert.That(Rule(css, selector), Does.Not.Contain("white-space: nowrap;"), selector);
            }

            Assert.That(Rule(css, ".aside"), Does.Contain("flex-shrink: 0;"));
            Assert.That(Rule(css, ".aside"), Does.Contain("white-space: nowrap;"));
            Assert.That(Rule(css, ".chevron"), Does.Contain("flex-shrink: 0;"));
        });
    }

    private static string Rule(string css, string selector) =>
        System.Text.RegularExpressions.Regex.Match(css, System.Text.RegularExpressions.Regex.Escape(selector) + @"\s*\{(?<rule>[^}]*)\}").Groups["rule"].Value;

    [Test]
    public void Divider_IsOnByDefault_AndFalseDropsIt()
    {
        var plain = Context.Render<ListRow>(p => p.Add(x => x.Text, "A"));
        var without = Context.Render<ListRow>(p => p.Add(x => x.Text, "A").Add(x => x.Divider, false));

        Assert.Multiple(() =>
        {
            Assert.That(plain.Find(".row").ClassList, Does.Not.Contain("no-divider"));
            Assert.That(without.Find(".row").ClassList, Does.Contain("no-divider"));
        });
    }

    [Test]
    public void DividerFalse_AlsoDropsItOnATappableRow()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "A").Add(x => x.Divider, false).Add(x => x.OnClick, () => { }));

        Assert.That(cut.Find("button").ClassList, Does.Contain("no-divider"));
    }

    [Test]
    public void HeadlineLayout_PutsTheTextAndAsideOnOneLine_AndTheDetailUnderThem()
    {
        var cut = Context.Render<ListRow>(p => p
            .Add(x => x.Layout, ListRowLayout.Headline)
            .Add(x => x.Text, "Tue 20 Oct")
            .Add(x => x.Aside, "70kg × 3")
            .Add(x => x.Detail, "60×8 · 62.5×8 · 70×3"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".row").ClassList, Does.Contain("headline"));
            Assert.That(cut.Find(".text").TextContent, Is.EqualTo("Tue 20 Oct"));
            Assert.That(cut.Find(".aside").TextContent, Is.EqualTo("70kg × 3"));
            Assert.That(cut.Find(".detail").TextContent, Is.EqualTo("60×8 · 62.5×8 · 70×3"));
        });
    }

    [Test]
    public void StackedLayout_IsTheDefault()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "A"));

        Assert.That(cut.Find(".row").ClassList, Does.Not.Contain("headline"));
    }

    [Test]
    public void Leading_ComesBeforeTheText()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "70kg × 3").Add(x => x.Leading, "<b>2</b>"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".row").Children.First().ClassName, Is.EqualTo("leading"));
            Assert.That(cut.Find(".leading").TextContent, Is.EqualTo("2"));
        });
    }

    [TestCase(ChangeDirection.Up, "up", "up 5kg")]
    [TestCase(ChangeDirection.Down, "down", "down 5kg")]
    public void Change_ShowsAnArrowWithTheAmount_AndReadsAsUpOrDown(ChangeDirection direction, string cssClass, string spoken)
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "Squat").Add(x => x.Change, new StatChange(direction, "5kg")));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".change").ClassList, Does.Contain(cssClass));
            Assert.That(cut.Find(".change").TextContent.Trim(), Does.Contain("5kg"));
            Assert.That(cut.Find(".change .visually-hidden").TextContent + "5kg", Is.EqualTo(spoken));
            Assert.That(cut.FindComponent<Icon>().Instance.Tone, Is.EqualTo(direction == ChangeDirection.Up ? IconTone.Success : IconTone.Error));
        });
    }

    [Test]
    public void Change_None_ShowsADashReadAsNoChange()
    {
        var cut = Context.Render<ListRow>(p => p.Add(x => x.Text, "Squat").Add(x => x.Change, new StatChange(ChangeDirection.None, "")));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".change").ClassList, Does.Contain("same"));
            Assert.That(cut.Find(".change .visually-hidden").TextContent, Is.EqualTo("no change"));
        });
    }

    [Test]
    public void Trailing_IsAfterTheAside_AndBeforeTheChevron()
    {
        var cut = Context.Render<ListRow>(p => p
            .Add(x => x.Text, "Set 1")
            .Add(x => x.Aside, "a")
            .Add(x => x.Trailing, "<i>x</i>")
            .Add(x => x.OnClick, () => { }));

        Assert.That(cut.Find("button").Children.TakeLast(3).Select(c => c.ClassName), Is.EqualTo(new[] { "aside", "trailing", "chevron" }));
    }
}
