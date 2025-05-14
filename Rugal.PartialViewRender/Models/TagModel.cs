namespace Rugal.PartialViewRender.Models;

public enum PvNodeType
{
    None,
    View,
    Slot,
    Tag,
    Attr,
}
public enum PropPassType
{
    Cover,
    Fill,
    Append,
    Multi,
}
public sealed class PvNode
{
    #region Value Property
    public string Id { get; set; }
    public int Depth { get; set; } = 0;
    public bool IsRoot { get; private set; }
    #endregion

    #region Object Property
    public PvPosition Position => new(Depth, Id);
    public List<PvNode> Children { get; set; } = [];
    public PvSlotsSet Slot { get; private set; } = new();
    public PvAttrsSet Attr { get; set; } = new();
    #endregion

    #region Node Property
    public string SlotName
    {
        get => Slot.SlotName;
        set => Slot.SlotName = value;
    }
    public PvNodeType NodeType { get; set; } = PvNodeType.None;
    public PvNode Parent { get; set; }
    public PvNode Root { get; set; }
    #endregion

    #region Public Method
    public PvNode InitRoot()
    {
        Root = this;
        IsRoot = true;
        return this;
    }
    public PvNode AddChildrenTo(PvNode Node, int Depth, string Id)
    {
        var QueryNode = Query(Depth, Id) ?? throw new Exception($"Node is not found at Depth {Depth}");

        Node.Parent = QueryNode;
        Node.Depth = Depth + 1;
        Node.Root = Root;
        QueryNode.Children.Add(Node);
        return this;
    }
    public PvNode Query(int Depth, string Id)
    {
        var Result = Rcs_Query(Root, Depth, Id);
        return Result;
    }
    public IEnumerable<PvNode> GetNodeList()
    {
        var Result = new List<PvNode>();
        RCS_GetNodeList(Root, Result);

        Result = [.. Result.OrderBy(Item => Item.Depth)];
        return Result;
    }
    #endregion

    #region Private Process
    private static PvNode Rcs_Query(PvNode Node, int Depth, string Id)
    {
        if (Node.Depth == Depth && Node.Id == Id)
            return Node;

        if (Node.Depth >= Depth)
            return null;

        foreach (var Item in Node.Children)
        {
            var TryQuery = Rcs_Query(Item, Depth, Id);
            if (TryQuery is not null)
                return TryQuery;
        }

        return null;
    }
    private static void RCS_GetNodeList(PvNode Node, List<PvNode> Output)
    {
        Output.Add(Node);
        foreach (var Item in Node.Children)
            RCS_GetNodeList(Item, Output);
    }
    #endregion
}
public class PvPosition
{
    public int Depth { get; set; }
    public string Id { get; set; }
    public PvPosition() { }
    public PvPosition(int Depth, string Id)
    {
        this.Depth = Depth;
        this.Id = Id;
    }
}