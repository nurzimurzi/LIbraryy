using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormLibr
{
    public partial class UserControl3 : UserControl
    {
        // Свойства
        public int LoanId { get; private set; }
        public int BookId { get; private set; }
        public DateTime DueDate { get; private set; }

        // Событие для кнопки "Продлить"
        public event EventHandler<ExtendLoanEventArgs> ExtendClicked;

        public UserControl3()
        {
            InitializeComponent();

            // Настройка внешнего вида
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;

            // Добавляем обработчик клика на всю карточку (опционально)
            this.Click += UserControl3_Click;

            // Подписываемся на событие кнопки
            button1.Click += Button1_Click;
        }

        // Метод для заполнения данных о выдаче
        public void SetLoanData(int loanId, int bookId, string bookName,
                               DateTime loanDate, DateTime dueDate, bool isOverdue)
        {
            LoanId = loanId;
            BookId = bookId;
            DueDate = dueDate;

            label1.Text = bookName;
            label2.Text = $"Выдана: {loanDate:dd.MM.yyyy}";
            label3.Text = $"Вернуть до: {dueDate:dd.MM.yyyy}";

            // Если просрочка - выделяем красным
            if (isOverdue)
            {
                label3.ForeColor = Color.Red;
                label3.Text += " (ПРОСРОЧКА!)";
                this.BackColor = Color.FromArgb(255, 240, 240); // Светло-розовый
            }
            else
            {
                label3.ForeColor = SystemColors.ControlDarkDark;
                this.BackColor = Color.White;
            }
        }

        // Клик по кнопке "Продлить"
        private void Button1_Click(object sender, EventArgs e)
        {
            ExtendClicked?.Invoke(this, new ExtendLoanEventArgs(LoanId, BookId, DueDate));
        }

        // Клик по карточке (можно открыть детали книги)
        private void UserControl3_Click(object sender, EventArgs e)
        {
            Form4 parentForm = this.FindForm() as Form4;
            if (parentForm != null)
            {
                // Можно открыть Form3 с деталями книги
                // Form3 bookForm = new Form3(BookId, parentForm.UserId, parentForm.RoleId);
                // bookForm.ShowDialog();
            }
        }
    }

    // Класс для передачи данных события
    public class ExtendLoanEventArgs : EventArgs
    {
        public int LoanId { get; private set; }
        public int BookId { get; private set; }
        public DateTime DueDate { get; private set; }

        public ExtendLoanEventArgs(int loanId, int bookId, DateTime dueDate)
        {
            LoanId = loanId;
            BookId = bookId;
            DueDate = dueDate;
        }
    }
}