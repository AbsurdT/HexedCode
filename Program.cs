using iText.Kernel.Font;
using iText.Kernel.Pdf;
using Newtonsoft.Json;

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
    // Получаем 8 точку
    double deltaT3 = CalculateResponseTime(standarts.T3);
    //
    Console.WriteLine();
    //  Получаем 10 точку, минимальный  и максимальные наливы
    var result = CalculateRDC(standarts.Volume);
    double rdc = result.volume;
    double volumeMin = result.min;
    double volumeMax = result.max;
    //
    // Считаем код 8 точки
    string hexedT3 = ((int)Math.Round(deltaT3, MidpointRounding.AwayFromZero) + 75).ToString("X2");
    //
    // Считаем 11 точку
    var deltaE1 = rdc + (volumeMax - volumeMin) / 2000 * deltaT3 / standarts.DivConst * 100;

    if(IsInRange(Math.Round(deltaE1,2, MidpointRounding.AwayFromZero), -5.1, 5.1))
    {
        //  Считаем код 11 точки
        string hexedE1 = ((int)Math.Floor(deltaE1 * 10) + 50).ToString("X2");
        Console.WriteLine("Полученный код:");
        Console.ForegroundColor = ConsoleColor.Green;
        string code = $"{hexedE1}00{hexedT3}";
        Console.WriteLine(code);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
    Console.WriteLine();
    Console.WriteLine("Нажмите любую кнопку для повторного ввода");
    Console.ReadKey();
}
static double CalculateResponseTime(double standart)
{
    while (true)
    {
        double deltaT3 = IsNumber() - standart;
        if (IsInRange(deltaT3, -75, 75))
            return deltaT3;
    }
}
static (double volume, double min, double max) CalculateRDC(double standart)
{
    while (true)
    {
        double[] volumes = new double[5];
        for (int i = 0; i < volumes.Length; i++)
        {
            Console.WriteLine($"Введите объем {i + 1} испытания:");
            volumes[i] = IsNumber();
            Console.WriteLine();
        }
        double volume = volumes.Average() - standart;
        if (IsInRange(volume, -11.4, 11.4))
            return (volume, volumes.Min(), volumes.Max());
    }
}
static bool IsInRange(double value, double min, double max)
{
    if (value <= max && value >= min)
        return true;
    else
    {
        Console.WriteLine($"Значение '{value}' не попадает в диапазон [{min}; {max}], попробуйте снова");
        return false;
    }
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
        Console.WriteLine("Введите эталон Объема:");
        double volume = IsNumber();
        Console.WriteLine("Введите постоянную деления:");
        double divConst = IsNumber();
        Standarts standarts = new Standarts()
        {
            T3 = T3,
            Volume = volume,
            DivConst = divConst
        };
        try
        {
            string standartsData = JsonConvert.SerializeObject(standarts);
            await File.WriteAllTextAsync("Standarts.json", standartsData);
        }
        catch(JsonException ex)
        {
            Console.WriteLine($"Serialize Error: {ex.Message}");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Unknown Exception: {ex.Message}");
        }
    }
}
static async Task<Standarts?> PrintStandarts()
{
    string standartsData = await File.ReadAllTextAsync("Standarts.json");
    Standarts? standarts = null;
    try
    {
        standarts = JsonConvert.DeserializeObject<Standarts>(standartsData);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Deserialize Error: {ex.Message}");
    }
    if ( standarts == null)
    {
        Console.WriteLine("Не удалось десериализовать");
        return null;
    }
    Console.WriteLine("Текущие эталонные значения:");
    Console.WriteLine($"Время Т3 = {standarts.T3}");
    Console.WriteLine($"Объем Е1 = {standarts.Volume}");
    Console.WriteLine($"Постоянная деления = {standarts.DivConst}");
    return standarts;
}
public class Standarts
{
    public double T3 { get; set; }
    public double Volume { get; set; }
    public double DivConst { get; set; }
}

