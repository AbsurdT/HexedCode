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
    //
    //  Считаем код 11 точки
    string hexedE1 = ((int)Math.Floor(deltaE1 * 10) + 50).ToString("X2");
    //
    Console.WriteLine("Полученный код:");
    Console.ForegroundColor = ConsoleColor.Green;
    string code = $"{hexedE1}00{hexedT3}";
    Console.WriteLine(code);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Gray;
    Task reWrite = Task.Run(() => PdfRewrite(code));
    await reWrite;
    Console.WriteLine("Нажмите любую кнопку для повторного ввода");
    Console.ReadKey();
}
static void PdfRewrite(string textToWrite)
{
    DateTime today = DateTime.Today;
    // Путь к исходному и целевому PDF
    var path = $@"C:\Program Files (x86)\Potok CR\report\UI\{today.Year.ToString()}\{today.Month.ToString("00")}\{today.Day.ToString("00")}";
    if (!Directory.Exists(path))
    {
        Console.WriteLine("Редактирование не удалось");
        return;
    }
    var fileName = GetFileName(path);
    if (fileName == null) return;

    string src = @$"{path}\{fileName}";  // полный путь к файлу

    string newFolder = "Копии";
    string outputFolderPath = System.IO.Path.Combine(
       Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
       newFolder
    );
    string dest = Path.Combine(outputFolderPath,newFolder, fileName);  // путь для сохранения нового файла

    // Проверка существования файла
    if (!File.Exists(src))
    {
        Console.WriteLine($"Файл {src} не найден.");
        return;
    }
    // Открытие документа для редактирования
    //using (PdfReader reader = new PdfReader(src))
    //using (PdfWriter writer = new PdfWriter(temp))
    //{
    //    // Открытие PDF для работы
    //    PdfDocument pdfDoc = new PdfDocument(reader, writer);
    //    PdfPage page = pdfDoc.GetPage(1);

    //    // Получение PdfCanvas для рисования
    //    var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);

    //    // Убираем старый текст, рисуя белый прямоугольник
    //    canvas.SetFillColor(ColorConstants.WHITE);
    //    canvas.Rectangle(100, 640, 200, 30); // Позиция текста
    //    canvas.Fill();

    //    // Добавляем новый текст


    //    // Закрытие документа
    //    pdfDoc.Close();
    //}
    using (PdfReader reader = new PdfReader(src))
    using (PdfWriter writer = new PdfWriter(dest))
    {
        // Открытие PDF для работы
        PdfDocument pdfDoc = new PdfDocument(reader, writer);
        PdfPage page = pdfDoc.GetPage(1);

        // Получение PdfCanvas для рисования
        var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);
        var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
        canvas.BeginText()
              .SetFontAndSize(font, 12)
              .MoveText(100, 658) // Новая позиция для текста
              .ShowText(textToWrite)
              .EndText();

        pdfDoc.Close();
    }
    Console.WriteLine("Редактирование завершено.");
}
static void CreateOutputFolder(string path)
{
    if (!System.IO.Directory.Exists(path))
    {
        try
        {
            System.IO.Directory.CreateDirectory(path);
        }
        catch (IOException ie)
        {
            Console.WriteLine("IO Error: " + ie.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("General Error: " + e.Message);
        }
    }
}
static string GetFileName(string path)
{

    // Получаем все файлы в указанной директории
    var files = new DirectoryInfo(path).GetFiles("*.pdf");

    // Находим файл с самым поздним временем создания
    var lastAddedFile = files.OrderByDescending(f => f.CreationTime).FirstOrDefault();

    if (lastAddedFile != null)
    {
        return lastAddedFile.Name;
    }
    else
    {
        Console.WriteLine("В папке нет файлов.");
        return null;
    }
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
    }
}
static async Task<Standarts> PrintStandarts()
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

