namespace Rugal.PartialViewRender.Model
{
    public class PvOption
    {
        public string PvName { get; set; }
        public object Data { get; set; }
        public TableColumnModel TableColumn { get; private set; }
        public TableColumnModel WithTableColumn()
        {
            TableColumn = new TableColumnModel(this);
            return TableColumn;
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
    }
}