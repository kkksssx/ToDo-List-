using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Task
{
    public int Id { get; }
    public string Description { get; }
    public DateTime DueDate { get; private set; }
    public bool IsCompleted { get; private set; }

    public Task(int id, string description, DateTime dueDate, bool isCompleted = false)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание задачи не может быть пустым");

        if (dueDate != DateTime.MinValue && dueDate < DateTime.Today)
            throw new ArgumentException("Дата выполнения должна быть в настоящим или будущим днем");

        Id = id;
        Description = description;
        DueDate = dueDate;
        IsCompleted = isCompleted;
    }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
    }

    public void UpdateDueDate(DateTime newDueDate)
    {
        if (newDueDate < DateTime.Today)
            throw new ArgumentException("Новая дата не может быть в прошлом");
        DueDate = newDueDate;
    }

    public override string ToString()
    {
        string status = IsCompleted ? "[выполнено]" : "[в процессе]";
        string dueInfo = DueDate == DateTime.MinValue ? "Без срока" : $"до {DueDate.ToShortDateString()}";
        return $"ID: {Id} | Описание: {Description} | Срок: {dueInfo} | Статус: {status}";
    }
}