using System.ComponentModel;

namespace SysBot.Pokemon;

public class CurrySettings
{
    private const string Curry = nameof(Curry);
    private const string Counts = nameof(Counts);
    public override string ToString() => "Curry Bot Settings";

    [Category(Curry), Description("Sum of all encounter slots for this area.")]
    public int CurrySlotTotal { get; set; } = 1;

    [Category(Curry), Description("The list of curry slots to target, formatted like \"20-33, 50-100\".")]
    public string CurryTargetSlots { get; set; } = string.Empty;

    [Category(Curry), Description("Number of berries to add to each curry. Ranges from 1-10. Will use maximum if set to 0.")]
    public int CurryBerriesToUse { get; set; }

    [Category(Curry), Description("Number of times to cook curry before rebooting the game to restore ingredients.")]
    public int CurryTimesToCook { get; set; } = 30;

    [Category(Curry), Description("Chance of a curry spawn. Set this for the final curry grade using this routine. Koffing = 0.01, Wobbuffet = 0.05, Milcery = 0.15.")]
    public float CurryTargetChance { get; set; } = 0.15f;
}
