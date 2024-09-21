using Microsoft.AspNetCore.Razor.TagHelpers;
using Rugal.PartialViewRender.Model;
using Rugal.PartialViewRender.TagBase;

namespace Rugal.PartialViewRender.Tags
{
    [HtmlTargetElement("pv-slot", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PvSlotTag : PvNodeTagBase
    {
        public PropPassType PassType { get; set; }
        public Enum Slot { get; set; }
        public PvSlotTag(IServiceProvider Provider) : base(Provider) { }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            if (Slot is null)
                return;

            InitSlot();
            InitAttributes(PassType);
            InitChildrenAttributes();
        }
        protected override void BeforeChildBuild()
        {
            base.BeforeChildBuild();
            Node.NodeType = PvNodeType.Slot;

            if (PassType == PropPassType.None)
                PassType = PropPassType.Cover;

            Node.PassType = PassType;
            Node.SlotName = Slot.ToString();
        }
        protected virtual void InitSlot()
        {
            if (!string.IsNullOrWhiteSpace(Content))
                Node.Slot.Content = Content;
        }
        protected virtual void InitChildrenAttributes()
        {
            foreach (var OptionNode in Children)
            {
                if (OptionNode.NodeType != PvNodeType.Attr)
                    continue;

                if (OptionNode.Attr is null)
                    continue;

                Node.Attr.AddFrom(OptionNode.Attr);
            }
        }
    }

    [HtmlTargetElement("pv-tag", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PvTag : PvNodeTagBase
    {
        public string Tag { get; set; }
        public PvAttrsSet PassAttr { get; set; }
        public PvSlotsSet PassSlot { get; set; }
        public PvTag(IServiceProvider _Provider) : base(_Provider) { }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            Node.NodeType = PvNodeType.Tag;

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
        public PvAttrTag(IServiceProvider _Provider) : base(_Provider) { }
        public PropPassType PassType { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            Node.NodeType = PvNodeType.Attr;

            if (PassType == PropPassType.None)
            {
                PassType = Parent.PassType switch
                {
                    PropPassType.AppendAll => PropPassType.AppendAll,
                    PropPassType.CoverAll => PropPassType.CoverAll,
                    _ => PropPassType.Cover
                };
            }

            InitAttributes(PassType);

            output.TagName = null;
        }
    }

    [HtmlTargetElement("pv-layout", TagStructure = TagStructure.WithoutEndTag)]
    public class PvLayoutTag : PvNodeTagBase
    {
        public PropPassType PassType { get; set; } = PropPassType.Cover;
        public PvLayoutTag(IServiceProvider _Provider) : base(_Provider) { }
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
}