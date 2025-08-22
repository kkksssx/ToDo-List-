using System;
using System.Collections.Generic;
using System.Linq;

//класс проекта
public class Project
{
    public int Id { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedDate { get; }
    public DateTime? Deadline { get; set; }
    public int Priority { get; set; }
    public bool IsCompleted { get; set; }
    public List<int> TaskIds { get; } = new List<int>(); // айди задач проекта

    //конструктор проекта
    public Project(int id, string name, string description, DateTime? deadline = null, int priority = 1, bool isCompleted = false)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        CreatedDate = DateTime.Now;
        Deadline = deadline;
        Priority = priority;
        IsCompleted = isCompleted;
    }

    //добавление задачи в проект
    public void AddTask(int taskId)
    {
        if (!TaskIds.Contains(taskId))
            TaskIds.Add(taskId);
    }

    //удаление
    public void RemoveTask(int taskId)
    {
        TaskIds.Remove(taskId);
    }

    //отметка проекта
    public void MarkAsCompleted()
    {
        IsCompleted = true;
    }

    //количество задач проекта
    public int GetTasksCount() => TaskIds.Count;

    //для строкового представления
    public override string ToString()
    {
        string status = IsCompleted ? "[завершен]" : "[активен]";
        string deadlineInfo = Deadline.HasValue ? $"до {Deadline.Value.ToShortDateString()}" : "без срока";
        return $"Проект #{Id} {Name} | {status} | Приоритет: {Priority}/10 | Задачи: {GetTasksCount()} | Срок: {deadlineInfo}";
    }
}