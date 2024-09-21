using Microsoft.AspNetCore.Html;
using Rugal.PartialViewRender.Service;

namespace Rugal.PartialViewRender.Model
{
    public class PvOption<TPvType> where TPvType : Enum
    {
        private string _PvName;
        public TPvType PvType { get; private set; }
        public string PvName => _PvName ?? PvType.ToString();
        public PvSlotsStore Slots { get; private set; } = new();
        public PvAttrsStore Attrs { get; private set; } = new();
        public PvOption(TPvType _PvType)
        {
            PvType = _PvType;
        }
        public PvOption(TPvType _PvType, string _PvName) : this(_PvType)
        {
            this._PvName = _PvName;
        }
        public PvOption<TPvType> WithPvName(string PvName)
        {
            _PvName = PvName;
            return this;
        }
        public IHtmlContent RenderSlot(Enum SlotName, string NullContent = null)
        {
            return RenderSlot(SlotName.ToString(), NullContent);
        }
        public IHtmlContent RenderSlot(string SlotName, string NullContent = null)
        {
            var SlotValue = Slots.Get(SlotName);
            if (SlotValue is not null && SlotValue.HasContent())
                return new HtmlString(SlotValue.Content);

            return new HtmlString(NullContent);
        }
        public PvSlotsSet GetSlot(string SlotName) => Slots.Get(SlotName);
        public PvSlotsSet GetSlot(Enum SlotName) => Slots.Get(SlotName);
        public PvSlotsStore QuerySlot(params Enum[] SlotNames) => Slots.Query(SlotNames);
        public PvSlotsStore QuerySlot<TSlotName>(params TSlotName[] SlotNames) where TSlotName : Enum
            => Slots.Query(SlotNames);
        public PvAttrsSet GetAttr(string AttrName) => Attrs.Get(AttrName);
        public PvAttrsSet GetAttr(Enum AttrName) => Attrs.Get(AttrName);
        public PvAttrsStore QueryAttr(params Enum[] SlotNames) => Attrs.Query(SlotNames);
        public PvAttrsStore QueryAttr<TSlotName>(params TSlotName[] SlotNames) where TSlotName : Enum
            => Attrs.Query(SlotNames);

        public string Map(params string[] Paths)
        {
            var Result = ToFullPaths(".", [PvName, .. Paths]);
            return Result;
        }
        public string Stick(params string[] Paths)
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
    public class PvResult<TPvType> where TPvType : Enum
    {
        private readonly PvRender<TPvType> Render;
        public IHtmlContent Content => GetContent();
        public PvOption<TPvType> Option { get; private set; }
        public TPvType PvType { get; private set; }
        public PvResult(PvRender<TPvType> _Render, TPvType _PvType)
        {
            Render = _Render;
            PvType = _PvType;
        }
        public PvResult<TPvType> WithOption(PvOption<TPvType> _Option)
        {
            Option = _Option;
            return this;
        }
        public PvResult<TPvType> SetOption(Action<PvOption<TPvType>> SetFunc)
        {
            Option ??= new PvOption<TPvType>(PvType);
            SetFunc(Option);
            return this;
        }
        private IHtmlContent GetContent()
        {
            Option ??= new PvOption<TPvType>(PvType);
            var ContentResult = Render.Render(PvType, Option);
            return ContentResult;
        }
    }
}