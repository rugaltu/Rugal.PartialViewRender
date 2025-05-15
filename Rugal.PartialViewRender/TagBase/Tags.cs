using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rugal.PartialViewRender.Models;
using Rugal.PartialViewRender.Services;
using System.Text.RegularExpressions;

namespace Rugal.PartialViewRender.TagBase;

public abstract class PvNodeTagBase : TagHelper
{
    #region DI Service
    protected PvLayoutStore LayoutStore { get; private set; }
    protected IServiceProvider Provider { get; private set; }
    protected IWebHostEnvironment Env { get; private set; }
    #endregion

    #region Public Property
    public Enum Layout { get; set; }
    #endregion

    #region Key Stoer
    protected List<string> ExceptPassAttribute { get; private set; }
    #endregion

    #region Property
    protected TagHelperContext Context { get; private set; }
    protected TagHelperOutput Output { get; private set; }
    public TagHelperContent ChildrenContent { get; protected set; }
    public string Content { get; protected set; }
    public int Depth => Node.Depth;
    public string Id => Node.Id;
    #endregion

    #region Node Property
    protected PvNode Node { get; set; }
    protected PvNode Parent => Node?.Parent;
    protected IEnumerable<PvNode> Children => Node.Children;
    protected PvNode Tree => GetTree();
    protected PvNode CurrentPosition => GetCurrentPosition();
    #endregion

    public PvNodeTagBase(IServiceProvider Provider)
    {
        this.Provider = Provider;
        LayoutStore = this.Provider.GetRequiredService<PvLayoutStore>();
        Env = this.Provider.GetRequiredService<IWebHostEnvironment>();
    }
    public override async void Process(TagHelperContext context, TagHelperOutput output)
    {
        Context = context;
        Output = output;

        ExceptPassAttribute = [];

        Setup();
        InitNode();
        InitLayout();
        SetItem(PvEnvs.KEY_NODE_CURRENT_POSITION, Node);
        BeforeChildBuild();
        ChildrenContent = await output.GetChildContentAsync();
        Content = ChildrenContent.GetContent().Trim();

        if (!Node.IsRoot)
            SetItem(PvEnvs.KEY_NODE_CURRENT_POSITION, Parent);

        base.Process(context, output);
    }

    #region Protected Method
    protected virtual void Setup()
    {
        var DefaultExceptPass = new[]
        {
            "^pv-.*",
            "^pass-.*",
            "^slot$",
        };
        ExceptPassAttribute.AddRange(DefaultExceptPass);

        if (Env.IsDevelopment())
        {
            Output.Attributes.Add("pv-tag", Output.TagName);
            if (Layout is not null)
                Output.Attributes.Add("pv-layout", Layout);
        }

        Output.TagName = "section";
    }
    protected virtual void BeforeChildBuild() { }
    protected virtual void InitNode()
    {
        Node = new PvNode()
        {
            Id = Context.UniqueId,
        };

        if (HasItem(PvEnvs.KEY_NODE_TREE))
        {
            Tree.AddChildrenTo(Node, CurrentPosition.Depth, CurrentPosition.Id);
            return;
        }

        Node.InitRoot();
        SetItem(PvEnvs.KEY_NODE_TREE, Node);
    }
    protected virtual void InitAttributes(PropPassType DefualtPassType)
    {
        foreach (var Attr in Context.AllAttributes)
        {
            var IsAnyMatch = ExceptPassAttribute
                .Any(Item => Regex.IsMatch(Attr.Name, Item));

            if (IsAnyMatch)
                continue;

            var NewAttrValue = new PvAttrsValue(Attr.Value.ToString(), DefualtPassType);
            Node.Attr.Add(Attr.Name, NewAttrValue);
        }
    }
    protected virtual void InitLayout()
    {
        if (Layout is null)
            return;

        if (!LayoutStore.TryGet(Layout, out var LayoutValue))
            return;

        Node.Attr.AddFrom(LayoutValue);
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
    protected virtual void RenderAttributes(PvAttrsSet RenderAttr)
    {
        if (RenderAttr is null || !RenderAttr.Any())
            return;

        foreach (var Attr in RenderAttr)
        {
            var AttrName = Attr.Key;

            if (Attr.Value.PassType == PropPassType.Fill && Output.Attributes.ContainsName(AttrName))
                continue;

            var MergeAttrs = new List<string>();
            if (Attr.Value.PassType == PropPassType.Append)
            {
                if (Output.Attributes.TryGetAttribute(AttrName, out var GetAttr))
                    MergeAttrs.Add(GetAttr.Value.ToString());
            }

            MergeAttrs.Add(Attr.Value.AttrValue);
            var SetAttrsValue = string.Join(' ', MergeAttrs);

            Output.Attributes.RemoveAll(AttrName);
            Output.Attributes.Add(AttrName, SetAttrsValue);
        }
    }
    protected virtual void RenderSlot(PvSlotsSet RenderSlot)
    {
        if (RenderSlot is null || !RenderSlot.HasContent())
            return;

        if (RenderSlot.PassType == PropPassType.Cover)
        {
            Output.Content.SetHtmlContent(RenderSlot.Content);
            return;
        }

        Output.Content.AppendHtml(Content);
        Output.Content.AppendHtml(RenderSlot.Content);
    }
    protected virtual void ClearContent(TagHelperOutput Output)
    {
        Output.TagName = null;
        ChildrenContent = null;
        Content = null;
        Output.Content.SetContent(null);
    }
    #endregion


    #region Item Control
    protected virtual TModel GetItem<TModel>(string Key) where TModel : class
    {
        if (Context is null)
            throw new Exception("Context is null, please get tree in/after Process() method");

        if (!Context.Items.ContainsKey(Key))
            return null;

        var Result = Context.Items[Key] as TModel;
        return Result;
    }
    protected virtual bool HasItem(string Key)
    {
        if (Context is null)
            throw new Exception("Context is null, please get tree in/after Process() method");

        return Context.Items.ContainsKey(Key);
    }
    protected virtual void SetItem<TModel>(string Key, TModel Object)
    {
        if (Context is null)
            throw new Exception("Context is null, please get tree in/after Process() method");

        if (Context.Items.ContainsKey(Key))
            Context.Items.Remove(Key);

        Context.Items.Add(Key, Object);
    }
    #endregion

    #region Get Property Function
    private PvNode GetTree()
    {
        var Result = GetItem<PvNode>(PvEnvs.KEY_NODE_TREE);
        if (Result is null)
            throw new Exception($"{PvEnvs.KEY_NODE_TREE} is null, please get tree after Init() method");

        return Result;
    }
    private PvNode GetCurrentPosition()
    {
        var Result = GetItem<PvNode>(PvEnvs.KEY_NODE_CURRENT_POSITION);
        if (Result is null)
            throw new Exception($"{PvEnvs.KEY_NODE_CURRENT_POSITION} is null, please get tree after Init() method");

        return Result;
    }
    #endregion
}
public abstract class PvTagBase<TPvs> : PvNodeTagBase where TPvs : Enum
{
    private readonly PvRender<TPvs> Render;
    public TPvs PvType { get; set; }
    public string PvName { get; set; }
    public PvSlotsStore PassSlot { get; set; }
    public PvAttrsStore PassAttr { get; set; }
    public PvTagBase(IServiceProvider Provider) : base(Provider)
    {
        Render = base.Provider.GetRequiredService<PvRender<TPvs>>();
    }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        Node.NodeType = PvNodeType.View;

        var RenderView = Render.Create(PvType)
            .SetOption(Option =>
            {
                Option.WithPvName(PvName);
                SetChildrenOption(Option);
                SetPassOption(Option);
                SetDefaultContent(Option);
            });

        var RenderContent = RenderView.Content;
        PvName = RenderView.Option.PvName;
        if (!string.IsNullOrWhiteSpace(PvName))
            Node.Attr.Add("pv-name", new PvAttrsValue(PvName, PropPassType.Cover));

        RenderAttributes(RenderView.Option.ParentAttrs);
        if (RenderView.Option.ParentTag is not null)
            Output.TagName = RenderView.Option.ParentTag;

        RenderAttributes(Node.Attr);
        output.Content.SetHtmlContent(RenderContent);
    }
    protected override void Setup()
    {
        base.Setup();
        if (Env.IsDevelopment())
            Output.Attributes.Add("pv-type", PvType);
    }
    private PvOption<TPvs> SetChildrenOption(PvOption<TPvs> Option)
    {
        foreach (var OptionNode in Children)
        {
            if (OptionNode.NodeType != PvNodeType.Slot || OptionNode.SlotName is null)
                continue;

            var AddSlot = OptionNode.Slot;
            var AddAttr = OptionNode.Attr;
            Option.Slots.Add(AddSlot.SlotName, AddSlot);

            if (AddAttr.Any())
                Option.Attrs.Add(OptionNode.SlotName, AddAttr);
        }
        return Option;
    }
    private PvOption<TPvs> SetPassOption(PvOption<TPvs> Option)
    {
        if (PassSlot is not null)
            Option.Slots.AddFrom(PassSlot);

        if (PassAttr is not null)
            Option.Attrs.AddFrom(PassAttr);

        return Option;
    }
    private PvOption<TPvs> SetDefaultContent(PvOption<TPvs> Option)
    {
        if (string.IsNullOrWhiteSpace(Content))
            return Option;

        Option.Slots.Add(PvEnvs.KEY_DEFAULT_SLOT, new PvSlotsSet(PvEnvs.KEY_DEFAULT_SLOT, Content));
        return Option;
    }
}