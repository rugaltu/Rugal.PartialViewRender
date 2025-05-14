using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Rugal.PartialViewRender.Services;

namespace Rugal.PartialViewRender.Models;

public interface IPvOption
{
    public PvSlotsStore Slots { get; }
    public PvAttrsStore Attrs { get; }
    public PvAttrsSet ParentAttrs { get; }
    public string PvName { get; }
    public string ParentTag { get; }
}
public abstract class PvOption
{
    public static PvOption<TPvType> Create<TPvType>(TPvType PvType) where TPvType : Enum
    {
        return new PvOption<TPvType>(PvType);
    }
}

public class PvOption<TPvType> : IPvOption where TPvType : Enum
{
    protected string _PvName;
    public string PvName => _PvName ?? PvType.ToString();
    public bool HasPvName => !string.IsNullOrWhiteSpace(PvName);
    public string ParentTag { get; protected set; }
    public PvSlotsStore Slots { get; protected set; } = new();
    public PvAttrsStore Attrs { get; protected set; } = new();
    public PvAttrsSet ParentAttrs { get; protected set; } = new();
    public TPvType PvType { get; protected set; }
    public PvOption(TPvType PvType)
    {
        this.PvType = PvType;
    }
    public PvOption(TPvType PvType, string PvName) : this(PvType)
    {
        _PvName = PvName;
    }
    public PvOption<TPvType> WithPvName(string PvName)
    {
        _PvName = PvName;
        return this;
    }
    public PvSlotsSet GetSlot(Enum SlotName) => Slots.Get(SlotName);
    public PvSlotsSet GetSlotCreate(Enum SlotName)
    {
        if (HasSlot(SlotName))
            return GetSlot(SlotName);

        var NewSlot = new PvSlotsSet()
        {
            SlotName = SlotName.ToString(),
        };
        AddSlot(SlotName, NewSlot);
        return NewSlot;
    }
    public PvOption<TPvType> AddSlot(Enum SlotName, string SlotValue, PropPassType PassType = PropPassType.Append)
    {
        AddSlot(SlotName, new PvSlotsSet()
        {
            Content = SlotValue,
            PassType = PassType,
            SlotName = SlotName.ToString()
        });
        return this;
    }
    public PvOption<TPvType> AddSlot(Enum SlotName, IHtmlContent SlotValue, PropPassType PassType = PropPassType.Append)
    {
        AddSlot(SlotName, SlotValue.ToString(), PassType);
        return this;
    }
    public PvOption<TPvType> AddSlot(Enum SlotName, PvSlotsSet SlotSet)
    {
        Slots.Add(SlotName, SlotSet);
        return this;
    }
    public PvOption<TPvType> FillSlot(Enum SlotName, string SlotValue)
    {
        FillSlot(SlotName, new PvSlotsSet(SlotName.ToString(), SlotValue));
        return this;
    }
    public PvOption<TPvType> FillSlot(Enum SlotName, PvSlotsSet SlotSet)
    {
        SlotSet.PassType = PropPassType.Fill;
        Slots.Add(SlotName, SlotSet);
        return this;
    }
    public PvOption<TPvType> MultiSlot(Enum SlotName, string SlotValue)
    {
        MultiSlot(SlotName, new PvSlotsSet(SlotName.ToString(), SlotValue));
        return this;
    }
    public PvOption<TPvType> MultiSlot(Enum SlotName, PvSlotsSet SlotSet)
    {
        SlotSet.PassType = PropPassType.Multi;
        Slots.Add(SlotName, SlotSet);
        return this;
    }
    public string GetSlotContent(Enum SlotName) => Slots.Get(SlotName)?.Content;
    public PvSlotsStore QuerySlot<TSlot>(IEnumerable<TSlot> SlotNames) where TSlot : Enum
        => Slots.Query(SlotNames);
    public PvSlotsStore QuerySlot(params Enum[] SlotNames) => Slots.Query(SlotNames);
    public PvSlotsStore QuerySlot<TSlot>(params TSlot[] SlotNames) where TSlot : Enum =>
        Slots.Query(SlotNames);
    public bool HasSlot(Enum SlotName) => Slots.ContainsKey(SlotName);
    public bool HasSlotAny<TSlot>(params TSlot[] SlotNames) where TSlot : Enum =>
        HasSlotAny(SlotNames.Select(Item => Item.ToString()));
    public bool HasSlotAny(IEnumerable<string> SlotNames)
        => Slots.Any(Item => SlotNames.Contains(Item.SlotName));
    public bool TryGetSlot(Enum SlotName, out PvSlotsSet Slot)
    {
        return Slots.TryGet(SlotName, out Slot);
    }
    public bool TryGetSlotMulti(Enum SlotName, out PvSlotsSet[] MultiSlot)
    {
        var Result = Slots.TryGet(SlotName, out var Slot);
        MultiSlot = Slot?.ToMulti();
        return Result;
    }
    public bool TryGetSlotContent(Enum SlotName, out string Content)
    {
        Content = null;
        if (Slots.TryGet(SlotName, out var Slot) && Slot.HasContent())
        {
            Content = Slot.Content;
            return true;
        }
        return false;
    }
    public bool TryGetSlotContentMulti(Enum SlotName, out string[] Contents)
    {
        Contents = null;
        if (Slots.TryGet(SlotName, out var Slot))
        {
            var GetContents = Slot.ToMultiContent();
            if (GetContents.Length > 0)
            {
                Contents = GetContents;
                return true;
            }
        }
        return false;
    }
    public bool TryGetSlotRender(Enum SlotName, out IHtmlContent RenderContent)
    {
        RenderContent = null;
        if (Slots.TryGet(SlotName, out var Slot) && Slot.HasContent())
        {
            RenderContent = Slot.RenderContent;
            return true;
        }
        return false;
    }
    public bool TryGetSlotRenderMulti(Enum SlotName, out IHtmlContent[] RenderContents)
    {
        RenderContents = null;
        if (Slots.TryGet(SlotName, out var Slot))
        {
            var GetRenderContents = Slot.ToMultiRenderContent();
            if (GetRenderContents.Length > 0)
            {
                RenderContents = GetRenderContents;
                return true;
            }
        }
        return false;
    }

    public PvOption<TPvType> WithParentTag(string ParentTag)
    {
        this.ParentTag = ParentTag;
        return this;
    }
    public PvOption<TPvType> WithParentTagFrom(PvOption<TPvType> Option)
    {
        ParentTag = Option.ParentTag;
        return this;
    }

    public PvOption<TPvType> AddParentAttr(string AttrName, string AttrValue, PropPassType PassType = PropPassType.Append)
    {
        AddParentAttr(AttrName, new PvAttrsValue(AttrValue, PassType));
        return this;
    }
    public PvOption<TPvType> AddParentAttr<TAttr>(TAttr Attr, PropPassType PassType = PropPassType.Append) where TAttr : class
    {
        if (Attr is string StringAttr)
        {
            AddParentAttr(StringAttr, null, PassType);
            return this;
        }

        var AllProperty = Attr
            .GetType()
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var Property in AllProperty)
        {
            var Value = Property.GetValue(Attr);
            if (Value is not null)
                AddParentAttr(Property.Name, Value.ToString(), PassType);
        }

        return this;
    }
    public PvOption<TPvType> AddParentAttr(string AttrName, PvAttrsValue AttrValue)
    {
        ParentAttrs.Add(AttrName, AttrValue);
        return this;
    }

    public PvOption<TPvType> AddParentAttrFrom(PvAttrsSet Attrs)
    {
        foreach (var Attr in Attrs)
            ParentAttrs.Add(Attr.Key, Attr.Value);
        return this;
    }
    public PvOption<TPvType> AddParentAttrFrom(PvOption<TPvType> Option)
    {
        AddParentAttrFrom(Option.ParentAttrs);
        return this;
    }
    public PvOption<TPvType> WithParentOption(PvOption<TPvType> Option)
    {
        WithParentTagFrom(Option);
        AddParentAttrFrom(Option);
        return this;
    }
    public PvOption<TPvType> SaveSlots(RazorPageBase Page)
    {
        SaveSlots(Page, PvType);
        return this;
    }
    public PvOption<TPvType> SaveSlots(RazorPageBase Page, TPvType SavePvType)
    {
        Page.ViewContext.HttpContext.Items[SavePvType] = Slots;
        return this;
    }


    public IHtmlContent RenderSlot(Enum SlotName, string NullContent = null)
    {
        var SlotValue = Slots.Get(SlotName);
        if (SlotValue is not null && SlotValue.HasContent())
            return new HtmlString(SlotValue.Content);

        return new HtmlString(NullContent);
    }
    public PvAttrsSet GetAttr(string AttrName) => Attrs.Get(AttrName);
    public PvAttrsSet GetAttr(Enum AttrName) => Attrs.Get(AttrName);
    public PvAttrsStore QueryAttr(params Enum[] SlotNames) => Attrs.Query(SlotNames);
    public PvAttrsStore QueryAttr<TSlotName>(params TSlotName[] SlotNames) where TSlotName : Enum
        => Attrs.Query(SlotNames);
}
public class PvResult<TPvType> where TPvType : Enum
{
    private readonly PvRender<TPvType> Render;
    public IHtmlContent Content => GetContent();
    public PvOption<TPvType> Option { get; private set; }
    public TPvType PvType { get; private set; }
    public PvResult(PvRender<TPvType> Render, TPvType PvType)
    {
        this.Render = Render;
        this.PvType = PvType;
        Option = new PvOption<TPvType>(PvType);
    }
    public PvResult<TPvType> WithOption(PvOption<TPvType> Option)
    {
        this.Option = Option;
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