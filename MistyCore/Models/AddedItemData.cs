namespace MistyCore.Models;

public class AddedItemData
{
    public string QualifiedItemId { get; set; } = "(O)0";
    public int Stack { get; set; } = 1;
    public bool IsRecipe { get; set; } = false;
    public int Quality { get; set; }
}