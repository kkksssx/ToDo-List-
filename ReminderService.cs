using System;
using System.Linq;//Для работы с LINQ (Language Integrated Query) - технологии запросов к коллекциям и данным
using System.Threading; //для timer-неуправляемый ресурс 
//неуправляемые чистим вручную
//класс для проверки и напоминании о важным 
public class ReminderService : IDisposable //IDisposable-интерфейс для освобождения ресурсов(таймер)
{
    private readonly TaskManager _taskManager;//для списка задач
    private readonly Timer _timer;//запускает Checkreminders каждые 24ч
    private bool _disposed = false;//флажок, чтобы не освободить ресурсы дважды
    private static bool _remindersShownThisSession = false;//флаг для отслеживания показывались ли напоминания

    //конструктор 
    public ReminderService(TaskManager taskManager)
    {
        _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager), "TaskManager cannot be null");
        _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
        _timer = new Timer(_ => CheckReminders(), null, TimeSpan.Zero, TimeSpan.FromHours(24));//запускает таймер и повторяет каждые 24 ч
    }

    //вызывается таймером и проверяет задачи, фильтрует и выводит срочные
    public void CheckReminders()
    {
        try
        {
            var today = DateTime.Today;
            var urgentTasks = _taskManager.GetAllTasks()
                .Where(t => !t.IsCompleted &&//не завершина
                       ((t.DueDate == today) ||//срок сегодня
                        (t is PersonalTask pt && pt.Priority >= 7 && t.DueDate <= today.AddDays(3)) ||//задача с высоким приоритетом в ближайшие 3 дня
                        (t.DueDate < today)))//просрочена
                .OrderBy(t => t.DueDate)//сортировка по сроку
                .ToList();//в список

            if (urgentTasks.Any())//если есть срочные
            {
                Console.WriteLine("\n === ЕЖЕДНЕВНОЕ НАПОМИНАНИЕ === ");
                Console.WriteLine($"У вас есть {urgentTasks.Count} срочных задач:");
                //перебор задач
                foreach (var task in urgentTasks)
                {
                    if (task.DueDate < today)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{task} [ПРОСРОЧЕНА!]");
                        Console.ResetColor();
                    }//если просрочена красным 
                    else if (task.DueDate == today)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;//сегодня желтым
                        Console.WriteLine(task);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(task);
                    }
                }
                Console.WriteLine("===============================\n");
                Console.WriteLine("Нажмите любую клавишу для продолжения...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при проверке напоминаний: {ex.Message}");
        }
    }

    //освобождение ресурсов. обязательный метод
    public void Dispose()
    {
        Dispose(true); //ручное освобождение 
        GC.SuppressFinalize(this);//предупреждает не вызывать финализатор(спецметод сборщика мусора), тк была очистка
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _timer?.Dispose();//останавливает таймер и освобождает ресурсы
            }
            _disposed = true;
        }
    }

    //для проверки в начале 
    // Метод для показа напоминаний при запуске (вызывается только один раз из Main)
    public void ShowStartupReminders()
    {
        if (_remindersShownThisSession) return;

        CheckReminders();
        _remindersShownThisSession = true;
    }

    // Метод для периодической проверки (вызывается таймером)
    private void CheckPeriodicReminders()
    {
        // Периодические напоминания можно показывать многократно
        CheckReminders();
    }
}