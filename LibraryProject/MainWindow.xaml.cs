using System;
using System.Linq;
using System.Windows;
using LibraryProject.Data;
using LibraryProject.Models;

namespace LibraryProject
{
    public partial class MainWindow : Window
    {
        private LibraryContext _db = new LibraryContext();

        public MainWindow()
        {
            InitializeComponent();
            _db.Database.EnsureCreated();
            LoadData();
        }

        private void LoadData()
        {
            var books = _db.Books.ToList();
            dgBooks.ItemsSource = books;
            cbAuthorFilter.ItemsSource = _db.Authors.ToList();
            cbGenreFilter.ItemsSource = _db.Genres.ToList();
            lblTotalStock.Text = books.Sum(b => b.QuantityInStock).ToString();
        }

        private void FilterData(object sender, EventArgs e)
        {
            var query = _db.Books.ToList().AsQueryable();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                query = query.Where(b => b.Title.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase));

            if (cbAuthorFilter.SelectedItem is Author a)
                query = query.Where(b => b.Authors.Any(auth => auth.Id == a.Id));

            if (cbGenreFilter.SelectedItem is Genre g)
                query = query.Where(b => b.Genres.Any(genre => genre.Id == g.Id));

            dgBooks.ItemsSource = query.ToList();
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            var book = new Book();
            if (new BookWindow(book, _db).ShowDialog() == true)
            {
                if (_db.Books.Any(b => b.ISBN == book.ISBN)) {
                    MessageBox.Show("ISBN уже занят!");
                    return;
                }
                _db.Books.Add(book);
                _db.SaveChanges();
                LoadData();
            }
        }

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            if (dgBooks.SelectedItem is Book selected)
            {
                if (new BookWindow(selected, _db).ShowDialog() == true)
                {
                    _db.SaveChanges();
                    LoadData();
                }
            }
        }

        private void DeleteBook_Click(object sender, RoutedEventArgs e) {
            if (dgBooks.SelectedItem is Book selected) {
                if (MessageBox.Show("Удалить книгу?", "Вопрос", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    _db.Books.Remove(selected);
                    _db.SaveChanges();
                    LoadData();
                }
            }
        }

        private void ResetFilters(object sender, RoutedEventArgs e) { txtSearch.Clear(); cbAuthorFilter.SelectedItem = null; cbGenreFilter.SelectedItem = null; LoadData(); }
        private void ManageAuthors_Click(object sender, RoutedEventArgs e) { new AuthorWindow(_db).ShowDialog(); LoadData(); }
        private void ManageGenres_Click(object sender, RoutedEventArgs e) { new GenreWindow(_db).ShowDialog(); LoadData(); }
    }
}