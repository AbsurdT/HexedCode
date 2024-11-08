using System.Text.Json;

if (!File.Exists("Standarts.json"))
{
    await ChangeIt("да");
    await PrintStandarts();
}
else
{
    await PrintStandarts();
    Console.WriteLine();
    Console.WriteLine("Изменить? да/нет");
    await ChangeIt(Console.ReadLine());
}
while (true)
{
    Console.Clear();
    Standarts standarts = await PrintStandarts();
    Console.WriteLine();
    Console.WriteLine("Введите время Response Time - T3:");
    double deltaT3 = IsNumber() - standarts.T3;
    Console.WriteLine();
    Console.WriteLine("Введите объем E1 Fuel Percentage:");
    double deltaE1 = IsNumber() - standarts.E1;
    Console.WriteLine();
    string hexedT3 = ((int)Math.Round(deltaT3, MidpointRounding.AwayFromZero) + 75).ToString("X2");
    string hexedE1 = ((int)Math.Floor(deltaE1 * 10) + 50).ToString("X2");
    Console.WriteLine("Полученный код:");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{hexedE1}00{hexedT3}");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("Нажмите любую кнопку для повторного ввода");
    Console.ReadKey();
}
static double IsNumber()
{
    while (true)
    {
        bool isNumber = double.TryParse(Console.ReadLine().Replace('.', ','), out double input);
        if (isNumber) return input;
        Console.WriteLine("Попробуйте снова");
    }
}
static async Task ChangeIt(string yesOrNot)
{
    if (yesOrNot == "да")
    {
        Console.WriteLine("Введите эталон Времени Т3:");
        double T3 = IsNumber();
        Console.WriteLine("Введите эталон Объема Е1:");
        double E1 = IsNumber();
        Standarts standarts = new Standarts()
        {
            T3 = T3,
            E1 = E1
        };
        string standartsData = JsonSerializer.Serialize(standarts);
        await File.WriteAllTextAsync("Standarts.json", standartsData);
    }
}
static async Task<Standarts> PrintStandarts()
{
    string standartsData = await File.ReadAllTextAsync("Standarts.json");
    Standarts? standarts = JsonSerializer.Deserialize<Standarts>(standartsData);
    Console.WriteLine("Текущие эталонные значения:");
    Console.WriteLine($"Время Т3 = {standarts.T3}");
    Console.WriteLine($"Объем Е1 = {standarts.E1}");
    return standarts;
}
public class Standarts
{
    public double T3 { get; set; }
    public double E1 { get; set; }
}

