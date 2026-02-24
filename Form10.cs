using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class Form10 : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Libraryy;Integrated Security=True";
        private int userId;
        private int roleId;

        public Form10(int userId, int roleId)
        {
            InitializeComponent();
            this.userId = userId;
            this.roleId = roleId;

            // Настройка DataGridView
            SetupDataGridView();

            // Загрузка данных
            LoadFilters();
            LoadLoans();

            // Подключение событий
            this.button1.Click += BtnApplyFilter_Click;
            this.button2.Click += BtnResetFilter_Click;
            this.button5.Click += BtnBack_Click;
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.RowHeadersVisible = false;

            // Настройка колонок
            Column1.DataPropertyName = "ReaderName";
            Column1.HeaderText = "Читатель";

            Column2.DataPropertyName = "BookName";
            Column2.HeaderText = "Книга";
            Column2.Width = 250;

            Column3.DataPropertyName = "LoanDate";
            Column3.HeaderText = "Дата выдачи";
            Column3.DefaultCellStyle.Format = "dd.MM.yyyy";

            Column4.DataPropertyName = "DueDate";
            Column4.HeaderText = "Дата возврата";
            Column4.DefaultCellStyle.Format = "dd.MM.yyyy";

            Column5.DataPropertyName = "StatusName";
            Column5.HeaderText = "Статус";
            Column5.Width = 150;
        }

        private void LoadFilters()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Загрузка читателей
                    DataTable readersTable = new DataTable();
                    string readersQuery = @"
                        SELECT DISTINCT 
                            u.Id,
                            u.Surname + ' ' + u.Name + ' ' + ISNULL(u.Patronmic, '') as FullName
                        FROM Loans l
                        JOIN [User] u ON l.UserId = u.Id
                        ORDER BY FullName";

                    SqlDataAdapter readersAdapter = new SqlDataAdapter(readersQuery, connection);
                    readersAdapter.Fill(readersTable);

                    DataRow allReaders = readersTable.NewRow();
                    allReaders["Id"] = 0;
                    allReaders["FullName"] = "Все читатели";
                    readersTable.Rows.InsertAt(allReaders, 0);

                    comboBox1.DisplayMember = "FullName";
                    comboBox1.ValueMember = "Id";
                    comboBox1.DataSource = readersTable;
                    comboBox1.SelectedIndex = 0;

                    // Загрузка статусов
                    DataTable statusesTable = new DataTable();
                    string statusesQuery = "SELECT Id, Name FROM StatusLoan ORDER BY Name";

                    SqlDataAdapter statusesAdapter = new SqlDataAdapter(statusesQuery, connection);
                    statusesAdapter.Fill(statusesTable);

                    DataRow allStatuses = statusesTable.NewRow();
                    allStatuses["Id"] = 0;
                    allStatuses["Name"] = "Все статусы";
                    statusesTable.Rows.InsertAt(allStatuses, 0);

                    comboBox2.DisplayMember = "Name";
                    comboBox2.ValueMember = "Id";
                    comboBox2.DataSource = statusesTable;
                    comboBox2.SelectedIndex = 0;

                    // Настройка дат
                    dateTimePicker2.Value = DateTime.Now.AddMonths(-1);
                    dateTimePicker1.Value = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки фильтров: " + ex.Message);
            }
        }

        private void LoadLoans()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            u.Surname + ' ' + u.Name + ' ' + ISNULL(u.Patronmic, '') as ReaderName,
                            b.NameBook as BookName,
                            l.LoanDate,
                            l.DueDate,
                            sl.Name as StatusName
                        FROM Loans l
                        JOIN [User] u ON l.UserId = u.Id
                        JOIN BookCopies bc ON l.CopyId = bc.Id
                        JOIN Book b ON bc.BookId = b.Id
                        JOIN StatusLoan sl ON l.StatusLoanId = sl.Id
                        WHERE 1=1";

                    // Применяем фильтры
                    if (comboBox1.SelectedValue != null && (int)comboBox1.SelectedValue > 0)
                    {
                        query += " AND l.UserId = @userId";
                    }

                    if (comboBox2.SelectedValue != null && (int)comboBox2.SelectedValue > 0)
                    {
                        query += " AND l.StatusLoanId = @statusId";
                    }

                    query += " AND l.LoanDate >= @dateFrom AND l.LoanDate <= @dateTo";
                    query += " ORDER BY l.LoanDate DESC";

                    SqlCommand command = new SqlCommand(query, connection);

                    if (comboBox1.SelectedValue != null && (int)comboBox1.SelectedValue > 0)
                    {
                        command.Parameters.AddWithValue("@userId", comboBox1.SelectedValue);
                    }

                    if (comboBox2.SelectedValue != null && (int)comboBox2.SelectedValue > 0)
                    {
                        command.Parameters.AddWithValue("@statusId", comboBox2.SelectedValue);
                    }

                    command.Parameters.AddWithValue("@dateFrom", dateTimePicker2.Value.Date);
                    command.Parameters.AddWithValue("@dateTo", dateTimePicker1.Value.Date.AddDays(1).AddSeconds(-1));

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridView1.DataSource = dt;
                    label6.Text = $"Всего записей: {dt.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void BtnApplyFilter_Click(object sender, EventArgs e)
        {
            LoadLoans();
        }

        private void BtnResetFilter_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            dateTimePicker2.Value = DateTime.Now.AddMonths(-1);
            dateTimePicker1.Value = DateTime.Now;
            LoadLoans();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            Form5 librarianForm = new Form5(userId, roleId);
            librarianForm.Show();
            this.Close();
        }
    }
}