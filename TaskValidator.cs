using System;
using System.Collections.Generic;
using System.Linq;

public static class TaskValidator
{
    //БАЗОВАЯ ВАЛИДАЦИЯ

    /// Проверяет описание задачи
    public static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание задачи не может быть пустым");

        if (description.Length > 500)
            throw new ArgumentException("Описание не может превышать 500 символов");
    }

    /// Проверяет дату выполнения
    public static void ValidateDueDate(DateTime dueDate)
    {
        if (dueDate != DateTime.MinValue && dueDate < DateTime.Today)
            throw new ArgumentException("Дата выполнения должна быть в настоящем или будущем");
    }

    /// Проверяет проект для рабочих задач
    public static void ValidateProject(string project)
    {
        if (string.IsNullOrWhiteSpace(project))
            throw new ArgumentException("Проект не может быть пустым");

        if (project.Length > 100)
            throw new ArgumentException("Название проекта не может превышать 100 символов");
    }

    /// Проверяет приоритет для личных задач
    public static void ValidatePriority(int priority)
    {
        if (priority < 1 || priority > 10)
            throw new ArgumentException("Приоритет должен быть от 1 до 10");
    }

    /// Проверяет новую дату при редактировании
    public static void ValidateNewDueDate(DateTime newDueDate)
    {
        if (newDueDate < DateTime.Today)
            throw new ArgumentException("Новая дата не может быть в прошлом");
    }

    //ВАЛИДАЦИЯ ВВОДА ИЗ КОНСОЛИ

    /// Проверяет и возвращает валидное описание задачи
    public static string ValidateAndGetDescription(string? input, string fieldName = "Описание")
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException($"{fieldName} не может быть пустым");

        string trimmedInput = input.Trim();

        if (trimmedInput.Length > 500)
            throw new ArgumentException($"{fieldName} не может превышать 500 символов");

        return trimmedInput;
    }

    /// Проверяет и возвращает дату 
    public static DateTime ValidateAndGetDueDate(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return DateTime.MinValue; // Без срока

        if (DateTime.TryParse(input, out DateTime dueDate))
        {
            ValidateDueDate(dueDate);
            return dueDate;
        }

        throw new ArgumentException("Неверный формат даты. Используйте формат дд.мм.гггг");
    }

    /// Проверяет и возвращает валидный проект
    public static string ValidateAndGetProject(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Проект не может быть пустым");

        string trimmedInput = input.Trim();

        if (trimmedInput.Length > 100)
            throw new ArgumentException("Название проекта не может превышать 100 символов");

        return trimmedInput;
    }

    /// Проверяет приоритет
    public static int ValidateAndGetPriority(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Приоритет не может быть пустым");

        if (int.TryParse(input, out int priority))
        {
            ValidatePriority(priority);
            return priority;
        }

        throw new ArgumentException("Приоритет должен быть числом от 1 до 10");
    }

    /// Проверяет ID задачи
    public static int ValidateAndGetTaskId(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("ID не может быть пустым");

        if (int.TryParse(input, out int id) && id > 0)
            return id;

        throw new ArgumentException("ID должен быть положительным числом");
    }

    /// Проверяет выбор поля для редактирования
    public static string ValidateFieldChoice(string? input, ToDoTask task)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Необходимо выбрать поле для редактирования");

        var validChoices = new[] { "1", "2", "3", "0" };
        if (!validChoices.Contains(input))
            throw new ArgumentException("Неверный выбор. Введите 1, 2, 3 или 0");

        // Проверяем, доступно ли поле 3 для этого типа задачи
        if (input == "3")
        {
            if (task is not WorkTask && task is not PersonalTask)
                throw new ArgumentException("Это поле недоступно для данного типа задачи");
        }

        return input;
    }

    //ВАЛИДАЦИЯ ДЛЯ ОПЕРАЦИЙ 
    /// Проверяет наличие задач и возвращает список
    public static List<ToDoTask> ValidateTasksExist(IReadOnlyCollection<ToDoTask> tasks, string operationName)
    {
        if (tasks == null)
            throw new ArgumentNullException(nameof(tasks), "Список задач не может быть пустым");

        if (!tasks.Any())
            throw new InvalidOperationException($"Нет задач для {operationName}.");

        return tasks.ToList();
    }

    /// Проверяет существование задачи по ID
    public static ToDoTask ValidateTaskExists(ToDoTask? task, int taskId)
    {
        if (task == null)
            throw new ArgumentException($"Задача с ID {taskId} не найдена");

        return task;
    }

    /// Проверяет, что задача еще не выполнена
    public static void ValidateTaskNotCompleted(ToDoTask task, string operationName)
    {
        if (task.IsCompleted)
            throw new InvalidOperationException($"Нельзя {operationName} выполненную задачу");
    }

    // === ВАЛИДАЦИЯ РЕДАКТИРОВАНИЯ ===

    /// Проверяет параметры редактирования
    public static void ValidateTaskEditParameters(ToDoTask task, string? newDescription, DateTime? newDueDate, string? newProject, int? newPriority)
    {
        if (!string.IsNullOrWhiteSpace(newDescription))
        {
            ValidateDescription(newDescription);
        }

        if (newDueDate.HasValue)
        {
            ValidateNewDueDate(newDueDate.Value);
        }

        if (newProject != null && task is WorkTask)
        {
            ValidateProject(newProject);
        }

        if (newPriority.HasValue && task is PersonalTask)
        {
            ValidatePriority(newPriority.Value);
        }
    }

    /// Проверяет, были ли переданы параметры для редактирования
    public static bool HasEditParameters(string? newDescription, DateTime? newDueDate, string? newProject, int? newPriority)
    {
        return !string.IsNullOrWhiteSpace(newDescription) ||
               newDueDate.HasValue ||
               !string.IsNullOrWhiteSpace(newProject) ||
               newPriority.HasValue;
    }

    /// Проверяет доступность поля для редактирования для данного типа задачи
    public static void ValidateFieldAccessibility(ToDoTask task, string fieldName)
    {
        switch (fieldName)
        {
            case "Project" when task is not WorkTask:
                throw new ArgumentException("Редактирование проекта доступно только для рабочих задач");

            case "Priority" when task is not PersonalTask:
                throw new ArgumentException("Редактирование приоритета доступно только для личных задач");

            case "DueDate" when task.IsCompleted:
                throw new ArgumentException("Нельзя изменять дату у выполненной задачи");
        }
    }

    // === УНИВЕРСАЛЬНЫЕ МЕТОДЫ ===

    /// Универсальный метод для ввода с валидацией и повторными попытками
    public static T GetValidatedInput<T>(string prompt, Func<string, T> validator)
    {
        while (true)
        {
            try
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (input == null)
                    throw new ArgumentException("Ввод не может быть пустым");

                return validator(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}. Попробуйте еще раз.");
            }
        }
    }

    /// Проверяет подтверждение действия (y/n)
    public static bool ValidateConfirmation(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var confirmations = new[] { "y", "yes", "д", "да", "1", "true" };
        return confirmations.Contains(input.Trim().ToLower());
    }

    // === КОМПЛЕКСНАЯ ВАЛИДАЦИЯ ===

    /// Комплексная валидация параметров задачи по типу
    public static void ValidateTaskParameters(TaskType taskType, string description, DateTime dueDate, string? project = null, int? priority = null)
    {
        ValidateDescription(description);
        ValidateDueDate(dueDate);

        switch (taskType)
        {
            case TaskType.Work:
                if (project == null)
                    throw new ArgumentException("Для рабочей задачи необходимо указать проект");
                ValidateProject(project);
                break;

            case TaskType.Personal:
                if (!priority.HasValue)
                    throw new ArgumentException("Для личной задачи необходимо указать приоритет");
                ValidatePriority(priority.Value);
                break;
        }
    }
    public static void ValidateEditParameters(string title, string description, DateTime dueDate, bool isCompleted)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название задачи не может быть пустым");

        if (title.Length > 100)
            throw new ArgumentException("Название задачи не может превышать 100 символов");

        if (description != null && description.Length > 500)
            throw new ArgumentException("Описание не может превышать 500 символов");

        if (dueDate < DateTime.Now.Date)
            throw new ArgumentException("Дата выполнения не может быть в прошлом");
    }
}