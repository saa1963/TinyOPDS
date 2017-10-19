using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using TinyOPDS.Properties;
using System.IO;

namespace TinyOPDS.Data
{
    public class MyHomeLibrary : ILibrary
    {
        private string ConnectionString;
        public MyHomeLibrary()
        {
            if (!String.IsNullOrWhiteSpace(Settings.Default.LibraryPath))
            {
                ConnectionString = GetConnectionString();
            }
            Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LibraryPath") ConnectionString = GetConnectionString();
        }

        private string GetConnectionString()
        {
            var dir =  Path.Combine(Directory.GetParent(Settings.Default.LibraryPath).FullName, "MyHomeLib", "Data");
            var files = Directory.GetFiles(dir, "*.hlc2");
            var file = "";
            var minDate = DateTime.MinValue;
            foreach (var f in files)
            {
                var fi = new FileInfo(f);
                if (fi.CreationTime > minDate)
                {
                    minDate = fi.CreationTime;
                    file = f;
                }
            }
            if (file == "")
            {
                throw new FileNotFoundException(String.Format("Файл БД в папке {0} не найден", dir));
            }
            return "Data Source=" + file + ";Version=3;";
        }
        public string LibraryPath { get; set; }
        public bool IsChanged { get; set; }

        public int Count
        {
            get
            {
                using (var cn = new SQLiteConnection(ConnectionString))
                {
                    if (cn.State != ConnectionState.Open) cn.Open();
                    var o = new SQLiteCommand("select count(BookID) from Books", cn).ExecuteScalar();
                    return (int)(long)o;
                }
            }
        }

        public int FB2Count
        {
            get
            {
                using (var cn = new SQLiteConnection(ConnectionString))
                {
                    if (cn.State != ConnectionState.Open) cn.Open();
                    return (int)(long)new SQLiteCommand("select count(BookID) from Books where SearchExt = '.FB2'", cn).ExecuteScalar();
                }
            }
        }

        public int EPUBCount
        {
            get
            {
                using (var cn = new SQLiteConnection(ConnectionString))
                {
                    if (cn.State != ConnectionState.Open) cn.Open();
                    return (int)(long)new SQLiteCommand("select count(BookID) from Books where SearchExt = '.EPUB'", cn).ExecuteScalar();
                }
            }
        }

        public List<string> Titles
        {
            get
            {
                //lock (_books)
                //{
                //    return _books.Values.Select(b => b.Title).Distinct().OrderBy(a => a, new OPDSComparer(Localizer.Language.Equals("ru"))).ToList();
                //}
                var lst = new List<string>();
                using (var cn = new SQLiteConnection(ConnectionString))
                {
                    if (cn.State != ConnectionState.Open) cn.Open();
                    using (var dr = new SQLiteCommand("select distinct Title from Books order by Title", cn).ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lst.Add(dr[0].ToString());
                        }
                    }
                    return lst;
                }
            }
        }

        public List<string> Authors
        {
            get
            {
                var lst = new List<string>();
                using (var cn = new SQLiteConnection(ConnectionString))
                {
                    if (cn.State != ConnectionState.Open) cn.Open();
                    using (var dr = new SQLiteCommand("select distinct SearchName from Authors order by SearchName", cn).ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lst.Add(dr[0].ToString());
                        }
                    }
                    return lst;
                }
            }
        }

        public List<string> Sequences
        {
            get
            {
                var lst = new List<string>();
                using (var cn = new SQLiteConnection(ConnectionString))
                {
                    if (cn.State != ConnectionState.Open) cn.Open();
                    using (var dr = new SQLiteCommand("select distinct SeriesTitle from Series order by SeriesTitle", cn).ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lst.Add(dr[0].ToString());
                        }
                    }
                    return lst;
                }
            }
        }

        public List<Genre> FB2Genres { get { throw new NotImplementedException(); } }

        public Dictionary<string, string> SoundexedGenres { get { throw new NotImplementedException(); } }

        public List<Genre> Genres { get { throw new NotImplementedException(); } }

        public event EventHandler LibraryLoaded;

        public bool Add(Book book)
        {
            throw new NotImplementedException();
        }

        public void Append(Book book)
        {
            throw new NotImplementedException();
        }

        public bool Contains(string bookPath)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string fileName)
        {
            throw new NotImplementedException();
        }

        public List<string> GetAuthorsByName(string name, bool isOpenSearch)
        {
            throw new NotImplementedException();
        }

        public Book GetBook(string id)
        {
            throw new NotImplementedException();
        }

        public List<Book> GetBooksByAuthor(string author)
        {
            throw new NotImplementedException();
        }

        public List<Book> GetBooksByGenre(string genre)
        {
            throw new NotImplementedException();
        }

        public List<Book> GetBooksBySequence(string sequence)
        {
            throw new NotImplementedException();
        }

        public List<Book> GetBooksByTitle(string title)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void LoadAsync()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}
