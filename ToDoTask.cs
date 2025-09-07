using System;
using System.Text.Json.Serialization;//Атрибуты для управления JSON сериализацией/десериализацией

//атрибуты для поддержки полиморфной сериализации/десериализации JSON
//позволяют System.Text.Json правильно определять тип объекта при работе с наследованием
[JsonDerivedType(typeof(ToDoTask), typeDiscriminator: "base")]// Базовый тип
[JsonDerivedType(typeof(WorkTask), typeDiscriminator: "work")]// Рабочая задача
[JsonDerivedType(typeof(PersonalTask), typeDiscriminator: "personal")]// Личная задача
public class ToDoTask //Основной класс для задачи
{
    public int Id { get; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; } 
    public bool IsCompleted { get; protected set; }

    [JsonConstructor]// Конструктор для JSON десериализации (чтения из JSON)

    public ToDoTask(int id, string description, DateTime dueDate, bool isCompleted = false)
    {    //валидация
        TaskValidationService.ValidateDescription(description);
        TaskValidationService.ValidateDueDate(dueDate);
        //значения для свойств
        Id = id;
        Description = description ?? throw new ArgumentNullException(nameof(description));//если нулевое описание вызываем исключение 
        DueDate = dueDate;
        IsCompleted = isCompleted;
    }

    public virtual void MarkAsCompleted() { IsCompleted = true; } //+virtual для переопределения в наследнике

    public virtual void UpdateDueDate(DateTime newDueDate)
    {
        if (newDueDate < DateTime.Today)
            throw new ArgumentException("Новая дата не может быть в прошлом");
        DueDate = newDueDate;
    }

    public override string ToString() //переопределения метода из базового класса для нормального вывода содержимого
    {
        string status = IsCompleted ? "[выполнено]" : "[в процессе]";
        string dueInfo = DueDate == DateTime.MinValue ? "Без срока" : $"до {DueDate.ToShortDateString()}";
        return $"ID: {Id} | Тип: Обычная | Описание: {Description} | Срок: {dueInfo} | Статус: {status}";
    }
}