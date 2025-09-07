using System;
using System.Collections.Generic;//коллекций
using System.IO;// Для работы с файловой системой, потоками данных и операциями ввода-вывода.
using System.Linq;//Для работы с LINQ (Language Integrated Query) - технологии запросов к коллекциям и данным
using System.Threading.Tasks;//фундаментальная часть .NET для асинхронного программирования

//класс для управления проектами
public class ProjectManager
{
    private readonly List<Project> _projects = new List<Project>();//коллекция хранения проектов
    private readonly HashSet<int> _usedIds = new HashSet<int>();//их айди
    // private const string FilePath = "projects.txt";
    private readonly Random _random = new Random();
    private readonly IProjectStorage _projectStorage;
    private TaskManager? _taskManager;//ссыдка на менеджер задач

    //для установки ссылки на менеджер задач после создания объекта
    public void SetTaskManager(TaskManager taskManager)
    {
        _taskManager = taskManager;
    }
    //конструктор инициализирует и загружает из файла проекты
    public ProjectManager(IProjectStorage projectStorage)
    {
        _projectStorage = projectStorage;
        LoadProjects();
    }

    //для добавления проекта
    public void AddProject(string name, string description, DateTime? deadline = null, int priority = 1)
    {
        //используем фабрику для сощдания объекта
        var project = ProjectFactory.CreateProject(name, description, deadline, priority);
        _projects.Add(project);//добавляет в коллекцию
        _usedIds.Add(project.Id);//айди*
        SaveProjects();
    }

    //добавление задачи в проект
    public bool AddTaskToProject(int projectId, int taskId)
    {
        var project = FindProjectById(projectId);//поиск проекта по айди
        if (project == null) return false;

        project.AddTask(taskId);
        SaveProjects();
        return true;
    }

    //удаление задачи из проекта
    public bool RemoveTaskFromProject(int projectId, int taskId)
    {
        var project = FindProjectById(projectId);
        if (project == null) return false;

        project.RemoveTask(taskId);//удаляем задачу

        //если в проекте не осталось задач
        if (project.TaskIds.Count == 0)
        {
            RemoveProject(projectId);//удаляет весь проект
        }
        else
        {
            SaveProjects();
        }

        return true;
    }

    //удаление проекта
    public bool RemoveProject(int projectId)
    {
        var project = FindProjectById(projectId);
        if (project == null) return false;

        // удаляем все задачи, связанные с этим проектом
        if (_taskManager != null)
        {
            //получаем все ID задач проекта
            var taskIdsToRemove = project.TaskIds.ToList();

            //удаляем все задачи проекта через менеджер задач
            foreach (var taskId in taskIdsToRemove)
            {
                _taskManager.RemoveTask(taskId);
            }
        }

        _projects.Remove(project);
        _usedIds.Remove(projectId);
        SaveProjects();
        return true;
    }

    //завершение проекта
    public bool MarkProjectAsCompleted(int projectId)
    {
        var project = FindProjectById(projectId);
        if (project == null) return false;

        project.MarkAsCompleted();
        SaveProjects();
        return true;
    }

    //для поиска и получения данных
    public Project? FindProjectById(int projectId) => _projects.FirstOrDefault(p => p.Id == projectId);

    public Project? FindProjectByName(string name) => _projects.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Project> GetAllProjects() => _projects.OrderBy(p => p.Name);

    public IEnumerable<Project> GetProjectsByPriority(int minPriority) =>
        _projects.Where(p => p.Priority >= minPriority).OrderByDescending(p => p.Priority);

    public IEnumerable<int> GetTaskIdsForProject(int projectId)
    {
        var project = FindProjectById(projectId);
        return project?.TaskIds ?? Enumerable.Empty<int>();
    }
    private IEnumerable<ToDoTask> GetActualTasksForProject(string projectName)
    {
        return _taskManager?.GetTasksByProject(projectName) ?? Enumerable.Empty<ToDoTask>();
    }
    public bool ProjectExists(string projectName) => _projects.Any(p => p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));

    //загрузка проектов
    private void LoadProjects()
    {
        _projects.Clear();
        _usedIds.Clear();

        var loadedProjects = _projectStorage.LoadProjects();
        foreach (var project in loadedProjects)
        {
            if (!_usedIds.Contains(project.Id))
            {
                _projects.Add(project);
                _usedIds.Add(project.Id);
            }
        }
    }

    //сохранение проектов
    private void SaveProjects()
    {
        _projectStorage.SaveProjects(_projects);
    }

    public void AddTaskToProjectSafe(string? projectName, int taskId)
    {
        if (string.IsNullOrEmpty(projectName)) return;

        var project = FindProjectByName(projectName);
        if (project == null)
        {
            AddProject(projectName, $"Проект для задач: {projectName}");
            project = FindProjectByName(projectName);
        }

        project?.AddTask(taskId);
        SaveProjects();
    }

    public void RemoveTaskFromProjectSafe(string? projectName, int taskId)
    {
        if (string.IsNullOrEmpty(projectName)) return;

        var project = FindProjectByName(projectName);
        if (project == null) return;

        project.RemoveTask(taskId);

        //проверяем, остались ли еще задачи в проекте
        if (project.TaskIds.Count == 0)
        {
            //удаляем проект только если в нем нет задач
            RemoveProject(project.Id);
        }
        else
        {
            SaveProjects();
        }
    }

}