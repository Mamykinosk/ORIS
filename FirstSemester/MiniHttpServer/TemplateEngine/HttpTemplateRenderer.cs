using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TemplateEngine
{
    /// <summary>
    /// Реализация простого шаблонизатора HTML.
    /// </summary>
    public class HtmlTemplateRenderer : IHtmlTemplateRenderer
    {
        /// <summary>
        /// Рендерит шаблон, загружая его из файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу шаблона.</param>
        /// <param name="dataModel">Объект с данными.</param>
        /// <returns>Отрендеренный HTML.</returns>
        public string RenderFromFile(string filePath, object dataModel)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Template file not found", filePath);

            string template = File.ReadAllText(filePath);
            return RenderFromString(template, dataModel);
        }

        /// <summary>
        /// Рендерит шаблон из файла и сохраняет результат в новый файл.
        /// </summary>
        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
        {
            /*
            string path = " ";

            using (FileStream fs = File.Create(path))
            {

            }

            string result = RenderFromFile(inputFilePath, dataModel);
            File.WriteAllText(outputFilePath, result);
            */
            return string.Empty;
        }

        /// <summary>
        /// Основной метод рендеринга из строки. Обрабатывает условия, циклы и переменные.
        /// </summary>
        public string RenderFromString(string htmlTemplate, object dataModel)
        {
            if (string.IsNullOrEmpty(htmlTemplate))
                return string.Empty;

            string processedHtml = ProcessLogic(htmlTemplate, dataModel);

            return ReplaceVariables(processedHtml, dataModel);
        }

        /// <summary>
        /// Рекурсивный метод обработки блоков $if и $foreach.
        /// </summary>
        private string ProcessLogic(string template, object model)
        {
            int ifIndex = template.IndexOf("$if");
            int forIndex = template.IndexOf("$foreach");

            if (ifIndex == -1 && forIndex == -1)
                return template;

            int effectiveIfIndex = ifIndex == -1 ? int.MaxValue : ifIndex;
            int effectiveForIndex = forIndex == -1 ? int.MaxValue : forIndex;

            if (effectiveIfIndex < effectiveForIndex)
            {
                return ProcessIfBlock(template, model, ifIndex);
            }
            else
            {
                return ProcessForeachBlock(template, model, forIndex);
            }
        }

        /// <summary>
        /// Обработка блока $if.
        /// </summary>
        private string ProcessIfBlock(string template, object model, int startIndex)
        {
            int endIndex = FindMatchingEndTag(template, startIndex, "$if", "$endif");
            if (endIndex == -1) return template;

            string fullBlock = template.Substring(startIndex, endIndex - startIndex + "$endif".Length);
            
            var match = Regex.Match(fullBlock, @"\$if\((.*?)\)");
            if (!match.Success) return template;

            string conditionPath = match.Groups[1].Value.Trim();
            
            string contentBody = fullBlock.Substring(match.Length, fullBlock.Length - match.Length - "$endif".Length);

            string trueBlock = contentBody;
            string falseBlock = string.Empty;

            int elseIndex = contentBody.IndexOf("$else");
            if (elseIndex != -1)
            {
                trueBlock = contentBody.Substring(0, elseIndex); // $if(...)| ... | $else
                falseBlock = contentBody.Substring(elseIndex + "$else".Length); // $else | ... | $endif 
            }

            bool conditionResult = false;
            try
            {
                object val = GetValue(model, conditionPath);
                if (val is bool b) conditionResult = b;
                else if (val != null) conditionResult = true; 
            }
            catch { /* false */ }

            string resultBlock = conditionResult ? trueBlock : falseBlock;

            string processedResult = ProcessLogic(resultBlock, model);

            string templateBefore = template.Substring(0, startIndex);
            string templateAfter = template.Substring(endIndex + "$endif".Length);

            return ProcessLogic(templateBefore + processedResult + templateAfter, model);
        }

        /// <summary>
        /// Обработка блока $foreach.
        /// </summary>
        private string ProcessForeachBlock(string template, object model, int startIndex)
        {
            int endIndex = FindMatchingEndTag(template, startIndex, "$foreach", "$endfor");
            if (endIndex == -1) return template;

            string fullBlock = template.Substring(startIndex, endIndex - startIndex + "$endfor".Length); // $foreach ... $endfor 
            
            var match = Regex.Match(fullBlock, @"\$foreach\s*\(\s*var\s+(\w+)\s+in\s+([^\)]+)\s*\)");
            if (!match.Success) return template;

            string itemName = match.Groups[1].Value; 
            string listPath = match.Groups[2].Value;
            
            string loopBody = fullBlock.Substring(match.Length, fullBlock.Length - match.Length - "$endfor".Length);

            object collectionObj = GetValue(model, listPath);
            StringBuilder sb = new StringBuilder();

            if (collectionObj is IEnumerable list)
            {
                foreach (var item in list)
                {
                    var loopContext = new Dictionary<string, object>();
                    if (model is IDictionary<string, object> parentDict)
                    {
                        foreach (var kvp in parentDict) loopContext[kvp.Key] = kvp.Value;
                    }
                    
                    loopContext[itemName] = item;

                    string processedBody = ProcessLogic(loopBody, loopContext);
                    string materializedBody = ReplaceVariables(processedBody, loopContext);
                    
                    sb.Append(materializedBody);
                }
            }

            string templateBefore = template.Substring(0, startIndex);
            string templateAfter = template.Substring(endIndex + "$endfor".Length);

            return ProcessLogic(templateBefore + sb.ToString() + templateAfter, model);
        }

        /// <summary>
        /// Ищет индекс закрывающего тега с учетом вложенности.
        /// </summary>
        private int FindMatchingEndTag(string text, int startIndex, string openTag, string closeTag)
        {
            int balance = 0;
            int index = startIndex;

            while (index < text.Length)
            {
                if (IsSubstringAt(text, index, openTag))
                {
                    balance++;
                    index += openTag.Length;
                }
                else if (IsSubstringAt(text, index, closeTag))
                {
                    balance--;
                    if (balance == 0) return index;
                    index += closeTag.Length;
                }
                else
                {
                    index++;
                }
            }
            return -1;
        }

        private bool IsSubstringAt(string text, int index, string sub)
        {
            if (index + sub.Length > text.Length) return false;
            return text.Substring(index, sub.Length) == sub;
        }
        
        private string ReplaceVariables(string text, object model)
        {
            return Regex.Replace(text, @"\$\{(.*?)\}", match =>
            {
                string path = match.Groups[1].Value.Trim();
                object val = GetValue(model, path);
                return val?.ToString() ?? "";
            });
        }

        private object GetValue(object model, string path)
        {
            if (model == null || string.IsNullOrWhiteSpace(path)) return null;

            string[] parts = path.Split('.');
            object currentObj = model;

            if (currentObj is IDictionary<string, object> dict)
            {
                if (dict.ContainsKey(parts[0]))
                {
                    currentObj = dict[parts[0]];
                    if (parts.Length == 1) return currentObj;
                    
                    var newParts = new string[parts.Length - 1];
                    Array.Copy(parts, 1, newParts, 0, newParts.Length);
                    parts = newParts;
                }
            }

            foreach (var propName in parts)
            {
                if (currentObj == null) return null;
                Type type = currentObj.GetType();
                PropertyInfo prop = type.GetProperty(propName);
                if (prop != null)
                    currentObj = prop.GetValue(currentObj);
                else
                    return null;
            }

            return currentObj;
        }
    }
}