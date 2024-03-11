//#region Pv Model
class PvNode {
    constructor(_Element, _PvType = null, _Parent = null) {

        this.Id = this._GenerateId();
        this.PvType = _PvType;
        this.Name = null;
        this.OutName = null;
        this.InName = null;
        this.SlotName = null;

        this.Parent = _Parent;
        this.Element = _Element;
        this.Nodes = [];

        this._InitName();
    }

    //#region Get Property
    get AbsPath() {
        let Nodes = [];
        let FindNode = this;
        while (FindNode instanceof PvRender == false && FindNode != null) {
            Nodes.push(FindNode);
            FindNode = FindNode.Parent;
        }
        let Result = Nodes
            .filter(Item => Item.Name != null)
            .reverse()
            .map(Item => Item.Name)
            .join('.');
        return Result;
    }
    get Content() {
        if (this.Element == null)
            return null;

        return this.Element.innerHTML;
    }
    get FullContent() {
        if (this.Element == null)
            return null;

        return this.Element.outerHTML;
    }
    get Children() {
        let Children;
        if (this.IsTemplate)
            Children = this.Element.content.children;
        else
            Children = this.Element.children;
        return Children;
    }
    get Attrs() {
        return [...this.Element.attributes];
    }
    get AttrsNotPv() {
        let Exp = new RegExp(/^pv-/);
        return this.Attrs.filter(Item => !Exp.test(Item.name));
    }
    get NamePaths() {
        if (this.Name == null) {
            debugger;
            return null;
        }
        return this.Name.split('.');
    }
    get InPaths() {
        if (this.InName == null)
            return null;
        return this.InName.split('.');
    }
    get SlotPaths() {
        if (this.SlotName == null)
            return null;
        return this.SlotName.split('.');
    }

    get Pvs() {
        return this.NextTree('pv-name');
    }
    get Outs() {
        return this.NextTree('pv-name').filter(Item => Item.IsPvOut);
    }
    get Ins() {
        return this.NextTree('pv-in');
    }
    get Slots() {
        return this.NextTree('pv-slot');
    }
    //#endregion

    //#region Get Boolean Property
    get HasSlots() {
        return this.Slots.length > 0;
    }
    get HasIns() {
        return this.Ins.length > 0;
    }
    get HasContent() {
        if (this.Content == null)
            return false;

        return this.Content != '' && this.Content != null;
    }
    get HasTextNodes() {
        let FindElement = this.IsTemplate ? this.Element.content : this.Element;
        let ChildNodes = FindElement.childNodes;
        let FindAny = [...ChildNodes]
            .filter(Item => Item.data != null)
            .filter(Item => Item.data
                .replaceAll(' ', '')
                .replaceAll('\n', '').length > 0);

        let Result = FindAny.length > 0;
        return Result;
    }
    get HasNodes() {
        return this.Nodes.length > 0;
    }
    get HasAttrs() {
        return this.Attrs.length > 0;
    }
    get HasAttrsNotPv() {
        return this.AttrsNotPv.length > 0;
    }
    get IsTemplate() {
        return this.Element.tagName == 'TEMPLATE';
    }
    get IsPvName() {
        return this.IsMatch('pv-name');
    }
    get IsPvOut() {
        return this.IsMatch('pv-out');
    }
    get IsPvIn() {
        return this.IsMatch('pv-in');
    }
    get IsPvSlot() {
        return this.IsMatch('pv-slot');
    }
    //#endregion

    //#region Init Method
    _InitName() {
        if (this.Element == null || this.PvType == null)
            return;

        this.Name = this.GetAttr('pv-name');
        if (this.IsPvIn) {
            this.InName = this.GetAttr('pv-in');
            if (this.Name == null || this.Name == '')
                this.Name = this.InName;
        }

        if (this.IsPvOut) {
            this.OutName = this.GetAttr('pv-out');
            if (this.OutName == null || this.OutName == '')
                this.OutName = this.Name;
        }

        if (this.IsPvSlot) {
            this.SlotName = this.GetAttr('pv-slot');
            if (this.Name == null || this.Name == '')
                this.Name = this.SlotName;
        }
        this.SetAttr('_NodeId', this.Id);
    }
    //#endregion

    //#region Public Method
    CloneTo(Target) {
        if (Target instanceof PvNode)
            Target.Nodes.push(...this.Nodes);
        return this;
    }
    CloneFrom(Source) {
        if (Source instanceof PvNode)
            this.Nodes.push(...Source.Nodes);
        return this;
    }
    IsMatch(...PvTypes) {
        if (this.Element == null)
            return false;

        PvTypes ??= [this.PvType];
        if (PvTypes == null)
            return false;

        let FindType = PvTypes
            .filter(Item => Item != null && Item != '')
            .map(Item => `[${Item}]`)
            .join(',');

        return this.Element.matches(FindType);
    }
    NextTree(...PvTypes) {
        let Result = [];
        for (let Item of this.Nodes) {
            let Tree = this._RCS_NextTree(Item, PvTypes);
            Result.push(...Tree);
        }

        return Result;
    }
    //#endregion

    //#region With Method
    WithParent(_Parent) {
        this.Parent = _Parent;
        return this;
    }
    WithPvType(_PvType) {
        this.PvType = _PvType;
        this._InitName();
        return this;
    }
    //#endregion

    //#region Set And Get Method
    SetContent(...SourceNodes) {
        this.Element.innerHTML = null;
        for (let Item of SourceNodes)
            this.Element.innerHTML += Item.Content;
        return this;
    }
    SetContentNode(...SourceNodes) {
        this.Element.innerHTML = null;
        for (let Item of SourceNodes) {
            if (!this.IsTemplate) {
                let CloneElement = Item.Element.cloneNode(true);
                this.Element.append(CloneElement);
            }
            else
                this.Element.innerHTML += Item.FullContent;
        }
        return this;
    }

    SetAttr(Name, Value) {
        this.Element.setAttribute(Name, Value);
        return this;
    }
    GetAttr(Name) {
        return this.Element.getAttribute(Name);
    }
    //#endregion

    //#region Private Process
    _GenerateId() {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }
    //#endregion

    //#region Private RCS Common Process
    _RCS_VisitNode(TargetNode, VisitFunc) {
        let IsNext = VisitFunc(TargetNode);
        if (IsNext == true) {
            for (let Item of TargetNode.Nodes) {
                this._RCS_VisitNode(Item, VisitFunc);
            }
        }
    }
    _RCS_NextTree(TargetNode, PvTypes) {
        let Result = [];
        if (TargetNode.IsMatch(...PvTypes)) {
            Result.push(TargetNode);
            return Result;
        }
        for (let Item of TargetNode.Nodes) {
            let NodeResult = this._RCS_NextTree(Item, PvTypes);
            Result.push(...NodeResult);
        }
        return Result;
    }
    //#endregion

    //#region Log
    //#endregion
}
//#endregion

/**
 *  PvRender.js v1.1.5
 *  From Rugal Tu
 * */
class PvRender {
    constructor() {
        this.Nodes = [];
        this._Init();
    }

    //#region Get Property
    get NodeList() {
        let Result = [];
        for (let NodeItem of this.Nodes)
            NodeItem._RCS_VisitNode(NodeItem, Item => {
                Result.push(Item);
                return true;
            });
        return Result;
    }
    get Pvs() {
        return this.NextTree('pv-name');
    }
    get Outs() {
        return this.NextTree('pv-name').filter(Item => Item.IsPvOut);
    }
    get Ins() {
        return this.NextTree('pv-in');
    }
    get Slots() {
        return this.NextTree('pv-slot');
    }
    //#endregion

    //#region Init Method
    _Init() {
        document.addEventListener('DOMContentLoaded', () => {
            this._InitTree();
            this._SetTree();
        });
    }
    //#endregion

    //#region Build Tree
    _InitTree() {
        let AllTypes = ['pv-name', 'pv-in', 'pv-slot'];
        let AllQuery = [];
        for (let Item of AllTypes) {
            let NotInPvName = `not([pv-name] [${Item}])`;
            let Query = `[${Item}]:${NotInPvName}`;
            AllQuery.push(Query);
        }

        this.Nodes.length = 0;
        let RootNodes = document.querySelectorAll(AllQuery.join(','));
        for (let Item of RootNodes) {
            let Tree = this._RCS_BuildTree(Item);
            this.Nodes.push(Tree);
        }
        return this.Nodes;
    }
    _RCS_BuildTree(Target, Parent = null) {
        let RootNode = this._SwitchNode(Target, Parent);
        this._RCS_BuildChildren(RootNode);
        return RootNode;
    }
    _RCS_BuildChildren(RootNode) {
        RootNode.Nodes.length = 0;
        for (let Child of RootNode.Children) {
            let ChildNode = this._RCS_BuildTree(Child, RootNode);
            RootNode.Nodes.push(ChildNode);
        }
    }
    _SwitchNode(Target, Parent) {
        let Result = new PvNode(Target)
            .WithParent(Parent);

        if (Result.IsPvName || Result.IsPvOut)
            Result.WithPvType('pv-name');

        if (Result.IsPvIn)
            Result.WithPvType('pv-in');

        if (Result.IsPvSlot)
            Result.WithPvType('pv-slot');

        return Result;
    }
    NextTree(...PvTypes) {
        let Result = [];
        for (let Item of this.Nodes) {
            if (Item.IsMatch(...PvTypes)) {
                Result.push(Item);
                continue;
            }

            let NodeResult = Item.NextTree(...PvTypes);
            if (NodeResult.length > 0)
                Result.push(...NodeResult);
        }
        return Result;
    }
    //#endregion

    //#region Set Tree
    _SetTree() {
        let PvTypes = ['pv-slot', 'pv-in'];
        for (let Item of this.NextTree(...PvTypes)) {
            this._RCS_SetTree(Item, PvTypes);
        }
    }
    _RCS_SetTree(TargetNode, PvTypes) {

        let FindNode = this._FindNode(TargetNode);
        if (FindNode == null)
            return;

        if (TargetNode.Nodes.length > 0) {
            let ContentNodes = TargetNode.Nodes
                .filter(Item => !Item.IsMatch(...PvTypes));

            if (ContentNodes.length > 0)
                FindNode.SetContentNode(...ContentNodes);
            else {
                for (let Item of TargetNode.NextTree(...PvTypes))
                    this._RCS_SetTree(Item, PvTypes);
            }
        }
        else if (TargetNode.Content && TargetNode.Content != '')
            FindNode.SetContent(TargetNode);

        this._SetNodeAttr(FindNode, TargetNode);
        this._RCS_BuildChildren(FindNode);

        for (let Item of FindNode.NextTree(...PvTypes))
            this._RCS_SetTree(Item, PvTypes);
    }
    _FindNode(TargetNode) {
        let TargetPathNodes = [];
        let FindNode = TargetNode;

        while (FindNode instanceof PvNode && (FindNode.IsPvIn || FindNode.IsPvSlot)) {
            TargetPathNodes.push(FindNode);
            FindNode = FindNode?.Parent ?? this;
        }

        while (FindNode && TargetPathNodes.length > 0) {
            let NextPathNode = TargetPathNodes.pop();
            let NextPaths = NextPathNode.NamePaths;
            if (NextPathNode.IsPvIn)
                NextPaths = NextPathNode.InPaths;
            else if (NextPathNode.IsPvSlot)
                NextPaths = NextPathNode.SlotPaths;

            while (FindNode && NextPaths.length > 0) {
                let NextName = NextPaths.shift();
                if (NextPathNode.IsPvSlot || FindNode instanceof PvRender) {
                    FindNode = FindNode
                        .NextTree('pv-name')
                        .find(Item => Item.IsMatch(`pv-name="${NextName}"`));
                }
                else if (NextPathNode.IsPvIn) {
                    let NextNodes = FindNode
                        .NextTree('pv-out');

                    FindNode = NextNodes
                        .find(Item => Item.OutName == NextName || Item.IsMatch(`pv-out="${NextName}"`));
                }
            }
        }
        return FindNode;
    }
    _SetNodeAttr(TargetNode, SourceNode) {
        if (TargetNode == null)
            return;
        for (let Item of SourceNode.Attrs) {
            let AttrName = Item.name;
            let AttrValue = Item.value;
            if (AttrName.includes('pv-'))
                continue;

            if (AttrName.includes('.')) {
                let Names = AttrName.split('.');
                let Action = Names.pop();
                AttrName = Names.join('.');

                let TargetAttrValue = TargetNode.GetAttr(AttrName);
                AttrValue = this._TransAttrValue(TargetAttrValue, Action, AttrName, AttrValue);
            }
            TargetNode.SetAttr(AttrName, AttrValue);
        }
    }
    _TransAttrValue(TargetValue, Action, AttrName, AttrValue) {
        let SplitReg = null;
        let JoinChar = '';
        switch (AttrName) {
            case 'style':
                SplitReg = /\s*;\s*/;
                JoinChar = '; ';
                break;
            default:
                SplitReg = /\s+/;
                JoinChar = ' ';
                break;
        }

        TargetValue ??= '';

        let TargetValues = TargetValue
            .split(SplitReg)
            .filter(Val => Val != '' && Val != null);

        AttrValue.split(SplitReg)
            .filter(Val => Val != '' && Val != null)
            .forEach(Item => {
                TargetValues = this._TransActionAttrValue(Action, Item, TargetValues);
            });

        let ValueResult = TargetValues.join(JoinChar);
        return ValueResult;
    }
    _TransActionAttrValue(Action, SourceValue, TargetValues) {
        switch (Action.toLowerCase()) {
            case 'add':
                if (!TargetValues.includes(SourceValue))
                    TargetValues.push(SourceValue);
                break;
            case 'remove':
                let RemoveIndex = TargetValues
                    .findIndex(Val => Val == SourceValue);
                if (RemoveIndex >= 0)
                    TargetValues.splice(RemoveIndex, 1);
                break;
            default:
                this._Log_UnknowAttrAction(Action);
                break;
        }
        return TargetValues;
    }
    //#endregion

    //#region Tree Viewer
    View(Mode = 0) {
        for (let Item of this.Nodes) {
            let NodeResult = this._RCS_View(Item, Mode);
            let JsonResult = JSON.stringify(NodeResult, null, 2)
                .replaceAll('"', '')
                .replace(/^{\n/, '')
                .replace(/}$/, '')
                .replaceAll(',\n', ',\n\n');

            console.log(JsonResult);
        }
    }
    _RCS_View(TargetNode, Mode = 0) {
        let RootKey = `${TargetNode.Name} [${TargetNode.PvType}]`;
        switch (Mode) {
            case 1:
                RootKey = `[${TargetNode.PvType}] ${TargetNode.Name}`;
                break;
            default:
                break;
        }
        let Result = {};
        Result[RootKey] = {};
        for (let Item of TargetNode.Nodes) {
            let NodeResult = this._RCS_View(Item, Mode);
            for (let Key in NodeResult) {
                Result[RootKey][Key] = NodeResult[Key];
            }
        }
        return Result;
    }
    //#endregion

    //#region Log
    _Log_PathsIsNotFound(PathNodes) {
        let Message = PathNodes
            .map(Item => `[${Item.PvType}:${Item.Name}]`);

        console.warn(Message.join(' > ') + ' is not found');
    }
    _Log_UnknowAttrAction(Action) {
        let Message = `"${Action}" is unknown attribute action`;
        console.warn(Message);
    }
    //#endregion
}

const Pv = new PvRender();
