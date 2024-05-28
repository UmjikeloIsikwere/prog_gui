using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace StudentManagementApp
{
    public class Student
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int PhysicsGrade { get; set; }
        public int MathGrade { get; set; }
    }

    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=students.db";

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadStudents();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists("students.db"))
            {
                SQLiteConnection.CreateFile("students.db");
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    var createStudentsTable = new SQLiteCommand(
                        "CREATE TABLE Students (" +
                        "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "FullName TEXT NOT NULL, " +
                        "PhoneNumber TEXT)", connection);
                    createStudentsTable.ExecuteNonQuery();

                    var createGradesTable = new SQLiteCommand(
                        "CREATE TABLE Grades (" +
                        "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "StudentId INTEGER NOT NULL, " +
                        "PhysicsGrade INTEGER NOT NULL, " +
                        "MathGrade INTEGER NOT NULL, " +
                        "FOREIGN KEY(StudentId) REFERENCES Students(Id))", connection);
                    createGradesTable.ExecuteNonQuery();
                }
            }
        }

        private void LoadStudents()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    var command = new SQLiteCommand(
                        "SELECT s.Id, s.FullName, s.PhoneNumber, " +
                        "g.PhysicsGrade, g.MathGrade " +
                        "FROM Students s " +
                        "LEFT JOIN Grades g ON s.Id = g.StudentId", connection);

                    var reader = command.ExecuteReader();
                    var students = new List<Student>();

                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            PhoneNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            PhysicsGrade = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            MathGrade = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                        });
                    }

                    StudentsListView.ItemsSource = students;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    var insertStudentCommand = new SQLiteCommand(
                        "INSERT INTO Students (FullName, PhoneNumber) " +
                        "VALUES (@FullName, @PhoneNumber)", connection);
                    insertStudentCommand.Parameters.AddWithValue("@FullName", FullNameTextBox.Text);
                    insertStudentCommand.Parameters.AddWithValue("@PhoneNumber", PhoneNumberTextBox.Text);
                    insertStudentCommand.ExecuteNonQuery();

                    var lastInsertId = connection.LastInsertRowId;

                    var insertGradesCommand = new SQLiteCommand(
                        "INSERT INTO Grades (StudentId, PhysicsGrade, MathGrade) " +
                        "VALUES (@StudentId, @PhysicsGrade, @MathGrade)", connection);
                    insertGradesCommand.Parameters.AddWithValue("@StudentId", lastInsertId);
                    insertGradesCommand.Parameters.AddWithValue("@PhysicsGrade", int.Parse(PhysicsGradeTextBox.Text));
                    insertGradesCommand.Parameters.AddWithValue("@MathGrade", int.Parse(MathGradeTextBox.Text));
                    insertGradesCommand.ExecuteNonQuery();

                    transaction.Commit();

                    FullNameTextBox.Clear();
                    PhoneNumberTextBox.Clear();
                    PhysicsGradeTextBox.Clear();
                    MathGradeTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении данных: " + ex.Message);
                }
            }

            LoadStudents();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsListView.SelectedItem is Student selectedStudent)
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    try
                    {
                        connection.Open();
                        var transaction = connection.BeginTransaction();

                        var updateStudentCommand = new SQLiteCommand(
                            "UPDATE Students SET FullName = @FullName, PhoneNumber = @PhoneNumber " +
                            "WHERE Id = @Id", connection);
                        updateStudentCommand.Parameters.AddWithValue("@FullName", FullNameTextBox.Text);
                        updateStudentCommand.Parameters.AddWithValue("@PhoneNumber", PhoneNumberTextBox.Text);
                        updateStudentCommand.Parameters.AddWithValue("@Id", selectedStudent.Id);
                        updateStudentCommand.ExecuteNonQuery();

                        var updateGradesCommand = new SQLiteCommand(
                            "UPDATE Grades SET PhysicsGrade = @PhysicsGrade, MathGrade = @MathGrade " +
                            "WHERE StudentId = @StudentId", connection);
                        updateGradesCommand.Parameters.AddWithValue("@PhysicsGrade", int.Parse(PhysicsGradeTextBox.Text));
                        updateGradesCommand.Parameters.AddWithValue("@MathGrade", int.Parse(MathGradeTextBox.Text));
                        updateGradesCommand.Parameters.AddWithValue("@StudentId", selectedStudent.Id);
                        updateGradesCommand.ExecuteNonQuery();

                        transaction.Commit();

                        FullNameTextBox.Clear();
                        PhoneNumberTextBox.Clear();
                        PhysicsGradeTextBox.Clear();
                        MathGradeTextBox.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при обновлении данных: " + ex.Message);
                    }
                }

                LoadStudents();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsListView.SelectedItem is Student selectedStudent)
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    try
                    {
                        connection.Open();
                        var transaction = connection.BeginTransaction();

                        var deleteGradesCommand = new SQLiteCommand(
                            "DELETE FROM Grades WHERE StudentId = @StudentId", connection);
                        deleteGradesCommand.Parameters.AddWithValue("@StudentId", selectedStudent.Id);
                        deleteGradesCommand.ExecuteNonQuery();

                        var deleteStudentCommand = new SQLiteCommand(
                            "DELETE FROM Students WHERE Id = @Id", connection);
                        deleteStudentCommand.Parameters.AddWithValue("@Id", selectedStudent.Id);
                        deleteStudentCommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при удалении данных: " + ex.Message);
                    }
                }

                LoadStudents();
            }
        }
    }
}
