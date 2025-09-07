using FluentValidation;//для валидатора
using FluentValidation.Results;//для результатов валидации
using System;
using System.Collections.Generic;//для коллекций
using System.Linq;//Для работы с LINQ (Language Integrated Query) - технологии запросов к коллекциям и данным

//валитор для базового класса задач
public class ToDoTaskValidator : AbstractValidator<ToDoTask>
{
    public ToDoTaskValidator()
    {   //правило для описания зачачи
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание задачи не может быть пустым")
            .MaximumLength(500).WithMessage("Описание не может превышать 500 символов");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .When(x => x.DueDate != DateTime.MinValue)
            .WithMessage("Дата выполнения должна быть в настоящем или будущем");
    }
}

//валидатор для рабочих задач (наследует от базового валидатора)
public class WorkTaskValidator : AbstractValidator<WorkTask>
{
    public WorkTaskValidator()
    {
        Include(new ToDoTaskValidator());

        RuleFor(x => x.Project)
            .NotEmpty().WithMessage("Проект не может быть пустым")
            .MaximumLength(100).WithMessage("Название проекта не может превышать 100 символов");
    }
}

// Валидатор для личных задач
public class PersonalTaskValidator : AbstractValidator<PersonalTask>
{
    public PersonalTaskValidator()
    {
        Include(new ToDoTaskValidator());

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10).WithMessage("Приоритет должен быть от 1 до 10");
    }
}


// Статический сервис для валидации задач 
public static class TaskValidationService
{
    // Создаем экземпляры валидаторов (один раз при загрузке класса)
    private static readonly ToDoTaskValidator _baseValidator = new();
    private static readonly WorkTaskValidator _workValidator = new();
    private static readonly PersonalTaskValidator _personalValidator = new();

    // Метод для валидации задачи с автоматическим определением типа
    public static ValidationResult ValidateTask(ToDoTask task)
    {
        return task switch
        {
            WorkTask workTask => _workValidator.Validate(workTask),
            PersonalTask personalTask => _personalValidator.Validate(personalTask),
            _ => _baseValidator.Validate(task)
        };
    }

    // Метод для валидации и автоматического выброса исключения при ошибках
    public static void ValidateAndThrow(ToDoTask task)
    {
        var result = ValidateTask(task);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    //отдельные методы для валидации отдельных свойств
    public static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Описание задачи не может быть пустым");

        if (description.Length > 500)
            throw new ValidationException("Описание не может превышать 500 символов");
    }

    public static void ValidateDueDate(DateTime dueDate)
    {
        if (dueDate != DateTime.MinValue && dueDate < DateTime.Today)
            throw new ValidationException("Дата выполнения должна быть в настоящем или будущем");
    }

    public static void ValidateProject(string project)
    {
        if (string.IsNullOrWhiteSpace(project))
            throw new ValidationException("Проект не может быть пустым");

        if (project.Length > 100)
            throw new ValidationException("Название проекта не может превышать 100 символов");
    }

    public static void ValidatePriority(int priority)
    {
        if (priority < 1 || priority > 10)
            throw new ValidationException("Приоритет должен быть от 1 до 10");
    }

    public static void ValidateTaskId(int taskId)
    {
        if (taskId <= 0)
            throw new ValidationException("ID должен быть положительным числом");
    }

    //метод для проверки существования задач
    public static List<ToDoTask> ValidateTasksExist(IReadOnlyCollection<ToDoTask> tasks, string operationName)
    {
        if (tasks == null || !tasks.Any())
            throw new ValidationException($"Нет задач для {operationName}.");

        return tasks.ToList();
    }

    // Метод для проверки существования конкретной задачи
    public static ToDoTask ValidateTaskExists(ToDoTask? task, int taskId)
    {
        if (task == null)
            throw new ValidationException($"Задача с ID {taskId} не найдена");

        return task;
    }

    // Метод для проверки что задача не выполнена
    public static void ValidateTaskNotCompleted(ToDoTask task, string operationName)
    {
        if (task.IsCompleted)
            throw new ValidationException($"Нельзя {operationName} выполненную задачу");
    }

    // Комплексная валидация всех параметров задачи
    public static void ValidateTaskParameters(TaskType taskType, string description, DateTime dueDate,
        string? project = null, int? priority = null)
    {
        ValidateDescription(description);
        ValidateDueDate(dueDate);

        switch (taskType)
        {
            case TaskType.Work:
                if (string.IsNullOrEmpty(project))
                    throw new ValidationException("Для рабочей задачи необходимо указать проект");
                ValidateProject(project!);
                break;

            case TaskType.Personal:
                if (!priority.HasValue)
                    throw new ValidationException("Для личной задачи необходимо указать приоритет");
                ValidatePriority(priority.Value);
                break;
        }
    }
}