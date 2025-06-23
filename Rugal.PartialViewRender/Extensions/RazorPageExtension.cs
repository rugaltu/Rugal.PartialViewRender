using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Rugal.PartialViewRender.Models;

namespace Rugal.PartialViewRender.Extensions;

public static class RazorPageExtension
{
    public static PvSlotsSet LoadSlot<TSlot>(this RazorPage Page, TSlot Slot, bool CreateIfNull = true)
        where TSlot : Enum
    {
        if (TryLoadSlot(Page, Slot, out var OutSlot))
            return OutSlot;

        if (!CreateIfNull)
            return null;

        return SaveSlot(Page, Slot, new PvSlotsSet());
    }
    public static IHtmlContent LoadSlotRender<TSlot>(this RazorPage Page, TSlot Slot)
        where TSlot : Enum
    {
        if (!TryLoadSlotRender(Page, Slot, out var OutRender))
            return OutRender;

        return null;
    }
    public static bool TryLoadSlot<TSlot>(this RazorPage Page, TSlot Slot, out PvSlotsSet OutSlot)
    {
        if (Page.Context.Items.TryGetValue(Slot, out var GetSlot))
        {
            if (GetSlot is not PvSlotsSet Result)
                throw new Exception($"LoadSlots error: slot {Slot} value is not PvSlotsSet type");

            OutSlot = Result;
            return true;
        }

        OutSlot = null;
        return false;
    }
    public static bool TryLoadSlotRender<TSlot>(this RazorPage Page, TSlot Slot, out IHtmlContent RenderResult)
    {
        RenderResult = null;
        if (!TryLoadSlot(Page, Slot, out var OutSlot))
            return false;

        RenderResult = OutSlot.RenderContent;
        return true;
    }
    public static PvSlotsSet SaveSlot<TSlot>(this RazorPage Page, TSlot Slot, PvSlotsSet SetSlot)
    {
        Page.Context.Items[Slot] = SetSlot;
        return SetSlot;
    }
    public static bool TrySaveSlot<TSlots>(this RazorPage Page, TSlots Slot, PvSlotsSet SetSlot)
    {
        if (Page.Context.Items.ContainsKey(Slot))
            return false;

        SaveSlot(Page, Slot, SetSlot);
        return true;
    }
}