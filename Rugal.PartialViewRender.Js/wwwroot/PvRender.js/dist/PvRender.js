document.addEventListener('DOMContentLoaded', () => {
    InitPvName();

    const AllPvSlot = document.querySelectorAll('[pv-slot]');
    AllPvSlot.forEach(PvSlot => {
        let PvName = PvSlot.getAttribute('pv-slot');
        SetPvSlot(PvName, PvSlot);
    });

    const AllPvAttr = document.querySelectorAll('[pv-attr]');
    AllPvAttr.forEach(PvAttr => {
        let PvName = PvAttr.getAttribute('pv-attr');
        let PvNames = PvName.split('.');
        SetPvAttr(PvNames, PvAttr);
    });

    AllPvSlot.forEach(Item => Item.remove());
    AllPvAttr.forEach(Item => Item.remove());
});
function SetPvSlot(PvName, PvSlot) {
    let QueryResult = `[pv-name="${PvName}"]`;
    let FindPv = document.querySelector(QueryResult);
    if (FindPv == null)
        return;

    FindPv.innerHTML = PvSlot.innerHTML;
    let InnerPv = FindPv.querySelectorAll('[pv-name]');
    InnerPv.forEach(Dom => {
        let ParentPvName = RCS_FindParentPvName(Dom);
        if (ParentPvName == null)
            return;

        let DomPvName = Dom.getAttribute('pv-name');
        let SetPvName = `${ParentPvName}.${DomPvName}`;
        Dom.setAttribute('pv-name', SetPvName);
    });
}

function SetPvAttr(PvNames, PvSlot) {
    let AllQuerys = PvNames
        .map(Item => `[pv-name="${Item}"]`);

    let QueryResult = AllQuerys.join(' ');
    let FindPv = document.querySelector(QueryResult);

    for (let Item of PvSlot.attributes) {
        if (Item.name.toLowerCase().includes('pv-'))
            continue;
        FindPv.setAttribute(Item.name, Item.value);
    }
}

function InitPvName() {
    let AllPv = document.querySelectorAll('[pv-name]');
    AllPv.forEach(Dom => {
        let PvName = Dom.getAttribute('pv-name');
        let SetPvName = RCS_CombinePvName(Dom, PvName);
        Dom.setAttribute('pv-name', SetPvName);
    });
}

function RCS_CombinePvName(Dom, PvName) {
    if (Dom.parentElement.tagName == 'BODY')
        return PvName;

    let GetPvName = Dom.parentElement.getAttribute('pv-name');
    if (GetPvName != null)
        PvName = `${GetPvName}.${PvName}`;

    return RCS_CombinePvName(Dom.parentElement, PvName);
}

function RCS_FindParentPvName(Dom) {
    let GetParen = Dom.parentElement;
    let ParenPvName = GetParen.getAttribute('pv-name');
    if (ParenPvName != null)
        return ParenPvName;

    if (GetParen.tagName == 'BODY')
        return null;

    return RCS_FindParentPvName(Dom.parentElement);
}