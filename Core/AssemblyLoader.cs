using System;
using System.Reflection;

namespace LethalBingo.Core;

internal static class AssemblyLoader
{
    public static void LoadEmbeddedDLL()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string resourceName = "LethalBingo.Resources." + new AssemblyName(args.Name).Name + ".dll";

            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resourceName);
            
            if (stream == null)
                return null;
            
            byte[] assemblyData = new byte[stream.Length];

            _ = stream.Read(assemblyData, 0, assemblyData.Length);

            return Assembly.Load(assemblyData);
        };
    }
}