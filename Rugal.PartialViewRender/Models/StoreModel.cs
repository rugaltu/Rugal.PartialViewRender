using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Options;

namespace Rugal.PartialViewRender.Models;

public abstract class StoreBase<TStoreKey, TStoreValue, TStore>
    where TStoreKey : Enum
    where TStore : StoreBase<TStoreKey, TStoreValue, TStore>, new()
{
    protected Dictionary<string, TStoreValue> Store { get; set; } = [];
    public virtual string[] Keys => [.. Store.Keys];
    public virtual TStore Add(string StoreKey, TStoreValue StoreValue)
    {
        BaseAdd(StoreKey, StoreValue);
        return ConvertThis();
    }
    public virtual TStore Add(TStoreKey StoreKey, TStoreValue StoreValue)
    {
        BaseAdd(StoreKey.ToString(), StoreValue);
        return ConvertThis();
    }
    public virtual TStore AddFrom(TStore Source)
    {
        foreach (var Slot in Source)
            BaseAdd(Slot.Key, Slot.Value);
        return ConvertThis();
    }
    public virtual TStore AddFrom(TStore Source, params TStoreKey[] StoreKeys)
    {
        BaseAddFrom(Source, StoreKeys.Select(Item => Item.ToString()).ToArray());
        return ConvertThis();
    }
    public virtual TStore AddFrom(TStore Source, IEnumerable<string> StoreKeys)
    {
        BaseAddFrom(Source, StoreKeys);
        return ConvertThis();
    }
    public virtual TStore AddFrom<TSlotName>(TStore Source, params TSlotName[] StoreKeys)
        where TSlotName : TStoreKey
    {
        BaseAddFrom(Source, StoreKeys.Select(Item => Item.ToString()).ToArray());
        return ConvertThis();
    }
    public virtual TStore AddFrom<TSlotName>(TStore Source, IEnumerable<TSlotName> StoreKeys)
        where TSlotName : TStoreKey
    {
        BaseAddFrom(Source, StoreKeys.Select(Item => Item.ToString()).ToArray());
        return ConvertThis();
    }
    public virtual TStoreValue Get(TStoreKey StoreKey)
    {
        return BaseGet(StoreKey.ToString());
    }
    public virtual TStoreValue Get(string StoreKey)
    {
        return BaseGet(StoreKey);
    }
    public virtual bool TryGet(TStoreKey StoreKey, out TStoreValue StoreValue)
    {
        return BaseTryGet(StoreKey.ToString(), out StoreValue);
    }
    public virtual bool TryGet(string StoreKey, out TStoreValue StoreValue)
    {
        return BaseTryGet(StoreKey, out StoreValue);
    }
    public virtual TStore Remove(TStoreKey StoreKey)
    {
        BaseRemove(StoreKey.ToString());
        return ConvertThis();
    }
    public virtual TStore Remove(string StoreKey)
    {
        BaseRemove(StoreKey);
        return ConvertThis();
    }
    public TStore QueryStore(IEnumerable<string> StoreKeys)
    {
        var Result = new TStore()
            .AddFrom(ConvertThis(), StoreKeys);
        return Result;
    }
    public TStore QueryStore<TSlotName>(IEnumerable<TSlotName> StoreKeys)
        where TSlotName : TStoreKey
    {
        var Result = new TStore()
            .AddFrom(ConvertThis(), StoreKeys);
        return Result;
    }
    public TStore QueryStore(params TStoreKey[] StoreKeys)
    {
        var Result = new TStore()
            .AddFrom(ConvertThis(), StoreKeys);
        return Result;
    }
    public TStore QueryStore<TSlotName>(params TSlotName[] StoreKeys)
        where TSlotName : TStoreKey
    {
        var Result = new TStore()
            .AddFrom(ConvertThis(), StoreKeys);
        return Result;
    }
    public TStoreValue OrderFirst(IEnumerable<string> StoreKeys)
    {
        var Values = Store.Where(Item => StoreKeys.Contains(Item.Key));
        if (!Values.Any())
            return default;

        var Result = Values.FirstOrDefault();
        return Result.Value;
    }
    public TStoreValue OrderFirst<TSlotName>(IEnumerable<TSlotName> StoreKeys)
        where TSlotName : TStoreKey
    {
        var Result = OrderFirst(StoreKeys.Select(Item => Item.ToString()));
        return Result;
    }
    public TStoreValue OrderFirst(params TStoreKey[] StoreKeys)
    {
        var Result = OrderFirst(StoreKeys.Select(Item => Item.ToString()));
        return Result;
    }
    public TStoreValue OrderFirst<TSlotName>(params TSlotName[] StoreKeys)
        where TSlotName : TStoreKey
    {
        var Result = OrderFirst(StoreKeys.Select(Item => Item.ToString()));
        return Result;
    }
    public TStoreValue OrderLast(IEnumerable<string> StoreKeys)
    {
        var Values = Store.Where(Item => StoreKeys.Contains(Item.Key));
        if (!Values.Any())
            return default;

        var Result = Values.LastOrDefault();
        return Result.Value;
    }
    public TStoreValue OrderLast<TSlotName>(IEnumerable<TSlotName> StoreKeys)
        where TSlotName : TStoreKey
    {
        var Result = OrderLast(StoreKeys.Select(Item => Item.ToString()));
        return Result;
    }
    public TStoreValue OrderLast(params TStoreKey[] StoreKeys)
    {
        var Result = OrderLast(StoreKeys.Select(Item => Item.ToString()));
        return Result;
    }
    public TStoreValue OrderLast<TSlotName>(params TSlotName[] StoreKeys)
        where TSlotName : TStoreKey
    {
        var Result = OrderLast(StoreKeys.Select(Item => Item.ToString()));
        return Result;
    }
    public virtual bool Any() => Store.Count != 0;
    public virtual bool Any(Func<TStoreValue, bool> AnyFunc) => Store.Values.Any(AnyFunc);
    public virtual bool ContainsKey(TStoreKey StoreKey) => ContainsKey(StoreKey.ToString());
    public virtual bool ContainsKey(string StoreKey) => Store.ContainsKey(StoreKey);
    protected virtual TStoreValue AddProcess(string StoreKey, TStoreValue StoreValue)
    {
        return StoreValue;
    }
    protected virtual void BaseAdd(string StoreKey, TStoreValue StoreValue)
    {
        if (string.IsNullOrWhiteSpace(StoreKey))
            throw new ArgumentException("StoreKey cannot be null or empty", nameof(StoreKey));

        if (StoreValue is null)
            throw new ArgumentNullException(nameof(StoreValue));

        StoreValue = AddProcess(StoreKey, StoreValue);

        Store.Remove(StoreKey);
        Store.Add(StoreKey, StoreValue);
    }
    protected virtual TStoreValue BaseGet(string StoreKey)
    {
        if (Store.TryGetValue(StoreKey, out var StoreValue))
            return StoreValue;

        return default;
    }
    protected virtual bool BaseTryGet(string StoreKey, out TStoreValue StoreValue)
    {
        if (Store.TryGetValue(StoreKey, out StoreValue))
            return true;

        return false;
    }
    protected virtual void BaseRemove(string StoreKey)
    {
        Store.Remove(StoreKey);
    }
    protected virtual void BaseAddFrom(TStore Source, IEnumerable<string> StoreKeys)
    {
        foreach (var StoreKey in StoreKeys)
        {
            var SlotValue = Source.Get(StoreKey);
            if (SlotValue is not null)
                Add(StoreKey, SlotValue);
        }
    }
    public IEnumerator<KeyValuePair<string, TStoreValue>> GetEnumerator()
    {
        return Store.GetEnumerator();
    }
    private TStore ConvertThis()
    {
        return (TStore)this;
    }
}
public abstract class StoreBase<TStoreValue, TStore> : StoreBase<Enum, TStoreValue, TStore>
    where TStore : StoreBase<Enum, TStoreValue, TStore>, new()
{
}
public class PvSlotsSet
{
    public string PvName { get; set; }
    public PropPassType PassType { get; set; }
    public object PassData { get; set; }
    public string SlotName { get; set; }
    public string Content { get; set; }
    public List<PvSlotsSet> MultiSlots { get; set; } = [];
    public PvAttrsSet Attrs { get; set; } = new PvAttrsSet();
    public IHtmlContent RenderContent => new HtmlString(Content);
    public bool HasContent() => !string.IsNullOrWhiteSpace(Content?.Trim());
    public PvSlotsSet() { }
    public PvSlotsSet(string SlotName, string Content)
    {
        this.SlotName = SlotName;
        this.Content = Content;
    }
    public PvSlotsSet WithFrom(PvSlotsSet Source)
    {
        WithFrom(Source, Source.PassType);
        return this;
    }
    public PvSlotsSet WithFrom(PvSlotsSet Source, PropPassType PassType)
    {
        if (PassType == PropPassType.Cover)
        {
            PvName = Source.PvName;
            SlotName = Source.SlotName;
            Content = Source.Content;
        }
        else if (PassType == PropPassType.Append)
        {
            Content += Source.Content;
        }
        else if (PassType == PropPassType.Fill)
        {
            if (string.IsNullOrWhiteSpace(Content?.Trim()))
                Content = Source.Content;
            if (string.IsNullOrWhiteSpace(PvName))
                PvName = Source.PvName;
        }
        else if (PassType == PropPassType.Multi)
        {
            MultiSlots.Add(Source);
        }
        return this;
    }
    public PvSlotsSet[] ToMulti()
    {
        var AllSlots = new List<PvSlotsSet> { this };
        AllSlots.AddRange(MultiSlots);
        return [.. AllSlots];
    }
    public string[] ToMultiContent()
    {
        var Result = ToMulti()
            .Select(Item => Item.Content)
            .Where(Item => !string.IsNullOrWhiteSpace(Item))
            .ToArray();

        return Result;
    }
    public IHtmlContent[] ToMultiRenderContent()
    {
        var Result = ToMultiContent()
            .Select(Item => new HtmlString(Item))
            .ToArray();

        return Result;
    }
}
public class PvSlotsStore : StoreBase<PvSlotsSet, PvSlotsStore>
{
    protected override void BaseAdd(string StoreKey, PvSlotsSet StoreValue)
    {
        var HasSlot = Store.ContainsKey(StoreKey);
        if (StoreValue.PassType == PropPassType.Cover)
            base.BaseAdd(StoreKey, StoreValue);
        else if (StoreValue.PassType == PropPassType.Fill && !HasSlot)
            base.BaseAdd(StoreKey, StoreValue);
        else if (StoreValue.PassType == PropPassType.Append)
        {
            if (!HasSlot)
                base.BaseAdd(StoreKey, StoreValue);
            else
            {
                var CurrentSlot = Get(StoreValue.SlotName);
                CurrentSlot.WithFrom(StoreValue, PropPassType.Append);
            }
        }
        else if (StoreValue.PassType == PropPassType.Multi)
        {
            if (!HasSlot)
                base.BaseAdd(StoreKey, StoreValue);
            else
            {
                var CurrentSlot = Get(StoreValue.SlotName);
                CurrentSlot.MultiSlots.Add(StoreValue);
            }
        }
    }
    protected override PvSlotsSet AddProcess(string StoreKey, PvSlotsSet StoreValue)
    {
        StoreValue.SlotName = StoreKey;
        return StoreValue;
    }
}
public class PvAttrsValue
{
    public string AttrValue { get; set; }
    public PropPassType PassType { get; set; } = PropPassType.Cover;
    public PvAttrsValue(string AttrValue, PropPassType PassType = PropPassType.Cover)
    {
        this.AttrValue = AttrValue;
        this.PassType = PassType;
    }
}
public class PvAttrsSet : StoreBase<PvAttrsValue, PvAttrsSet>
{
    protected override void BaseAdd(string StoreKey, PvAttrsValue StoreValue)
    {
        if (!BaseTryGet(StoreKey, out var GetValue) || StoreValue.PassType == PropPassType.Cover)
        {
            base.BaseAdd(StoreKey, StoreValue);
            return;
        }

        var Values = new[]
        {
            GetValue.AttrValue,
            StoreValue.AttrValue,
        };

        GetValue.AttrValue = string.Join(' ', Values);
    }
}
public class PvAttrsStore : StoreBase<PvAttrsSet, PvAttrsStore>
{
    protected override void BaseAdd(string StoreKey, PvAttrsSet StoreValue)
    {
        if (!BaseTryGet(StoreKey, out var GetAttrSet))
        {
            Store.Add(StoreKey, StoreValue);
            return;
        }

        GetAttrSet.AddFrom(StoreValue);
    }
}
public class PvLayoutStore : PvAttrsStore { }

public interface IPvGlobalSlotsStore { }
public class PvGlobalSlotsStore : IPvGlobalSlotsStore { }
public class PvGlobalSlotsStore<TPvs> : StoreBase<PvSlotsStore, PvGlobalSlotsStore<TPvs>>, IPvGlobalSlotsStore
    where TPvs : Enum
{
    public virtual PvGlobalSlotsStore<TPvs> Add(TPvs Pv, Enum SlotName, PvSlotsSet SlotSet)
    {
        if (!TryGet(Pv, out var SlotsStore))
        {
            SlotsStore = new PvSlotsStore();
            Add(Pv, SlotsStore);
        }

        SlotsStore.Add(SlotName, SlotSet);
        return this;
    }
    public virtual PvGlobalSlotsStore<TPvs> Add(TPvs Pv, Enum SlotName, string SlotValue, PropPassType PassType = PropPassType.Fill)
    {
        return Add(Pv, SlotName, new PvSlotsSet(SlotName.ToString(), SlotValue)
        {
            PassType = PassType,
        });
    }
    public virtual PvGlobalSlotsStore<TPvs> Add(TPvs Pv, Enum SlotName, PropPassType PassType = PropPassType.Fill)
    {
        return Add(Pv, SlotName, new PvSlotsSet(SlotName.ToString(), null)
        {
            PassType = PassType,
        });
    }
}
