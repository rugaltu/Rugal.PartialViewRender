document.addEventListener('DOMContentLoaded', () => {
    const AllPvDom = document.querySelectorAll('[pv-name]');
    AllPvDom.forEach(PvDom => {
        let PvName = PvDom.getAttribute('pv-name');
        let PvNameQuery = `[pv-name="${PvName}"]`;

        let AllPvTemplate = document.querySelectorAll(`${PvNameQuery} [pv-template]`);
        AllPvTemplate.forEach(PvTemplate => {
            let TemplateName = PvTemplate.getAttribute('pv-template');
            let TemplateQuery = `[pv-template="${TemplateName}"]`;

            let SlotQuery = `[pv-slot="${PvName}.${TemplateName }"]`;
            let GetSlotView = document.querySelector(SlotQuery);
            if (GetSlotView == null)
                return;

            let GetTemplate = document.querySelector(`${PvNameQuery} ${TemplateQuery}`);
            GetTemplate.innerHTML = GetSlotView.innerHTML;
        });
    });
});