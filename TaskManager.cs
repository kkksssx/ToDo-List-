using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class TaskManager
{
    private readonly List<Task> _tasks = new List<Task>();
    private readonly HashSet<int> _usedIds = new HashSet<int>();
    private readonly Random _random = new Random();
    private const string FilePath = "tasks.txt";
    public TaskManager()
    {
        LoadTasks();
    }
    public void AddTask(string description, DateTime dueDate)
    {
        var task = new Task(GenerateId(), description, dueDate);
        _tasks.Add(task);
        SaveTasks();
    }
    public bool MarkTaskAsDone(int taskId)
    {
        var task = FindTaskById(taskId);
        if (task == null) return false;

        task.MarkAsCompleted();
        SaveTasks();
        return true;
    }
    public bool RemoveTask(int taskId)
    {
        var task = FindTaskById(taskId);
        if (task == null) return false;

        _tasks.Remove(task);
        _usedIds.Remove(task.Id);
        SaveTasks();
        return true;
    }
    public IEnumerable<Task> GetAllTasks() => _tasks.AsReadOnly();
    private Task FindTaskById(int taskId) => _tasks.FirstOrDefault(t => t.Id == taskId);
    private int GenerateId()
    {
        int newId;
        do
        {
            newId = _random.Next(100, 1000);
        } while (_usedIds.Contains(newId));

        _usedIds.Add(newId);
        return newId;
    }
    private void LoadTasks()
    {
        if (!File.Exists(FilePath)) return;

        foreach (string line in File.ReadAllLines(FilePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('|');
            if (parts.Length != 4) continue;

            try
            {
                var task = new Task(int.Parse(parts[0]), parts[1], new DateTime(long.Parse(parts[2])),
                    bool.Parse(parts[3]));

                _tasks.Add(task);
                _usedIds.Add(task.Id);
            }
            catch
            {
                
            }
        }
    }
    private void SaveTasks()
    {
        var lines = _tasks.Select(x => $"{x.Id}|{x.Description}|{x.DueDate.Ticks}|{x.IsCompleted}");
        File.WriteAllLines(FilePath, lines);
    }
}
