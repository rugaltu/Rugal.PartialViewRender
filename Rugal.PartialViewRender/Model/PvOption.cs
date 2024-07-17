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
            var ClearPaths = Paths
                .Select(Item => Item.TrimStart('.').TrimEnd('.'))
                .ToList();

            ClearPaths.Insert(0, PvName);
            var Result = string.Join('.', ClearPaths);
            return Result;
        }
        public string With(params string[] Paths)
        {
            var PathList = Paths.ToList();
            PathList.Insert(0, PvName);

            var Result = string.Join("", PathList);
            return Result;
        }
    }
}