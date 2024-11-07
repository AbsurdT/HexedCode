using System.Text.RegularExpressions;

while (true)
{
    Console.WriteLine("Введите время Response Time - T3:");
    double t3 = IsNumber();
    Console.WriteLine("Введите объем E1 Fuel Percentage:");
    double e1 = IsNumber();
    string hexedT3 = ((int)Math.Round(t3, MidpointRounding.AwayFromZero) + 75).ToString("X2");
    string hexedE1 = ((int)Math.Floor(e1 * 10) + 50).ToString("X2");
    Console.WriteLine("Полученный код:");
    Console.WriteLine($"{hexedE1}00{hexedT3}");
    Console.WriteLine("Нажмите любую кнопку для повторного ввода");
    Console.ReadKey();
    Console.Clear();
}
static double IsNumber()
{
    while (true)
    {
        bool isNumber = double.TryParse(Regex.Replace(Console.ReadLine(), @"[.]", ","), out double input);
        if (isNumber) return input;
        Console.WriteLine("Попробуйте снова");
    }
}

