/**
 *  PvRender.js v1.2.6
 *  From Rugal Tu
 * */
class PvBase {
    constructor() {
        this.Nodes = [];
        this.Id = this._GenerateId();
    }
    //#region Pv Property
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
    get Layouts() {
        return this.NextTree('pv-layout');
    }
    //#endregion

    //#region Public Method
    NextNode(NodeName, PvType) {
        let Result = this.NextTree(PvType)
            .find(Item => {
                let IsFind = Item.IsMatch(`${PvType}="${NodeName}"`);
                if (PvType == 'pv-out')
                    IsFind = IsFind || Item.OutName == NodeName;
                return IsFind;
            });
        return Result;
    }
    //#endregion

    //#region Private Process
    _GenerateId() {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        );
    }
    //#endregion
}
class PvNode extends PvBase {
    constructor(_Element, _IsTempNode = false) {
        super();

        this.Element = _Element;
        this.PvType = null;
        this.Parent = null;

        this.Name = null;
        this.OutName = null;
        this.InName = null;
        this.SlotName = null;
        this.Layout = null;
        this.Floor = 0;

        this.IsNotifyChange = false;

        this._InitName(_IsTempNode);
    }
    //#region Get Value Property
    get AbsPath() {

        if (this.Name == null)
            return null;

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
    //#endregion

    //#region Path Property
    get NamePaths() {
        if (this.Name == null)
            return null;

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
    //#endregion

    //#region Has Property
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
    get HasLayout() {
        return this.IsMatch('pv-layout');
    }
    //#endregion

    //#region Is Property
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
    get IsPvAppend() {
        return this.IsMatch('pv-append');
    }
    //#endregion

    //#region Init Method
    _InitName(IsTempNode) {
        if (this.Element == null)
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

        if (this.HasLayout)
            this.Layout = this.GetAttr('pv-layout');

        if (!IsTempNode)
            this.SetAttr('_nodeid', this.Id);
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
    Remove() {
        this.Element.remove();
        let Parent = this.Parent;
        if (Parent == null)
            return;
        let DeleteIndex = Parent.Nodes.findIndex(Item => Item.Id == this.Id);
        Parent.Nodes.splice(DeleteIndex, 1);
    }
    //#endregion

    //#region With Method
    WithParent(_Parent) {
        this.Parent = _Parent;
        if (this.Parent != null)
            this.Floor = this.Parent.Floor + 1;
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
        if (SourceNodes.length == 0)
            return false;

        this.Element.innerHTML = null;
        return this.AppendContent(...SourceNodes);
    }
    AppendContent(...SourceNodes) {
        if (SourceNodes.length == 0)
            return false;

        let OrgInner = this.Element.innerHTML;
        for (let Item of SourceNodes)
            this.Element.innerHTML += Item.Content;

        return OrgInner != this.Content;
    }

    SetContentNode(...SourceNodes) {
        if (SourceNodes.length == 0)
            return false;

        this.Element.innerHTML = null;
        return this.AppendContentNode(...SourceNodes);
    }
    AppendContentNode(...SourceNodes) {
        if (SourceNodes.length == 0)
            return false;

        let OrgInner = this.Element.innerHTML;
        for (let Item of SourceNodes) {
            if (!this.IsTemplate) {
                //let CloneElement = Item.Element;
                let CloneElement = Item.Element.cloneNode(true);
                this.Element.append(CloneElement);
            }
            else
                this.Element.innerHTML += Item.FullContent;
        }
        return OrgInner != this.Content;
    }

    SetAttr(Name, Value) {
        this.Element.setAttribute(Name, Value);
        return this;
    }
    GetAttr(Name) {
        return this.Element.getAttribute(Name);
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
class PvRender extends PvBase {
    constructor() {
        super();
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
    //#endregion

    //#region Public Method
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

    //#region Init Method
    _Init() {
        document.addEventListener('DOMContentLoaded', () => {
            this._InitTree();
            this._SetTree();
            this._ClearPv();
        });
    }
    //#endregion

    //#region Build Tree
    _InitTree() {
        let DomRootNode = new PvNode(document.body, true);
        let RootDoms = this._RCS_CreateRootNodes(DomRootNode);
        for (let Item of RootDoms) {
            let Tree = this._RCS_BuildTree(Item);
            this.Nodes.push(Tree);
        }
        return this.Nodes;
    }
    _RCS_CreateRootNodes(RootNode, Result) {
        Result ??= [];
        if (RootNode.Children == null)
            return Result;

        for (let Item of RootNode.Children) {
            if (this._CheckAnyPvAttrs(Item)) {
                Result.push(Item);
                continue;
            }
            let NewNode = new PvNode(Item);
            this._RCS_CreateRootNodes(NewNode, Result);
        }
        return Result;
    }
    _RCS_HasUpperPvElement(TargetElement) {
        while (TargetElement.parentElement) {
            TargetElement = TargetElement.parentElement
            if (this._CheckAnyPvAttrs(TargetElement))
                return true;
        }
        return false;
    }
    _CheckAnyPvAttrs(TargetElement) {
        let PvAttrs = [...TargetElement.attributes]
            .filter(Val => Val.name.includes('pv-'));
        let HasPvAttrs = PvAttrs.length > 0;
        return HasPvAttrs;
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
    //#endregion

    //#region Set Tree
    _SetTree() {
        let InputTypes = ['pv-in', 'pv-slot', 'pv-layout'];
        let SetNodes = this.NextTree(...InputTypes);
        for (let SetNode of SetNodes)
            this._RCS_SetTree(SetNode, InputTypes);

        while (true) {
            let NotifyNode = this.NodeList
                .find(Item => Item.IsNotifyChange)

            if (NotifyNode == null)
                break;

            this._RCS_SetTree(NotifyNode, InputTypes, false);
        }
    }
    _RCS_SetTree(SourceNode, QueryTypes, IsCanNotify = true) {

        SourceNode.IsNotifyChange = false;

        let TargetNode = this._FindNode(SourceNode);
        if (TargetNode == null)
            return;

        let IsAttrSet = this._SetNodeAttr(TargetNode, SourceNode);
        let IsNodeSet = false;
        if (SourceNode.Id != TargetNode.Id) {
            IsNodeSet = this._SetNodeContent(TargetNode, SourceNode);
            if (IsNodeSet)
                this._RCS_BuildChildren(TargetNode);
        }

        for (let Item of SourceNode.Nodes)
            this._RCS_SetTree(Item, QueryTypes, IsCanNotify)

        if (!IsNodeSet && !IsAttrSet)
            return;

        if (TargetNode.IsPvSlot || TargetNode.IsPvIn) {
            this._RCS_SetTree(TargetNode, QueryTypes, IsCanNotify);
            return;
        }

        let ParentInput = SourceNode;
        while (ParentInput.Parent) {
            if (IsCanNotify && (ParentInput.IsPvSlot || ParentInput.IsPvIn)) {
                ParentInput.IsNotifyChange = true;
                break;
            }
            ParentInput = ParentInput.Parent;
        }
    }
    _SetNodeContent(TargetNode, SourceNode) {
        let InputType = ['pv-slot', 'pv-in'];
        let IsInnerChange = false;
        if (SourceNode.Nodes.length > 0) {
            let ContentNodes = SourceNode.Nodes
                .filter(Item => !Item.IsMatch(...InputType));

            IsInnerChange = SourceNode.IsPvAppend ?
                TargetNode.AppendContentNode(...ContentNodes) :
                TargetNode.SetContentNode(...ContentNodes);
        }
        else if (SourceNode.Content && SourceNode.Content != '') {
            IsInnerChange = SourceNode.IsPvAppend ?
                TargetNode.AppendContent(SourceNode) :
                TargetNode.SetContent(SourceNode);
        }

        return IsInnerChange;
    }
    _FindRootNode(TargetNode) {
        while (TargetNode.Parent)
            TargetNode = TargetNode.Parent;

        return TargetNode;
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
            return false;

        let Attrs = SourceNode.Attrs.map(Item => {
            return {
                Name: Item.name,
                Value: Item.value,
            }
        });

        if (SourceNode.HasLayout)
            this._SetNodeLayout(TargetNode, SourceNode);

        const SkipAttrs = [
            'pv-name', 'pv-slot', 'pv-in', 'pv-out'
        ];

        const SkipRegex = new RegExp(/^v-slot\b/);

        let IsSet = false;
        for (let Item of Attrs) {
            let AttrName = Item.Name;
            let AttrValue = Item.Value;
            if (SkipAttrs.includes(AttrName) || AttrName[0] == '_' || SkipRegex.test(AttrName))
                continue;

            if (AttrName.includes('.')) {
                let Names = AttrName.split('.');
                let Action = Names.pop();
                AttrName = Names.join('.');

                let TargetAttrValue = TargetNode.GetAttr(AttrName);
                AttrValue = this._TransAttrValue(TargetAttrValue, Action, AttrName, AttrValue);
            }

            TargetNode.SetAttr(AttrName, AttrValue);
            IsSet = true;
        }
        return IsSet;
    }
    _SetNodeLayout(TargetNode, SourceNode) {
        let FullLayout = SourceNode.Layout;
        if (FullLayout == null)
            return;

        let Layouts = FullLayout.split(' ');
        for (let Layout of Layouts) {
            let LayoutNode = this._FindLayoutNode(SourceNode, Layout);
            if (LayoutNode == null)
                continue;

            this._SetNodeAttr(TargetNode, LayoutNode);
        }
    }
    _FindLayoutNode(TargetNode, LayoutName) {
        if (TargetNode == null)
            return null;

        if (TargetNode.Name == LayoutName)
            return TargetNode;

        let TryGetNode = TargetNode.Pvs
            .find(Item => Item.Name == LayoutName);

        if (TryGetNode)
            return TryGetNode;

        let UpperNode = TargetNode.Parent;
        if (UpperNode == null && TargetNode instanceof PvNode == true)
            UpperNode = this;

        return this._FindLayoutNode(UpperNode, LayoutName);
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

    //#region Clear Pv Template
    _ClearPv() {
        let ClearNodes = this.Nodes.filter(Item => Item.IsTemplate);
        for (let Item of ClearNodes) {
            Item.Remove();
            let DeleteIndex = this.Nodes.findIndex(Val => Val.Id == Item.Id);
            this.Nodes.splice(DeleteIndex, 1);
        }

        let AllTemplate = this.NodeList.filter(Item => Item.IsTemplate);
        for (let Item of AllTemplate) {
            let IsCanRemove = this._CheckRemove(Item);
            if (!IsCanRemove)
                continue;

            Item.Remove();
        }
    }
    _CheckRemove(TargetNode) {
        let VSlotExp = new RegExp(/^v-slot\b/);
        let IsVSlot = TargetNode.Attrs.filter(Item => VSlotExp.test(Item.name)).length > 0;
        if (!IsVSlot)
            return true;

        let FindNode = TargetNode;
        while (FindNode.Parent) {
            FindNode = FindNode.Parent;
            if (FindNode.Element.tagName.includes('V-'))
                return false;
        }

        return true;
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

        let Name = TargetNode.Name ?? `<${TargetNode.Element.tagName.toLowerCase()}>`;
        let PvType = TargetNode.PvType == null ? '' : `[${TargetNode.PvType}]`;
        let RootKey = `${Name} ${PvType}`.trimEnd();
        switch (Mode) {
            case 1:
                RootKey = `${PvType} ${Name}`;
                break;
            case 2:
                RootKey = `${PvType} ${Name} (${TargetNode.Id})`;
                break;
            default:
                break;
        }

        let Result = {};
        Result[RootKey] = {};
        for (let Item of TargetNode.Nodes) {
            let NodeResult = this._RCS_View(Item, Mode);
            for (let Key in NodeResult) {
                let GetNode = Result[RootKey];
                let SetKey = Key;
                if (SetKey in GetNode) {
                    let NextIndex = Object
                        .keys(GetNode)
                        .filter(Val => Val.includes(SetKey)).length + 1;
                    SetKey = `${SetKey}(${NextIndex})`;
                }
                GetNode[SetKey] = NodeResult[Key];
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