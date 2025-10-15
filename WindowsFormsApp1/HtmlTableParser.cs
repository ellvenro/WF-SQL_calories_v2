using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using HtmlAgilityPack;

namespace WindowsFormsApp1
{
    public class HtmlTableParser
    {
        public List<HtmlNode> FindRowsWithSubstring(string htmlContent, string columnName, string substring)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Предполагаем, что таблица имеет тег <table>
            var table = doc.DocumentNode.SelectSingleNode("//table");

            // Получаем заголовки колонок
            var headers = table.SelectNodes(".//th").Select(th => th.InnerText.Trim()).ToList();
            int columnIndex = headers.IndexOf(columnName);

            if (columnIndex == -1)
            {
                throw new Exception($"Колонка с именем '{columnName}' не найдена.");
            }

            //Находим все строки, содержащие подстроку в указанной колонке
            var rowsWithSubstring = table.SelectNodes(".//tr")
                .Where(tr => tr.SelectNodes(".//td")?.Count > columnIndex && tr.SelectNodes(".//td")[columnIndex]?.InnerText.Contains(substring) == true)
                .ToList();

            return rowsWithSubstring;
        }

    }
}
