using Microsoft.AspNetCore.Razor.TagHelpers;
using Rugal.PartialViewRender.TagBase;

namespace Rugal.PartialViewRender.Js.PartialView.Dtvl
{
    [HtmlTargetElement("dtvl-pv")]
    public class DtvlPvTag : PvTagBase<DtvlPv>
    {
        public DtvlPvTag(IServiceProvider _Provider) : base(_Provider)
        {
        }
    }
}