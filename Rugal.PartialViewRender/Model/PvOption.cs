using System.Text.Json;
using static System.Formats.Asn1.AsnWriter;

namespace Rugal.PartialViewRender.Model
{
    public class PvOption<TPvType>
        where TPvType : Enum
    {
        public TPvType PvType { get; private set; }
        public string PvName { get; private set; }
        public PvOption(string _PvName, TPvType _PvType)
        {
            PvName = _PvName;
            PvType = _PvType;
        }
        public string Map(params string[] Paths)
        {
            var Result = ToFullPaths(".", [PvName, .. Paths]);
            return Result;
        }
        public string With(params string[] Paths)
        {
            var Result = ToFullPaths("", [PvName, .. Paths]);
            return Result;
        }
        public string MainEvent(string EventName)
        {
            var FullPath = ToMainPaths([PvType.ToString(), EventName]);
            var Result = @$"{FullPath}({{ PvName:'{PvName}', Store: {PvName}, $Event: $event }})";
            return Result;
        }
        public string MainStore(params string[] StorePaths)
        {
            var FullPath = ToMainPaths([PvType.ToString(), .. StorePaths]);
            return FullPath;
        }

        private static List<string> ToClearPaths(IEnumerable<string> Paths)
        {
            return Paths
                .Select(Item => Item.TrimStart('.').TrimEnd('.'))
                .ToList();
        }
        private static string ToFullPaths(string Separator, IEnumerable<string> Paths)
        {
            var AllPaths = ToClearPaths(Paths);
            var Result = string.Join(Separator, AllPaths);
            return Result;
        }
        private static string ToMainPaths(IEnumerable<string> Paths)
        {
            var PvTypeName = typeof(TPvType).Name;
            var FullMainPath = ToFullPaths(".", [PvTypeName, .. Paths]);
            return FullMainPath;
        }
    }
}