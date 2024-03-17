namespace SpousesIsland
{
    internal class ModConfig
    {
        //general
        public int CustomChance { get; set; } = 10;
        public bool ScheduleRandom { get; set; }
        public bool IslandClothes { get; set; }

        public bool Devan { get; set; }

        //spouses blacklisted
        public string Blacklist { get; set; } = "";

        //children-related
        public bool AllowChildren { get; set; }
        public bool UseFurnitureBed { get; set; }
        public string Childbedcolor { get; set; } = "1"; //if not using furniture bed
    }
}