using System;
using System.Threading.Tasks;//для работы с асинхронностью и задачами(.Run)
using System.Linq;

//класс приложения с main
public class Program
{
    private static readonly TaskManager _taskManager = new TaskManager();//для управления списком задач
    private static ReminderService? _reminderService;//для напоминаний, может быть null

    public static void Main()
    {
        _reminderService = new ReminderService(_taskManager);
        //фоновая проверка напоминаний (Task.Run()-запускает асинхронную задачу на фоне)
        Task.Run(() => _reminderService?.GetType().GetMethod("CheckReminders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(_reminderService, new object?[] { null }));//получаем тип объяекта, вызываем метод вручную,чтобы он на фоне проверял задачи

        Console.WriteLine("Система управления задачами с напоминаниями");

        while (true)
        {
            DisplayMenu();
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1": AddTask(); break;
                case "2": MarkTaskAsDone(); break;
                case "3": DisplayAllTasks(); break;
                case "4": RemoveTask(); break;
                case "5":
                    _reminderService?.Dispose();//освобождение ресурсов
                    Console.WriteLine("Выход из программы...");
                    return;
                default:
                    Console.WriteLine("\nНеверный выбор, введите число от 1 до 5.");
                    WaitForUserInput();
                    break;
            }
        }
    }

    //выводменю
    private static void DisplayMenu()
    {
        Console.WriteLine("\nСписок задач");
        Console.WriteLine("----------------------");
        Console.WriteLine("1. Добавить задачу");
        Console.WriteLine("2. Отметить задачу как выполненную");
        Console.WriteLine("3. Показать все задачи");
        Console.WriteLine("4. Удалить задачу");
        Console.WriteLine("5. Выход");
        Console.WriteLine("----------------------");
        Console.Write("Выберите действие: ");
    }

    private static void AddTask()
    {
        try
        { //тип задачи
            Console.WriteLine("\nВыберите тип задачи:");
            Console.WriteLine("1. Обычная задача");
            Console.WriteLine("2. Рабочая задача (с проектом)");
            Console.WriteLine("3. Личная задача (с приоритетом)");
            Console.Write("Ваш выбор: ");
            var typeChoice = Console.ReadLine();

            string taskType = "default";//для определения типа задачи
            string? project = null;//название проекта
            int priority = 1;//для личных

            switch (typeChoice)
            {
                case "1":
                    taskType = "default";
                    break;
                case "2":
                    taskType = "work";//тип задачи рабочая
                    var existingProjects = _taskManager.GetAllProjects().ToList();//получаем список проектов
                    if (existingProjects.Any())//выводит
                    {
                        Console.WriteLine("\nСуществующие проекты:");
                        for (int i = 0; i < existingProjects.Count; i++)//предлагает из существующих или новый
                        {
                            Console.WriteLine($"{i + 1}. {existingProjects[i]}");
                        }
                        Console.WriteLine($"{existingProjects.Count + 1}. Создать новый проект");
                        Console.Write("Выберите проект: ");
                        //обработка выбора
                        if (int.TryParse(Console.ReadLine(), out int projectChoice))
                        {
                            if (projectChoice > 0 && projectChoice <= existingProjects.Count)
                            {
                                project = existingProjects[projectChoice - 1];
                            }
                            else if (projectChoice == existingProjects.Count + 1)
                            {
                                Console.Write("Введите название нового проекта: ");
                                project = Console.ReadLine();
                                while (string.IsNullOrWhiteSpace(project))
                                {
                                    Console.WriteLine("Название проекта не может быть пустым");
                                    Console.Write("Введите название проекта: ");
                                    project = Console.ReadLine();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Неверный выбор проекта.");
                                WaitForUserInput();
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Неверный ввод.");
                            WaitForUserInput();
                            return;
                        }
                    }
                    else
                    {
                        Console.Write("Введите название проекта: ");
                        project = Console.ReadLine();
                        while (string.IsNullOrWhiteSpace(project))
                        {
                            Console.WriteLine("Название проекта не может быть пустым");
                            Console.Write("Введите название проекта: ");
                            project = Console.ReadLine();
                        }
                    }
                    break;
                case "3":
                    taskType = "personal";//устанавливает тип задачи личной
                    //добавление приоритета
                    bool validPriority = false;
                    while (!validPriority)
                    {
                        Console.Write("Введите приоритет (1-10): ");
                        if (int.TryParse(Console.ReadLine(), out priority) && priority >= 1 && priority <= 10)
                        {
                            validPriority = true;
                        }
                        else
                        {
                            Console.WriteLine("Неверный приоритет. Введите число от 1 до 10.");
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Неверный выбор типа задачи. Будет создана обычная задача.");
                    taskType = "default";
                    break;
            }
            //добавление описания
            Console.Write("\nВведите описание задачи: ");
            var description = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("Описание не может быть пустым");
                Console.Write("Введите описание задачи: ");
                description = Console.ReadLine();
            }
            //добавление даты
            DateTime dueDate = DateTime.MinValue;
            bool validDate = false;

            while (!validDate)
            {
                Console.Write("Введите срок выполнения (дд.мм.гггг или Enter для пропуска): ");
                var dateInput = Console.ReadLine();

                if (string.IsNullOrEmpty(dateInput))
                {
                    validDate = true;
                }
                else if (!DateTime.TryParse(dateInput, out dueDate))
                {
                    Console.WriteLine("Неверный формат даты");
                }
                else if (dueDate < DateTime.Today)
                {
                    Console.WriteLine("Дата не может быть в прошлом");
                }
                else
                {
                    validDate = true;
                }
            }
            //добавление задачи
            _taskManager.AddTask(description!, dueDate, taskType, project, priority);
            Console.WriteLine("\nЗадача добавлена");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nОшибка: {ex.Message}");
        }
        finally//даже если были ошибки
        {
            WaitForUserInput();
        }
    }

    //отметка задачи как выполненная 
    private static void MarkTaskAsDone()
    {
        try
        {
            var tasks = _taskManager.GetAllTasks().ToList();//список всех задач
            if (!tasks.Any())
            {
                Console.WriteLine("\nНет задач для отметки.");
                WaitForUserInput();
                return;
            }

            DisplayTasksForSelection();//отображаем 
            Console.Write("\nВведите ID задачи для отметки как выполненной: ");

            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("Ошибка: ID должен быть числом");
            }
            else if (_taskManager.MarkTaskAsDone(id))
            {
                Console.WriteLine($"\nЗадача с ID {id} отмечена как выполненная!");
            }
            else
            {
                Console.WriteLine($"\nЗадача с ID {id} не найдена.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nОшибка: {ex.Message}");
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
            var tasks = _taskManager.GetAllTasks().ToList();
            if (!tasks.Any())
            {
                Console.WriteLine("\nНет задач для удаления.");
                WaitForUserInput();
                return;
            }

            DisplayTasksForSelection();
            Console.Write("\nВведите ID задачи для удаления: ");

            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("Ошибка: ID должен быть числом");
            }
            else if (_taskManager.RemoveTask(id))
            {
                Console.WriteLine($"\nЗадача с ID {id} удалена");
            }
            else
            {
                Console.WriteLine($"\nЗадача с ID {id} не найдена");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nОшибка: {ex.Message}");
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
}