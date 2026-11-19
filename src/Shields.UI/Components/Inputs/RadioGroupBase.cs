using Microsoft.AspNetCore.Components;

namespace Shields.UI.Components.Inputs;

/// <summary>Shared behaviour of the radio groups: after a tap the parent may decline the value, and the browser must then show the value the parent kept.</summary>
public abstract class RadioGroupBase<TValue> : ComponentBase
{
    private TValue? pickedValue;
    private bool pickSettled;
    private bool restoreFocus;

    /// <summary>The radios by option index, filled by the markup's element references.</summary>
    protected Dictionary<int, ElementReference> Radios { get; } = [];

    /// <summary>Changes when the radios must be recreated, used as the fieldset's key.</summary>
    protected int Generation { get; private set; }

    /// <summary>Name shared by the radios of this group so the browser treats them as one.</summary>
    protected string GroupName { get; } = $"radio-{Guid.NewGuid():N}";

    /// <summary>What the choices pick, e.g. Range. Read by screen readers only.</summary>
    [Parameter, EditorRequired]
    public string Label { get; set; } = "";

    /// <summary>Choices, in order. Hints are not shown.</summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<RadioOption<TValue>> Options { get; set; } = [];

    /// <summary>Chosen value. When it matches no option, none shows as chosen.</summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>Raised with the value of the option tapped.</summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>Value of the group's data-testid attribute.</summary>
    [Parameter]
    public string? TestId { get; set; }

    /// <summary>Whether the option is the chosen one.</summary>
    protected bool IsSelected(RadioOption<TValue> option) => EqualityComparer<TValue>.Default.Equals(option.Value, Value);

    /// <summary>Raises <see cref="ValueChanged"/> and notes that the tap is finished, so the next render can check whether the parent took it.</summary>
    protected async Task Pick(TValue value)
    {
        pickedValue = value;
        try
        {
            await ValueChanged.InvokeAsync(value);
        }
        finally
        {
            pickSettled = true;
        }
    }

    /// <summary>Recreates the radios when the parent declined the tapped value and puts focus back on the chosen one.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (restoreFocus)
        {
            restoreFocus = false;
            var chosen = Options.ToList().FindIndex(IsSelected);
            if (chosen >= 0 && Radios.TryGetValue(chosen, out var radio))
            {
                await radio.FocusAsync();
            }

            return;
        }

        if (!pickSettled)
        {
            return;
        }

        pickSettled = false;
        if (!EqualityComparer<TValue>.Default.Equals(pickedValue, Value))
        {
            Generation++;
            restoreFocus = true;
            StateHasChanged();
        }
    }
}
