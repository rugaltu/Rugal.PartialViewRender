using System.Text.RegularExpressions;

namespace Rugal.PartialViewRender.Model
{
    public class PvOption
    {
        public string PvName { get; set; }
        public string JsPath { get; set; }
        public object Data { get; set; }
        public string JsFile(string JsName)
        {
            JsName = JsName.TrimStart('/');
            if (!Regex.IsMatch(JsName, ".js$", RegexOptions.IgnoreCase))
                JsName += ".js";
            return $"{JsPath}/{JsName}";
        }
    }
}
