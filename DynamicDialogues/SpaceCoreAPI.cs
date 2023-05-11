using System;
using System.Reflection;
using StardewValley;

namespace DynamicDialogues
{
    public interface ISpaceCoreAPI
    {
        /// Must take (Event, GameLocation, GameTime, string[])
        void AddEventCommand(string command, MethodInfo info);

        void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter);
    }
}
