// ReSharper disable ClassNeverInstantiated.Global
namespace DynamicDialogues.Models;

/// <summary>
/// A class used for object hunts.
/// </summary>
internal class ObjectData
{
    public string ItemId = "(O)0";
    public int X;
    public int Y;

    public ObjectData()
    {
    }

    public ObjectData(ObjectData od)
    {
        ItemId = od.ItemId;
        X = od.X;
        Y = od.Y;
    }
}