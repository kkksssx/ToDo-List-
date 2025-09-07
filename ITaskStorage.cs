using System.Collections.Generic;//содержит классы и интерфейсы для работы с типобезопасными коллекциями

//ИНТЕРФЕЙС - это как "инструкция" или "договор" для работы с задачами
//"Любой класс, который хочет работать с задачами, ДОЛЖЕН уметь делать вот это"
public interface ITaskStorage
{
    List<ToDoTask> LoadTasks();
    void SaveTasks(List<ToDoTask> tasks);
}