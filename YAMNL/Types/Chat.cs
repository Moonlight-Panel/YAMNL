using System.Text;
using System.Text.RegularExpressions;
using Logging.Net;
using Newtonsoft.Json.Linq;

namespace YAMNL.Types
{
    public class Chat
    {
        public string JSON { get; }

        public string StyledMessage { get; private set; }
        public string Message { get; private set; }

        private readonly Func<string, string>? _translationProvider;

        public Chat(string json, Func<string, string>? translationProvider = null)
        {
            JSON = json;
            _translationProvider = translationProvider;

            StyledMessage = ParseComponent(JToken.Parse(JSON));
            Message = Regex.Replace(StyledMessage, "\\$[0-9a-fk-r]", "");
        }

        private string ParseComponent(JToken token, string styleCode = "")
        {
            return token.Type switch
            {
                JTokenType.Array => ParseArray((JArray)token, styleCode),
                JTokenType.Object => ParseObject((JObject)token, styleCode),
                JTokenType.String => (string)token!,
                JTokenType.Integer => (string)token!,
                _ => throw new Exception($"Type {token.Type} is not supported")
            };
        }

        private string ParseObject(JObject jObject, string styleCode = "")
        {
            var sb = new StringBuilder();

            var colorProp = jObject.GetValue("color");
            if (colorProp != null)
            {
                var color = ParseComponent(colorProp);
                var style = TextStyle.GetTextStyle(color);
                if (style != null)
                {
                    styleCode = style.ToString();
                }
                else
                {
                    Logger.Warn($"Unknown chat style: {color}");
                    styleCode = "";
                }
            }

            var extraProp = jObject.GetValue("extra");
            if (extraProp != null)
            {
                var extras = (JArray)extraProp!;

                foreach (var item in extras)
                    sb.Append(ParseComponent(item, styleCode) + "§r");
            }

            var textProp = jObject.GetValue("text");
            var translateProp = jObject.GetValue("translate");

            if (textProp != null)
            {
                return styleCode + ParseComponent(textProp, styleCode) + sb.ToString();
            }
            else if (translateProp != null)
            {
                var usingData = new List<string>();

                var usingProp = jObject.GetValue("using");
                var withProp = jObject.GetValue("with");
                if (usingProp != null && withProp == null)
                    withProp = usingProp;

                if (withProp != null)
                {
                    var array = (JArray)withProp;
                    for (int i = 0; i < array.Count; i++)
                    {
                        usingData.Add(ParseComponent(array[i], styleCode));
                    }
                }

                var ruleName = ParseComponent(translateProp!);
                if (_translationProvider == null)
                {
                    Logger.Warn("No translation provider given. Consider passing MineSharp.Data.Languages.Language.GetRule(string name) as an argument.");
                    return styleCode + string.Join(" ", usingData) + sb.ToString();
                }
                return styleCode + TranslateString(_translationProvider(ruleName), usingData)
                                 + sb.ToString();
            }
            else return sb.ToString();
        }

        private string ParseArray(JArray jArray, string styleCode = "")
        {
            var sb = new StringBuilder();
            foreach (var token in jArray)
            {
                sb.Append(ParseComponent(token, styleCode));
            }
            return sb.ToString();
        }

        private static string TranslateString(string rule, List<string> usings)
        {
            int usingIndex = 0;
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < rule.Length; i++)
            {
                if (rule[i] == '%' && i + 1 < rule.Length)
                {
                    //Using string or int with %s or %d
                    if (rule[i + 1] == 's' || rule[i + 1] == 'd')
                    {
                        if (usings.Count > usingIndex)
                        {
                            result.Append(usings[usingIndex]);
                            usingIndex++;
                            i += 1;

                            continue;
                        }
                    }

                    else if (char.IsDigit(rule[i + 1]) && i + 3 < rule.Length && rule[i + 2] == '$'
                             && (rule[i + 3] == 's' || rule[i + 3] == 'd'))
                    {
                        int specifiedIdx = rule[i + 1] - '1';

                        if (usings.Count > specifiedIdx)
                        {
                            result.Append(usings[specifiedIdx]);
                            usingIndex++;
                            i += 3;

                            continue;
                        }
                    }
                }

                result.Append(rule[i]);
            }

            return result.ToString();
        }

        public override string ToString() => JSON;
    }

    public class TextStyle
    {
        public static TextStyle[] KnownStyles = new[] {
            new TextStyle('0', "black"),
            new TextStyle('1', "dark_blue"),
            new TextStyle('2', "dark_green"),
            new TextStyle('4', "dark_red"),
            new TextStyle('3', "dark_aqua") { Aliases = new[] { "dark_cyan" } },
            new TextStyle('5', "dark_purple") { Aliases = new[] { "dark_magenta" } },
            new TextStyle('6', "gold") { Aliases = new[] { "dark_yellow" } },
            new TextStyle('7', "gray"),
            new TextStyle('8', "dark_gray"),
            new TextStyle('9', "blue"),
            new TextStyle('a', "green"),
            new TextStyle('b', "aqua") { Aliases = new[] { "cyan" } },
            new TextStyle('c', "red"),
            new TextStyle('d', "light_purple") { Aliases = new[] {"magenta" } },
            new TextStyle('e', "yellow"),
            new TextStyle('f', "white"),

            new TextStyle('k', "magic"),
            new TextStyle('l', "bold"),
            new TextStyle('m', "strikethrough"),
            new TextStyle('n', "underline"),
            new TextStyle('o', "italic"),
            new TextStyle('r', "reset"),
        };

        private const char PREFIX = '$';

        public char Code { get; private set; }
        public string Name { get; private set; }
        public string[] Aliases { get; set; } = Array.Empty<string>();

        public TextStyle(char code, string name)
        {
            Code = code;
            Name = name;
        }

        public override string ToString() => $"{PREFIX}{Code}";

        public static TextStyle? GetTextStyle(string name)
        {
            return KnownStyles.FirstOrDefault(x =>
                    string.Equals(name, x.Name, StringComparison.OrdinalIgnoreCase) ||
                    x.Aliases.Any(y => string.Equals(name, y, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
