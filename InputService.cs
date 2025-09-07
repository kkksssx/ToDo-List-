using System;
using System.Collections.Generic;//содержит классы и интерфейсы для работы с типобезопасными коллекциями
using System.Linq;//Для работы с LINQ (Language Integrated Query) - технологии запросов к коллекциям и данным

//статический сервис для обработки пользовательскоо ввода с валидацией
public static class InputService
{
    //основной метод для получения валидированного ввода
    public static T GetValidatedInput<T>(string prompt, Func<string, T> validator)
    {
        while (true)
        {
            try
            {
                if (!string.IsNullOrEmpty(prompt))
                    Console.Write(prompt);

                string? input = Console.ReadLine();

                if (input == null)
                    throw new ArgumentException("Ввод не может быть пустым");

                return validator(input);
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Ошибка валидации: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}. Попробуйте еще раз.");
            }
        }
    }

    //метод для получения описания с валидацией
    public static string GetDescriptionInput(string fieldName = "Описание")
    {
        return GetValidatedInput(
            $"\nВведите {fieldName.ToLower()}: ",
            input =>
            {
                if (string.IsNullOrWhiteSpace(input))
                    throw new ValidationException($"{fieldName} не может быть пустым");

                string trimmedInput = input.Trim();
                if (trimmedInput.Length > 500)
                    throw new ValidationException($"{fieldName} не может превышать 500 символов");

                return trimmedInput;
            }
        );
    }

    //метод для получения даты выполнения с валидацией
    public static DateTime GetDueDateInput(bool allowEmpty = true)
    {
        return GetValidatedInput(
            "Введите срок выполнения (дд.мм.гггг или Enter для пропуска): ",
            input =>
            {
                if (string.IsNullOrEmpty(input) && allowEmpty)
                    return DateTime.MinValue;

                if (DateTime.TryParse(input, out DateTime dueDate))
                {
                    if (dueDate != DateTime.MinValue && dueDate < DateTime.Today)
                        throw new ValidationException("Дата выполнения должна быть в настоящем или будущем");

                    return dueDate;
                }

                throw new ValidationException("Неверный формат даты. Используйте формат дд.мм.гггг");
            }
        );
    }

    //метод для получения приоритета с валидацией
    public static int GetPriorityInput()
    {
        return GetValidatedInput(
            "Введите приоритет (1-10): ",
            input =>
            {
                if (int.TryParse(input, out int priority))
                {
                    if (priority < 1 || priority > 10)
                        throw new ValidationException("Приоритет должен быть от 1 до 10");

                    return priority;
                }

                throw new ValidationException("Приоритет должен быть числом от 1 до 10");
            }
        );
    }

    public static int GetTaskIdInput() //метод для получения айди задачи с валидацией
    {
        return GetValidatedInput(
            "Введите ID задачи: ",
            input =>
            {
                if (int.TryParse(input, out int id) && id > 0)
                    return id;

                throw new ValidationException("ID должен быть положительным числом");
            }
        );
    }

    public static int GetProjectIdInput()//метод для получения ID проекта с валидацией
    {
        return GetValidatedInput(
            "Введите ID проекта: ",
            input =>
            {
                if (int.TryParse(input, out int id) && id > 0)
                    return id;

                throw new ValidationException("ID проекта должен быть положительным числом");
            }
        );
    }

    public static bool GetConfirmationInput(string message)//метод для получения подтверждения
    {
        return GetValidatedInput(
            $"{message} (y/n): ",
            input =>
            {
                var confirmations = new[] { "y", "yes", "д", "да", "1", "true" };
                return confirmations.Contains(input.Trim().ToLower());
            }
        );
    }

    //метод для получения названия проекта с проверкой уникальности
    public static string GetProjectNameInput(ProjectManager projectManager, int? existingProjectId = null)
    {
        return GetValidatedInput(
            "Введите название проекта: ",
            input =>
            {
                if (string.IsNullOrWhiteSpace(input))
                    throw new ValidationException("Название проекта не может быть пустым");

                string trimmedInput = input.Trim();
                if (trimmedInput.Length > 100)
                    throw new ValidationException("Название проекта не может превышать 100 символов");

                // Проверка уникальности имени
                var existingProject = projectManager.FindProjectByName(trimmedInput);
                if (existingProject != null && existingProject.Id != existingProjectId)
                    throw new ValidationException("Проект с таким названием уже существует");

                return trimmedInput;
            }
        );
    }
}