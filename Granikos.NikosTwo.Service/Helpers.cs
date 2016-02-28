using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Granikos.NikosTwo.Service.ConfigurationService.Models;

namespace Granikos.NikosTwo.Service
{
    public static class Helpers
    {
        // http://stackoverflow.com/questions/1600962/displaying-the-build-date
        public static DateTime GetBuildDate(this Assembly assembly)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            var b = new byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            var i = BitConverter.ToInt32(b, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.ToLocalTime();
            return dt;
        }

        // http://stackoverflow.com/questions/1600962/displaying-the-build-date
        public static VersionInfo GetVersionInfo(this Assembly assembly)
        {
            return new VersionInfo
            {
                Assembly = assembly.GetName().Name,
                BuildDate = assembly.GetBuildDate(),
                Version = assembly.GetName().Version
            };
        }

        public static IEnumerable<Tuple<Type,string>> GetExportedTypesWithContracts<T>(this CompositionContainer container)
        {
            foreach (var part in container.Catalog.Parts)
            {
                foreach (var def in part.ExportDefinitions)
                {
                    if (def.Metadata.ContainsKey("ExportTypeIdentity") &&
                        def.Metadata["ExportTypeIdentity"].Equals(typeof (T).FullName))
                    {
                        yield return Tuple.Create(ReflectionModelServices.GetPartType(part).Value, def.ContractName);
                    }
                }
                
            }
        }

        public static string ToCanonicalPath(this string directory)
        {
            var path = Path.GetFullPath(directory);
            if (Path.GetDirectoryName(path) != null)
                path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return path.ToLower();
        }
    }
}