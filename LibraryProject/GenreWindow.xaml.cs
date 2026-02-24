using System;
using System.Linq;
using System.Windows;
using LibraryProject.Models;

namespace LibraryProject
{
    public partial class GenreWindow : Window
    {
        private Data.LibraryContext _db;
        public GenreWindow(Data.LibraryContext db)
        {
            InitializeComponent();
            _db = db;
            Load();
        }
        private void Load() => lstGenres.ItemsSource = _db.Genres.ToList();

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            // Проверка на заглавную букву
            if (!char.IsUpper(name[0])) {
                MessageBox.Show("Название жанра должно начинаться с ЗАГЛАВНОЙ буквы!");
                return;
            }

            // Проверка на дубликат
            bool exists = _db.Genres.AsEnumerable().Any(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            
            if (exists) {
                MessageBox.Show("Такой жанр уже существует (в любом регистре)!", "Дубликат");
                return;
            }

            _db.Genres.Add(new Genre { Name = name, Description = txtDesc.Text });
            _db.SaveChanges();
            txtName.Clear(); txtDesc.Clear(); Load();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (lstGenres.SelectedItem is Genre g) {
                _db.Genres.Remove(g);
                _db.SaveChanges();
                Load();
            }
        }
    }
}