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
        Object objectLock = new object();
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

        private List<string> _titles = null;
        public List<string> Titles
        {
            get
            {
                if (_titles == null)
                {
                    _titles = new List<string>();
                    using (var cn = new SQLiteConnection(ConnectionString))
                    {
                        if (cn.State != ConnectionState.Open) cn.Open();
                        using (var dr = new SQLiteCommand("select distinct Title from Books order by Title", cn).ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                _titles.Add(dr[0].ToString());
                            }
                        }
                    }
                }
                return _titles;
            }
        }

        private List<string> _authors = null;
        public List<string> Authors
        {
            get
            {
                if (_authors == null)
                {
                    _authors = new List<string>();
                    using (var cn = new SQLiteConnection(ConnectionString))
                    {
                        if (cn.State != ConnectionState.Open) cn.Open();
                        using (var dr = new SQLiteCommand("select distinct SearchName from Authors order by SearchName", cn).ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                _authors.Add(dr[0].ToString());
                            }
                        }
                    }
                }
                return _authors;
            }
        }

        private List<string> _sequences = null;
        public List<string> Sequences
        {
            get
            {
                if (_sequences == null)
                {
                    _sequences = new List<string>();
                    using (var cn = new SQLiteConnection(ConnectionString))
                    {
                        if (cn.State != ConnectionState.Open) cn.Open();
                        using (var dr = new SQLiteCommand("select distinct SeriesTitle from Series order by SeriesTitle", cn).ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                _sequences.Add(dr[0].ToString());
                            }
                        }
                    }
                }
                return _sequences;
            }
        }

        private List<Genre> _genres = null;
        public List<Genre> FB2Genres
        {
            get
            {
                if (_genres == null)
                {
                    _genres = new List<Genre>();
                    using (var cn = new SQLiteConnection(ConnectionString))
                    {
                        if (cn.State != ConnectionState.Open) cn.Open();
                        using (var dr = new SQLiteCommand("select * from Genres where ParentCode = '0' order by GenreCode", cn).ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var g = new Genre();
                                g.Name = dr["GenreAlias"].ToString();
                                g.Translation = dr["GenreAlias"].ToString();
                                using (var dr1 = new SQLiteCommand("select * from Genres where ParentCode = '" +
                                    dr["GenreCode"].ToString() + "' order by GenreCode", cn).ExecuteReader())
                                {
                                    while (dr1.Read())
                                    {
                                        var g1 = new Genre();
                                        g1.Name = dr1["GenreAlias"].ToString();
                                        g1.Translation = dr1["GenreAlias"].ToString();
                                        g1.Tag = dr1["FB2Code"].ToString();
                                        g.Subgenres.Add(g1);
                                    }
                                }
                                _genres.Add(g);
                            }
                        }
                    }
                }
                return _genres;
            }
        }

        private Dictionary<string, string> _soundexedGenres = null;
        public Dictionary<string, string> SoundexedGenres
        {
            get
            {
                if (_soundexedGenres == null)
                {
                    _soundexedGenres = new Dictionary<string, string>();
                    foreach (Genre genre in FB2Genres)
                        foreach (Genre subgenre in genre.Subgenres)
                        {
                            _soundexedGenres[subgenre.Name.SoundexByWord()] = subgenre.Tag;
                            string reversed = string.Join(" ", subgenre.Name.Split(' ', ',').Reverse()).Trim();
                            _soundexedGenres[reversed.SoundexByWord()] = subgenre.Tag;
                        }
                }
                return _soundexedGenres;
            }
        }

        public List<Genre> Genres
        {
            get
            {
                return FB2Genres.SelectMany(g => g.Subgenres).OrderBy(s => s.Translation).ToList();
            }
        }

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
            List<string> authors = new List<string>();
            lock (objectLock)
            {
                if (isOpenSearch) authors = Authors.Where(a => a.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                else authors = Authors.Where(a => a.StartsWith(name, StringComparison.OrdinalIgnoreCase)).ToList();
                if (isOpenSearch && authors.Count == 0)
                {
                    string reversedName = name.Reverse();
                    authors = Authors.Where(a => a.IndexOf(reversedName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }
                return authors;
            }
        }

        public Book GetBook(string id)
        {
            //using (var cn = new SQLiteConnection(ConnectionString))
            //{
            //    if (cn.State != ConnectionState.Open) cn.Open();
            //    var o = new SQLiteCommand("select * from Books where BookID = ", cn).ExecuteScalar();
            //    return (int)(long)o;
            //}
            return new Book();
        }

        public int GetBooksByAuthorCount(string author)
        {
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                if (cn.State != ConnectionState.Open) cn.Open();
                var rt = (int)(long)new SQLiteCommand("select count(*) from Author_List al inner join Authors a on al.AuthorID = a.AuthorID where a.SearchName like '" + author + "%'", cn).ExecuteScalar();
                return rt;
            }
        }



        public List<Book> GetBooksByAuthor(string author)
        {
            var lst = new List<Book>();
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                if (cn.State != ConnectionState.Open) cn.Open();
                using (var dr = new SQLiteCommand(
                    "select b.* from Author_List al inner join Authors a on al.AuthorID = a.AuthorID inner join Books b on al.BookID = b.BookID where a.SearchName like '" 
                        + author + "%' order by b.Title", cn).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        //var o = new Book(dr["folder"].ToString() + "@" + dr["FileName"].ToString() + dr["ext"].ToString());
                        //o.AddedDate = ConvertDate(dr["UpdateDate"].ToString());
                        //o.Annotation = dr["Annotation"].ToString();
                        //o.BookDate = DateTime.MinValue;
                        //o.ID = Utils.CreateGuid(Utils.IsoOidNamespace, o.FileName).ToString();
                        //o.Title = dr["Title"].ToString();
                        //o.Language = dr["Lang"].ToString();
                        //o.HasCover = false;
                        //o.DocumentDate = DateTime.MinValue;
                        //if (!(dr["SeriesID"] is DBNull))
                        //    o.Sequence = dr["SeriesID"].ToString();
                        //else
                        //    o.Sequence = null;
                        //if (!(dr["SeqNumber"] is DBNull))
                        //    o.NumberInSequence = (uint)(long)dr["SeqNumber"];
                        //else
                        //    o.NumberInSequence = 0;
                        //o.DocumentSize = (uint)(long)dr["BookSize"];
                        //var _id = (long)dr["BookID"];
                        //using (var dr1 = new SQLiteCommand("", cn).ExecuteReader())
                        //{

                        //}
                    }
                }
            }
            return lst;
        }

        private DateTime ConvertDate(string s)
        {
            if (!String.IsNullOrWhiteSpace(s))
                return new DateTime(Int32.Parse(s.Substring(0, 4)), Int32.Parse(s.Substring(5, 2)), Int32.Parse(s.Substring(8, 2)));
            else
                return DateTime.MinValue;
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
