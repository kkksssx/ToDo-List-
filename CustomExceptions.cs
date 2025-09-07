using System;
using System.Collections.Generic;//содержит классы и интерфейсы для работы с типобезопасными коллекциями
using System.Linq;//Для работы с LINQ (Language Integrated Query) - технологии запросов к коллекциям и данным
using FluentValidation.Results;//Для работы с результатами валидации FluentValidation библиотеки

//специальный класс исключений для ошибок валидации
//наследуется от стандартного Exception, но добавляет дополнительную информацию
public class ValidationException : Exception
{
    //список ошибок валидации (может быть null если ошибок нет)
    // ValidationFailure - это класс из FluentValidation с информацией об ошибке
    public IEnumerable<ValidationFailure>? Errors { get; }

    //конструктор для одиночной ошибки (просто текстовое сообщение)
    public ValidationException(string message) : base(message)
    {
        Errors = null;
    }

    //конструктор для множественных ошибок (принимает список ValidationFailure)
    public ValidationException(IEnumerable<ValidationFailure> errors)
        : base("Ошибки валидации: " + string.Join("; ", errors.Select(e => e.ErrorMessage)))
    {
        Errors = errors;// Сохраняем полный список ошибок
    }
}