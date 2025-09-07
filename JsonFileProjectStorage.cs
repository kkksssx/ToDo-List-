using System;
using System.Collections.Generic;//содержит классы и интерфейсы для работы с типобезопасными коллекциями
using System.IO;// Для работы с файловой системой, потоками данных и операциями ввода-вывода.
using System.Text.Json;// Для сериализации и десериализации JSON (современная альтернатива Newtonsoft.Json).

//класс для сохранения проектов в JSON файл
public class JsonFileProjectStorage : IProjectStorage
{
    private const string FilePath = "projects.json";

    public List<Project> LoadProjects()
    {
        if (!File.Exists(FilePath))
            return new List<Project>();

        try
        {
            //читаем весь текст из файла
            string json = File.ReadAllText(FilePath);
            var options = new JsonSerializerOptions //настраиваем параметры для чтения JSON
            {
                PropertyNameCaseInsensitive = true, //игнорируем регистр букв в названиях свойств
                WriteIndented = true
            };
            //преобразует JSON текст обратно в список проектов
            //если что-то не так вернем пустой список вместо ошибки
            return JsonSerializer.Deserialize<List<Project>>(json, options) ?? new List<Project>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке проектов: {ex.Message}");
            return new List<Project>();
        }
    }

    //метод для сохранения проектов в файл
    public void SaveProjects(List<Project> projects)
    {
        try
        {
            //настраиваем параметры для записи JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, //отступы в файле
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase //названия свойств в camelCase (например: "projectName")
            };

            string json = JsonSerializer.Serialize(projects, options);//преобразуем список проектов в JSON текст
            File.WriteAllText(FilePath, json);//записываем JSON текст в файл (перезаписываем весь файл)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении проектов: {ex.Message}");
        }
    }
}