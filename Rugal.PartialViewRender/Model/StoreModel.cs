namespace Rugal.PartialViewRender.Model
{
    public abstract class StoreBase<TStoreValue, TStore> where TStore : StoreBase<TStoreValue, TStore>, new()
    {
        protected Dictionary<string, TStoreValue> Store { get; set; } = [];
        public virtual TStore Add(string StoreKey, TStoreValue StoreValue)
        {
            if (string.IsNullOrWhiteSpace(StoreKey))
                throw new ArgumentException("StoreKey cannot be null or empty", nameof(StoreKey));

            if (StoreValue is null)
                throw new ArgumentNullException(nameof(StoreValue));

            StoreValue = AddProcess(StoreKey, StoreValue);
            BaseAdd(StoreKey, StoreValue);

            return ConvertThis();
        }
        public virtual TStore Add(Enum StoreKey, TStoreValue StoreValue)
        {
            Add(StoreKey.ToString(), StoreValue);
            return ConvertThis();
        }
        public virtual TStore AddFrom(TStore Source)
        {
            foreach (var Slot in Source)
                Add(Slot.Key, Slot.Value);
            return ConvertThis();
        }
        public virtual TStore AddFrom(TStore Source, params string[] StoreKeys)
        {
            foreach (var StoreKey in StoreKeys)
            {
                var SlotValue = Source.Get(StoreKey);
                if (SlotValue is not null)
                    Add(StoreKey, SlotValue);
            }
            return ConvertThis();
        }
        public virtual TStore AddFrom(TStore Source, params Enum[] StoreKeys)
        {
            var QueryKeys = StoreKeys
                .Select(Item => Item.ToString())
                .ToArray();

            AddFrom(Source, QueryKeys);
            return ConvertThis();
        }
        public virtual TStore AddFrom<TSlotName>(TStore Source, params TSlotName[] StoreKeys)
            where TSlotName : Enum
        {
            var QueryKeys = StoreKeys
                .Select(Item => Item.ToString())
                .ToArray();

            AddFrom(Source, QueryKeys);
            return ConvertThis();
        }
        public virtual TStoreValue Get(string StoreKey)
        {
            if (Store.TryGetValue(StoreKey, out var StoreValue))
                return StoreValue;

            return default;
        }
        public virtual TStoreValue Get(Enum StoreKey)
        {
            if (Store.TryGetValue(StoreKey.ToString(), out var StoreValue))
                return StoreValue;

            return default;
        }
        public virtual bool TryGet(string StoreKey, out TStoreValue StoreValue)
        {
            if (Store.TryGetValue(StoreKey, out StoreValue))
                return true;

            return false;
        }
        public virtual bool TryGet(Enum StoreKey, out TStoreValue StoreValue)
        {
            if (Store.TryGetValue(StoreKey.ToString(), out StoreValue))
                return true;

            return false;
        }
        public virtual TStore Remove(string StoreKey)
        {
            Store.Remove(StoreKey);
            return ConvertThis();
        }
        public TStore Query(params Enum[] SlotNames)
        {
            var Result = new TStore()
                .AddFrom(ConvertThis(), SlotNames);
            return Result;
        }
        public TStore Query<TSlotName>(params TSlotName[] SlotNames)
            where TSlotName : Enum
        {
            var Result = new TStore()
                .AddFrom(ConvertThis(), SlotNames);
            return Result;
        }
        public virtual bool Any() => Store.Count != 0;
        public virtual bool ContainsKey(string StoreKey) => Store.ContainsKey(StoreKey);
        protected virtual TStoreValue AddProcess(string StoreKey, TStoreValue StoreValue)
        {
            return StoreValue;
        }
        protected virtual void BaseAdd(string StoreKey, TStoreValue StoreValue)
        {
            Store.Remove(StoreKey);
            Store.Add(StoreKey, StoreValue);
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
    public class PvSlotsSet
    {
        public PropPassType PassType { get; set; }
        public string SlotName { get; set; }
        public string Content { get; set; }
        public bool HasContent() => Content is not null;
    }
    public class PvSlotsStore : StoreBase<PvSlotsSet, PvSlotsStore> { }
    public class PvAttrsValue
    {
        public string AttrValue { get; set; }
        public PropPassType PassType { get; set; } = PropPassType.Cover;
        public PvAttrsValue(string _AttrValue, PropPassType _PassType = PropPassType.Cover)
        {
            AttrValue = _AttrValue;
            PassType = _PassType;
        }
    }
    public class PvAttrsSet : StoreBase<PvAttrsValue, PvAttrsSet>
    {
        protected override void BaseAdd(string StoreKey, PvAttrsValue StoreValue)
        {
            if (!TryGet(StoreKey, out var GetValue) || StoreValue.PassType == PropPassType.Cover)
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
            if (!TryGet(StoreKey, out var GetAttrSet))
            {
                Store.Add(StoreKey, StoreValue);
                return;
            }

            GetAttrSet.AddFrom(StoreValue);
        }
    }
    public class PvLayoutStore : PvAttrsStore { }
}