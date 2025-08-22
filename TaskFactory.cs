using System;

//статический класс-фабрика для создания объектов задач
public static class TaskFactory//фабричный паттерн = централизовнанное создание объектов
{//создаём объектв не используя new, а обращаясь к статическим членам
    public static ToDoTask CreateTask( //создание задачи основной метод
        TaskType taskType,
        string description,
        DateTime dueDate,
        string? project = null,
        int priority = 1,
        bool isCompleted = false,
        int id = 0) 
    {
        ValidateParameters(description, dueDate, taskType, project, priority);
        //выбор типа создаваемой задачи
        return taskType switch
        {
            TaskType.Default => CreateDefaultTask(id, description, dueDate, isCompleted),
            TaskType.Work => CreateWorkTask(id, description, dueDate, project!, isCompleted),
            TaskType.Personal => CreatePersonalTask(id, description, dueDate, priority, isCompleted),
            _ => throw new ArgumentException("Неизвестный тип задачи")
        };
    }

    //для обычной задачи
    private static ToDoTask CreateDefaultTask(int id, string description, DateTime dueDate, bool isCompleted)
    {
        TaskValidator.ValidateDescription(description);
        TaskValidator.ValidateDueDate(dueDate);

        return new ToDoTask(id, description, dueDate, isCompleted);
    }

    //для рабочей
    private static WorkTask CreateWorkTask(int id, string description, DateTime dueDate, string project, bool isCompleted)
    {
        TaskValidator.ValidateDescription(description);
        TaskValidator.ValidateDueDate(dueDate);
        TaskValidator.ValidateProject(project);

        return new WorkTask(id, description, dueDate, project, isCompleted);
    }

    //для личной
    private static PersonalTask CreatePersonalTask(int id, string description, DateTime dueDate, int priority, bool isCompleted)
    {
        TaskValidator.ValidateDescription(description);
        TaskValidator.ValidateDueDate(dueDate);
        TaskValidator.ValidatePriority(priority);

        return new PersonalTask(id, description, dueDate, priority, isCompleted);
    }

    //валидация параметров
    private static void ValidateParameters(string description, DateTime dueDate, TaskType taskType, string? project, int priority)
    {
        if (taskType == TaskType.Work && string.IsNullOrEmpty(project))
            throw new ArgumentException("Для рабочей задачи необходимо указать проект");

        if (taskType == TaskType.Personal && (priority < 1 || priority > 10))
            throw new ArgumentException("Приоритет должен быть от 1 до 10");
    }
}