using System;
using System.Collections.Generic;//для коллекций
using System.IO;//работа с файлом
using System.Linq;

public class TaskManager //класс для управления задачами
{
    private readonly List<ToDoTask> _tasks = new List<ToDoTask>();//коллекция задач
    private readonly HashSet<int> _usedIds = new HashSet<int>(); //коллекция айдишников
    private readonly Random _random = new Random();//рандомайзер для айди
    private const string FilePath = "tasks.txt";//для сохранения

    public TaskManager()
    {
        LoadTasks();
    }

    //добавление задачис учетом вида
    public void AddTask(string description, DateTime dueDate, string taskType = "default", string? project = null, int priority = 1)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание задачи не может быть пустым");

        ToDoTask task;
        int id = GenerateId();
        //выбор типа задачи
        switch (taskType.ToLower())
        {
            case "work":
                if (string.IsNullOrWhiteSpace(project))
                    throw new ArgumentException("Для рабочей задачи необходимо указать проект");
                task = new WorkTask(id, description, dueDate, project!);
                break;

            case "personal":
                if (priority < 1 || priority > 10)
                    throw new ArgumentException("Приоритет должен быть от 1 до 10");
                task = new PersonalTask(id, description, dueDate, priority);
                break;

            default:
                task = new ToDoTask(id, description, dueDate);
                break;
        }
        //добавляем в список и сохраняем
        _tasks.Add(task);
        SaveTasks();
    }

    //поиск по айди
    public bool MarkTaskAsDone(int taskId)
    {
        var task = FindTaskById(taskId);
        if (task == null) return false;

        task.MarkAsCompleted();
        SaveTasks();
        return true;
    }

    public bool RemoveTask(int taskId)
    {
        var task = FindTaskById(taskId);
        if (task == null) return false;
        //удаляем из списка задач и айди
        _tasks.Remove(task);
        _usedIds.Remove(task.Id);
        SaveTasks();
        return true;
    }

    //сортируем рабочие-личные-обычные(по сроку и приоритету)
    public IReadOnlyCollection<ToDoTask> GetAllTasks() //только для чтения 
    {
        return _tasks
            .OrderBy(t => t is WorkTask ? 0 : t is PersonalTask ? 1 : 2)//по типу с проверкой типа объекта 
            .ThenByDescending(t => (t as PersonalTask)?.Priority ?? 0)//по убыванию приоритета для личных с приведением типа 
            .ThenBy(t => t.DueDate)//по сроку от ближайшего
            .ToList()//в коллекцию
            .AsReadOnly();//нельзя изменить
    }

    //группируем рабочие по проектам и возращаем группами
    public IEnumerable<IGrouping<string, WorkTask>> GetWorkTasksByProject()//коллекция групп с ключом по названию проекта и списком задач в этом проекте
    {
        return _tasks.OfType<WorkTask>() //только рабочие задачи
                   .GroupBy(t => t.Project) //группировка по ключу(названию) на группы
                   .OrderBy(g => g.Key);//сортировка групп по алфавиту
    }

    //возврат всех проектов
    public IEnumerable<string> GetAllProjects()//список всех названий проектов
    {
        return _tasks.OfType<WorkTask>()//только рабочие выбирает
                   .Select(t => t.Project)//извлекает название проекта из каждой
                   .Distinct() //убирает повторы названий 
                   .OrderBy(p => p);//сортировка по алфавиту
    }

    //сортировка личных по приоритету и сроку
    public IEnumerable<ToDoTask> GetTasksWithPriority(int minPriority)
    {
        return _tasks.OfType<PersonalTask>()//только личные
                   .Where(t => t.Priority >= minPriority && !t.IsCompleted)//по приоритету ток невыполненные
                   .OrderByDescending(t => t.Priority)//сначала задачи с высоким приоритетом
                   .ThenBy(t => t.DueDate);//потом по сроку выполнения
    }

    //ищет задачу по айди с возможным значением null
    private ToDoTask? FindTaskById(int taskId) => _tasks.FirstOrDefault(t => t.Id == taskId);

    //генерирует случайный айди и проверяет чтобы не повторялся
    private int GenerateId()
    {
        int newId;
        do
        {
            newId = _random.Next(100, 1000);
        } while (_usedIds.Contains(newId));

        _usedIds.Add(newId);
        return newId;
    }

    //загружает из файла
    private void LoadTasks()
    {
        if (!File.Exists(FilePath)) return;
        //перебирает каждую строку по очереди начиная с текущей и возращает массив строк
        foreach (string line in File.ReadAllLines(FilePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;//пропускает пустые
            //разбивает и проверяет строку на корректное количество частей 
            var parts = line.Split('|');
            if (parts.Length < 4) continue;

            try
            {
                ToDoTask task;
                //извлечение данных
                int id = int.Parse(parts[0]);
                string description = parts[1];
                DateTime dueDate = new DateTime(long.Parse(parts[2]));
                bool isCompleted = bool.Parse(parts[3]);
                //определение типа задачи
                if (parts.Length == 6 && parts[4] == "Work")
                {
                    task = new WorkTask(id, description, dueDate, parts[5], isCompleted);
                }
                else if (parts.Length == 6 && parts[4] == "Personal")
                {
                    task = new PersonalTask(id, description, dueDate, int.Parse(parts[5]), isCompleted);
                }
                else
                {
                    task = new ToDoTask(id, description, dueDate, isCompleted);
                }

                _tasks.Add(task);
                _usedIds.Add(task.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке задачи: {ex.Message}");
            }
        }
    }

    //сохраняет задачи в файл
    private void SaveTasks()
    {
        try
        {
            var lines = _tasks.Select(x => //преобразование каждой задачив строку 
            {
                string typeInfo = x switch //определяет тип и добавляет к нужному типу
                {
                    WorkTask workTask => $"|Work|{workTask.Project}",
                    PersonalTask personalTask => $"|Personal|{personalTask.Priority}",
                    _ => "|Default"
                };
                //формирование строки
                return $"{x.Id}|{x.Description}|{x.DueDate.Ticks}|{x.IsCompleted}{typeInfo}";
            });
            //записываем в файл для каждой задачи одна строка
            File.WriteAllLines(FilePath, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении задач: {ex.Message}");
        }
    }
}