class PartialView {
    constructor() {
        this.Init();
    }

    Init() {
        document.addEventListener('DOMContentLoaded', () => {
            this.InitPvName();

            const AllPvSlot = document.querySelectorAll('[pv-slot]');
            AllPvSlot.forEach(PvSlot => {
                let PvPath = PvSlot.getAttribute('pv-slot');
                this.SetPvSlot(PvPath, PvSlot);
            });

            const AllPvIn = document.querySelectorAll('[pv-in]');
            AllPvIn.forEach(PvIn => {
                let PvPath = PvIn.getAttribute('pv-in');
                this.SetPvIn(PvPath, PvIn);
            });

            AllPvSlot.forEach(Item => Item.remove());
        });
    }

    InitPvName() {
        let AllPv = document.querySelectorAll('[pv-name]');
        AllPv.forEach(Dom => {
            let PvName = Dom.getAttribute('pv-name');
            let SetPvName = this.RCS_CombinePvName(Dom, PvName);
            Dom.setAttribute('pv-name', SetPvName);
        });
    }

    SetPvSlot(PvPath, PvSlot) {
        let QueryResult = `[pv-name="${PvPath}"]`;
        let PvTarget = document.querySelector(QueryResult);
        if (PvTarget == null)
            return;

        if (PvSlot.attributes.length > 1)
            this.SetPvAttr(PvSlot, PvTarget);

        if (PvSlot.innerHTML != '') {
            PvTarget.innerHTML = PvSlot.innerHTML;
            this.ReNameInnerPvName(PvTarget);
        }
    }

    SetPvIn(PvPath, PvIn) {
        let Paths = PvPath.split('.');
        let PvName = Paths.shift();
        let PvOut = Paths.join('.');
        let QueryResult = `[pv-name="${PvName}"] [pv-out="${PvOut}"]`;
        let PvTarget = document.querySelector(QueryResult);
        if (PvTarget == null)
            return;

        if (PvIn.attributes.length > 1)
            this.SetPvAttr(PvIn, PvTarget);

        if (PvIn.innerHTML != '') {
            PvTarget.innerHTML = PvIn.innerHTML;
            this.ReNameInnerPvName(PvTarget);
        }
    }

    SetPvAttr(PvSource, PvTarget) {

        for (let Item of PvSource.attributes) {
            let AttrName = Item.name;
            if (AttrName == 'pv-slot')
                continue;

            if (!AttrName.includes('.'))
                PvTarget.setAttribute(AttrName, Item.value);
            else {
                let AttrNames = AttrName.split('.');
                let AttrAction = AttrNames.pop();
                AttrName = AttrNames.join('.');
                let AttrValue = PvTarget.getAttribute(AttrName);
                if (AttrValue == null) {
                    PvTarget.setAttribute(AttrName, '');
                    AttrValue = PvTarget.getAttribute(AttrName);
                }
                let AttrValues = AttrValue
                    .split(/\s*;\s*/)
                    .filter(Val => Val != '' && Val != null);
                let SetAttrValues = Item.value
                    .split(/\s*;\s*/)
                    .filter(Val => Val != '' && Val != null);

                switch (AttrAction.toLowerCase()) {
                    case 'add':
                        SetAttrValues.forEach(Item => {
                            if (!AttrValues.includes(Item))
                                AttrValues.push(Item);
                        });
                        break;
                    case 'remove':
                        SetAttrValues.forEach(Item => {
                            let FindIndex = AttrValues
                                .findIndex(Val => Val == Item);
                            if (FindIndex >= 0)
                                AttrValues.splice(FindIndex, 1);
                        });
                        break;
                }
                let JoinChar = AttrName == 'style' ? '; ' : ' ';
                let SetAttrResult = AttrValues.join(JoinChar);
                PvTarget.setAttribute(AttrName, SetAttrResult);
            }
        }
    }

    ReNameInnerPvName(Pv) {
        let InnerPv = Pv.querySelectorAll('[pv-name]');
        InnerPv.forEach(Dom => {
            let ParentPvPath = this.RCS_FindParentPvName(Dom);
            if (ParentPvPath == null)
                return;

            let DomPvPath = Dom.getAttribute('pv-name');
            let SetPvPath = `${ParentPvPath}.${DomPvPath}`;
            Dom.setAttribute('pv-name', SetPvPath);
        });
    }

    //#region RCS Private Function
    RCS_CombinePvName(Dom, PvName) {
        if (Dom.parentElement.tagName == 'BODY')
            return PvName;

        let GetPvName = Dom.parentElement.getAttribute('pv-name');
        if (GetPvName != null) {
            PvName = `${GetPvName}.${PvName}`;
            return PvName;
        }

        return this.RCS_CombinePvName(Dom.parentElement, PvName);
    }

    RCS_FindParentPvName(Dom) {
        let GetParen = Dom.parentElement;
        let ParenPvName = GetParen.getAttribute('pv-name');
        if (ParenPvName != null)
            return ParenPvName;

        if (GetParen.tagName == 'BODY')
            return null;

        return this.RCS_FindParentPvName(Dom.parentElement);
    }
    //#endregion
}

const Pv = new PartialView();