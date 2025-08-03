public class Program
{
    private static readonly TaskManager _taskManager = new TaskManager();
    public static void Main()
    {
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
                case "5": return;
                default: Console.WriteLine("\nНеверный выбор, введите число от 1 до 5."); break;
            }
        }
    }
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
        Console.Write("\nВведите описание задачи: ");
        var description = Console.ReadLine();

        while (string.IsNullOrWhiteSpace(description))
        {
            Console.WriteLine("Описание не может быть пустым");
            Console.Write("Введите описание задачи: ");
            description = Console.ReadLine();
        }

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
            else
            {
                validDate = true;
            }
        }

        try
        {
            _taskManager.AddTask(description, dueDate);
            Console.WriteLine("\nЗадача добавлена");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nОшибка: {ex.Message}");
        }
    }

    private static void MarkTaskAsDone()
    {
        var tasks = _taskManager.GetAllTasks().ToList();
        if (!tasks.Any())
        {
            Console.WriteLine("\nНет задач для отметки.");
            return;
        }

        DisplayAllTasks();
        Console.Write("\nВведите ID задачи для отметки как выполненной: ");

        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Ошибка: ID должен быть числом");
            return;
        }

        if (_taskManager.MarkTaskAsDone(id))
            Console.WriteLine($"\nЗадача с ID {id} отмечена как выполненная!");
        else
            Console.WriteLine($"\nЗадача с ID {id} не найдена.");
    }

    private static void DisplayAllTasks()
    {
        var tasks = _taskManager.GetAllTasks().ToList();
        Console.WriteLine("\nСписок задач:");
        Console.WriteLine("------------");

        if (!tasks.Any())
        {
            Console.WriteLine("Нет задач.");
        }
        else
        {
            foreach (var task in tasks)
                Console.WriteLine(task);
        }
    } 
    private static void RemoveTask()
    {
        var tasks = _taskManager.GetAllTasks().ToList();
        if (!tasks.Any())
        {
            Console.WriteLine("\nНет задач для удаления.");
            return;
        }

        DisplayAllTasks();
        Console.Write("\nВведите ID задачи для удаления: ");

        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Ошибка: ID должен быть числом");
            return;
        }

        if (_taskManager.RemoveTask(id))
            Console.WriteLine($"\nЗадача с ID {id} удалена");
        else
            Console.WriteLine($"\nЗадача с ID {id} не найдена");
    }
}