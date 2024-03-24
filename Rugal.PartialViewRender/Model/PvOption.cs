namespace Rugal.PartialViewRender.Model
{
    public class PvOption
    {
        public string PvName { get; set; }
        public object Data { get; set; }
        public TableColumnModel ToTableColumn() => new(this);
    }
}