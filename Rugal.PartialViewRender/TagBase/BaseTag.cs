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
    protected const string KEY_TREE = "Tree";
    protected const string KEY_CURRENT_POSITION = "CurrentPosition";
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
        SetItem(KEY_CURRENT_POSITION, Node);
        BeforeChildBuild();
        ChildrenContent = await output.GetChildContentAsync();
        Content = ChildrenContent.GetContent().Trim();

        if (!Node.IsRoot)
            SetItem(KEY_CURRENT_POSITION, Parent);

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

        if (HasItem(KEY_TREE))
        {
            Tree.AddChildrenTo(Node, CurrentPosition.Depth, CurrentPosition.Id);
            return;
        }

        Node.InitRoot();
        SetItem(KEY_TREE, Node);
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
    protected virtual void RenderAttributes(PvAttrsSet RenderAttr)
    {
        if (RenderAttr is null || !RenderAttr.Any())
            return;

        foreach (var Attr in RenderAttr)
        {
            var AttrName = Attr.Key;
            var MergeAttrs = new List<string>();
            if (AnyAppend(Attr.Value.PassType))
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

        if (AnyCover(RenderSlot.PassType))
        {
            Output.Content.SetHtmlContent(RenderSlot.Content);
            return;
        }

        Output.Content.AppendHtml(Content);
        Output.Content.AppendHtml(RenderSlot.Content);
    }
    #endregion

    #region Protected Process
    protected virtual bool AnyCover(PropPassType PassType)
    {
        return PassType == PropPassType.Cover || PassType == PropPassType.CoverAll;
    }
    protected virtual bool AnyAppend(PropPassType PassType)
    {
        return PassType == PropPassType.Append || PassType == PropPassType.AppendAll;
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
        var Result = GetItem<PvNode>(KEY_TREE);
        if (Result is null)
            throw new Exception($"{KEY_TREE} is null, please get tree after Init() method");

        return Result;
    }
    private PvNode GetCurrentPosition()
    {
        var Result = GetItem<PvNode>(KEY_CURRENT_POSITION);
        if (Result is null)
            throw new Exception($"{KEY_CURRENT_POSITION} is null, please get tree after Init() method");

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
            });

        RenderAttributes(Node.Attr);
        output.Content.SetHtmlContent(RenderView.Content);
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

            if (OptionNode.Slot.HasContent())
                Option.Slots.Add(OptionNode.SlotName, OptionNode.Slot);

            if (OptionNode.Attr.Any())
                Option.Attrs.Add(OptionNode.SlotName, OptionNode.Attr);
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
}