using System;

//класс-фабрика для создания объектов проектов
public static class ProjectFactory//фабричный паттерн = централизовнанное создание объектов
{    //создаём объектв не используя new, а обращаясь к статическим членам
    private static int _nextId = 1;

    //создание проекта
    public static Project CreateProject(
        string name,
        string description,
        DateTime? deadline = null,
        int priority = 1,
        bool isCompleted = false)
    {
        ValidateProjectParameters(name, description, priority);

        return new Project(_nextId++, name, description, deadline, priority, isCompleted);
    }

    //создание проекта из задачи
    public static Project CreateProjectFromTask(WorkTask task, string projectName, string projectDescription)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        ValidateProjectParameters(projectName, projectDescription, 5);

        var project = CreateProject(projectName, projectDescription);
        project.AddTask(task.Id);
        return project;
    }

    //валидация параметров
    private static void ValidateProjectParameters(string name, string description, int priority)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название проекта не может быть пустым");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание проекта не может быть пустым");

        if (priority < 1 || priority > 10)
            throw new ArgumentException("Приоритет проекта должен быть от 1 до 10");
    }

    public static int GetNextId() => _nextId; //возращает следующий айди которыый будет использован
    public static void ResetIdCounter() => _nextId = 1;
}