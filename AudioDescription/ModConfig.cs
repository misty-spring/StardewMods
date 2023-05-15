namespace AudioDescription
{
    internal class ModConfig
    {
        public string Type { get; set; } = "HUDMessage";
        public string Blacklist { get; set; } 
        public int CoolDown { get; set; } = 5;
        public bool PlayerSounds { get; set; } = false;
        public bool NPCs { get; set; } = true;
        public bool Environment { get; set; } = true;
        public bool ItemSounds { get; set; } = false;
        public bool FishingCatch { get; set; } = true;
	public bool Minigames { get; set; } = false;
    }
}