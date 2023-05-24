using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using StardewValley;

namespace DynamicDialogues
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface ISpaceCoreAPI
    {
        string[] GetCustomSkills();
        int GetLevelForCustomSkill(Farmer farmer, string skill);
        void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt);
        int GetProfessionId(string skill, string profession);

        // Must have [XmlType("Mods_SOMETHINGHERE")] attribute (required to start with "Mods_")
        void RegisterSerializerType(Type type);
        void RegisterCustomProperty( Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter );
    }
}
