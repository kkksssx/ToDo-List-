using System;
using System.Threading.Tasks;//для работы с асинхронностью и задачами(.Run)
using System.Linq;//Для работы с LINQ (Language Integrated Query) - технологии запросов к коллекциям и данным
using System.Text;//Для работы со строками, кодировками и построения текста
using System.Collections.Generic;
using FluentValidation.Results;//для работы с результатами валидации, возвращаемыми библиотекой FluentValidation
using Microsoft.Extensions.DependencyInjection;
//директива в языке C#, которая импортирует пространство имен Microsoft.Extensions.DependencyInjection, содержащее типы для реализации паттерна "внедрение зависимостей" (Dependency Injection, DI)

//класс приложения с main
public class Program
{
    private static TaskManager _taskManager = null!;//для управления списком задач
    private static ReminderService? _reminderService;//для напоминаний, может быть null
    private static ProjectManager _projectManager = null!;//для менеджера проектов

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;//кодировка для поддержки юникод
        try
        {
            //настройка Dependency Injection- Внедрение зависимости (Dependency injection, DI) — процесс предоставления внешней зависимости программному компоненту
            var serviceProvider = ConfigureServices();

            //получение экземпляров через DI
            _projectManager = serviceProvider.GetService<ProjectManager>()!;
            _taskManager = serviceProvider.GetService<TaskManager>()!;

            //установка обратной ссылки
            _projectManager.SetTaskManager(_taskManager);

            _reminderService = new ReminderService(_taskManager);

            //запуск фоновой проверки напоминаний
            Task.Run(() => _reminderService.CheckReminders());

            Console.WriteLine("Система управления задачами с напоминаниями");

            MainMenuLoop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка инициализации: {ex.Message}");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        Console.WriteLine("Выберите тип хранилища:");
        Console.WriteLine("1 - InMemory (в памяти)");
        Console.WriteLine("2 - JsonFile (файлы JSON)");

        var choice = Console.ReadLine();

        if (choice == "1")
        {
            // InMemory хранилище (данные только в оперативной памяти)
            services.AddSingleton<ITaskStorage, InMemoryTaskStorage>();
            services.AddSingleton<IProjectStorage, InMemoryProjectStorage>();
        }
        else
        {
            // Файловое JSON хранилище (данные сохраняются в файлы)
            services.AddSingleton<ITaskStorage, JsonFileTaskStorage>();
            services.AddSingleton<IProjectStorage, JsonFileProjectStorage>();
        }
        //регистрация менеджеров
        services.AddSingleton<ProjectManager>();
        services.AddSingleton<TaskManager>();

        return services.BuildServiceProvider();//сборка провайдера сервисов
    }

    private static void MainMenuLoop()
    {
        while (true)
        {
            DisplayMenu();
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1": AddTask(); break;
                case "2": AddProject(); break;
                case "3": MarkTaskAsDone(); break;
                case "4": DisplayAllTasks(); break;
                case "5": DisplayAllProjects(); break;
                case "6": RemoveTask(); break;
                case "7": EditTask(); break;
                case "8": RemoveProject(); break;
                case "9":
                    _reminderService?.Dispose();
                    Console.WriteLine("Выход из программы...");
                    return;
                default:
                    Console.WriteLine("\nНеверный выбор, введите число от 1 до 9.");
                    WaitForUserInput();
                    break;
            }
        }
    }

    //выводменю
    private static void DisplayMenu()
    {
        Console.WriteLine("\n Список задач");
        Console.WriteLine("1. Добавить задачу");
        Console.WriteLine("2. Добавить проект");
        Console.WriteLine("3. Отметить задачу как выполненную");
        Console.WriteLine("4. Показать все задачи");
        Console.WriteLine("5. Показать все проекты");
        Console.WriteLine("6. Удалить задачу");
        Console.WriteLine("7. Редактировать задачу");
        Console.WriteLine("8. Удалить проект");
        Console.WriteLine("9. Выход");
        Console.WriteLine("==========================");
        Console.Write("Выберите действие: ");
    }

    private static void AddTask()
    {
        try
        {
            // Выбор типа задачи
            Console.WriteLine("\nВыберите тип задачи:");
            Console.WriteLine("1. Обычная задача");
            Console.WriteLine("2. Рабочая задача (с проектом)");
            Console.WriteLine("3. Личная задача (с приоритетом)");
            Console.Write("Ваш выбор: ");
            var typeChoice = Console.ReadLine();

            TaskType taskType = typeChoice switch
            {
                "1" => TaskType.Default,
                "2" => TaskType.Work,
                "3" => TaskType.Personal,
                _ => throw new ValidationException("Неверный тип задачи")
            };

            string? project = null;
            int priority = 1;

            //для рабочих задач
            if (taskType == TaskType.Work)
            {
                project = SelectOrCreateProject();
                if (project == null)
                {
                    Console.WriteLine("Создание задачи отменено.");
                    return;
                }
            }
            //для личных
            if (taskType == TaskType.Personal)
            {
                priority = InputService.GetPriorityInput();
            }

            string description = InputService.GetDescriptionInput("Описание задачи");
            DateTime dueDate = InputService.GetDueDateInput();

            _taskManager.AddTask(description, dueDate, taskType, project, priority);
            Console.WriteLine("\nЗадача добавлена успешно!");
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"\n Ошибка валидации: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n Ошибка: {ex.Message}");
        }
        finally
        {
            WaitForUserInput();
        }
    }

    //для выбора или создание проекта
    private static string? SelectOrCreateProject()
    {
        var existingProjects = _projectManager.GetAllProjects().ToList();

        if (existingProjects.Any())
        {
            Console.WriteLine("\n=== СУЩЕСТВУЮЩИЕ ПРОЕКТЫ ===");
            for (int i = 0; i < existingProjects.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {existingProjects[i].Name}");
            }
            Console.WriteLine($"{existingProjects.Count + 1}. Создать новый проект");
            Console.WriteLine($"{existingProjects.Count + 2}. Отмена");

            int choice = InputService.GetValidatedInput(
                "\nВыберите проект или действие: ",
                input =>
                {
                    if (!int.TryParse(input, out int result))
                        throw new ArgumentException("Введите число");

                    int maxChoice = existingProjects.Count + 2;
                    if (result < 1 || result > maxChoice)
                        throw new ArgumentException($"Введите число от 1 до {maxChoice}");

                    return result;
                }
            );

            if (choice == existingProjects.Count + 2) //отмена
            {
                return null;
            }
            else if (choice == existingProjects.Count + 1) //новый проект
            {
                return CreateNewProject();
            }
            else //существующий проект
            {
                return existingProjects[choice - 1].Name;
            }
        }
        else
        {
            Console.WriteLine("\nНет существующих проектов.");
            Console.WriteLine("1. Создать новый проект");
            Console.WriteLine("2. Отмена");

            int choice = InputService.GetValidatedInput(
                "Выберите действие: ",
                input =>
                {
                    if (!int.TryParse(input, out int result))
                        throw new ArgumentException("Введите число");

                    if (result < 1 || result > 2)
                        throw new ArgumentException("Введите 1 или 2");

                    return result;
                }
            );

            return choice == 1 ? CreateNewProject() : null;
        }
    }

    //создание нового проекта
    private static string? CreateNewProject()
    {
        try
        {
            string name = InputService.GetProjectNameInput(_projectManager);
            string description = InputService.GetDescriptionInput("Описание проекта");

            DateTime? deadline = InputService.GetValidatedInput(
                "Введите срок проекта (дд.мм.гггг или Enter для пропуска): ",
                input => string.IsNullOrEmpty(input) ? (DateTime?)null : DateTime.Parse(input)
            );

            int priority = InputService.GetPriorityInput();

            _projectManager.AddProject(name, description, deadline, priority);
            Console.WriteLine($"\nПроект '{name}' создан успешно!");
            return name;
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"\n Ошибка валидации: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n Ошибка при создании проекта: {ex.Message}");
            return null;
        }
    }
    //отметка задачи как выполненная 
    private static void MarkTaskAsDone()
    {
        try
        {
            var tasks = TaskValidationService.ValidateTasksExist(_taskManager.GetAllTasks(), "отметки как выполненной");
            DisplayTasksForSelection();

            int id = InputService.GetTaskIdInput();
            var task = TaskValidationService.ValidateTaskExists(tasks.FirstOrDefault(t => t.Id == id), id);

            if (task.IsCompleted)
            {
                Console.WriteLine($"\n Задача с ID {id} уже выполнена.");
                return;
            }

            if (_taskManager.MarkTaskAsDone(id))
            {
                Console.WriteLine($"\n Задача с ID {id} отмечена как выполненная!");
            }
            else
            {
                Console.WriteLine($"\n Задача с ID {id} не найдена.");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"\n Ошибка валидации: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n Ошибка: {ex.Message}");
        }
        finally
        {
            WaitForUserInput();
        }
    }

    //выводит все здачи
    private static void DisplayAllTasks()
    {
        DisplayTasksForSelection();
        WaitForUserInput();
    }

    //удаление по айди
    private static void RemoveTask()
    {
        try
        {
            var tasks = TaskValidationService.ValidateTasksExist(_taskManager.GetAllTasks(), "удаления");
            DisplayTasksForSelection();

            int id = InputService.GetTaskIdInput();
            var task = TaskValidationService.ValidateTaskExists(tasks.FirstOrDefault(t => t.Id == id), id);

            bool confirm = InputService.GetConfirmationInput($"Вы уверены, что хотите удалить задачу \"{task.Description}\"?");
            if (!confirm)
            {
                Console.WriteLine("Удаление отменено.");
                return;
            }

            if (_taskManager.RemoveTask(id))
            {
                Console.WriteLine($"\n Задача с ID {id} удалена");
            }
            else
            {
                Console.WriteLine($"\n Задача с ID {id} не найдена");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"\n Ошибка валидации: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n Ошибка: {ex.Message}");
        }
        finally
        {
            WaitForUserInput();
        }
    }


    //выводит список задач по категориям
    private static void DisplayTasksForSelection()
    {
        var tasks = _taskManager.GetAllTasks().ToList();
        Console.WriteLine("\nСписок задач:");
        Console.WriteLine("============");

        // Рабочие задачи
        var workTasksByProject = _taskManager.GetWorkTasksByProject();
        if (workTasksByProject.Any())
        {
            Console.WriteLine("\n=== РАБОЧИЕ ЗАДАЧИ ===");
            foreach (var projectGroup in workTasksByProject)
            {
                Console.WriteLine($"\nПроект: {projectGroup.Key}");
                foreach (var task in projectGroup.OrderBy(t => t.DueDate))
                {
                    Console.WriteLine($"  {task}");
                }
            }
        }

        // Личные задачи
        var personalTasks = tasks.OfType<PersonalTask>()
                               .OrderByDescending(t => t.Priority)
                               .ThenBy(t => t.DueDate)
                               .ToList();
        if (personalTasks.Any())
        {
            Console.WriteLine("\n=== ЛИЧНЫЕ ЗАДАЧИ ===");
            foreach (var task in personalTasks)
            {
                Console.WriteLine(task);
            }
        }

        // Обычные задачи
        var defaultTasks = tasks.Where(t => t is not WorkTask and not PersonalTask)
                               .OrderBy(t => t.DueDate)
                               .ToList();
        if (defaultTasks.Any())
        {
            Console.WriteLine("\n=== ОБЫЧНЫЕ ЗАДАЧИ ===");
            foreach (var task in defaultTasks)
            {
                Console.WriteLine(task);
            }
        }

        if (!tasks.Any())
        {
            Console.WriteLine("Нет задач.");
        }
    }

    //пауза до продолжения
    private static void WaitForUserInput()
    {
        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    //редактирование
    private static void EditTask()
    {
        try
        {
            var tasks = TaskValidationService.ValidateTasksExist(_taskManager.GetAllTasks(), "редактирования");
            DisplayTasksForSelection();

            int taskId = InputService.GetTaskIdInput();
            var task = TaskValidationService.ValidateTaskExists(tasks.FirstOrDefault(t => t.Id == taskId), taskId);
            Console.WriteLine($"\nРедактирование задачи: {task}");
            Console.WriteLine("Какие поля вы хотите изменить?");
            Console.WriteLine("1. Описание");
            Console.WriteLine("2. Срок выполнения");

            if (task is WorkTask)
            {
                Console.WriteLine("3. Проект");
            }
            else if (task is PersonalTask)
            {
                Console.WriteLine("3. Приоритет");
            }

            Console.WriteLine("0. Отмена");

            string fieldChoice = InputService.GetValidatedInput(
                 "Выберите поле для изменения: ",
                 input =>
                 {
                     var validChoices = new[] { "1", "2", "3", "0" };
                     if (!validChoices.Contains(input))
                         throw new ValidationException("Неверный выбор. Введите 1, 2, 3 или 0");

                     if (input == "3")
                     {
                         if (task is not WorkTask && task is not PersonalTask)
                             throw new ValidationException("Это поле недоступно для данного типа задачи");
                     }

                     return input;
                 }
             );

            if (fieldChoice == "0")
            {
                Console.WriteLine("Редактирование отменено.");
                return;
            }

            string? newDescription = null;
            DateTime? newDueDate = null;
            string? newProject = null;
            int? newPriority = null;

            switch (fieldChoice)
            {
                case "1":
                    newDescription = InputService.GetDescriptionInput("новое описание");
                    break;

                case "2":
                    TaskValidationService.ValidateTaskNotCompleted(task, "изменения даты");
                    newDueDate = InputService.GetDueDateInput(false);
                    break;

                case "3" when task is WorkTask:
                    TaskValidationService.ValidateTaskNotCompleted(task, "изменения проекта");
                    newProject = InputService.GetValidatedInput(
                        "Введите новый проект: ",
                        input =>
                        {
                            TaskValidationService.ValidateProject(input);
                            return input;
                        }
                    );
                    break;

                case "3" when task is PersonalTask:
                    TaskValidationService.ValidateTaskNotCompleted(task, "изменения приоритета");
                    newPriority = InputService.GetPriorityInput();
                    break;
            }

            if (newDescription == null && !newDueDate.HasValue && newProject == null && !newPriority.HasValue)
            {
                Console.WriteLine("Не указаны параметры для редактирования.");
                return;
            }

            if (_taskManager.EditTask(taskId, newDescription, newDueDate, newProject, newPriority))
            {
                Console.WriteLine($"\n Задача с ID {taskId} успешно отредактирована!");
            }
            else
            {
                Console.WriteLine($"\n Не удалось отредактировать задачу с ID {taskId}.");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"\n Ошибка валидации: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n Ошибка при редактировании задачи: {ex.Message}");
        }
        finally
        {
            WaitForUserInput();
        }
    }
    //добавление проекта
    private static void AddProject()
    {
        try
        {
            string name = InputService.GetProjectNameInput(_projectManager);
            string description = InputService.GetDescriptionInput("Описание проекта");

            DateTime? deadline = InputService.GetValidatedInput(
                "Введите срок проекта (дд.мм.гггг или Enter для пропуска): ",
                input => string.IsNullOrEmpty(input) ? (DateTime?)null : DateTime.Parse(input)
            );

            int priority = InputService.GetPriorityInput();

            _projectManager.AddProject(name, description, deadline, priority);
            Console.WriteLine("\n Проект добавлен успешно!");
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"\n Ошибка валидации: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n Ошибка: {ex.Message}");
        }
        finally
        {
            WaitForUserInput();
        }
    }

    //вывод всех проектов
    private static void DisplayAllProjects()
    {
        var projects = _projectManager.GetAllProjects().ToList();

        if (!projects.Any())
        {
            Console.WriteLine("\nНет проектов.");
        }
        else
        {
            Console.WriteLine("\n=== ВСЕ ПРОЕКТЫ ===");
            foreach (var project in projects)
            {
                Console.WriteLine(project);

                // Показываем задачи проекта
                var taskIds = _projectManager.GetTaskIdsForProject(project.Id);
                var tasks = taskIds.Select(id => _taskManager.FindTaskById(id)).Where(t => t != null);

                foreach (var task in tasks)
                {
                    Console.WriteLine($"  - {task}");
                }

                Console.WriteLine();
            }
        }

        WaitForUserInput();
    }

    //удаление проекта
    private static void RemoveProject()
    {
        try
        {
            var projects = _projectManager.GetAllProjects().ToList();
            if (!projects.Any())
            {
                Console.WriteLine("\nНет проектов для удаления.");
                return;
            }

            Console.WriteLine("\nСписок проектов:");
            foreach (var project in projects)
            {
                Console.WriteLine($"#{project.Id} - {project.Name}");
            }

            int projectId = InputService.GetProjectIdInput();
            var projectToRemove = _projectManager.FindProjectById(projectId);

            if (projectToRemove == null)
            {
                Console.WriteLine("Проект с таким ID не найден");
                return;
            }

            bool confirm = InputService.GetConfirmationInput($"Вы уверены, что хотите удалить проект \"{projectToRemove.Name}\"?");
            if (!confirm)
            {
                Console.WriteLine("Удаление отменено.");
                return;
            }

            if (_projectManager.RemoveProject(projectId))
            {
                Console.WriteLine("\n Проект удален успешно!");
            }
            else
            {
                Console.WriteLine("\n Не удалось удалить проект.");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"\n Ошибка валидации: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n Ошибка: {ex.Message}");
        }
        finally
        {
            WaitForUserInput();
        }
    }
}