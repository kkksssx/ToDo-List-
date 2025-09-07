using System.Collections.Generic;//содержит классы и интерфейсы для работы с типобезопасными коллекциями

//ИНТЕРФЕЙС - это как шаблон для работы с проектами
//Он не содержит реализацию, а только ОПИСЫВАЕТ что нужно уметь делать
public interface IProjectStorage
{
    List<Project> LoadProjects();
    void SaveProjects(List<Project> projects);
}