namespace Rugal.PartialViewRender.Model
{
    public class PvOption<TPvType>
        where TPvType : Enum
    {
        public TPvType PvType { get; private set; }
        public string PvName { get; private set; }
        public object Data { get; set; }
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
        public string Main(params string[] Paths)
        {
            var PvTypeName = typeof(TPvType).Name;
            var Result = ToFullPaths(".", [PvTypeName, PvType.ToString(), .. Paths]);
            return Result;
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
    }
}