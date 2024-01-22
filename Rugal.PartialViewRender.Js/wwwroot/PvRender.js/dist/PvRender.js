document.addEventListener('DOMContentLoaded', () => {
    const AllPvSlot = document.querySelectorAll('[pv-slot]');
    AllPvSlot.forEach(PvSlot => {
        let PvName = PvSlot.getAttribute('pv-slot');
        let PvNames = PvName.split('.');
        SetPvTepmlate(PvNames, PvSlot);
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
function SetPvTepmlate(PvNames, PvSlot) {
    let LastName = PvNames.pop();
    let AllQuerys = PvNames
        .map(Item => `[pv-name="${Item}"]`);

    AllQuerys.push(`[pv-template="${LastName}"]`);
    let QueryResult = AllQuerys.join(' ');
    let FindPv = document.querySelector(QueryResult);
    if (FindPv == null)
        return;

    FindPv.innerHTML = PvSlot.innerHTML;
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