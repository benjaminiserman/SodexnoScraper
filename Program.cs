using System;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

string address = "https://menus.sodexomyway.com/BiteMenu/MenuOnly?menuId=15109&locationId=10344001&whereami=http://dsu.sodexomyway.com/dining-near-me/trojan-marketplace";

DateTime start = new(2021, 8, 15);
DateTime end = new(2021, 12, 6);
DateTime current = start;

while (current < end)
{
    Console.WriteLine($"{current.ToShortDateString()}\n{address}&startDate={current.Month}/{current.Day}/{current.Year}");
    HtmlDocument doc = new HtmlWeb().Load($"{address}&startDate={current.Month}/{current.Day}/{current.Year}");

    StringBuilder text = new();

    HtmlNodeCollection meals = doc.DocumentNode.SelectNodes("//div[contains(@class, 'accordion-block')]");
    if (meals == null)
    {
        Console.WriteLine("No more data.");
        return;
    }

    DateTime firstDay = current.AddDays(-1);

    foreach (var meal in meals)
    {
        var found = meal.GetClasses();
        string mealName = found.Skip(1).First();

        HtmlNodeCollection col = meal.SelectNodes(".//div[contains(@class,'col-xs-9')]");
        if (col is null) continue;

        if (mealName == "brunch" || mealName == "breakfast")
        {
            firstDay = firstDay.AddDays(1);
            text.Append($"\n{firstDay.DayOfWeek}, {firstDay.ToShortDateString()}\n");
        }

        text.AppendLine($"\n{mealName}\n");

        foreach (var x in col)
        {
            string result = x.SelectSingleNode("a")?.GetAttributeValue("data-foodItemName", string.Empty);

            if (!string.IsNullOrWhiteSpace(result))
            {
                HtmlNodeCollection allergens = x.SelectSingleNode("div[@id]")?.SelectNodes("img");
                if (allergens != null)
                {
                    bool foundAllergen = false;
                    bool foundV = false;

                    foreach (var y in allergens)
                    {
                        string s = y.GetAttributeValue("alt", string.Empty);
                        if (s != "mindful")
                        {
                            if (!foundV && !s.Contains("contains"))
                            {
                                result += " is";
                                foundV = true;
                            }
                            else if (!foundAllergen && s.Contains("contains"))
                            {
                                result += ", contains:";
                                foundAllergen = true;
                            }

                            if (s.Contains("contains")) result += s.Substring("contains".Length);
                            else result += $" {s}";
                        }
                    }
                }

                text.AppendLine(result);
            }
        }
    }

    File.WriteAllText($"{current.Month}-{current.Day}-{current.Year}.txt", text.ToString());

    current = current.AddDays(7);
}