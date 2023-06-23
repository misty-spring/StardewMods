namespace FarmVisitors
{
    public class Data
    {
        public static Point GetRandomTile(GameLocation location, int maxTries = 10)
        {
            var r = Game1.random;
            var data = ModEntry.Locations[location.NameOrUniqueName];
            var tile = data[r.Next(data.Count)];
            
            for (var i = 0; i < maxTries; i++)
            {
                var isClean = !location.isObjectAt(tile.X,tile.Y) && location.isAreaClear(new Rectangle(tile.X, tile.Y, 1, 1)) && location
                    .isTileLocationTotallyClearAndPlaceableIgnoreFloors(tile.ToVector2());
                if (isClean)
                    break;
                
                tile = data[r.Next(data.Count)];
            }

            return data[r.Next(data.Count)];
        }
        
        internal static string TurnToString(List<string> list)
        {
            var result = "";

            foreach (var s in list)
            {
                result = s.Equals(list[0]) ? $"{s}" : $"{result}, {s}";
            }
            return result;
        }

    }
}