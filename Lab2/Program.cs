
var student1 = new Student(1, "Alice", 20, new List<string> { "Math", "Science" });
var student2 = student1 with { Courses = new List<string>(student1.Courses) { "History" } };
var instructor1 = new Instructor { Name = "John Doe", Department = "Mathematics", Email = "john.doe@example.com" };
Console.WriteLine(instructor1);

var listaStudenti = new List<string>();
while (true)
{
    Console.Write("Introdu numele studentului (sau Enter pentru a termina): ");
    var nume = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(nume))
    {
        listaStudenti.Add(nume);    
    }
        break;
    
}
Console.WriteLine("Lista completa de studenti:");
foreach (var nume in listaStudenti)
{
    Console.WriteLine(nume);
}

var listaCursuri = new List<Course>
{
    new Course("Matematica", 2),
    new Course("Programare Orientata pe Obiecte", 6),
    new Course("Structuri de Date", 5),
    new Course("Baze de Date", 4),
    new Course("Algoritmi", 5),
    new Course("Limba Engleza", 3),
    new Course("Sisteme de Operare", 4),
    new Course("Educatie Fizica", 1)
};

Func<Course, bool> filtreazaCursuri = c => c.Credits > 3;

var cursuriFilrate = listaCursuri.Where(filtreazaCursuri).ToList();

Console.WriteLine("\nCursuri cu mai mult de 3 credite:");
foreach (var curs in cursuriFilrate)
{
    Console.WriteLine($"- {curs.Title}: {curs.Credits} credite");
}

static void Afiseaza(object obj)
{
    switch (obj)
    {
        case Student s:
            Console.WriteLine($"Student: {s.Name}, Numar cursuri: {s.Courses?.Count ?? 0}");
            break;
        case Course c:
            Console.WriteLine($"Course: {c.Title}, Credite: {c.Credits}");
            break;
        case not null:
            Console.WriteLine("Tip necunoscut");
            break;
        case null:
            Console.WriteLine("Obiect null");
            break;
    }
}

Afiseaza(student1);
Afiseaza(new Course("POO", 5));
Afiseaza("altceva");

public record Student(int id, string Name, int Age, List<string> Courses);
public class Instructor
{
    public string Name { get; init; }
    public string Department { get; init; }
    public string Email { get; init; }
}
public record Course(string Title, int Credits);
