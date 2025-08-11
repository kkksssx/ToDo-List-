using System;

//класс рабочей задачи наследуем от основного c доп project
public class WorkTask : ToDoTask
{
    public string Project { get; set; }

    public WorkTask(int id, string description, DateTime dueDate, string project, bool isCompleted = false)
        : base(id, description, dueDate, isCompleted) //принимает и преедаёт параметры в базовый 
    {
        Project = project ?? throw new ArgumentNullException(nameof(project));
    }

    //переопределение 
    public override string ToString()
    {
        string status = IsCompleted ? "[выполнено]" : "[в процессе]";
        string dueInfo = DueDate == DateTime.MinValue ? "Без срока" : $"до {DueDate.ToShortDateString()}";
        string urgency = (DueDate - DateTime.Today).TotalDays <= 1 ? "СРОЧНО! " : "";
        return $"ID: {Id} | Тип: Рабочая | Проект: {Project} | {urgency}Описание: {Description} | Срок: {dueInfo} | Статус: {status}";
    }
}

//класс личной задачи с допполем priority
public class PersonalTask : ToDoTask
{
    public int Priority { get; set; }

    public PersonalTask(int id, string description, DateTime dueDate, int priority, bool isCompleted = false)
        : base(id, description, dueDate, isCompleted)
    {
        if (priority < 1 || priority > 10)
            throw new ArgumentException("Приоритет должен быть от 1 до 10");

        Priority = priority;
    }

    public override string ToString()
    {
        string status = IsCompleted ? "[выполнено]" : "[в процессе]";
        string dueInfo = DueDate == DateTime.MinValue ? "Без срока" : $"до {DueDate.ToShortDateString()}";
        string urgency = (DueDate - DateTime.Today).TotalDays <= 1 ? "СРОЧНО! " : "";
        return $"ID: {Id} | Тип: Личная | Приоритет: {Priority}/10 | {urgency}Описание: {Description} | Срок: {dueInfo} | Статус: {status}";
    }
}