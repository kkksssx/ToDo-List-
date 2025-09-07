using FluentValidation;//для валидатора
using FluentValidation.Results;//результатов валидации
using System;
using System.Linq;//Для работы с LINQ (Language Integrated Query) - технологии запросов к коллекциям и данным

//валидатор для проектов
public class ProjectValidator : AbstractValidator<Project>
{
    public ProjectValidator()
    {   //правило для названия проекта
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название проекта не может быть пустым")
            .MaximumLength(100).WithMessage("Название проекта не может превышать 100 символов");
        //для описания
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание проекта не может быть пустым")
            .MaximumLength(500).WithMessage("Описание проекта не может превышать 500 символов");
        //приоритета
        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10).WithMessage("Приоритет должен быть от 1 до 10");
        //для срока
        RuleFor(x => x.Deadline)
            .GreaterThanOrEqualTo(DateTime.Today)
            .When(x => x.Deadline.HasValue)
            .WithMessage("Срок проекта не может быть в прошлом");
    }
}

//статический сервис для валидации проектов
public static class ProjectValidationService
{
    // Создаем единственный экземпляр валидатора (паттерн Singleton)
    private static readonly ProjectValidator _validator = new();

    public static ValidationResult ValidateProject(Project project) //метод для валидации всего проекта
    {
        return _validator.Validate(project);
    }

    // Метод для валидации и автоматического выброса исключения при ошибках
    public static void ValidateAndThrow(Project project)
    {
        var result = ValidateProject(project);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    // Отдельные методы для валидации отдельных свойств проекта
    public static void ValidateProjectName(string name)// Проверка названия проекта
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Название проекта не может быть пустым");

        if (name.Length > 100)
            throw new ValidationException("Название проекта не может превышать 100 символов");
    }

    public static void ValidateProjectDescription(string description)//описания
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Описание проекта не может быть пустым");

        if (description.Length > 500)
            throw new ValidationException("Описание проекта не может превышать 500 символов");
    }

    public static void ValidateProjectPriority(int priority)//приоритета
    {
        if (priority < 1 || priority > 10)
            throw new ValidationException("Приоритет проекта должен быть от 1 до 10");
    }

    public static void ValidateProjectDeadline(DateTime? deadline)//срока
    {
        if (deadline.HasValue && deadline.Value < DateTime.Today)
            throw new ValidationException("Срок проекта не может быть в прошлом");
    }

    public static void ValidateProjectId(int projectId)//айди
    {
        if (projectId <= 0)
            throw new ValidationException("ID проекта должен быть положительным числом");
    }
}