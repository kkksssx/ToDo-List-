using System;
using System.Text.Json.Serialization;//Атрибуты для управления JSON сериализацией/десериализацией
public enum TaskType //тип определитель, который помогает не допускать ошибки
{                    //редактор будет до сборки говорить, что ты дурачок если опечатаешься 
    Default,
    Work,
    Personal
}
//класс рабочей задачи наследуем от основного c доп project
public class WorkTask : ToDoTask
{
    public string Project { get; set; }

    [JsonConstructor]//атрибут для JSON десериализации (чтения из JSON)

    //добавляет свойство название проекта
    public WorkTask(int id, string description, DateTime dueDate, string project, bool isCompleted = false)
        : base(id, description, dueDate, isCompleted) //принимает и преедаёт параметры в базовый 
    {
        TaskValidationService.ValidateProject(project);
        Project = project;
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

    [JsonConstructor]//атрибут для JSON десериализации (чтения из JSON)

    public PersonalTask(int id, string description, DateTime dueDate, int priority, bool isCompleted = false)
        : base(id, description, dueDate, isCompleted)
    {
        TaskValidationService.ValidatePriority(priority);
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