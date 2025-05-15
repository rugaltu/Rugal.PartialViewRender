using Microsoft.AspNetCore.Razor.TagHelpers;
using Rugal.PartialViewRender.Models;
using Rugal.PartialViewRender.TagBase;

namespace Rugal.PartialViewRender.Tags;

[HtmlTargetElement("pv-slot", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PvSlotTag : PvNodeTagBase
{
    public string PvName { get; set; }
    public PropPassType PassType { get; set; } = PropPassType.Cover;
    public IPvOption ExportOption { get; set; }
    public PvSlotsSet ExportSlot { get; set; }
    public PvAttrsSet ExportAttr { get; set; }
    public object PassData { get; set; }
    public Enum Slot { get; set; }
    public PvSlotTag(IServiceProvider Provider) : base(Provider) { }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        if (Slot is null)
            return;

        InitSlot(PassType);
        InitAttributes(PassType);
        InitChildrenAttributes();

        ExportOption?.Slots.Add(Slot, Node.Slot);
        ExportOption?.Attrs.Add(Slot, Node.Attr);
        ExportSlot?.WithFrom(Node.Slot);
        ExportAttr?.AddFrom(Node.Attr);

        ClearContent(output);
    }
    protected override void BeforeChildBuild()
    {
        base.BeforeChildBuild();
        Node.NodeType = PvNodeType.Slot;
        Node.SlotName = Slot.ToString();
    }
    protected virtual void InitSlot(PropPassType PassType)
    {
        Node.Slot.PassType = PassType;
        Node.Slot.PassData = PassData;
        Node.Slot.Attrs = Node.Attr;
        if (!string.IsNullOrWhiteSpace(Content))
        {
            Node.Slot.Content = Content;
        }
    }

}

[HtmlTargetElement("pv-tag", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PvTag : PvNodeTagBase
{
    public string PvName { get; set; }
    public string Tag { get; set; }
    public PvAttrsSet PassAttr { get; set; }
    public PvSlotsSet PassSlot { get; set; }
    public PvTag(IServiceProvider Provider) : base(Provider) { }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        Node.NodeType = PvNodeType.Tag;

        InitChildrenAttributes();

        if (!string.IsNullOrWhiteSpace(PvName))
            Node.Attr.Add("pv-name", new PvAttrsValue(PvName, PropPassType.Cover));

        RenderAttributes(Node.Attr);
        RenderAttributes(PassAttr);
        RenderSlot(PassSlot);

        Tag ??= "section";
        output.TagName = Tag;
    }
}

[HtmlTargetElement("pv-attr", TagStructure = TagStructure.WithoutEndTag)]
public class PvAttrTag : PvNodeTagBase
{
    public PvAttrTag(IServiceProvider Provider) : base(Provider) { }
    public PropPassType PassType { get; set; }
    public PvAttrsSet PassAttr { get; set; }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        Node.NodeType = PvNodeType.Attr;

        if (PassAttr is not null)
            Node.Attr.AddFrom(PassAttr);

        InitAttributes(PassType);
        output.TagName = null;
    }
}

[HtmlTargetElement("pv-layout", TagStructure = TagStructure.WithoutEndTag)]
public class PvLayoutTag : PvNodeTagBase
{
    public PropPassType PassType { get; set; } = PropPassType.Cover;
    public PvLayoutTag(IServiceProvider Provider) : base(Provider) { }
    protected override void Setup()
    {
        base.Setup();
        ExceptPassAttribute.Add("layout");
    }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        InitAttributes(PassType);

        if (Node.Attr.Any())
            LayoutStore.Add(Layout, Node.Attr);

        output.TagName = null;
    }
}