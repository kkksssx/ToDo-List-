using System;
using System.Collections.Generic;//содержит классы и интерфейсы для работы с типобезопасными коллекциями
using System.IO;// Для работы с файловой системой, потоками данных и операциями ввода-вывода.
using System.Text.Json;// Для сериализации и десериализации JSON (современная альтернатива Newtonsoft.Json).


//класс для сохранения задач в JSON файл
//зеализует интерфейс ITaskStorage - умеет сохранять и загружать задачи
public class JsonFileTaskStorage : ITaskStorage
{
    private const string FilePath = "tasks.json";

    public List<ToDoTask> LoadTasks()
    {
        if (!File.Exists(FilePath))
            return new List<ToDoTask>();

        try
        {
            string json = File.ReadAllText(FilePath); //читаем весь текст из файла tasks.json
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            //преобразуем JSON текст обратно в список задач
            return JsonSerializer.Deserialize<List<ToDoTask>>(json, options) ?? new List<ToDoTask>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке задач: {ex.Message}");
            return new List<ToDoTask>();//возвращаем пустой список, чтобы программа не сломалась
        }
    }

    public void SaveTasks(List<ToDoTask> tasks)
    {
        try
        {
            var options = new JsonSerializerOptions//настраиваем параметры для записи JSON
            {
                WriteIndented = true,//делаем отступы для удобного чтения файла
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string json = JsonSerializer.Serialize(tasks, options);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении задач: {ex.Message}");
        }
    }
}