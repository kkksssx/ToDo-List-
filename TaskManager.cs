using Microsoft.VisualBasic;
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
    private readonly ProjectManager _projectManager;//ссылка на менеджер проетов

    //конструктор
    public TaskManager(ProjectManager projectManager)
    {
        _projectManager = projectManager;
        //установка обратной ссылки
        _projectManager.SetTaskManager(this);
        LoadTasks();
    }


    //добавление задачис учетом вида
    public void AddTask(string description, DateTime dueDate, TaskType taskType, string? project = null, int priority = 1)
    {
        int id = GenerateId(); // Генерируем уникальный ID
        var task = TaskFactory.CreateTask(taskType, description, dueDate, project, priority, false, id);
        _tasks.Add(task);

        // Если это рабочая задача с проектом, добавляем в проект
        if (task is WorkTask workTask && !string.IsNullOrEmpty(project) && _projectManager != null)
        {
            var existingProject = _projectManager.FindProjectByName(project);
            if (existingProject == null)
            {
                _projectManager.AddProject(project, $"Проект для задач: {project}");
                existingProject = _projectManager.FindProjectByName(project);
            }

            if (existingProject != null)
            {
                // ИСПРАВЛЕННЫЙ ВЫЗОВ - передаем название проекта, а не ID
                _projectManager.AddTaskToProjectSafe(project, task.Id); // project - string, а не existingProject.Id - int
            }
        }

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
//удаление хадачи
    public bool RemoveTask(int taskId)
    {
        var task = FindTaskById(taskId);
        if (task == null) return false;

        //удаляем задачу из проектов
        if (task is WorkTask workTask && !string.IsNullOrEmpty(workTask.Project))
        {
            //передаем название проекта, а не айди проекта
            _projectManager?.RemoveTaskFromProjectSafe(workTask.Project, taskId);
        }

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
    //получение задач по проекту
    public IEnumerable<ToDoTask> GetTasksByProject(string projectName)
    {
        return _tasks.OfType<WorkTask>()
                    .Where(t => t.Project == projectName)
                    .Cast<ToDoTask>();
    }
    //ищет задачу по айди с возможным значением null
    public ToDoTask? FindTaskById(int taskId) => _tasks.FirstOrDefault(t => t.Id == taskId);

    //генерирует случайный айди и проверяет чтобы не повторялся
    private int GenerateId()
    {
        int newId;
        int attempts = 0;
        const int maxAttempts = 10; // Защита от бесконечного цикла

        do
        {
            newId = _random.Next(100, 1000); 
            attempts++;

            if (attempts > maxAttempts)
            {
                throw new InvalidOperationException("Не удалось сгенерировать уникальный ID");
            }
        } while (_usedIds.Contains(newId));

        _usedIds.Add(newId);
        return newId;
    }

    //загружает из файла
    private void LoadTasks()
    {
        if (!File.Exists(FilePath)) return;

        foreach (string line in File.ReadAllLines(FilePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('|');
            if (parts.Length < 4) continue;

            try
            {
                ToDoTask task;
                int id = int.Parse(parts[0]);
                string description = parts[1];
                DateTime dueDate = new DateTime(long.Parse(parts[2]));
                bool isCompleted = bool.Parse(parts[3]);

                //проверяем на дубликат 
                if (_usedIds.Contains(id))
                {
                    Console.WriteLine($"Обнаружен дубликат ID: {id}. Задача будет пропущена.");
                    continue;
                }

                //добавляем айди в коллекцию использованных
                _usedIds.Add(id);

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке задачи: {ex.Message}");
            }
        }

        //после загрузки проверяем и исправляем дубликаты
        FixDuplicateIds();
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

    //изменения задачи 
    public bool EditTask(int taskId, string? newDescription, DateTime? newDueDate, string? newProject, int? newPriority)
    {
        var task = FindTaskById(taskId);
        if (task == null) return false;

        //запоминаем старый проект для рабочих задач
        string? oldProject = (task as WorkTask)?.Project;
        bool changesMade = false;

        //изменение описания
        if (!string.IsNullOrWhiteSpace(newDescription))
        {
            task.Description = newDescription;
            changesMade = true;
        }

        //изменение даты выполнения
        if (newDueDate.HasValue)
        {
            task.DueDate = newDueDate.Value;
            changesMade = true;
        }

        //изменение проекта для рабочих задач
        if (task is WorkTask workTask && newProject != null)
        {
            if (string.IsNullOrWhiteSpace(newProject))
                throw new ArgumentException("Проект не может быть пустым");

            workTask.Project = newProject;
            changesMade = true;
        }

        //изменение приоритета для личных задач
        if (task is PersonalTask personalTask && newPriority.HasValue)
        {
            if (newPriority < 1 || newPriority > 10)
                throw new ArgumentException("Приоритет должен быть от 1 до 10");

            personalTask.Priority = newPriority.Value;
            changesMade = true;
        }

        //безопасное обновление привязки к проектам
        UpdateProjectAssociation(taskId, task, oldProject, newProject);

        if (changesMade)
        {
            SaveTasks();
        }

        return changesMade;
    }

    private void UpdateProjectAssociation(int taskId, ToDoTask task, string? oldProject, string? newProject)
    {
        //работаем только с projectManager и рабочими задачами
        if (_projectManager == null || task is not WorkTask) return;

        //если проект не изменился ничего не делаем
        if (oldProject == newProject) return;

        //удаляем из старого проекта
        _projectManager.RemoveTaskFromProjectSafe(oldProject, taskId);

        //добавляем в новый 
        _projectManager.AddTaskToProjectSafe(newProject, taskId);
    }
    public bool IsIdUnique(int id) => !_usedIds.Contains(id);

    // для исправления дубликатов
    public void FixDuplicateIds()
    {
        var duplicates = _tasks
            .GroupBy(t => t.Id)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in duplicates)
        {
            //первую задачу оставляем с оригинальным 
            var firstTask = group.First();

            //остальным задачам назначаем новые
            foreach (var task in group.Skip(1))
            {
                _tasks.Remove(task);
                _usedIds.Remove(task.Id);

                int newId = GenerateId();

                //создаем новую задачу с тем же содержимым но новым айди
                ToDoTask newTask;
                if (task is WorkTask workTask)
                {
                    newTask = new WorkTask(newId, workTask.Description, workTask.DueDate, workTask.Project, workTask.IsCompleted);
                }
                else if (task is PersonalTask personalTask)
                {
                    newTask = new PersonalTask(newId, personalTask.Description, personalTask.DueDate, personalTask.Priority, personalTask.IsCompleted);
                }
                else
                {
                    newTask = new ToDoTask(newId, task.Description, task.DueDate, task.IsCompleted);
                }

                _tasks.Add(newTask);
            }
        }

        if (duplicates.Any())
        {
            SaveTasks();
            Console.WriteLine($"Исправлено {duplicates.Count} дубликатов ID");
        }
    }
}
