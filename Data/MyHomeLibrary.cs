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
                        using (var dr = new SQLiteCommand("select distinct SearchSeriesTitle from Series order by SearchSeriesTitle", cn).ExecuteReader())
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

            return _lst[id];
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


        Dictionary<string, Book> _lst = new Dictionary<string, Book>();
        public List<Book> GetBooksByAuthor(string author)
        {
            _lst.Clear();
            var lst = new List<Book>();
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                if (cn.State != ConnectionState.Open) cn.Open();
                using (var dr = new SQLiteCommand(
                    "select b.* from Author_List al inner join Authors a on al.AuthorID = a.AuthorID inner join Books b on al.BookID = b.BookID where a.SearchName like '" 
                        + author + "%' collate NOCASE", cn).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var _id = dr["folder"].ToString() + "@" + dr["FileName"].ToString() + dr["ext"].ToString();
                        var o = new Book(_id);
                        o.AddedDate = ConvertDate(dr["UpdateDate"].ToString());
                        o.Annotation = dr["Annotation"].ToString();
                        o.BookDate = DateTime.MinValue;
                        o.ID = _id; //Utils.CreateGuid(Utils.IsoOidNamespace, o.FileName).ToString();
                        o.Title = dr["Title"].ToString();
                        o.Language = dr["Lang"].ToString();
                        o.HasCover = false;
                        o.DocumentDate = DateTime.MinValue;
                        if (!(dr["SeriesID"] is DBNull))
                            o.Sequence = dr["SeriesID"].ToString();
                        else
                            o.Sequence = null;
                        if (!(dr["SeqNumber"] is DBNull))
                            o.NumberInSequence = (uint)(long)dr["SeqNumber"];
                        else
                            o.NumberInSequence = 0;
                        o.DocumentSize = (uint)(long)dr["BookSize"];
                        using (var dr1 = new SQLiteCommand("select * from Author_List al inner join Authors a on al.AuthorID = a.AuthorID where al.BookID = " + dr["BookID"].ToString() + "  collate NOCASE", cn).ExecuteReader())
                        {
                            while (dr1.Read())
                            {
                                o.Authors.Add(dr1["SearchName"].ToString());
                            }
                        }
                        using (var dr1 = new SQLiteCommand("select * from Genre_List gl inner join Genres g on gl.GenreCode = g.GenreCode where gl.BookID = " + dr["BookID"].ToString() + "  collate NOCASE", cn).ExecuteReader())
                        {
                            while (dr1.Read())
                            {
                                o.Genres.Add(dr1["GenreAlias"].ToString());
                            }
                        }
                        lst.Add(o);
                        if (!_lst.ContainsKey(_id))
                            _lst.Add(_id, o);
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
            _lst.Clear();
            var lst = new List<Book>();
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                if (cn.State != ConnectionState.Open) cn.Open();
                using (var dr = new SQLiteCommand(
                    "select b.*, g.GenreAlias, a.SearchName from Genre_List gl inner join Genres g on gl.GenreCode = g.GenreCode inner join Books b on gl.BookID = b.BookID " +
                        "inner join Author_List al on b.BookID = al.BookID inner join Authors a on al.AuthorID = a.AuthorID " +
                        "where g.FB2Code = '" + genre + "' order by b.BookID", cn).ExecuteReader())
                {
                    bool first = true;
                    long _bookid = 0;
                    Book o = null;
                    string _id = null;
                    while (dr.Read())
                    {
                        if (_bookid != (long)dr["BookID"])
                        {
                            if (first) first = false;
                            else
                            {
                                lst.Add(o);
                                if (!_lst.ContainsKey(_id))
                                    _lst.Add(_id, o);
                            }
                            _id = dr["Folder"].ToString() + "@" + dr["FileName"].ToString() + dr["ext"].ToString();
                            o = new Book(_id);
                            o.AddedDate = ConvertDate(dr["UpdateDate"].ToString());
                            o.Annotation = dr["Annotation"].ToString();
                            o.BookDate = DateTime.MinValue;
                            o.ID = _id; //Utils.CreateGuid(Utils.IsoOidNamespace, o.FileName).ToString();
                            o.Title = dr["Title"].ToString();
                            o.Language = dr["Lang"].ToString();
                            o.HasCover = false;
                            o.DocumentDate = DateTime.MinValue;
                            if (!(dr["SeriesID"] is DBNull))
                                o.Sequence = dr["SeriesID"].ToString();
                            else
                                o.Sequence = null;
                            if (!(dr["SeqNumber"] is DBNull))
                                o.NumberInSequence = (uint)(long)dr["SeqNumber"];
                            else
                                o.NumberInSequence = 0;
                            o.DocumentSize = (uint)(long)dr["BookSize"];
                            o.Genres.Add(dr["GenreAlias"].ToString());
                            o.Authors.Add(dr["SearchName"].ToString());
                        }
                        else
                        {
                            o.Authors.Add(dr["SearchName"].ToString());
                        }
                    }
                    if (o != null)
                    {
                        lst.Add(o);
                        if (!_lst.ContainsKey(_id))
                            _lst.Add(_id, o);
                    }
                }
            }
            return lst;
        }

        public int GetBooksBySequenceCount(string sequence)
        {
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                if (cn.State != ConnectionState.Open) cn.Open();
                var cSql = "select count(*) from Books b inner join Series s on b.SeriesID = s.SeriesID where s.SearchSeriesTitle like @p1 collate NOCASE";
                var cmd = new SQLiteCommand(cSql, cn);
                cmd.Parameters.Add("@p1", DbType.String, 80).Value = sequence;
                return (int)(long)cmd.ExecuteScalar();
            }
        }

        public List<Book> GetBooksBySequence(string sequence)
        {
            _lst.Clear();
            var lst = new List<Book>();
            using (var cn = new SQLiteConnection(ConnectionString))
            {
                if (cn.State != ConnectionState.Open) cn.Open();
                var cSql = "select b.* from Books b inner join Series s on b.SeriesID = s.SeriesID where s.SearchSeriesTitle like @p1 collate NOCASE";
                var cmd = new SQLiteCommand(cSql, cn);
                cmd.Parameters.Add("@p1", DbType.String, 80).Value = sequence;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var _id = dr["folder"].ToString() + "@" + dr["FileName"].ToString() + dr["ext"].ToString();
                        var o = new Book(_id);
                        o.AddedDate = ConvertDate(dr["UpdateDate"].ToString());
                        o.Annotation = dr["Annotation"].ToString();
                        o.BookDate = DateTime.MinValue;
                        o.ID = _id; //Utils.CreateGuid(Utils.IsoOidNamespace, o.FileName).ToString();
                        o.Title = dr["Title"].ToString();
                        o.Language = dr["Lang"].ToString();
                        o.HasCover = false;
                        o.DocumentDate = DateTime.MinValue;
                        if (!(dr["SeriesID"] is DBNull))
                            o.Sequence = dr["SeriesID"].ToString();
                        else
                            o.Sequence = null;
                        if (!(dr["SeqNumber"] is DBNull))
                            o.NumberInSequence = (uint)(long)dr["SeqNumber"];
                        else
                            o.NumberInSequence = 0;
                        o.DocumentSize = (uint)(long)dr["BookSize"];
                        using (var dr1 = new SQLiteCommand("select * from Author_List al inner join Authors a on al.AuthorID = a.AuthorID where al.BookID = " + dr["BookID"].ToString() + "  collate NOCASE", cn).ExecuteReader())
                        {
                            while (dr1.Read())
                            {
                                o.Authors.Add(dr1["SearchName"].ToString());
                            }
                        }
                        using (var dr1 = new SQLiteCommand("select * from Genre_List gl inner join Genres g on gl.GenreCode = g.GenreCode where gl.BookID = " + dr["BookID"].ToString() + "  collate NOCASE", cn).ExecuteReader())
                        {
                            while (dr1.Read())
                            {
                                o.Genres.Add(dr1["GenreAlias"].ToString());
                            }
                        }
                        lst.Add(o);
                        if (!_lst.ContainsKey(_id))
                            _lst.Add(_id, o);
                    }
                }
            }
            return lst;
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
