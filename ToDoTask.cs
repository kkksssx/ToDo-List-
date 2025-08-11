using System;

public class ToDoTask //Основной класс для задачи
{
    public int Id { get; }
    public string Description { get; }
    public DateTime DueDate { get; protected set; } //protected для того чтобы можно было изменять только в классе
    public bool IsCompleted { get; protected set; }

    public ToDoTask(int id, string description, DateTime dueDate, bool isCompleted = false)
    {    //валидация
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание задачи не может быть пустым");
        
        if (dueDate != DateTime.MinValue && dueDate < DateTime.Today)
            throw new ArgumentException("Дата выполнения должна быть в настоящем или будущем");
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