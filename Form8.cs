using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class Form8 : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=Libraryy;Integrated Security=True";
        private int bookId;
        private string bookName;

        public Form8(int bookId, string bookName)
        {
            InitializeComponent();
            this.bookId = bookId;
            this.bookName = bookName;

            label4.Text = $"Экземпляры: {bookName}";

            // Настройка DataGridView
            SetupDataGridView();

            // Подписка на события
            button1.Click += BtnAddCopy_Click;
            button2.Click += BtnBack_Click;

            // Загрузка данных
            LoadCopies();
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
            Column1.DataPropertyName = "InventoryNumber";
            Column1.HeaderText = "Инвентарный номер";
            Column1.Width = 150;

            Column2.DataPropertyName = "StatusName";
            Column2.HeaderText = "Статус";
            Column2.Width = 100;

            Column3.HeaderText = "Действия";
            Column3.Width = 80;
            Column3.Text = "⚙️";
            Column3.UseColumnTextForButtonValue = true;
        }

        private void LoadCopies()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            bc.Id,
                            bc.InventoryNumber,
                            sc.NameStatusCopies as StatusName,
                            sc.Id as StatusId
                        FROM BookCopies bc
                        JOIN StatusCopies sc ON bc.StatusCopiesId = sc.Id
                        WHERE bc.BookId = @bookId
                        ORDER BY bc.InventoryNumber";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    adapter.SelectCommand.Parameters.AddWithValue("@bookId", bookId);

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridView1.DataSource = dt;

                    // Подсчет статистики
                    int total = dt.Rows.Count;
                    int available = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["StatusName"].ToString() == "Доступен")
                            available++;
                    }

                    label3.Text = $"Всего: {total} экз. | Доступно: {available} экз.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки экземпляров: " + ex.Message);
            }
        }

        private void BtnAddCopy_Click(object sender, EventArgs e)
        {
            string invNumber = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите инвентарный номер нового экземпляра:",
                "Добавление экземпляра",
                "");

            if (!string.IsNullOrWhiteSpace(invNumber))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Проверка уникальности инвентарного номера
                        string checkQuery = "SELECT COUNT(*) FROM BookCopies WHERE InventoryNumber = @inv";
                        SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                        checkCmd.Parameters.AddWithValue("@inv", invNumber);

                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Экземпляр с таким инвентарным номером уже существует!");
                            return;
                        }

                        // Получаем новый ID
                        string getIdQuery = "SELECT ISNULL(MAX(Id), 0) + 1 FROM BookCopies";
                        SqlCommand getIdCmd = new SqlCommand(getIdQuery, connection);
                        int newId = (int)getIdCmd.ExecuteScalar();

                        // Добавляем новый экземпляр (статус 1 = Доступен)
                        string insertQuery = @"
                            INSERT INTO BookCopies (Id, BookId, InventoryNumber, StatusCopiesId)
                            VALUES (@id, @bookId, @inv, 1)";

                        SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
                        insertCmd.Parameters.AddWithValue("@id", newId);
                        insertCmd.Parameters.AddWithValue("@bookId", bookId);
                        insertCmd.Parameters.AddWithValue("@inv", invNumber);

                        insertCmd.ExecuteNonQuery();

                        MessageBox.Show("Экземпляр добавлен!");
                        LoadCopies();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении: " + ex.Message);
                }
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.RowIndex >= 0) // Клик по кнопке действий
            {
                int copyId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["Id"].Value);
                string invNumber = dataGridView1.Rows[e.RowIndex].Cells["Column1"].Value.ToString();
                string currentStatus = dataGridView1.Rows[e.RowIndex].Cells["Column2"].Value.ToString();

                // Создаем контекстное меню
                ContextMenuStrip menu = new ContextMenuStrip();

                ToolStripMenuItem changeStatusItem = new ToolStripMenuItem("📝 Изменить статус");
                changeStatusItem.Click += (s, ev) => ChangeStatus(copyId, invNumber, currentStatus);

                ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ Удалить");
                deleteItem.ForeColor = Color.Red;
                deleteItem.Click += (s, ev) => DeleteCopy(copyId, invNumber);

                menu.Items.Add(changeStatusItem);
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.Add(deleteItem);

                // Показываем меню
                menu.Show(dataGridView1, dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location);
            }
        }

        private void ChangeStatus(int copyId, string invNumber, string currentStatus)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Получаем список статусов
                    DataTable statuses = new DataTable();
                    new SqlDataAdapter("SELECT Id, NameStatusCopies FROM StatusCopies", connection).Fill(statuses);

                    // Создаем форму выбора статуса
                    Form selectForm = new Form();
                    selectForm.Text = "Изменение статуса";
                    selectForm.Size = new Size(300, 200);
                    selectForm.StartPosition = FormStartPosition.CenterParent;

                    ComboBox cmbStatus = new ComboBox();
                    cmbStatus.Location = new Point(20, 20);
                    cmbStatus.Size = new Size(240, 30);
                    cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
                    cmbStatus.DisplayMember = "NameStatusCopies";
                    cmbStatus.ValueMember = "Id";
                    cmbStatus.DataSource = statuses;

                    Button btnOk = new Button();
                    btnOk.Text = "OK";
                    btnOk.Location = new Point(80, 60);
                    btnOk.Size = new Size(120, 30);
                    btnOk.DialogResult = DialogResult.OK;

                    selectForm.Controls.Add(cmbStatus);
                    selectForm.Controls.Add(btnOk);

                    if (selectForm.ShowDialog() == DialogResult.OK)
                    {
                        int newStatusId = (int)cmbStatus.SelectedValue;

                        string updateQuery = "UPDATE BookCopies SET StatusCopiesId = @status WHERE Id = @id";
                        SqlCommand cmd = new SqlCommand(updateQuery, connection);
                        cmd.Parameters.AddWithValue("@status", newStatusId);
                        cmd.Parameters.AddWithValue("@id", copyId);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Статус изменен!");
                        LoadCopies();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void DeleteCopy(int copyId, string invNumber)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем, был ли экземпляр когда-либо выдан
                    string checkQuery = "SELECT COUNT(*) FROM Loans WHERE CopyId = @id";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@id", copyId);

                    int loanCount = (int)checkCmd.ExecuteScalar();

                    if (loanCount > 0)
                    {
                        DialogResult result = MessageBox.Show(
                            "Этот экземпляр был в выдаче. Удалить его нельзя.\nХотите изменить статус на 'Списан'?",
                            "Внимание",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            // Меняем статус на "Списан" (предположим ID=4)
                            string updateQuery = "UPDATE BookCopies SET StatusCopiesId = 4 WHERE Id = @id";
                            SqlCommand updateCmd = new SqlCommand(updateQuery, connection);
                            updateCmd.Parameters.AddWithValue("@id", copyId);
                            updateCmd.ExecuteNonQuery();

                            MessageBox.Show("Статус изменен на 'Списан'");
                            LoadCopies();
                        }
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show(
                            $"Удалить экземпляр {invNumber}?",
                            "Подтверждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            string deleteQuery = "DELETE FROM BookCopies WHERE Id = @id";
                            SqlCommand deleteCmd = new SqlCommand(deleteQuery, connection);
                            deleteCmd.Parameters.AddWithValue("@id", copyId);
                            deleteCmd.ExecuteNonQuery();

                            MessageBox.Show("Экземпляр удален");
                            LoadCopies();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}