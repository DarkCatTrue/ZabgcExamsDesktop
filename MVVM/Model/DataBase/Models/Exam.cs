using System.ComponentModel;

namespace ZabgcExamsDesktop.MVVM.Model.DataBase.Models;

public partial class Exam : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private int _idExam;
    private int? _idGroup;
    private int? _idDiscipline;
    private int? _idTypeOfLesson;
    private int? _idTypeOfExam;
    private int? _idQualification;
    private DateTime? _dateEvent;
    private Group? _idGroupNavigation;

    public int IdExam { get => _idExam; set { _idExam = value; OnPropertyChanged(nameof(IdExam)); }}

    public int? IdGroup { get => _idGroup; set { _idGroup = value; OnPropertyChanged(nameof(IdGroup)); }}

    public int? IdDiscipline { get => _idDiscipline; set { _idDiscipline = value; OnPropertyChanged(nameof(IdDiscipline)); }}

    public int? IdTypeOfLesson { get => _idTypeOfLesson; set { _idTypeOfLesson = value; OnPropertyChanged(nameof(IdTypeOfLesson)); }}

    public int? IdTypeOfExam { get => _idTypeOfExam; set { _idTypeOfExam = value; OnPropertyChanged(nameof(IdTypeOfExam)); }}

    public int? IdQualification { get => _idQualification; set { _idQualification = value; OnPropertyChanged(nameof(IdQualification)); } }

    public DateTime? DateEvent { get => _dateEvent; set { _dateEvent = value; OnPropertyChanged(nameof(DateEvent)); }}

    public virtual Discipline? IdDisciplineNavigation { get; set; }

    public virtual Group? IdGroupNavigation { get => _idGroupNavigation; set { _idGroupNavigation = value; OnPropertyChanged(nameof(IdGroupNavigation)); }}

    public virtual Qualification? IdQualificationNavigation { get; set; }

    public virtual TypeOfExam? IdTypeOfExamNavigation { get; set; }

    public virtual TypeOfLesson? IdTypeOfLessonNavigation { get; set; }

    public virtual ICollection<Audience> IdAudiences { get; set; } = new List<Audience>();

    public virtual ICollection<Teacher> IdTeachers { get; set; } = new List<Teacher>();

}
