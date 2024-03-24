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
            PvName = TableColumn.PvName;
            return TableColumn;
        }
    }
}