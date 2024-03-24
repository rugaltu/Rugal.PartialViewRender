using Microsoft.AspNetCore.Html;

namespace Rugal.PartialViewRender.Model
{
    public class TableColumnModel
    {
        public PvOption Option { get; init; }
        public string PvName => $"TableColumn-{Option.PvName}";
        public string VSlotKey => $"TableColumns.{Option.PvName}".ToLower();
        public HtmlString VSlot => GetVSlot();
        public List<string> VSlotParma { get; init; }
        public TableColumnModel(PvOption _Option)
        {
            Option = _Option;
            VSlotParma =
            [
                "item",
                "Item = item",
                "value",
            ];
        }
        public TableColumnModel WithVSlotParam(string Param)
        {
            VSlotParma.Add(Param);
            return this;
        }
        private HtmlString GetVSlot()
        {
            var SetParam = string.Join(" , ", VSlotParma);
            var VSlotText = $@"v-slot:[{VSlotKey}]=""{{ {SetParam} }}""";
            var Result = new HtmlString(VSlotText);
            return Result;
        }
    }
}
