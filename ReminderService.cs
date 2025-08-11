using System;
using System.Linq;
using System.Threading; //для timer-неуправляемый ресурс 
//неуправляемые чистим вручную
//класс для проверки и напоминании о важным 
public class ReminderService : IDisposable //IDisposable-интерфейс для освобождения ресурсов(таймер)
{
    private readonly TaskManager _taskManager;//для списка задач
    private readonly Timer _timer;//запускает Checkreminders каждые 24ч
    private bool _disposed = false;//флажок, чтобы не освободить ресурсы дважды

    public ReminderService(TaskManager taskManager)
    {
        _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
        _timer = new Timer(new TimerCallback(CheckReminders), null, TimeSpan.Zero, TimeSpan.FromHours(24));//запускает таймер и повторяет каждые 24 ч
    }

    //вызывается таймером и проверяет задачи, фильтрует и выводит срочные
    private void CheckReminders(object? state)
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
                Console.WriteLine("\n=== ЕЖЕДНЕВНОЕ НАПОМИНАНИЕ ===");
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
}