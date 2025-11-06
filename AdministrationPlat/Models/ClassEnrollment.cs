namespace AdministrationPlat.Models;

public class ClassEnrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public Student? Student { get; set; }

    public int SchoolClassId { get; set; }

    public SchoolClass? SchoolClass { get; set; }
}
